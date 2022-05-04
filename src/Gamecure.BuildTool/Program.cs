using System.Diagnostics;
using Gamecure.BuildTool.Builds;
using Gamecure.BuildTool.Builds.OSX;
using Gamecure.BuildTool.Builds.Windows;
using Gamecure.BuildTool.CommandLine;
using Gamecure.BuildTool.Configuration;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

using var _ = Logger.Start(new ConsoleLogWriter());

var timer = Stopwatch.StartNew();

var result = await new CommandList<Context>("dotnet run --project src/Gamecure.BuildTool --")
    .WithCommand(new RootCommand<ConfigContext>("config")
        .WithOption(new Option<ConfigContext>("template")
        {
            Alias = "t",
            Validate = value => !string.IsNullOrWhiteSpace(value),
            Callback = (config, value) => config with { Template = value },
            Description = "Use a template file for the configuration. For example --template config_template.json"
        })
        .WithOption(new Option<ConfigContext>("project")
        {
            Alias = "p",
            Description = "The Google Project Name",
            Validate = s => !string.IsNullOrWhiteSpace(s),
            Callback = (config, value) => config with { GoogleProjectName = value }
        })
        .WithOption(new Option<ConfigContext>("clientid")
        {
            Alias = "cid",
            Description = "The Google Auth Client Id",
            Validate = s => !string.IsNullOrWhiteSpace(s),
            Callback = (config, value) => config with { GoogleClientId = value }
        })
        .WithOption(new Option<ConfigContext>("container")
        {
            Alias = "c",
            Description = "The Google Storage Container",
            Validate = s => !string.IsNullOrWhiteSpace(s),
            Callback = (config, value) => config with { GoogleContainer = value }
        })
        .WithOption(new Option<ConfigContext>("gitcommit")
        {
            Alias = "gc",
            Description = "The git commit hash for this version",
            Validate = s => !string.IsNullOrWhiteSpace(s),
            Callback = (config, value) => config with { GitCommit = value }
        })
        .WithOption(new Option<ConfigContext>("repository")
        {
            Alias = "r",
            Description = "The plastic repository",
            Validate = s => !string.IsNullOrWhiteSpace(s),
            Callback = (config, value) => config with { PlasticRepository = value }
        })
        .WithOption(new Option<ConfigContext>("output")
        {
            Alias = "o",
            Description = "The config file output",
            Validate = value => !string.IsNullOrWhiteSpace(value),
            Callback = (config, value) => config with { OutputFilename = value }
        })
        .WithOption(new Option<ConfigContext>("force")
        {
            Description = "Overwrite the config file if it already exists",
            Alias = "f",
            RequiresArguments = false,
            Callback = (config, _) => config with { Overwrite = true }
        })
        .OnCommand(config => new PipelineBuilder<ConfigContext>()
            .With<ReadConfigTemplate>()
            .With<SetConfigValues>()
            .With<ValidateConfig>()
            .With<WriteConfig>()
            .Build()
            .Invoke(config)))
    .WithCommand(new RootCommand<BuildContext>("build")
        .WithOption(new Option<BuildContext>("config")
        {
            Alias = "c",
            Description = "The Configuration {Release|Debug}",
            Validate = value => "release".Equals(value, StringComparison.OrdinalIgnoreCase) || "debug".Equals(value, StringComparison.OrdinalIgnoreCase),
            Callback = (build, value) => build with { Config = value }
        })
        .WithOption(new Option<BuildContext>("runtime")
        {
            Alias = "r",
            Description = "The runtime {win|osx|linux}",
            Validate = value => !string.IsNullOrWhiteSpace(value) && Enum.TryParse(typeof(BuildRuntime), value, true, out var _),
            Callback = (build, value) => build with { Runtime = Enum.Parse<BuildRuntime>(value!, true) }
        })
        .WithOption(new Option<BuildContext>("output")
        {
            Alias = "o",
            Description = "Output folder",
            Validate = value => !string.IsNullOrWhiteSpace(value),
            Callback = (build, value) => build with { OutputFolder = value }
        })
        .WithOption(new Option<BuildContext>("zip")
        {
            Alias = "z",
            Description = "Zip the build",
            Validate = value => !string.IsNullOrWhiteSpace(value),
            Callback = (build, value) => build with { ZipFile = value }
        })
        .WithOption(new Option<BuildContext>("configfile")
        {
            Alias = "cf",
            Description = "Path to a config.json to bundle with Gamecure.",
            Validate = value => !string.IsNullOrWhiteSpace(value),
            Callback = (build, value) => build with { ConfigFile = value }
        })
        .WithOption(new Option<BuildContext>("package")
        {
            Alias = "p",
            Description = "The type of package to build, Application and Installer. Installer is only available on Windows at the moment.",
            Validate = value => Enum.TryParse<BuildPackage>(value, true, out var _),
            Callback = (build, value) => build with { Package = Enum.Parse<BuildPackage>(value!, true) }
        })
        .OnCommand(build => new PipelineBuilder<BuildContext>()
            .With<SetBuildArguments>()
            .With<ZipBinary>()
            .With<RunBuildCommand>()

            // Mac AppBundle
            .With<CreateMacAppBundle>()
            .With<GenerateMacPList>()
            .With<MoveGamecureFiles>()
            .With<SetExecutablePermission>()

            // Config
            .With<CopyConfig>()

            // Windows installer
            .With<BuildWindowsInstaller>()
            .Build()
            .Invoke(build)))
    .Execute(args);

timer.Stop();

if (result is null)
{
    Logger.Error("The command failed validation and returned null.");
    return -2;
}
if (result.Failed)
{
    Logger.Error($"The command failed after {timer.Elapsed.TotalMilliseconds:##.000} ms with reason: {result.Reason}");
    return -1;
}

Logger.Info($"Command was successfully executed in {timer.Elapsed.TotalMilliseconds:##.000} ms");

return 0;

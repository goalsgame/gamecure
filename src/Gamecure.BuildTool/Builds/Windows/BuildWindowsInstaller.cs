using Gamecure.Core;
using Gamecure.Core.Common;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.BuildTool.Builds.Windows;

internal class BuildWindowsInstaller : IMiddleware<BuildContext>
{
    public bool ShouldRun(BuildContext context) => context.Package == BuildPackage.Installer && context.Runtime == BuildRuntime.Win;
    public async Task<BuildContext> OnInvoke(BuildContext context, ContextDelegate<BuildContext> next)
    {
        var configFile = Path.Combine(context.OutputFolder!, "config.json");

        if (!File.Exists(configFile))
        {
            return context with { Failed = true, Reason = $"Config file not found at path {configFile}. The installer requires a config file." };
        }

        var arguments = new[]
        {
            Path.Combine("src", "Gamecure.Installer", "Gamecure.Installer.wixproj"),
            "/t:Rebuild",
            $"-p:Configuration={context.Config}",
            $"-p:OutputPath={Path.GetFullPath(context.OutputFolder!)}",
            $"-p:GamecurePath={Path.GetFullPath(context.OutputFolder!)}",
            $"-p:ConfigPath={Path.GetFullPath(configFile)}"
        };

        var result = await ProcessRunner.Run("msbuild", string.Join(' ', arguments), GlobalConfiguration.BaseDirectory, TimeSpan.FromMinutes(10));
        if (!result.Success)
        {
            Logger.Error($"Failed to build with exit code: {result.ExitCode}");
            Logger.Error($"StdOut: {result.StdOutAsString()}");
            Logger.Error($"StdErr: {result.StdErrAsString()}");
            return context with { Failed = true, Reason = $"Failed to build the installer with exit code: {result.ExitCode}" };
        }
        return await next(context);
    }
}
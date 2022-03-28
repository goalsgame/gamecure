using System.Diagnostics;
using Gamecure.Core.Common;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Editor.DownloadAndCopy;

public class DownloadEditor : IMiddleware<DownloadEditorContext>
{
    public async Task<DownloadEditorContext> OnInvoke(DownloadEditorContext context, ContextDelegate<DownloadEditorContext> next)
    {
        var editorPath = Path.Combine(GlobalConfiguration.CacheDirectory, "editor");
        Directory.CreateDirectory(editorPath);

        var editorVersion = context.Version;

        Logger.Info($"Downloading editor from changeset {editorVersion.Changeset} on branch {editorVersion.Branch}");
        var storagePlatform = GlobalConfiguration.Platform switch
        {
            Platform.Linux => "linux",
            Platform.Windows => "win64",
            Platform.Macos => "mac",
            _ => throw new ArgumentOutOfRangeException()
        };

        var arguments = $"downsync --storage-uri gs://{context.Container}/{storagePlatform}/editor/storage --source-path gs://{context.Container}/{editorVersion.Filename} --target-path \"{editorPath}\"";
        Logger.Info($"Running command: {context.LongtailPath} {arguments}");
        Logger.Info("This might take some time...");
        var environmentVariables = new KeyValuePair<string, string>[]
        {
            new ("GOOGLE_APPLICATION_CREDENTIALS", context.CredentialsPath)
        };

        var timer = Stopwatch.StartNew();
        var downloadResult = await ProcessRunner.Run(context.LongtailPath, arguments, GlobalConfiguration.AppDataDirectory, TimeSpan.FromMinutes(20), environmentVariables);
        if (!downloadResult.Success)
        {
            Logger.Error($"Download editor failed with code {downloadResult.ExitCode}");
            Logger.Error($"Stdout:{Environment.NewLine}{downloadResult.StdOutAsString()}");
            Logger.Error($"Stderr:{Environment.NewLine}{downloadResult.StdErrAsString()}");
            return context with { Failed = true, Reason = $"Download failed with code {downloadResult.ExitCode}" };
        }
        Logger.Info($"Download completed after {timer.Elapsed.TotalSeconds:##.###} seconds.");
        return await next(context with { EditorPath = editorPath });
    }
}
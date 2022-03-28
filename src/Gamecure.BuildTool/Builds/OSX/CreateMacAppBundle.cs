using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.BuildTool.Builds.OSX;

internal class CreateMacAppBundle : IMiddleware<BuildContext>
{
    public bool ShouldRun(BuildContext context) => context.Runtime == BuildRuntime.Osx;
    public async Task<BuildContext> OnInvoke(BuildContext context, ContextDelegate<BuildContext> next)
    {
        if (context.OutputFolder == null)
        {
            return context with { Failed = true, Reason = "Output folder must be set when bundling a mac app." };
        }

        const string appName = "Gamecure.app";
        var appPath = Path.Combine(context.OutputFolder, appName);
        if (Directory.Exists(appPath))
        {
            Logger.Info($"Found an old {appName}, deleting the directory.");
            Directory.Delete(appPath, true);
        }

        var contentPath = Path.Combine(appPath, "Contents");
        var binaryPath = Path.Combine(contentPath, "MacOS");
        var resourcesPath = Path.Combine(contentPath, "Resources");

        Directory.CreateDirectory(appPath);
        Directory.CreateDirectory(contentPath);
        Directory.CreateDirectory(binaryPath);
        Directory.CreateDirectory(resourcesPath);

        return await next(context with
        {
            MacBundle = new MacBundleConfig(appPath, contentPath, binaryPath, resourcesPath)
        });
    }
}
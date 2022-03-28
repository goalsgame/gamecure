using Gamecure.Core;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.BuildTool.Builds.OSX;

internal class MoveGamecureFiles : IMiddleware<BuildContext>
{
    public bool ShouldRun(BuildContext context) => context.Runtime == BuildRuntime.Osx;
    public async Task<BuildContext> OnInvoke(BuildContext context, ContextDelegate<BuildContext> next)
    {
        var binaryFiles = new[]
        {
            "Gamecure.GUI",
            "libHarfBuzzSharp.dylib",
            "libSkiaSharp.dylib",
            "libAvaloniaNative.dylib"
        };

        foreach (var file in binaryFiles)
        {
            var source = Path.Combine(context.OutputFolder!, file);
            var destination = Path.Combine(context.MacBundle?.BinaryPath!, file);
            Logger.Trace($"Move file {source} to {destination}");
            File.Move(source, destination, true);
        }

        var iconPath = Path.Combine(GlobalConfiguration.BaseDirectory, "assets", "goals.icns");
        if (!File.Exists(iconPath))
        {
            return context with { Failed = true, Reason = $"Failed to find an icon at path: {iconPath}" };
        }
        Logger.Trace($"Copy icon from: {iconPath}");
        File.Copy(iconPath, Path.Combine(context.MacBundle?.ResourcesPath!, "goals.icns"));

        return await next(context);
    }
}
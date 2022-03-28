using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.BuildTool.Builds;

internal class CopyConfig : IMiddleware<BuildContext>
{
    public async Task<BuildContext> OnInvoke(BuildContext context, ContextDelegate<BuildContext> next)
    {
        if (context.ConfigFile == null)
        {
            Logger.Warning("No config file specified, the application requires a config.json and will not function correctly without one.");
            return await next(context);
        }

        if (!File.Exists(context.ConfigFile))
        {
            return context with { Failed = true, Reason = $"Failed to find config file {context.ConfigFile}" };
        }

        var destinationFolder = context.MacBundle?.BinaryPath ?? context.OutputFolder!;
        var filePath = Path.Combine(destinationFolder, "config.json");
        Logger.Trace($"Copy config from {context.ConfigFile} to {filePath}");
        try
        {
            File.Copy(context.ConfigFile, filePath, true);
        }
        catch (Exception e)
        {
            return context with { Failed = true, Reason = $"Failed to copy config from {context.ConfigFile} to {filePath} with a {e.GetType().Name} - {e.Message}" };
        }

        return await next(context);
    }
}
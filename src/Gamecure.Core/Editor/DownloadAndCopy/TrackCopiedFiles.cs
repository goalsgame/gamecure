using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Editor.DownloadAndCopy;

public class TrackCopiedFiles : IMiddleware<DownloadEditorContext>
{
    public async Task<DownloadEditorContext> OnInvoke(DownloadEditorContext context, ContextDelegate<DownloadEditorContext> next)
    {
        var filepath = Path.Combine(GlobalConfiguration.CacheDirectory, "copiedfiles.txt");
        if (File.Exists(filepath))
        {
            var filesCopied = await File.ReadAllLinesAsync(filepath);
            context = await next(context with { FilesCopied = filesCopied });
        }
        else
        {
            context = await next(context);
        }

        if (context.Failed || context.FilesCopied.Count == 0)
        {
            return context;
        }

        try
        {
            Logger.Trace("Store a list of copied files");
            await File.WriteAllLinesAsync(filepath, context.FilesCopied);
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to write copied files to {filepath}", e);
        }

        return context;
    }
}
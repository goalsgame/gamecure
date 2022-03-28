using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Editor.DownloadAndCopy;

public class StoreEditorVersion : IMiddleware<DownloadEditorContext>
{
    public async Task<DownloadEditorContext> OnInvoke(DownloadEditorContext context, ContextDelegate<DownloadEditorContext> next)
    {
        context = await next(context);
        if (context.Failed)
        {
            return context;
        }
        Logger.Trace($"Caching editor version: {context.Version.Changeset}");
        var path = Path.Combine(GlobalConfiguration.CacheDirectory, "editor.version");
        await File.WriteAllTextAsync(path, context.Version.Changeset.ToString());
        return context;
    }
}
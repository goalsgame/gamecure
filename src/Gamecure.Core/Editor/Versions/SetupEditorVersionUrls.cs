using Gamecure.Core.Common;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Editor.Versions;

public class SetupEditorVersionUrls : IMiddleware<EditorVersionsContext>
{
    public async Task<EditorVersionsContext> OnInvoke(EditorVersionsContext context, ContextDelegate<EditorVersionsContext> next)
    {
        var prefix = GlobalConfiguration.Platform switch
        {
            Platform.Windows => "win64/editor/index",
            Platform.Linux => "linux/editor/index",
            Platform.Macos => "mac/editor/index",
            _ => throw new NotSupportedException($"Editor sync is not supported for platform {GlobalConfiguration.Platform}")
        };

        var url = $"https://storage.googleapis.com/storage/v1/b/{context.Container}/o?prefix={prefix}";
        Logger.Trace($"Editor Indicies URL: {url}");

        return await next(context with
        {
            Prefix = prefix,
            IndexUrl = url
        });
    }
}
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Editor.Versions;

public class ReadCurrentVersion : IMiddleware<EditorVersionsContext>
{
    public async Task<EditorVersionsContext> OnInvoke(EditorVersionsContext context, ContextDelegate<EditorVersionsContext> next)
    {
        var path = Path.Combine(GlobalConfiguration.CacheDirectory, "editor.version");

        int? currentVersion = null;
        if (File.Exists(path))
        {
            var versionContent = await File.ReadAllTextAsync(path);
            if (!string.IsNullOrWhiteSpace(versionContent))
            {
                if (int.TryParse(versionContent, out var version))
                {
                    currentVersion = version;
                }
                else
                {
                    Logger.Warning($"Failed to parse the editor version contents of file {path}. Please remove this file and try again.");
                }
            }
        }
        return await next(context with
        {
            CurrentEditorVersion = currentVersion
        });
    }
}
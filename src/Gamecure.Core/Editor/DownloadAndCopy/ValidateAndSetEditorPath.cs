using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Editor.DownloadAndCopy;

public class ValidateAndSetEditorPath : IMiddleware<DownloadEditorContext>
{
    public async Task<DownloadEditorContext> OnInvoke(DownloadEditorContext context, ContextDelegate<DownloadEditorContext> next)
    {
        var path = GetSourcePath(context.EditorPath);

        if (!Directory.Exists(Path.Combine(path, "Game")))
        {
            return context with { Failed = true, Reason = $"Failed to find the Game directory in {path}." };
        }
        if (!Directory.Exists(Path.Combine(path, "Engine")))
        {
            return context with { Failed = true, Reason = $"Failed to find the Engine directory in {path}." };
        }

        // If there's a Staging directory, just append that to the path.
        static string GetSourcePath(string source) => Directory.Exists(Path.Combine(source, "Staging"))
            ? Path.Combine(source, "Staging")
            : source;

        return await next(context with { EditorPath = path });
    }
}
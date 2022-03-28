using System.Diagnostics;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Editor.DownloadAndCopy;

public class DeletePreviousEditor : IMiddleware<DownloadEditorContext>
{
    public bool ShouldRun(DownloadEditorContext context) => context.DeleteCopiedFiles;

    public async Task<DownloadEditorContext> OnInvoke(DownloadEditorContext context, ContextDelegate<DownloadEditorContext> next)
    {
        if (context.FilesCopied.Count > 0)
        {
            var timer = Stopwatch.StartNew();
            Logger.Trace("Delete previous editor version");
            foreach (var file in context.FilesCopied)
            {
                DeleteFile(Path.Combine(context.Workspace, file));
            }
            timer.Stop();
            Logger.Trace($"Deleted {context.FilesCopied.Count} files in {timer.Elapsed.TotalMilliseconds} ms");
        }
        else
        {
            Logger.Trace("No files to delete");
        }

        return await next(context);

        static void DeleteFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to delete file {path}", e);
            }
        }
    }
}
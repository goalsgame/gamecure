using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Editor.DownloadAndCopy;

public class CopyEditor : IMiddleware<DownloadEditorContext>
{
    public async Task<DownloadEditorContext> OnInvoke(DownloadEditorContext context, ContextDelegate<DownloadEditorContext> next)
    {
        const string LongtailFilePrefix = ".longtail";

        var destination = context.Workspace;
        var source = context.EditorPath;

        List<string> filesCopied = new();
        try
        {
            CreateDirectories(source, destination);
            // Copy all files
            foreach (var filePath in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
            {
                var fileName = Path.GetFileName(filePath);
                if (fileName.StartsWith(LongtailFilePrefix, true, null))
                {
                    Logger.Trace($"Ignoring file {filePath}");
                    continue;
                }
                var relativePath = Path.GetRelativePath(source, filePath);
                var destinationFilePath = Path.Combine(destination, relativePath);
                File.Copy(filePath, destinationFilePath, overwrite: true);
                filesCopied.Add(relativePath);
            }
        }
        catch (Exception e)
        {
            Logger.Error($"{nameof(CopyEditor)} threw and exception.", e);
            return context with { Failed = true, Reason = $"Failed to copy the editor with exception: {e.Message}" };
        }

        return await next(context with
        {
            FilesCopied = filesCopied
        });

        static void CreateDirectories(string source, string destination)
        {
            foreach (var directory in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
            {
                var path = Path.Combine(destination, Path.GetRelativePath(source, directory));
                if (!Directory.Exists(path))
                {
                    Logger.Trace($"Trying to create directory: {path}");
                    Directory.CreateDirectory(path);
                }
            }
        }
    }


}
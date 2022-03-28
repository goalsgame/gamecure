using System.IO.Compression;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.BuildTool.Builds;

internal class ZipBinary : IMiddleware<BuildContext>
{
    public bool ShouldRun(BuildContext context) => context.ZipFile != null;
    public async Task<BuildContext> OnInvoke(BuildContext context, ContextDelegate<BuildContext> next)
    {
        context = await next(context);

        if (context.Failed)
        {
            return context;
        }

        var source = context.MacBundle?.AppPath ?? context.OutputFolder!;

        var parentPath = Directory.GetParent(context.OutputFolder!);
        if (parentPath == null)
        {
            return context with { Failed = true, Reason = $"Failed to get the parent path of {context.OutputFolder}." };
        }

        var destination = Path.Combine(source, Path.Combine(parentPath.FullName, context.ZipFile!));
        Logger.Trace($"Zip source: {source}");
        Logger.Trace($"Zip destination: {destination}");
        try
        {
            if (File.Exists(destination))
            {
                File.Delete(destination);
            }
            ZipFile.CreateFromDirectory(source, destination, CompressionLevel.Fastest, true);
        }
        catch (Exception e)
        {
            return context with { Failed = true, Reason = $"Failed to zip {source} with {e.GetType().Name} - {e.Message}" };
        }
        return context;
    }
}
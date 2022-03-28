using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Longtail;

public class DeleteLongtail : IMiddleware<LongtailContext>
{
    public bool ShouldRun(LongtailContext context) => context.Reset;
    public async Task<LongtailContext> OnInvoke(LongtailContext context, ContextDelegate<LongtailContext> next)
    {
        var longtailFolder = Path.GetDirectoryName(context.LongtailPath);
        Logger.Trace($"Trying to delete longtail folder {longtailFolder}");
        try
        {
            if (longtailFolder != null)
            {
                Directory.Delete(longtailFolder, true);
                Logger.Trace($"Longtail folder was deleted");
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to delete longtail folder {longtailFolder}.", e);
        }
        return await next(context);
    }
}
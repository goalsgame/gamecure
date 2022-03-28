using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Google;

public class DeleteCredentialsCache : IMiddleware<GoogleAuthContext>
{
    public bool ShouldRun(GoogleAuthContext context) => context.Reset;

    public async Task<GoogleAuthContext> OnInvoke(GoogleAuthContext context, ContextDelegate<GoogleAuthContext> next)
    {
        try
        {
            if (File.Exists(context.CacheFilename))
            {
                File.Delete(context.CacheFilename);
            }
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to delete google credential cache file {context.CacheFilename}", e);
        }
        return await next(context);
    }
}
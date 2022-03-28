using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Google;

public class GoogleCredentialsCacheFilename : IMiddleware<GoogleAuthContext>
{
    public async Task<GoogleAuthContext> OnInvoke(GoogleAuthContext context, ContextDelegate<GoogleAuthContext> next)
    {
        var cacheFilename = Path.Combine(GlobalConfiguration.CacheDirectory, "google_cred.json");

        return await next(context with { CacheFilename = cacheFilename });
    }
}
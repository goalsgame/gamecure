using Gamecure.Core.Common;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Google;

public class GoogleCredentialsCache : IMiddleware<GoogleAuthContext>
{
    private record GoogleCacheTokens(string RefreshToken, DateTime Expires);
    public async Task<GoogleAuthContext> OnInvoke(GoogleAuthContext context, ContextDelegate<GoogleAuthContext> next)
    {
        var credentialsFile = Path.Combine(GlobalConfiguration.CacheDirectory, "google_cred.json");
        var refreshToken = await GetRefreshTokenFromCache(credentialsFile);
        context = await next(context with
        {
            RefreshToken = refreshToken
        });

        if (context.Failed)
        {
            return context;
        }

        var expires = DateTime.Now.AddSeconds(context.ExpiresIn);
        Logger.Trace($"Caching google credentials. Expires at {expires}");
        await using var file = File.Open(credentialsFile, FileMode.OpenOrCreate);
        file.SetLength(0);
        await Json.SerializeAsync(file, new GoogleCacheTokens(context.RefreshToken, expires));

        return context;

        static async Task<string> GetRefreshTokenFromCache(string filename)
        {
            if (!File.Exists(filename))
            {
                return string.Empty;
            }

            Logger.Trace("Google Credentials cache found");
            try
            {
                await using var file = File.OpenRead(filename);
                var credentials = await Json.DeserializeAsync<GoogleCacheTokens>(file);
                return credentials?.RefreshToken ?? string.Empty;
            }
            catch (Exception e)
            {
                Logger.Warning($"Corrupt cache file. ({e.GetType().Name} - {e.Message})");
                return string.Empty;
            }
        }
    }
}
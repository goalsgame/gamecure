using Gamecure.Core.Common;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Editor.DownloadAndCopy;

public class WriteTemporaryGoogleCredentials : IMiddleware<DownloadEditorContext>
{
    public async Task<DownloadEditorContext> OnInvoke(DownloadEditorContext context, ContextDelegate<DownloadEditorContext> next)
    {
        var credPath = Path.Combine(GlobalConfiguration.CacheDirectory, $"google_cred_{Guid.NewGuid().ToString()[..15]}.json");
        Logger.Trace($"Writing temporary Google credentials to {credPath}");
        var creds = new GoogleCredentials(context.ClientSecret, context.ClientId, context.RefreshToken, "authorized_user");
        {
            await using var file = File.Open(credPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            file.SetLength(0);
            await Json.SerializeAsync(file, creds);
        }
        try
        {
            return await next(context with { CredentialsPath = credPath });
        }
        finally
        {
            try
            {
                File.Delete(credPath);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to delete cred file {credPath}. Please delete manually.", e);
            }
        }
    }

    // These credentials are used by Longtail in this format.
    private record GoogleCredentials(string ClientSecret, string ClientId, string RefreshToken, string Type);
}
using Gamecure.Core.Common;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Longtail;

public class GetLongtailVersion : IMiddleware<LongtailContext>
{
    private static readonly HttpClient Client = new();
    static GetLongtailVersion()
    {
        Client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "GameCure");
    }

    public async Task<LongtailContext> OnInvoke(LongtailContext context, ContextDelegate<LongtailContext> next)
    {
        var longtailUrl = $"http://api.github.com/repos/DanEngelbrecht/golongtail/releases/tags/{context.LongtailVersion}";

        Logger.Trace($"Trying to get release from {longtailUrl}");
        var releaseStream = await Client.GetStreamAsync(longtailUrl);
        var release = await Json.DeserializeAsync<GitRelease>(releaseStream);
        if (release == null)
        {
            return context with { Failed = true, Reason = $"Failed to deserialize response from {longtailUrl}" };
        }

        var filename = GlobalConfiguration.Platform switch
        {
            Platform.Linux => "linux-x64.zip",
            Platform.Windows => "win32-x64.zip",
            Platform.Macos => "macos-x64.zip",
            _ => throw new ArgumentOutOfRangeException()
        };

        var asset = release.Assets.FirstOrDefault(a => a.Name == filename);
        if (asset == null)
        {
            return context with { Failed = true, Reason = $"Failed to find asset with name {filename}. Available names: {string.Join(", ", release.Assets.Select(a => a.Name))}" };
        }

        return await next(context with { LongtailDownloadUrl = asset.BrowserDownloadUrl });
    }
    private record GitRelease(GitReleaseAsset[] Assets);
    private record GitReleaseAsset(string Name, string BrowserDownloadUrl);
}
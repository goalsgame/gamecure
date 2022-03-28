using System.IO.Compression;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Longtail;

public class DownloadLongtail : IMiddleware<LongtailContext>
{
    private static readonly HttpClient _client = new();
    public async Task<LongtailContext> OnInvoke(LongtailContext context, ContextDelegate<LongtailContext> next)
    {
        if (context.LongtailDownloadUrl == string.Empty)
        {
            return context with { Failed = true, Reason = "Longtail download url is null, can't download." };
        }
        Logger.Trace($"Downloading Longtail version {context.LongtailVersion} from {context.LongtailDownloadUrl}");


        await using var stream = await _client.GetStreamAsync(context.LongtailDownloadUrl);
        using var zipStream = new ZipArchive(stream);
        zipStream.ExtractToDirectory(GlobalConfiguration.DependenciesDirectory, true);

        return await next(context);
    }
}
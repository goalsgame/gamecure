using System.Security.Cryptography;
using Gamecure.Core.Common;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Longtail;

public class ValidateLongtailChecksum : IMiddleware<LongtailContext>
{
    private static readonly Dictionary<Platform, string> _checksums = new()
    {
        // Version v0.2.5, used Convert.ToHexString(SHA256.HashData(File.ReadAllBytes({longtailexecutable})))
        { Platform.Linux, "2C897537F22BA8F79135953172151C6D8D99C1C3C26824EC43FED4CC63720C76" },
        { Platform.Macos, "80E50088A8363BE6B7A30EA808364FE199D8A675C6103381A334B15899670DD5" },
        { Platform.Windows, "01AB2087BC981AAC70E05613487A0120EE1C5F47B80BD4DCA3B083C0A3D1F9B7" }
    };

    public async Task<LongtailContext> OnInvoke(LongtailContext context, ContextDelegate<LongtailContext> next)
    {
        context = await next(context);

        Logger.Info("Validating checksum");
        if (context.Failed)
        {
            return context;
        }

        if (context.LongtailPath == string.Empty)
        {
            return context with { Failed = true, Reason = "LongtailPath is not set, cant' verify checksum" };
        }

        var bytes = await File.ReadAllBytesAsync(context.LongtailPath);
        var checksum = SHA256.HashData(bytes);
        var checksumString = Convert.ToHexString(checksum);

        var expectedChecksum = _checksums[GlobalConfiguration.Platform];
        if (!checksumString.Equals(expectedChecksum))
        {
            Logger.Error($"Checksum mismatch. Expected {expectedChecksum}, got {checksumString}");
            return context with { Failed = true, Reason = $"Checksum mismatch for longtail, please delete the executable at {context.LongtailPath}" };
        }
        Logger.Info("Checksum validation passed");
        return context;
    }
}
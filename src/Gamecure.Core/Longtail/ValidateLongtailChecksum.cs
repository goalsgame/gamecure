using System.Security.Cryptography;
using Gamecure.Core.Common;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Longtail;

public class ValidateLongtailChecksum : IMiddleware<LongtailContext>
{
    private static readonly Dictionary<Platform, string> _checksums = new()
    {
        // Version v0.3.0, used Convert.ToHexString(SHA256.HashData(File.ReadAllBytes({longtailexecutable})))
        { Platform.Linux, "A4DE83D33D3FD15332671016CD4943AB5CD322A32481B4896F298890B0D913E5" },
        { Platform.Macos, "842F19532980F808AD49FC1A265F77C6FBB9566D96A702F456858B844FE578EE" },
        { Platform.Windows, "812BC0F9E25C05E57C2F3961CEDA1027BF442227792700F224E57A3CAF5DA742" }
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
            return context with { Failed = true, Reason = "LongtailPath is not set, can't verify checksum" };
        }

        var bytes = await File.ReadAllBytesAsync(context.LongtailPath);
        var checksum = SHA256.HashData(bytes);
        var checksumString = Convert.ToHexString(checksum);

        var expectedChecksum = _checksums[GlobalConfiguration.Platform];
        if (!checksumString.Equals(expectedChecksum))
        {
            Logger.Error($"Checksum mismatch. Expected {expectedChecksum}, got {checksumString}");
            return context with { Failed = true, Reason = "Checksum mismatch for longtail, go to settings page, check the Reset box under \"Download dependencies\" and press Download and Validate." };
        }
        Logger.Info("Checksum validation passed");
        return context;
    }
}
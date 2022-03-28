using Gamecure.Core.Configuration;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Google;

public record GoogleAuthContext(AppConfig Config) : Context
{
    public string ClientSecret { get; init; } = string.Empty;
    public string CallbackUrl { get; init; } = string.Empty;
    public string AuthCode { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string CacheFilename { get; init; } = string.Empty;
    public bool Reset { get; init; }
}


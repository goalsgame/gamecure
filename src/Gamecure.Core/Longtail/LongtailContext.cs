using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Longtail;

public record LongtailContext(string LongtailVersion) : Context
{
    public string LongtailPath { get; init; } = string.Empty;
    public string LongtailDownloadUrl { get; init; } = string.Empty;
    public bool Reset { get; init; } = false;
}
using System.Text.Json;

namespace Gamecure.Core.Common;

public static class Json
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = new SnakeCaseNamingPolicy()
    };
    private static readonly JsonSerializerOptions OptionsPrettyPrint = new()
    {
        PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
        WriteIndented = true
    };

    // TODO: add more methods for serializing/deserializing when needed.
    public static ValueTask<T?> DeserializeAsync<T>(Stream utf8Stream) => JsonSerializer.DeserializeAsync<T>(utf8Stream, Options);
    public static Task SerializeAsync<T>(Stream stream, in T value, bool writeIndented = false) => JsonSerializer.SerializeAsync(stream, value, writeIndented ? OptionsPrettyPrint : Options);
}
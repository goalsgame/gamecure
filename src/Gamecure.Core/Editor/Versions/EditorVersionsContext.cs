using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Editor.Versions;

public record EditorVersionsContext(string Container, string AccessToken) : Context
{
    public string? IndexUrl { get; init; }
    public string? Prefix { get; init; }
    public EditorVersion[] Versions { get; init; } = Array.Empty<EditorVersion>();
    public int? CurrentEditorVersion { get; init; }
}
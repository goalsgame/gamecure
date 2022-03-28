using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Editor.DownloadAndCopy;

public record DownloadEditorContext(string Workspace, string LongtailPath, string Container, string RefreshToken, string ClientId, string ClientSecret, EditorVersion Version) : Context
{
    public string EditorPath { get; init; } = string.Empty;
    public string CredentialsPath { get; init; } = string.Empty;
    public IReadOnlyCollection<string> FilesCopied { get; init; } = Array.Empty<string>();
    public bool DeleteCopiedFiles { get; init; }
}
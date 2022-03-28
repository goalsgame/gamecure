namespace Gamecure.Core.Common;

public record ProcessResult
{
    public bool Success { get; init; }
    public string Reason { get; init; } = string.Empty;
    public int ExitCode { get; init; }
    public string[] StdErr { get; init; } = Array.Empty<string>();
    public string[] StdOut { get; init; } = Array.Empty<string>();

    public string StdOutAsString() => string.Join(Environment.NewLine, StdOut);
    public string StdErrAsString() => string.Join(Environment.NewLine, StdErr);
}
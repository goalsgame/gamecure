using Gamecure.Core.Configuration;
using Gamecure.Core.Pipeline;

namespace Gamecure.BuildTool.Configuration;

internal record ConfigContext : Context
{
    public bool Overwrite { get; init; }
    public string? OutputFilename { get; init; }
    public string? GoogleClientId { get; init; }
    public string? GoogleProjectName { get; init; }
    public string? GoogleContainer { get; init; }
    public string? GitCommit { get; init; }
    public string? Template { get; init; }
    public string? PlasticRepository { get; init; }
    public AppConfig? AppConfig { get; init; }
}
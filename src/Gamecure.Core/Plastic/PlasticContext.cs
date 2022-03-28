using Gamecure.Core.Configuration;
using Gamecure.Core.Pipeline;
using Gamecure.Core.Plastic.Parser;

namespace Gamecure.Core.Plastic;

public record PlasticContext(PlasticConfig Config) : Context
{
    public string Workspace { get; init; } = string.Empty;
    public PlasticChangeset[] Changesets { get; init; } = Array.Empty<PlasticChangeset>();
    public PlasticStatus? Status { get; init; }
    public string Version { get; init; } = string.Empty;
    public bool RequiresConfiguration { get; init; }
    public string PlasticCLIPath { get; init; } = string.Empty;
}
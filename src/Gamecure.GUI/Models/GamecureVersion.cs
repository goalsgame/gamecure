using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Gamecure.Core.Common;

namespace Gamecure.GUI.Models;

internal class GamecureVersion : IGamecureVersion
{
    private readonly IConfigurationService _configurationService;
    private static readonly HttpClient _client = new();

    public GamecureVersion(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    public async Task<bool> HasNewVersion()
    {
        var message = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/goalsgame/gamecure/commits?sha=main&per_page=1")
        {
            Headers =
            {
                { "Accept", "application/vnd.github.v3+json" },
                { "User-Agent", "goals-gamecure"}
            }
        };

        var appConfig = await _configurationService.GetAppConfig();
        try
        {
            var result = await _client.SendAsync(message);
            if (result.IsSuccessStatusCode)
            {
                var stream = await result.Content.ReadAsStreamAsync();
                var commits = await Json.DeserializeAsync<GitCommit[]>(stream);
                var sha = commits?.FirstOrDefault()?.Sha;
                if (sha == null)
                {
                    return false;
                }
                return sha != appConfig.GitCommit;
            }
        }
        catch
        {
            // ignore
        }

        return false;

    }

    private record GitCommit(string Sha);
}
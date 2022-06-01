using Gamecure.Core.Common;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Configuration;

public record JiraConfig(string Url, int ProjectId, int IssueType, int Priority);
public record PlasticConfig(string Repository, string[] Includes, string[] Excludes);
public record LongtailConfig(string Version);
public record AppConfig(string ClientId, string Project, string Scope, string TokenUrl, string AuthUrl, string GitCommit, string Container, PlasticConfig Plastic, LongtailConfig Longtail, JiraConfig? Jira);

public record AppConfigContext(string ConfigFilename) : Context
{
    public AppConfig? Configuration { get; init; }
}

public class ReadApplicationConfigFile : IMiddleware<AppConfigContext>
{
    public async Task<AppConfigContext> OnInvoke(AppConfigContext context, ContextDelegate<AppConfigContext> next)
    {
        {
            var filePath = Path.Combine(GlobalConfiguration.AppDataDirectory, context.ConfigFilename);
            if (File.Exists(filePath))
            {
                await using var file = File.OpenRead(filePath);
                var config = await Json.DeserializeAsync<AppConfig>(file);
                return await next(context with { Configuration = config });
            }
            Logger.Trace($"Could not find the {context.ConfigFilename} file in {GlobalConfiguration.AppDataDirectory}");
        }

        {
            var filePath = Path.Combine(GlobalConfiguration.BaseDirectory, context.ConfigFilename);
            if (File.Exists(filePath))
            {
                await using var file = File.OpenRead(filePath);
                var config = await Json.DeserializeAsync<AppConfig>(file);
                return await next(context with { Configuration = config });
            }
            Logger.Trace($"Could not find the {context.ConfigFilename} file in {GlobalConfiguration.BaseDirectory}");
        }
        return context with { Failed = true, Reason = $"Failed to find {context.ConfigFilename}." };
    }
}
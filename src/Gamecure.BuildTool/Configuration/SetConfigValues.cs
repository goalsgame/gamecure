using Gamecure.Core.Common.Logging;
using Gamecure.Core.Configuration;
using Gamecure.Core.Pipeline;

namespace Gamecure.BuildTool.Configuration;

internal class SetConfigValues : IMiddleware<ConfigContext>
{
    public async Task<ConfigContext> OnInvoke(ConfigContext context, ContextDelegate<ConfigContext> next)
    {
        var appConfig = context.AppConfig;
        if (appConfig == null)
        {
            return context with { Failed = true, Reason = "The AppConfig has not been initialized." };
        }

        Logger.Trace("Setting configuration values");
        appConfig = appConfig with
        {
            Project = GetValue(appConfig.Project, context.GoogleProjectName),
            ClientId = GetValue(appConfig.ClientId, context.GoogleClientId),
            Container = GetValue(appConfig.Container, context.GoogleContainer),
            Plastic = appConfig.Plastic with
            {
                Repository = GetValue(appConfig.Plastic.Repository, context.PlasticRepository)
            },
            GitCommit = GetValue(appConfig.GitCommit, context.GitCommit),
            Jira = GetJiraConfig(appConfig.Jira)
        };


        return await next(context with { AppConfig = appConfig });


        static string GetValue(string currentValue, string? newValue) => string.IsNullOrWhiteSpace(newValue) ? currentValue : newValue;
        static JiraConfig? GetJiraConfig(JiraConfig? config) => string.IsNullOrEmpty(config?.Url) ? null : config;
    }
}
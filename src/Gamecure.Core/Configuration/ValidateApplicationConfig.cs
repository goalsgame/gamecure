using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Configuration;
public class ValidateApplicationConfig : IMiddleware<AppConfigContext>
{
    public async Task<AppConfigContext> OnInvoke(AppConfigContext context, ContextDelegate<AppConfigContext> next)
    {
        if (context.Configuration == null)
        {
            return context with { Failed = true, Reason = "Configuration has not been set." };
        }

        if (string.IsNullOrWhiteSpace(context.Configuration.AuthUrl))
        {
            return context with { Failed = true, Reason = $"{nameof(AppConfig.AuthUrl)} has not been set." };
        }
        if (string.IsNullOrWhiteSpace(context.Configuration.ClientId))
        {
            return context with { Failed = true, Reason = $"{nameof(AppConfig.ClientId)} has not been set." };
        }
        if (string.IsNullOrWhiteSpace(context.Configuration.Project))
        {
            return context with { Failed = true, Reason = $"{nameof(AppConfig.Project)} has not been set." };
        }
        if (string.IsNullOrWhiteSpace(context.Configuration.Scope))
        {
            return context with { Failed = true, Reason = $"{nameof(AppConfig.Scope)} has not been set." };
        }
        if (string.IsNullOrWhiteSpace(context.Configuration.TokenUrl))
        {
            return context with { Failed = true, Reason = $"{nameof(AppConfig.TokenUrl)} has not been set." };
        }
        if (string.IsNullOrWhiteSpace(context.Configuration.Container))
        {
            return context with { Failed = true, Reason = $"{nameof(AppConfig.Container)} has not been set." };
        }

        if (context.Configuration.Jira is not null && string.IsNullOrWhiteSpace(context.Configuration.Jira.Url))
        {
            return context with { Failed = true, Reason = $"Jira has been configured, but the required field {nameof(AppConfig.Jira.Url)} has not been set." };
        }

        return await next(context);
    }
}
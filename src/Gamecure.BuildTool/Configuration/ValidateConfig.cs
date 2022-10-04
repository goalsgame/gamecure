using Gamecure.Core.Common.Logging;
using Gamecure.Core.Configuration;
using Gamecure.Core.Pipeline;

namespace Gamecure.BuildTool.Configuration;

internal class ValidateConfig : IMiddleware<ConfigContext>
{
    public async Task<ConfigContext> OnInvoke(ConfigContext context, ContextDelegate<ConfigContext> next)
    {
        Logger.Trace("Validating configuration values");
        if (string.IsNullOrWhiteSpace(context.AppConfig?.ClientId))
        {
            return context with { Failed = true, Reason = "Must specify a Google Client ID" };
        }
        if (string.IsNullOrWhiteSpace(context.AppConfig?.Project))
        {
            return context with { Failed = true, Reason = "Must specify a Google project name" };
        }
        if (string.IsNullOrWhiteSpace(context.AppConfig?.Plastic.Repository))
        {
            return context with { Failed = true, Reason = "Must specify a Plastic Repository" };
        }
        if (string.IsNullOrWhiteSpace(context.AppConfig?.TokenUrl))
        {
            return context with { Failed = true, Reason = "Must specify a Google Token URL" };
        }
        if (string.IsNullOrWhiteSpace(context.AppConfig?.AuthUrl))
        {
            return context with { Failed = true, Reason = "Must specify a Google Auth Url" };
        }
        if (string.IsNullOrWhiteSpace(context.AppConfig?.Scope))
        {
            return context with { Failed = true, Reason = "Must specify a Google Auth Scope" };
        }
        if (string.IsNullOrWhiteSpace(context.AppConfig?.Container))
        {
            return context with { Failed = true, Reason = "Must specify a Google Storage Container" };
        }

        if (context.AppConfig.Jira is null)
        {
            Logger.Warning($"{nameof(AppConfig.Jira)} is null, jira integration will be disabled.");
        }

        return await next(context);
    }
}
using Gamecure.Core.Common;
using Gamecure.Core.Configuration;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Google;

public class GoogleAuthSignIn : IMiddleware<GoogleAuthContext>
{
    public async Task<GoogleAuthContext> OnInvoke(GoogleAuthContext context, ContextDelegate<GoogleAuthContext> next)
    {
        if (!IsValid(context.Config))
        {
            return context with { Failed = true, Reason = $"The config has invalid values." };
        }

        var querystring = new Querystring()
            .Param("client_id", context.Config.ClientId)
            .Param("response_type", "code")
            .Param("redirect_uri", context.CallbackUrl)
            .Param("scope", context.Config.Scope);

        var result = await ApplicationLauncher.LaunchBrowser(context.Config.AuthUrl, querystring);
        if (!result)
        {
            return context with { Failed = true, Reason = "Browser was closed immediately." };
        }
        return await next(context);

        static bool IsValid(AppConfig config) =>
            !(string.IsNullOrWhiteSpace(config.AuthUrl)
              || string.IsNullOrWhiteSpace(config.ClientId)
              || string.IsNullOrWhiteSpace(config.Scope)
              || string.IsNullOrWhiteSpace(config.Project)
              || string.IsNullOrWhiteSpace(config.TokenUrl));
    }
}
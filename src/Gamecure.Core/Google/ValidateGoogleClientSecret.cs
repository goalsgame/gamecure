using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Google;

public class ValidateGoogleClientSecret : IMiddleware<GoogleAuthContext>
{
    public async Task<GoogleAuthContext> OnInvoke(GoogleAuthContext context, ContextDelegate<GoogleAuthContext> next)
    {
        if (string.IsNullOrWhiteSpace(context.ClientSecret))
        {
            return context with { Failed = true, Reason = "Google Client Secret has not been set." };
        }
        return await next(context);
    }
}
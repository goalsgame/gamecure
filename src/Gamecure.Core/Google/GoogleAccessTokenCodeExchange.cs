using Gamecure.Core.Common;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Google;

public class GoogleAccessTokenCodeExchange : IMiddleware<GoogleAuthContext>
{
    public async Task<GoogleAuthContext> OnInvoke(GoogleAuthContext context, ContextDelegate<GoogleAuthContext> next)
    {
        // We can't get the access token unless the rest of the pipeline succeeds
        context = await next(context);
        if (context.Failed)
        {
            return context;
        }

        var content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
        {
            new("code", context.AuthCode),
            new("client_id", context.Config.ClientId ),
            new("client_secret",  context.ClientSecret),
            new("redirect_uri", context.CallbackUrl ),
            new("grant_type", "authorization_code"),
            new("access_type", "offline" ),
        });

        using var client = new HttpClient(); // Maybe add static Lazy<HttpClient> ?

        var result = await client.PostAsync(context.Config.TokenUrl, content);
        if (!result.IsSuccessStatusCode)
        {
            return context with { Failed = true, Reason = $"Failed to get AccessToken with message: {result.ReasonPhrase}. Make sure you've set the correct Client Secret." };
        }
        var stream = await result.Content.ReadAsStreamAsync();
        var tokenAuth = await Json.DeserializeAsync<TokenAuthResult>(stream);
        if (tokenAuth == null)
        {
            return context with { Failed = true, Reason = "Failed to deserialize the response." };
        }

        return context with
        {
            AccessToken = tokenAuth.AccessToken,
            RefreshToken = tokenAuth.RefreshToken,
            ExpiresIn = tokenAuth.ExpiresIn,
        };
    }

    private record TokenAuthResult(string AccessToken, string RefreshToken, int ExpiresIn, string Scope, string TokenType);
}
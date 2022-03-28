using Gamecure.Core.Common;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Pipeline;

namespace Gamecure.Core.Google;

public class GoogleAccessTokenFromRefreshToken : IMiddleware<GoogleAuthContext>
{
    public bool ShouldRun(GoogleAuthContext context) => !string.IsNullOrWhiteSpace(context.RefreshToken);

    public async Task<GoogleAuthContext> OnInvoke(GoogleAuthContext context, ContextDelegate<GoogleAuthContext> next)
    {
        var content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
        {
            new("client_id", context.Config.ClientId ),
            new("client_secret",  context.ClientSecret),
            new("grant_type", "refresh_token"),
            new("refresh_token", context.RefreshToken),
        });

        using var client = new HttpClient();

        Logger.Trace("Using RefreshToken to get a new AccessToken");
        var result = await client.PostAsync(context.Config.TokenUrl, content);
        if (result.IsSuccessStatusCode)
        {
            var stream = await result.Content.ReadAsStreamAsync();
            var tokenResult = await Json.DeserializeAsync<TokenAuthResult>(stream);
            if (tokenResult != null && !string.IsNullOrWhiteSpace(tokenResult.AccessToken))
            {
                return context with
                {
                    AccessToken = tokenResult.AccessToken,
                    ExpiresIn = tokenResult.ExpiresIn
                };
            }
            Logger.Warning("Failed to deserialize the response from refresh token.");
        }
        Logger.Trace("RefreshToken failed, going through normal auth flow.");
        return await next(context);
    }

    private record TokenAuthResult(string AccessToken, int ExpiresIn, string Scope, string TokenType);
}
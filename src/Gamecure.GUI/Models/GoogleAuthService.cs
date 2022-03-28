using System.Threading.Tasks;
using Gamecure.Core.Common.Logging;
using Gamecure.Core.Google;
using Gamecure.Core.Pipeline;

namespace Gamecure.GUI.Models;

internal class GoogleAuthService : IGoogleAuthService
{
    private readonly IConfigurationService _configurationService;

    public GoogleAuthService(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }
    public async Task<GoogleAuthContext> Run(string clientSecret, bool reset)
    {
        var config = await _configurationService.GetAppConfig();

        Logger.Trace("Run google auth pipeline");
        var authPipeline = new PipelineBuilder<GoogleAuthContext>()
            .With<ValidateGoogleClientSecret>()
            .With<GoogleCredentialsCacheFilename>()
            .With<DeleteCredentialsCache>()
            .With<GoogleCredentialsCache>()
            .With<GoogleAccessTokenFromRefreshToken>()
            .With<GoogleCallbackUrl>()
            .With<GoogleAccessTokenCodeExchange>()
            .With<GoogleOAuth2Listener>()
            .With<GoogleAuthSignIn>()
            .Build();
        var result = await authPipeline(new GoogleAuthContext(config)
        {
            ClientSecret = clientSecret,
            Reset = reset
        });
        if (result.Failed)
        {
            Logger.Error($"Google Auth pipeline failed with error: {result.Reason}");
        }
        return result;
    }
}
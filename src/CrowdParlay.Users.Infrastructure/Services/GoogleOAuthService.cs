using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Application.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Mapster;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CrowdParlay.Users.Infrastructure.Services;

public class GoogleOAuthService : IGoogleOAuthService
{
    private readonly GoogleOAuthConfiguration _configuration;
    private readonly ILogger<GoogleOAuthService> _logger;

    public GoogleOAuthService(IOptions<GoogleOAuthConfiguration> configuration, ILogger<GoogleOAuthService> logger)
    {
        _configuration = configuration.Value;
        _logger = logger;
    }

    public async Task<string?> GetAccessTokenAsync(string code, IEnumerable<string> scopes, CancellationToken cancellationToken)
    {
        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            DataStore = new NullDataStore(),
            Scopes = scopes,
            ClientSecrets = new ClientSecrets
            {
                ClientId = _configuration.ClientId,
                ClientSecret = _configuration.ClientSecret
            }
        });

        try
        {
            var response = await flow.ExchangeCodeForTokenAsync(null, code, _configuration.AuthorizationFlowRedirectUri, cancellationToken);
            return response.AccessToken;
        }
        catch (TokenResponseException exception)
        {
            _logger.LogError(exception, "Failed to exchange Google OAuth authorization code for access token");
            return null;
        }
    }

    public async Task<GoogleUserInfo> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken)
    {
        var oauthService = new Oauth2Service(new BaseClientService.Initializer
        {
            HttpClientInitializer = GoogleCredential.FromAccessToken(accessToken)
        });

        var googleUserInfo = await oauthService.Userinfo.Get().ExecuteAsync(cancellationToken);
        var userInfo = googleUserInfo.Adapt<GoogleUserInfo>();

        userInfo.IsEmailVerified = googleUserInfo.VerifiedEmail!.Value;
        userInfo.AvatarUrl = googleUserInfo.Picture;

        return userInfo;
    }
}
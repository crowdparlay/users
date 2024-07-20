using System.Text.Json;
using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Application.Models;
using CrowdParlay.Users.Infrastructure;
using Google.Apis.Oauth2.v2.Data;
using Mapster;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CrowdParlay.Users.IntegrationTests.Services;

public class TestGoogleOAuthService : IGoogleOAuthService
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly GoogleOAuthConfiguration _configuration;

    public TestGoogleOAuthService(
        [FromKeyedServices("SnakeCase")] JsonSerializerOptions jsonSerializerOptions, 
        IOptions<GoogleOAuthConfiguration> configuration)
    {
        _jsonSerializerOptions = jsonSerializerOptions;
        _configuration = configuration.Value;
    }

    public Task<string?> GetAccessTokenAsync(string code, IEnumerable<string> scopes, CancellationToken cancellationToken) =>
        Task.FromResult("test.access.token")!;
    
    public string GetAuthorizationFlowUrl(IEnumerable<string> scopes, string state)
    {
        var query = new QueryBuilder
        {
            { "response_type", "code" },
            { "client_id", _configuration.ClientId },
            { "redirect_uri", _configuration.AuthorizationFlowRedirectUri },
            { "scope", string.Join(' ', scopes) },
            { "state", state }
        };
        
        return $"https://accounts.google.com/o/oauth2/v2/auth{query}";
    }

    public Task<GoogleUserInfo> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken)
    {
        const string sampleGoogleOAuthUserInfoResponse =
            """
            {
                "id": "109827451092475249003",
                "email": "test@gmail.com",
                "verified_email": true,
                "name": "Степной ишак",
                "given_name": "Степной",
                "family_name": "ишак",
                "picture": "https://lh3.googleusercontent.com/a/ACg8ocLBFcps9cE7iH7V80eZb4ZC5iLvJ_c1XD7w_8oC_60yR0L90dg=s96-c"
            }
            """;

        var googleUserInfo = JsonSerializer.Deserialize<Userinfo>(sampleGoogleOAuthUserInfoResponse, _jsonSerializerOptions)!;
        var userInfo = googleUserInfo.Adapt<GoogleUserInfo>();

        userInfo.IsEmailVerified = googleUserInfo.VerifiedEmail!.Value;
        userInfo.AvatarUrl = googleUserInfo.Picture;

        return Task.FromResult(userInfo);
    }
}
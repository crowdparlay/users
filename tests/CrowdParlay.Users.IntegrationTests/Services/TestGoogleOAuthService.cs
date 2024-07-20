using System.Text.Json;
using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Application.Models;
using Google.Apis.Oauth2.v2.Data;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdParlay.Users.IntegrationTests.Services;

public class TestGoogleOAuthService : IGoogleOAuthService
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public TestGoogleOAuthService([FromKeyedServices("SnakeCase")] JsonSerializerOptions jsonSerializerOptions) =>
        _jsonSerializerOptions = jsonSerializerOptions;

    public Task<string?> GetAccessTokenAsync(string code, IEnumerable<string> scopes, CancellationToken cancellationToken) =>
        Task.FromResult("test.access.token")!;

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
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.Json.Nodes;
using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Application.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdParlay.Users.IntegrationTests.Services;

public class TestGoogleOidcService : IGoogleOidcService
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public TestGoogleOidcService([FromKeyedServices("SnakeCase")] JsonSerializerOptions jsonSerializerOptions) =>
        _jsonSerializerOptions = jsonSerializerOptions;

    public Task<GoogleUserInfo?> GetUserInfoByIdTokenAsync(JwtSecurityToken idToken)
    {
        var sampleGoogleUserInfoResponse =
            $$"""
            {
              "iss": "https://accounts.google.com",
              "azp": "60607392567-hq4h3jmr4vg1740nonic5netgcmqmjlc.apps.googleusercontent.com",
              "aud": "60607392567-hq4h3jmr4vg1740nonic5netgcmqmjlc.apps.googleusercontent.com",
              "sub": "311855824689297142263",
              "email": "{{idToken.Claims.First(claim => claim.Type == "email").Value}}",
              "email_verified": "true",
              "nbf": "1720720070",
              "name": "Степной ишак",
              "picture": "https://lh3.googleusercontent.com/a/ACg8ocLBFcps9cE7iH7V80eZb4ZC5iLvJ_c1XD7w_8oC_60yR0L90dg=s96-c",
              "given_name": "Степной",
              "family_name": "ишак",
              "iat": "1720720370",
              "exp": "9999999999",
              "jti": "6ac82c4dad5db6fea26d73ab89a6d22f5a7a438d",
              "alg": "RS256",
              "kid": "87bbe0815b064e6d449cac999f0e50e72a3e4374",
              "typ": "JWT"
            }
            """;

        var responseJson = JsonSerializer.Deserialize<JsonObject>(sampleGoogleUserInfoResponse)!;
        var userInfo = responseJson.Deserialize<GoogleUserInfo>(_jsonSerializerOptions)!;

        userInfo.Id = responseJson["sub"]!.GetValue<string>();
        userInfo.IsEmailVerified = bool.Parse(responseJson["email_verified"]!.GetValue<string>());
        userInfo.AvatarUrl = responseJson["picture"]!.GetValue<string>();

        return Task.FromResult(userInfo)!;
    }
}
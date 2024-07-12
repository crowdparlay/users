using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Application.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdParlay.Users.Infrastructure.Services;

public class GoogleOidcService : IGoogleOidcService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public GoogleOidcService(IHttpClientFactory httpClientFactory, [FromKeyedServices("SnakeCase")] JsonSerializerOptions jsonSerializerOptions)
    {
        _httpClientFactory = httpClientFactory;
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    public async Task<GoogleUserInfo?> GetUserInfoByIdTokenAsync(JwtSecurityToken idToken)
    {
        var isGoogleIssuedToken = idToken.Claims.FirstOrDefault(claim => claim.Type == "iss")?.Value == "https://accounts.google.com";
        if (!isGoogleIssuedToken)
            return null;

        var isEmailVerified = idToken.Claims.FirstOrDefault(claim => claim.Type == "email_verified")?.Value == "true";
        if (!isEmailVerified)
            return null;

        using var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={idToken.RawData}");
        if (!response.IsSuccessStatusCode)
            return null;

        var responseJson = await response.Content.ReadFromJsonAsync<JsonObject>();
        var userInfo = responseJson.Deserialize<GoogleUserInfo>(_jsonSerializerOptions)!;

        userInfo.Id = responseJson!["sub"]!.GetValue<string>();
        userInfo.IsEmailVerified = bool.Parse(responseJson!["email_verified"]!.GetValue<string>());
        userInfo.AvatarUrl = responseJson!["picture"]!.GetValue<string>();

        return userInfo;
    }
}
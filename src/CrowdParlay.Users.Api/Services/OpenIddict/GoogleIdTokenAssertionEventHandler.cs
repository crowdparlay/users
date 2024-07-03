using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json.Nodes;
using CrowdParlay.Users.Application.Extensions;
using CrowdParlay.Users.Domain.Abstractions;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;

namespace CrowdParlay.Users.Api.Services.OpenIddict;

public class GoogleIdTokenAssertionEventHandler : IOpenIddictServerHandler<OpenIddictServerEvents.HandleTokenRequestContext>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IUsersRepository _usersRepository;
    private readonly JwtSecurityTokenHandler _jwtHandler = new();

    public GoogleIdTokenAssertionEventHandler(
        IHttpClientFactory httpClientFactory,
        IUsersRepository usersRepository)
    {
        _httpClientFactory = httpClientFactory;
        _usersRepository = usersRepository;
    }

    public async ValueTask HandleAsync(OpenIddictServerEvents.HandleTokenRequestContext context)
    {
        if (context.Request.ClientAssertionType != OpenIddictConstants.ClientAssertionTypes.JwtBearer)
            return;

        var jwt = _jwtHandler.ReadJwtToken(context.Request.Assertion);
        if (jwt.Issuer != "https://accounts.google.com")
            return;

        using var httpClient = _httpClientFactory.CreateClient();
        var tokenInfoResponse = await httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={jwt}");
        if (!tokenInfoResponse.IsSuccessStatusCode)
        {
            context.Reject(
                error: OpenIddictConstants.Errors.InvalidGrant,
                description: "The Google ID token is invalid.");
            return;
        }

        var claims = await tokenInfoResponse.Content.ReadFromJsonAsync<JsonObject>();
        var email = claims!["email"]!.GetValue<string>();
        var user = await _usersRepository.GetByEmailNormalizedAsync(email);
        if (user is null)
        {
            context.Reject(
                error: OpenIddictConstants.Errors.InvalidGrant,
                description: "There is no users associated with this Google identity.");
            return;
        }

        var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        if (context.Request.Scope is not null)
            identity.SetScopes(OpenIddictConstants.Claims.Scope, context.Request.Scope);

        var principal = new ClaimsPrincipal(identity.AddUserClaims(user));
        context.SignIn(principal);
    }
}
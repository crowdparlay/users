using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CrowdParlay.Users.Application.Extensions;
using CrowdParlay.Users.Domain.Abstractions;
using Dodo.Primitives;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server;

namespace CrowdParlay.Users.Api.Services.OpenIddict;

public class RefreshTokenGrantEventHandler : IOpenIddictServerHandler<OpenIddictServerEvents.HandleTokenRequestContext>
{
    private readonly IUsersRepository _usersRepository;
    private readonly JwtSecurityTokenHandler _jwtHandler = new();

    public RefreshTokenGrantEventHandler(IUsersRepository usersRepository) =>
        _usersRepository = usersRepository;

    public async ValueTask HandleAsync(OpenIddictServerEvents.HandleTokenRequestContext context)
    {
        if (!context.Request.IsRefreshTokenGrantType())
            return;

        var jwt = _jwtHandler.ReadJwtToken(context.Request.RefreshToken);
        var userId = Uuid.Parse(jwt.Subject);
        var user = await _usersRepository.GetByIdAsync(userId);
        if (user is null)
        {
            context.Reject(
                error: OpenIddictConstants.Errors.InvalidToken,
                description: "No user with the provided ID found.");
            return;
        }

        var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType);
        if (context.Request.Scope is not null)
            identity.SetScopes(OpenIddictConstants.Claims.Scope, context.Request.Scope);

        var principal = new ClaimsPrincipal(identity.AddUserClaims(user));
        context.SignIn(principal);
    }
}
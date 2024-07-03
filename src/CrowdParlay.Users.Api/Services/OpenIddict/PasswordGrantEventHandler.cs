using System.Security.Claims;
using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Application.Extensions;
using CrowdParlay.Users.Domain.Abstractions;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server;

namespace CrowdParlay.Users.Api.Services.OpenIddict;

public class PasswordGrantEventHandler : IOpenIddictServerHandler<OpenIddictServerEvents.HandleTokenRequestContext>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordService _passwordService;

    public PasswordGrantEventHandler(IUsersRepository usersRepository, IPasswordService passwordService)
    {
        _usersRepository = usersRepository;
        _passwordService = passwordService;
    }

    public async ValueTask HandleAsync(OpenIddictServerEvents.HandleTokenRequestContext context)
    {
        if (!context.Request.IsPasswordGrantType())
            return;

        var user = await _usersRepository.GetByUsernameOrEmailNormalizedAsync(context.Request.Username!);
        if (user is null || !_passwordService.VerifyPassword(user.PasswordHash, context.Request.Password!))
        {
            context.Reject(
                error: OpenIddictConstants.Errors.InvalidGrant,
                description: "The username or password is incorrect.");
            return;
        }

        var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType);
        if (context.Request.Scope is not null)
            identity.SetScopes(OpenIddictConstants.Claims.Scope, context.Request.Scope);

        var principal = new ClaimsPrincipal(identity.AddUserClaims(user));
        context.SignIn(principal);
    }
}
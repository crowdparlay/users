using System.Security.Claims;
using CrowdParlay.Auth.Application.Abstractions.Identity;
using CrowdParlay.Auth.Application.Exceptions;
using CrowdParlay.Auth.Application.Extensions;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using IAuthenticationService = CrowdParlay.Auth.Application.Abstractions.Identity.IAuthenticationService;

namespace CrowdParlay.Auth.Application.Features.Authentication.Commands;

public static class ExchangeRefreshToken
{
    public sealed record Command(string Username, string Scope, HttpContext Context) : IRequest<Response>;

    internal sealed class Validator : AbstractValidator<Command>
    {
        public Validator() => RuleFor(x => x.Username).NotEmpty();
    }

    internal sealed class Handler : IRequestHandler<Command, Response>
    {
        private readonly IUserService _users;
        private readonly IAuthenticationService _authenticationService;

        public Handler(IUserService users, IAuthenticationService authenticationService)
        {
            _users = users;
            _authenticationService = authenticationService;
        }

        public async ValueTask<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var user =
                await _users.FindByUsernameAsync(request.Username)
                ?? throw new NotFoundException("The user does not exist.");

            if (!await _authenticationService.CanSignInAsync(user))
                throw new ForbiddenException("The user is no longer allowed to sign in.");

            var authenticateResult = await request.Context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var identity = new ClaimsIdentity(
                claims: authenticateResult.Principal!.Claims,
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: OpenIddictConstants.Claims.Name,
                roleType: OpenIddictConstants.Claims.Role);

            identity.SetScopes(OpenIddictConstants.Claims.Scope, request.Scope);
            await identity.InjectClaimsAsync(user, _users);

            var ticket = new AuthenticationTicket(
                new ClaimsPrincipal(identity),
                new AuthenticationProperties(),
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            return new Response(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }
    }

    public sealed record Response(ClaimsPrincipal Principal, AuthenticationProperties Properties, string AuthenticationScheme);
}
using System.Security.Claims;
using CrowdParlay.Users.Application.Extensions;
using CrowdParlay.Users.Application.Exceptions;
using CrowdParlay.Users.Domain.Abstractions;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using IAuthenticationService = CrowdParlay.Users.Application.Abstractions.IAuthenticationService;

namespace CrowdParlay.Users.Application.Features.Authentication.Commands;

public static class ExchangeRefreshToken
{
    public sealed record Command(string Username, string Scope, HttpContext Context) : IRequest<Response>;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator() => RuleFor(x => x.Username).NotEmpty();
    }

    public sealed class Handler : IRequestHandler<Command, Response>
    {
        private readonly IUsersRepository _users;
        private readonly IAuthenticationService _authenticationService;

        public Handler(IUsersRepository users, IAuthenticationService authenticationService)
        {
            _users = users;
            _authenticationService = authenticationService;
        }

        public async ValueTask<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var user =
                await _users.GetByUsernameAsync(request.Username)
                ?? throw new NotFoundException("The user does not exist.");

            var authenticateResult = await request.Context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var identity = new ClaimsIdentity(
                claims: authenticateResult.Principal!.Claims,
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: OpenIddictConstants.Claims.Name,
                roleType: OpenIddictConstants.Claims.Role);

            identity.SetScopes(OpenIddictConstants.Claims.Scope, request.Scope);
            identity.InjectClaims(user, _users);

            var ticket = new AuthenticationTicket(
                new ClaimsPrincipal(identity),
                new AuthenticationProperties(),
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            return new Response(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }
    }

    public sealed record Response(ClaimsPrincipal Principal, AuthenticationProperties Properties, string AuthenticationScheme);
}
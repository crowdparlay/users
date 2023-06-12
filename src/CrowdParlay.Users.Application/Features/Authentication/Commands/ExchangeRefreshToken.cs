using System.Security.Claims;
using CrowdParlay.Users.Application.Extensions;
using CrowdParlay.Users.Application.Exceptions;
using CrowdParlay.Users.Domain.Abstractions;
using Dodo.Primitives;
using Mediator;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace CrowdParlay.Users.Application.Features.Authentication.Commands;

public static class ExchangeRefreshToken
{
    public sealed record Command(Uuid UserId, string Scope) : IRequest<Response>;

    public sealed class Handler : IRequestHandler<Command, Response>
    {
        private readonly IUsersRepository _users;

        public Handler(IUsersRepository users) => _users = users;

        public async ValueTask<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var user =
                await _users.GetByIdAsync(request.UserId, cancellationToken)
                ?? throw new NotFoundException("The user does not exist.");

            var identity = new ClaimsIdentity(
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
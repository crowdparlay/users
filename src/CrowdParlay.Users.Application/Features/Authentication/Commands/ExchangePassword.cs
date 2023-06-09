using System.Security.Claims;
using CrowdParlay.Users.Application.Extensions;
using CrowdParlay.Users.Application.Exceptions;
using CrowdParlay.Users.Domain.Abstractions;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using IAuthenticationService = CrowdParlay.Users.Application.Abstractions.IAuthenticationService;

namespace CrowdParlay.Users.Application.Features.Authentication.Commands;

public static class ExchangePassword
{
    public sealed record Command(string Username, string Password, string Scope) : IRequest<Response>;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Username).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
        }
    }

    public sealed class Handler : IRequestHandler<Command, Response>
    {
        private readonly IAuthenticationService _authentication;
        private readonly IUsersRepository _users;

        public Handler(IAuthenticationService authentication, IUsersRepository users)
        {
            _authentication = authentication;
            _users = users;
        }

        public async ValueTask<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var user =
                await _users.GetByUsernameAsync(request.Username)
                ?? throw new NotFoundException();

            if (!_authentication.Authenticate(user, request.Password))
                throw new ForbiddenException("The specified credentials are invalid.");

            var identity = new ClaimsIdentity(
                authenticationType: OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
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
using System.Security.Claims;
using CrowdParlay.Auth.Application.Abstractions.Identity;
using CrowdParlay.Auth.Application.Exceptions;
using CrowdParlay.Auth.Application.Extensions;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using IAuthenticationService = CrowdParlay.Auth.Application.Abstractions.Identity.IAuthenticationService;

namespace CrowdParlay.Auth.Application.Features.Authentication.Commands;

public static class ExchangePassword
{
    public sealed record Command(string Username, string Password, string Scope) : IRequest<Response>;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator(IPasswordValidator<Command> passwordValidator)
        {
            RuleFor(x => x.Username).NotEmpty();
            RuleFor(x => x.Password).Apply(passwordValidator);
        }
    }

    internal sealed class Handler : IRequestHandler<Command, Response>
    {
        private readonly IAuthenticationService _authentication;
        private readonly IUserService _users;

        public Handler(IAuthenticationService authentication, IUserService users)
        {
            _authentication = authentication;
            _users = users;
        }

        public async ValueTask<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var user =
                await _authentication.AuthenticateAsync(request.Username, request.Password)
                ?? throw new ForbiddenException("The specified credentials are invalid.");

            var identity = new ClaimsIdentity(
                authenticationType: OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
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
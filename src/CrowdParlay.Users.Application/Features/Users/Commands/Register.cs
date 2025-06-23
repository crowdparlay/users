using CrowdParlay.Communication;
using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Application.Extensions;
using CrowdParlay.Users.Application.Models;
using CrowdParlay.Users.Domain.Abstractions;
using CrowdParlay.Users.Domain.Entities;
using FluentValidation;
using MassTransit;
using Mediator;
using ValidationException = CrowdParlay.Users.Application.Exceptions.ValidationException;

namespace CrowdParlay.Users.Application.Features.Users.Commands;

public static class Register
{
    public sealed class Command : IRequest<Response>
    {
        public string Username { get; }
        public string Email { get; }
        public string DisplayName { get; }
        public string? Password { get; }
        public string? AvatarUrl { get; }
        public ExternalLoginTicket? ExternalLoginTicket { get; }
        
        public Command(
            string username, string? email, string displayName, string? password, string? avatarUrl,
            ExternalLoginTicket? externalLoginTicket)
        {
            if (email is not null)
                Email = email.Trim();
            else if (externalLoginTicket?.ProviderId == GoogleAuthenticationConstants.ExternalLoginProviderId)
                Email = externalLoginTicket.Identity;
            else
                throw new ArgumentNullException(email);

            Username = username.Trim();
            DisplayName = displayName.Trim();
            Password = password;
            AvatarUrl = avatarUrl;
            ExternalLoginTicket = externalLoginTicket;
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Username).Username();
            RuleFor(x => x.Email).Email();
            RuleFor(x => x.DisplayName).DisplayName();

            When(x => x.ExternalLoginTicket?.ProviderId == GoogleAuthenticationConstants.ExternalLoginProviderId, () =>
                RuleFor(x => x.Email).Equal(x => x.ExternalLoginTicket!.Identity));

            When(x => x.ExternalLoginTicket is null || x.Password is not null, () =>
                RuleFor(x => x.Password).Password());
        }
    }

    public sealed class Handler : IRequestHandler<Command, Response>
    {
        private readonly IUsersRepository _users;
        private readonly IExternalLoginsRepository _externalLoginsRepository;
        private readonly IPublishEndpoint _broker;
        private readonly IPasswordService _passwordService;

        public Handler(
            IUsersRepository users,
            IExternalLoginsRepository externalLoginsRepository,
            IPublishEndpoint broker,
            IPasswordService passwordService)
        {
            _users = users;
            _externalLoginsRepository = externalLoginsRepository;
            _broker = broker;
            _passwordService = passwordService;
        }

        public async ValueTask<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            if (await _users.GetByUsernameNormalizedAsync(request.Username, cancellationToken) is not null)
                throw new ValidationException(nameof(request.Username), "This username is already taken.");

            if (await _users.GetByEmailNormalizedAsync(request.Email, cancellationToken) is not null)
                throw new ValidationException(nameof(request.Email), "This email is already taken.");

            var passwordHash = request.Password is not null
                ? _passwordService.HashPassword(request.Password)
                : null;

            var user = new User
            {
                Id = Guid.CreateVersion7(),
                Username = request.Username,
                Email = request.Email,
                DisplayName = request.DisplayName,
                AvatarUrl = request.AvatarUrl,
                PasswordHash = passwordHash,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _users.AddAsync(user, cancellationToken);

            var @event = new UserCreatedEvent(user.Id.ToString(), user.Username, user.DisplayName, user.AvatarUrl);
            await _broker.Publish(@event, cancellationToken);

            if (request.ExternalLoginTicket is null)
                return new Response(user.Id, user.Username, user.Email, user.DisplayName, user.AvatarUrl);

            var externalLogin = new ExternalLogin
            {
                Id = Guid.CreateVersion7(),
                UserId = user.Id,
                ProviderId = request.ExternalLoginTicket.ProviderId,
                Identity = request.ExternalLoginTicket.Identity
            };

            await _externalLoginsRepository.AddAsync(externalLogin, cancellationToken);
            return new Response(user.Id, user.Username, user.Email, user.DisplayName, user.AvatarUrl);
        }
    }

    public sealed record Response(
        Guid Id,
        string Username,
        string Email,
        string DisplayName,
        string? AvatarUrl);
}
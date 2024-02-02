using CrowdParlay.Communication;
using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Application.Extensions;
using CrowdParlay.Users.Domain.Abstractions;
using CrowdParlay.Users.Domain.Entities;
using Dodo.Primitives;
using FluentValidation;
using MassTransit;
using Mediator;
using ValidationException = CrowdParlay.Users.Application.Exceptions.ValidationException;

namespace CrowdParlay.Users.Application.Features.Users.Commands;

public static class Register
{
    public sealed record Command : IRequest<Response>
    {
        public string Username { get; }
        public string Email { get; }
        public string DisplayName { get; }
        public string Password { get; }
        public string? AvatarUrl { get; }

        public Command(string username, string email, string displayName, string password, string? avatarUrl)
        {
            Username = username;
            Email = email.Trim();
            DisplayName = displayName.Trim();
            Password = password;
            AvatarUrl = avatarUrl;
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Username).Username();
            RuleFor(x => x.Email).Email();
            RuleFor(x => x.DisplayName).DisplayName();
            RuleFor(x => x.Password).Password();
        }
    }

    public sealed class Handler : IRequestHandler<Command, Response>
    {
        private readonly IUsersRepository _users;
        private readonly IPublishEndpoint _broker;
        private readonly IPasswordService _passwordService;

        public Handler(IUsersRepository users, IPublishEndpoint broker, IPasswordService passwordService)
        {
            _users = users;
            _broker = broker;
            _passwordService = passwordService;
        }

        public async ValueTask<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            if (await _users.GetByUsernameNormalizedAsync(request.Username, cancellationToken) is not null)
                throw new ValidationException(nameof(request.Username), "This username is already taken.");

            if (await _users.GetByEmailNormalizedAsync(request.Email, cancellationToken) is not null)
                throw new ValidationException(nameof(request.Email), "This email is already taken.");

            var user = new User
            {
                Id = Uuid.NewTimeBased(),
                Username = request.Username,
                Email = request.Email,
                DisplayName = request.DisplayName,
                AvatarUrl = request.AvatarUrl,
                PasswordHash = _passwordService.HashPassword(request.Password),
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _users.AddAsync(user, cancellationToken);

            var @event = new UserCreatedEvent(user.Id.ToString(), user.Username, user.DisplayName, user.AvatarUrl);
            await _broker.Publish(@event, cancellationToken);

            return new Response(user.Id, user.Username, user.Email, user.DisplayName, user.AvatarUrl);
        }
    }

    public sealed record Response(
        Uuid Id,
        string Username,
        string Email,
        string DisplayName,
        string? AvatarUrl);
}
using CrowdParlay.Communication;
using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Application.Exceptions;
using CrowdParlay.Users.Application.Extensions;
using CrowdParlay.Users.Domain.Abstractions;
using CrowdParlay.Users.Domain.Entities;
using Dodo.Primitives;
using FluentValidation;
using MassTransit;
using Mediator;

namespace CrowdParlay.Users.Application.Features.Users.Commands;

public static class Register
{
    public sealed record Command : IRequest<Response>
    {
        public string Username { get; }
        public string DisplayName { get; }
        public string Email { get; }
        public string Password { get; }
        public string? AvatarUrl { get; }

        public Command(string username, string displayName, string email, string password, string? avatarUrl)
        {
            Username = username;
            DisplayName = displayName.Trim();
            Email = email;
            Password = password;
            AvatarUrl = avatarUrl;
        }
    }

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Username).Username();
            RuleFor(x => x.DisplayName).DisplayName();
            RuleFor(x => x.Password).Password();
            RuleFor(x => x.Email).Email();
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
            if (await _users.GetByUsernameAsync(request.Username, cancellationToken) is not null)
                throw new ValidationException(nameof(request.Username), "This username is already taken.");

            if (await _users.GetByEmailAsync(request.Email, cancellationToken) is not null)
                throw new ValidationException(nameof(request.Email), "This email is already taken.");

            var user = new User
            {
                Id = Uuid.NewTimeBased(),
                Username = request.Username,
                DisplayName = request.DisplayName,
                Email = request.Email,
                AvatarUrl = request.AvatarUrl,
                PasswordHash = _passwordService.HashPassword(request.Password),
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _users.AddAsync(user, cancellationToken);

            var @event = new UserCreatedEvent(user.Id.ToString(), user.Username, user.DisplayName, user.AvatarUrl);
            await _broker.Publish(@event, cancellationToken);

            return new Response(user.Id, user.Username, user.DisplayName, user.Email, user.AvatarUrl);
        }
    }

    public sealed record Response(
        Uuid Id,
        string Username,
        string DisplayName,
        string Email,
        string? AvatarUrl);
}
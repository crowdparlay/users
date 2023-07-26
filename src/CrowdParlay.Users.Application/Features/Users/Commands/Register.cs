using CrowdParlay.Communication;
using CrowdParlay.Communication.Abstractions;
using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Application.Exceptions;
using CrowdParlay.Users.Application.Extensions;
using CrowdParlay.Users.Domain.Abstractions;
using CrowdParlay.Users.Domain.Entities;
using Dodo.Primitives;
using FluentValidation;
using Mediator;

namespace CrowdParlay.Users.Application.Features.Users.Commands;

public static class Register
{
    public sealed record Command : IRequest<Response>
    {
        public string Username { get; }
        public string DisplayName { get; }
        public string Password { get; }
        public string? AvatarUrl { get; }

        public Command(string username, string displayName, string password, string? avatarUrl)
        {
            Username = username;
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
            RuleFor(x => x.DisplayName).DisplayName();
            RuleFor(x => x.Password).Password();
        }
    }

    public sealed class Handler : IRequestHandler<Command, Response>
    {
        private readonly IUsersRepository _users;
        private readonly IMessageBroker _broker;
        private readonly IPasswordService _passwordService;

        public Handler(IUsersRepository users, IMessageBroker broker, IPasswordService passwordService)
        {
            _users = users;
            _broker = broker;
            _passwordService = passwordService;
        }

        public async ValueTask<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var sameExists = await _users.GetByUsernameAsync(request.Username, cancellationToken) is not null;
            if (sameExists)
                throw new AlreadyExistsException("User with the specified username already exists.");

            var user = new User
            {
                Id = Uuid.NewTimeBased(),
                Username = request.Username,
                DisplayName = request.DisplayName,
                AvatarUrl = request.AvatarUrl,
                PasswordHash = _passwordService.HashPassword(request.Password.Trim()),
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _users.AddAsync(user, cancellationToken);

            var @event = new UserCreatedEvent(user.Id.ToString(), user.Username, user.DisplayName, user.AvatarUrl);
            _broker.Users.Publish(@event);

            return new Response(user.Id, user.Username, user.DisplayName, user.AvatarUrl);
        }
    }

    public sealed record Response(
        Uuid Id,
        string Username,
        string DisplayName,
        string? AvatarUrl);
}
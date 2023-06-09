using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Application.Abstractions.Communication;
using CrowdParlay.Users.Application.Exceptions;
using CrowdParlay.Users.Domain.Abstractions;
using CrowdParlay.Users.Domain.Entities;
using Dodo.Primitives;
using FluentValidation;
using Mediator;
using ValidationException = CrowdParlay.Users.Application.Exceptions.ValidationException;

namespace CrowdParlay.Users.Application.Features.Users.Commands;

public static class Register
{
    public sealed record Command(string Username, string DisplayName, string Password, bool IsAuthenticated) : IRequest<Response>;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Username).NotEmpty();
            RuleFor(x => x.DisplayName).NotEmpty();
        }
    }

    public sealed class Handler : IRequestHandler<Command, Response>
    {
        private readonly IUsersRepository _users;
        private readonly IMessageBroker _broker;
        private readonly IPasswordHasher _hasher;

        public Handler(IUsersRepository users, IMessageBroker broker, IPasswordHasher hasher)
        {
            _users = users;
            _broker = broker;
            _hasher = hasher;
        }

        public async ValueTask<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            if (request.IsAuthenticated)
                throw new ForbiddenException();

            var sameExists = await _users.GetByUsernameAsync(request.Username) is not null;
            if (sameExists)
                throw new AlreadyExistsException("User with the specified username already exists.");

            var user = new User
            {
                Id = new Uuid(),
                Username = request.Username,
                DisplayName = request.DisplayName,
                PasswordHash = _hasher.HashPassword(request.Password),
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _users.AddAsync(user);

            var @event = new UserCreatedEvent(user.Id, user.Username, user.DisplayName);
            await _broker.UserCreatedEvent.PublishAsync(@event.UserId.ToString(), @event);

            return new Response(user.Id, user.Username);
        }
    }

    public sealed record Response(Uuid Id, string Username);
}
using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Application.Abstractions.Communication;
using CrowdParlay.Users.Application.Communication;
using CrowdParlay.Users.Application.Exceptions;
using CrowdParlay.Users.Domain.Abstractions;
using CrowdParlay.Users.Domain.Entities;
using Dodo.Primitives;
using FluentValidation;
using Mediator;

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
        private readonly IPasswordService _passwordService;

        public Handler(IUsersRepository users, IMessageBroker broker, IPasswordService passwordService)
        {
            _users = users;
            _broker = broker;
            _passwordService = passwordService;
        }

        public async ValueTask<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            if (request.IsAuthenticated)
                throw new ForbiddenException();

            var sameExists = await _users.GetByUsernameAsync(request.Username, cancellationToken) is not null;
            if (sameExists)
                throw new AlreadyExistsException("User with the specified username already exists.");

            var user = new User
            {
                Id = Uuid.NewTimeBased(),
                Username = request.Username,
                DisplayName = request.DisplayName,
                PasswordHash = _passwordService.HashPassword(request.Password),
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _users.AddAsync(user, cancellationToken);

            var @event = new UserCreatedEvent(user.Id.ToString(), user.Username, user.DisplayName);
            _broker.UserEvents.Publish(@event);

            return new Response(user.Id, user.Username);
        }
    }

    public sealed record Response(Uuid Id, string Username);
}
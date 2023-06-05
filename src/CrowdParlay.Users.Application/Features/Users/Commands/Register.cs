using CrowdParlay.Users.Application.Extensions;
using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Application.Abstractions.Communication;
using CrowdParlay.Users.Application.Exceptions;
using FluentValidation;
using Mediator;

namespace CrowdParlay.Users.Application.Features.Users.Commands;

public static class Register
{
    public sealed record Command(string Username, string DisplayName, string Password, bool IsAuthenticated) : IRequest<Response>;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator(IPasswordValidator<Command> passwordValidator)
        {
            RuleFor(x => x.Username).NotEmpty();
            RuleFor(x => x.DisplayName).NotEmpty();
            RuleFor(x => x.Password).Apply(passwordValidator);
        }
    }

    public sealed class Handler : IRequestHandler<Command, Response>
    {
        private readonly IUserService _users;
        private readonly IMessageBroker _broker;

        public Handler(IUserService users, IMessageBroker broker)
        {
            _users = users;
            _broker = broker;
        }

        public async ValueTask<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            if (request.IsAuthenticated)
                throw new ForbiddenException();
            
            var sameExists = await _users.FindByUsernameAsync(request.Username) is not null;
            if (sameExists)
                throw new AlreadyExistsException("User with the specified username already exists.");

            var errorDescriptions = await _users.CreateAsync(request.Username, request.DisplayName, request.Password);
            if (errorDescriptions is not null)
                throw new Exceptions.ValidationException(nameof(request.Password), errorDescriptions);

            var user =
                await _users.FindByUsernameAsync(request.Username)
                ?? throw new InvalidOperationException();

            var @event = new UserCreatedEvent(user.Id, user.UserName!, user.DisplayName);
            await _broker.UserCreatedEvent.PublishAsync(@event.UserId.ToString(), @event);

            return new Response(user.Id, user.UserName!);
        }
    }

    public sealed record Response(Guid Id, string Username);
}
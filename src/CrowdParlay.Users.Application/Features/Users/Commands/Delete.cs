using CrowdParlay.Communication;
using CrowdParlay.Communication.Abstractions;
using CrowdParlay.Users.Application.Exceptions;
using CrowdParlay.Users.Domain.Abstractions;
using Dodo.Primitives;
using FluentValidation;
using Mediator;

namespace CrowdParlay.Users.Application.Features.Users.Commands;

public static class Delete
{
    public sealed record Command(Uuid Id) : IRequest;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }

    public sealed class Handler : IRequestHandler<Command>
    {
        private readonly IUsersRepository _users;
        private readonly IMessageBroker _broker;

        public Handler(IUsersRepository users, IMessageBroker broker)
        {
            _users = users;
            _broker = broker;
        }
        
        public async ValueTask<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            _ = await _users.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException("User with the specified ID doesn't exist.");

            await _users.DeleteAsync(request.Id, cancellationToken);
            var @event = new UserDeletedEvent(request.Id.ToString());

            _broker.Users.Publish(@event);

            return Unit.Value;
        }
    }
}
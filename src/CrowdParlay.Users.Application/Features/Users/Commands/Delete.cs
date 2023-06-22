using CrowdParlay.Users.Application.Abstractions.Communication;
using CrowdParlay.Users.Application.Exceptions;
using CrowdParlay.Users.Domain.Abstractions;
using Dodo.Primitives;
using FluentValidation;
using Mediator;

namespace CrowdParlay.Users.Application.Features.Users.Commands;

public static class Delete
{
    public sealed record Command(Uuid Id) : IRequest<Unit>;

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
                ?? throw new NotFoundException("User with the specified id don't exists.");

            await _users.DeleteAsync(request.Id, cancellationToken);

            var @event = new UserDeletedEvent(request.Id);

            await _broker.UserDeletedEvent.PublishAsync(@event.UserId.ToString(), @event);

            return Unit.Value;
        }
    }
}
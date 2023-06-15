using CrowdParlay.Users.Application.Abstractions.Communication;
using CrowdParlay.Users.Application.Exceptions;
using CrowdParlay.Users.Domain.Abstractions;
using Dodo.Primitives;
using FluentValidation;
using Mediator;

namespace CrowdParlay.Users.Application.Features.Users.Commands;

public static class Delete
{
    public sealed record Command(Uuid id) : IRequest<Response>;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.id).NotEmpty();
        }
    }

    public sealed class Handler : IRequestHandler<Command, Response>
    {
        private readonly IUsersRepository _users;
        private readonly IMessageBroker _broker;

        public Handler(IUsersRepository users, IMessageBroker broker)
        {
            _users = users;
            _broker = broker;
        }


        public async ValueTask<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _users.GetByIdAsync(request.id, cancellationToken);
            if (user == null)
                throw new NotFoundException("User with the specified id don't exists.");
            
            await _users.DeleteAsync(request.id, cancellationToken);

            var @event = new UserDeletedEvent(request.id);

            await _broker.UserDeletedEvent.PublishAsync(@event.UserId.ToString(), @event);

            return new Response();
        }
    }

    public sealed record Response();
}
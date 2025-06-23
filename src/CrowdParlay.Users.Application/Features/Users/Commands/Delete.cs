using CrowdParlay.Communication;
using CrowdParlay.Users.Domain.Abstractions;
using FluentValidation;
using MassTransit;
using Mediator;

namespace CrowdParlay.Users.Application.Features.Users.Commands;

public static class Delete
{
    public sealed record Command(Guid Id) : IRequest;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator() => RuleFor(x => x.Id).NotEmpty();
    }

    public sealed class Handler : IRequestHandler<Command>
    {
        private readonly IUsersRepository _users;
        private readonly IPublishEndpoint _broker;

        public Handler(IUsersRepository users, IPublishEndpoint broker)
        {
            _users = users;
            _broker = broker;
        }

        public async ValueTask<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            await _users.DeleteAsync(request.Id, cancellationToken);

            var @event = new UserDeletedEvent(request.Id.ToString());
            await _broker.Publish(@event, cancellationToken);

            return Unit.Value;
        }
    }
}
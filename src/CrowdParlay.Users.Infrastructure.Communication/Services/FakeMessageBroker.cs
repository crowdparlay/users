using CrowdParlay.Users.Application.Abstractions.Communication;

namespace CrowdParlay.Users.Infrastructure.Communication.Services;

public class FakeMessageBroker : IMessageBroker
{
    public IMessageDestination<UserCreatedEvent> UserCreatedEvent { get; } = new FakeMessageDestination<UserCreatedEvent>();
    public IMessageDestination<UserUpdatedEvent> UserUpdatedEvent { get; } = new FakeMessageDestination<UserUpdatedEvent>();
    public IMessageDestination<UserDeletedEvent> UserDeletedEvent { get; } = new FakeMessageDestination<UserDeletedEvent>();
}
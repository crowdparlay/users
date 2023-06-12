using Dodo.Primitives;

namespace CrowdParlay.Users.Application.Abstractions.Communication;

public interface IMessageBroker
{
    public IMessageDestination<UserCreatedEvent> UserCreatedEvent { get; }
    public IMessageDestination<UserUpdatedEvent> UserUpdatedEvent { get; }
    public IMessageDestination<UserDeletedEvent> UserDeletedEvent { get; }
}

public record UserCreatedEvent(Uuid UserId, string Username, string DisplayName);
public record UserUpdatedEvent(Uuid UserId, string Username, string DisplayName);
public record UserDeletedEvent(Uuid UserId);
namespace CrowdParlay.Users.Application.Abstractions.Communication;

public interface IMessageBroker
{
    public IMessageDestination<UserCreatedEvent> UserCreatedEvent { get; }
    public IMessageDestination<UserUpdatedEvent> UserUpdatedEvent { get; }
    public IMessageDestination<UserDeletedEvent> UserDeletedEvent { get; }
}

public record UserCreatedEvent(Guid UserId, string Username, string DisplayName);
public record UserUpdatedEvent(Guid UserId, string Username, string DisplayName);
public record UserDeletedEvent(Guid UserId);
namespace CrowdParlay.Users.Application.Communication;

public record UserCreatedEvent(string UserId, string Username, string DisplayName);
public record UserUpdatedEvent(string UserId, string Username, string DisplayName);
public record UserDeletedEvent(string UserId);
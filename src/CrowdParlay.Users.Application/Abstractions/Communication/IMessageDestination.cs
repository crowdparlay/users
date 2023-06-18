namespace CrowdParlay.Users.Application.Abstractions.Communication;

public interface IMessageDestination
{
    public Task PublishAsync(string key, object message);
}
namespace CrowdParlay.Users.Application.Abstractions.Communication;

public interface IMessageDestination<in TMessage>
{
    public Task PublishAsync(string key, TMessage message);
}
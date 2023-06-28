namespace CrowdParlay.Users.Application.Abstractions.Communication;

public interface IMessageDestination
{
    public void Publish(object message);
}
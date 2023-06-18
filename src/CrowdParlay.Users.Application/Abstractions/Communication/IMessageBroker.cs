using CrowdParlay.Users.Domain;

namespace CrowdParlay.Users.Application.Abstractions.Communication;

public interface IMessageBroker
{
    public IMessageDestination UserEvents { get; }
}
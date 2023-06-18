using Avro.Specific;
using CrowdParlay.Users.Application.Abstractions.Communication;

namespace CrowdParlay.Users.Infrastructure.Communication.Services;

public class FakeMessageBroker : IMessageBroker
{
    public IMessageDestination UserEvents { get; } = new FakeMessageDestination();
}
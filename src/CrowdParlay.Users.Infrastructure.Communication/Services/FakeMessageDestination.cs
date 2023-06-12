using CrowdParlay.Users.Application.Abstractions.Communication;

namespace CrowdParlay.Users.Infrastructure.Communication.Services;

public class FakeMessageDestination<TMessage> : IMessageDestination<TMessage>
{
    public Task PublishAsync(string key, TMessage message) => Task.CompletedTask;
}
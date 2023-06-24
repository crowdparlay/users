using CrowdParlay.Users.Application.Abstractions.Communication;

namespace CrowdParlay.Users.Infrastructure.Communication.Services;

public class FakeMessageDestination : IMessageDestination
{
    public Task PublishAsync(string key, object message) => Task.CompletedTask;
}
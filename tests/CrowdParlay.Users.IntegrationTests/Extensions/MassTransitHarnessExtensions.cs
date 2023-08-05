using MassTransit.Internals;
using MassTransit.Testing;

namespace CrowdParlay.Users.IntegrationTests.Extensions;

public static class MassTransitHarnessExtensions
{
    public static async Task<TMessage?> LastOrDefaultAsync<TMessage>(this IPublishedMessageList published) where TMessage : class
    {
        var messages = await published.SelectAsync<TMessage>().ToListAsync();
        return (TMessage?)messages.LastOrDefault()?.MessageObject;
    }
}
using System.Net.Mime;
using System.Text;
using CrowdParlay.Communication;
using CrowdParlay.Communication.RabbitMq;
using CrowdParlay.Users.IntegrationTests.Attribute;
using CrowdParlay.Users.IntegrationTests.Props;
using CrowdParlay.Users.IntegrationTests.Setups;
using FluentAssertions;

namespace CrowdParlay.Users.IntegrationTests.Tests;

public class CommunicationTests
{
    [Theory(Timeout = 5000), Setups(typeof(TestContainersSetup), typeof(ServerSetup))]
    public async Task RegisterUser_ShouldProduce_UserCreatedEvent(HttpClient client, RabbitMqMessageBroker broker)
    {
        // Arrange
        var consumer = new AwaitableConsumer<UserCreatedEvent>();
        broker.Users.Subscribe(consumer);

        // Act
        var response = await client.PostAsync("/api/users/register", new StringContent(
            """
            {
                "username": "undrcrxwn123",
                "displayName": "Степной ишак",
                "password": "qwerty123!"
            }
            """, Encoding.UTF8, MediaTypeNames.Application.Json));
        
        var @event = await consumer.ConsumeOne();

        // Assert
        response.EnsureSuccessStatusCode();

        @event.Should().NotBeNull();
        @event.UserId.Should().NotBeEmpty();
        @event.Username.Should().BeEquivalentTo("undrcrxwn123");
        @event.DisplayName.Should().BeEquivalentTo("Степной ишак");
    }
}
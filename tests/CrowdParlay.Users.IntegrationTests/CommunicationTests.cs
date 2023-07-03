using System.Text;
using CrowdParlay.Communication;
using CrowdParlay.Communication.RabbitMq;
using CrowdParlay.Users.IntegrationTests.Attribute;
using CrowdParlay.Users.IntegrationTests.Props;
using CrowdParlay.Users.IntegrationTests.Setups;
using FluentAssertions;

namespace CrowdParlay.Users.IntegrationTests;

public class CommunicationTests
{
    [Theory, Setups(typeof(TestContainersSetup), typeof(ServerSetup))]
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
            """, Encoding.UTF8, "application/json"));
        
        var @event = await consumer.ConsumeOne();

        // Assert
        response.EnsureSuccessStatusCode();

        @event.Should().NotBeNull();
        @event.UserId.Should().NotBeEmpty();
        @event.Username.Should().BeEquivalentTo("undrcrxwn123");
        @event.DisplayName.Should().BeEquivalentTo("Степной ишак");
    }
}
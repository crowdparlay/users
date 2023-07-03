using System.Text;
using System.Text.Json;
using CrowdParlay.Communication;
using CrowdParlay.Communication.RabbitMq;
using CrowdParlay.Users.IntegrationTests.Attribute;
using CrowdParlay.Users.IntegrationTests.Props;
using CrowdParlay.Users.IntegrationTests.Setups;
using FluentAssertions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CrowdParlay.Users.IntegrationTests;

public class UsersControllerSetup : AutoDataAttribute
{
    public UsersControllerSetup() : base(() => new Fixture()
        .Customize(new TestContainersSetup())
        .Customize(new RabbitMqConsumerSetup())
        .Customize(new TestServerSetup())) { }
}

public class CommunicationTests
{
    [Theory, Setups(typeof(TestContainersSetup), typeof(ServerSetup))]
    public async Task RegisterUser_ShouldProduce_UserCreatedEvent(HttpClient client, RabbitMqMessageBroker broker)
    {
        // Arrange
        channel.ExchangeDeclare(RabbitMqConstants.Exchanges.Users, ExchangeType.Topic);
        var queueName = channel.QueueDeclare().QueueName;
        channel.QueueBind(queueName, RabbitMqConstants.Exchanges.Users, "users.*");
        
        var tcs = new TaskCompletionSource<BasicDeliverEventArgs>();
        consumer.Received += (_, args) =>
        {
            channel.BasicAck(args.DeliveryTag, multiple: false);
            tcs.SetResult(args);
        };
        
        channel.BasicConsume(queueName, autoAck: false, consumer);
        
        // Act
        var response = await client.PostAsync("/api/users/register", new StringContent(
            """
            {
                "username": "undrcrxwn123",
                "displayName": "Степной ишак",
                "password": "qwerty123!"
            }
            """, Encoding.UTF8, "application/json"));
        
        // Assert
        response.EnsureSuccessStatusCode();

        var args = await tcs.Task;

        args.RoutingKey.Should().Be("users.created");
        
        var body = Encoding.UTF8.GetString(args.Body.ToArray());
        var @event = JsonSerializer.Deserialize<UserCreatedEvent>(body)!;

        @event.Should().NotBeNull();
        @event.UserId.Should().NotBeEmpty();
        @event.Username.Should().BeEquivalentTo("undrcrxwn123");
        @event.DisplayName.Should().BeEquivalentTo("Степной ишак");
    }
}
using System.Net;
using System.Net.Http.Json;
using CrowdParlay.Communication;
using CrowdParlay.Communication.RabbitMq;
using CrowdParlay.Users.Application.Features.Users.Commands;
using CrowdParlay.Users.Application.Features.Users.Queries;
using CrowdParlay.Users.IntegrationTests.Attributes;
using CrowdParlay.Users.IntegrationTests.Props;
using FluentAssertions;

namespace CrowdParlay.Users.IntegrationTests.Tests;

public class UsersControllerTests
{
    [Theory(Timeout = 5000), ApiSetup]
    public async Task GetByIdRequest_ShouldReturn_SuccessResponse(HttpClient client, RabbitMqMessageBroker broker)
    {
        // Arrange
        var consumer = new AwaitableConsumer<UserCreatedEvent>();
        broker.Users.Subscribe(consumer);

        var registerRequest = new Register.Command("undrcrxwn123", "Степной ишак", "qwerty123!", "https://example.com/avatar.jpg");
        var registerMessage = await client.PostAsJsonAsync("/api/users/register", registerRequest);
        var registerResponse = await registerMessage.Content.ReadFromJsonAsync<Register.Response>();

        // Act
        var getByIdMessage = await client.GetAsync($"/api/users/{registerResponse!.Id}");
        var userCreatedEvent = await consumer.ConsumeOne();

        // Assert
        getByIdMessage.Should().HaveStatusCode(HttpStatusCode.OK);

        var getByIdResponse = await getByIdMessage.Content.ReadFromJsonAsync<GetById.Response>();
        getByIdResponse.Should().Be(new GetById.Response(
            registerResponse.Id,
            registerRequest.Username,
            registerRequest.DisplayName));

        userCreatedEvent.Should().Be(new UserCreatedEvent(
            registerResponse.Id.ToString(),
            registerResponse.Username,
            registerResponse.DisplayName,
            registerResponse.AvatarUrl));
    }

    [Theory(Timeout = 5000), ApiSetup]
    public async Task UpdateUser_ShouldChange_User(HttpClient client, RabbitMqMessageBroker broker)
    {
        // Arrange
        var consumer = new AwaitableConsumer<UserUpdatedEvent>();
        broker.Users.Subscribe(consumer);

        var registerRequest = new Register.Command("undrcrxwn123", "Степной ишак", "qwerty123!", null);
        var registerMessage = await client.PostAsJsonAsync("/api/users/register", registerRequest);
        var registerResponse = await registerMessage.Content.ReadFromJsonAsync<Register.Response>()!;

        var updateRequest = new Update.Command(
            Id: registerResponse!.Id,
            Username: "akavi",
            DisplayName: "Akavi",
            AvatarUrl: "https://example.com/avatar.jpg",
            OldPassword: null,
            NewPassword: null);

        // Act
        var updateMessage = await client.PutAsJsonAsync($"/api/users/{updateRequest.Id}", updateRequest);
        var userUpdatedEvent = await consumer.ConsumeOne();

        // Assert
        updateMessage.Should().HaveStatusCode(HttpStatusCode.OK);

        var updateResponse = await updateMessage.Content.ReadFromJsonAsync<Update.Response>();
        updateResponse.Should().Be(new Update.Response(
            registerResponse.Id,
            updateRequest.Username!,
            updateRequest.DisplayName!,
            updateRequest.AvatarUrl));

        userUpdatedEvent.Should().Be(new UserUpdatedEvent(
            updateResponse!.Id.ToString(),
            updateResponse.Username,
            updateResponse.DisplayName,
            updateResponse.AvatarUrl));
    }

    [Theory(Timeout = 5000), ApiSetup]
    public async Task UpdatePassword_ShouldChange_OnlyPassword(HttpClient client, RabbitMqMessageBroker broker)
    {
        // Arrange
        var consumer = new AwaitableConsumer<UserUpdatedEvent>();
        broker.Users.Subscribe(consumer);

        var registerRequest = new Register.Command("undrcrxwn123", "Степной ишак", "qwerty123!", "https://example.com/avatar.jpg");
        var registerMessage = await client.PostAsJsonAsync("/api/users/register", registerRequest);
        var registerResponse = await registerMessage.Content.ReadFromJsonAsync<Register.Response>()!;

        var updateRequest = new Update.Command(
            Id: registerResponse!.Id,
            Username: null,
            DisplayName: null,
            AvatarUrl: null,
            OldPassword: registerRequest.Password,
            NewPassword: "someNewPassword!");

        // Act
        var updateMessage = await client.PutAsJsonAsync($"/api/users/{updateRequest.Id}", updateRequest);
        var userUpdatedEvent = await consumer.ConsumeOne();

        // Assert
        updateMessage.Should().HaveStatusCode(HttpStatusCode.OK);

        var updateResponse = await updateMessage.Content.ReadFromJsonAsync<Update.Response>();
        updateResponse.Should().Be(new Update.Response(
            registerResponse.Id,
            registerRequest.Username,
            registerRequest.DisplayName,
            registerRequest.AvatarUrl));

        userUpdatedEvent.Should().Be(new UserUpdatedEvent(
            updateResponse!.Id.ToString(),
            updateResponse.Username,
            updateResponse.DisplayName,
            updateResponse.AvatarUrl));
    }
}

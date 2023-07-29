using System.Net;
using System.Net.Http.Json;
using CrowdParlay.Communication;
using CrowdParlay.Communication.RabbitMq;
using CrowdParlay.Users.Application.Features.Users.Commands;
using CrowdParlay.Users.Application.Features.Users.Queries;
using CrowdParlay.Users.IntegrationTests.Fixtures;
using CrowdParlay.Users.IntegrationTests.Props;
using FluentAssertions;

namespace CrowdParlay.Users.IntegrationTests.Tests;

[Collection("RabbitMqAffective")]
public class UsersControllerTests : IClassFixture<WebApplicationContext>
{
    private readonly IFixture _fixture;

    public UsersControllerTests(WebApplicationContext context) => _fixture = context.Fixture;

    [Fact(Timeout = 5000)]
    public async Task GetByIdRequest_ShouldReturn_SuccessResponse()
    {
        var client = _fixture.Create<HttpClient>();
        var broker = _fixture.Create<RabbitMqMessageBroker>();
        var consumer = new AwaitableConsumer<UserCreatedEvent>();
        broker.Users.Subscribe(consumer);

        var registerRequest = new Register.Command("undrcrxwn", "Степной ишак", "qwerty123!", "https://example.com/avatar.jpg");
        var registerMessage = await client.PostAsJsonAsync("/api/users/register", registerRequest);
        var registerResponse = await registerMessage.Content.ReadFromJsonAsync<Register.Response>();

        var getByIdMessage = await client.GetAsync($"/api/users/{registerResponse!.Id}");
        getByIdMessage.Should().HaveStatusCode(HttpStatusCode.OK);

        var getByIdResponse = await getByIdMessage.Content.ReadFromJsonAsync<GetById.Response>();
        getByIdResponse.Should().Be(new GetById.Response(
            registerResponse.Id,
            registerRequest.Username,
            registerRequest.DisplayName,
            registerRequest.AvatarUrl));

        var userCreatedEvent = await consumer.ConsumeOne();
        userCreatedEvent.Should().Be(new UserCreatedEvent(
            registerResponse.Id.ToString(),
            registerRequest.Username,
            registerRequest.DisplayName,
            registerResponse.AvatarUrl));
    }

    [Fact(Timeout = 5000)]
    public async Task GetByUsernameRequest_ShouldReturn_SuccessResponse()
    {
        var client = _fixture.Create<HttpClient>();
        var broker = _fixture.Create<RabbitMqMessageBroker>();
        var consumer = new AwaitableConsumer<UserCreatedEvent>();
        broker.Users.Subscribe(consumer);

        var registerRequest = new Register.Command("compartmental", "Степной ишак", "qwerty123!", "https://example.com/avatar.jpg");
        var registerMessage = await client.PostAsJsonAsync("/api/users/register", registerRequest);
        var registerResponse = await registerMessage.Content.ReadFromJsonAsync<Register.Response>();

        var getByUsernameMessage = await client.GetAsync($"/api/users/resolve?username={registerResponse!.Username}");
        getByUsernameMessage.Should().HaveStatusCode(HttpStatusCode.OK);

        var getByUsernameResponse = await getByUsernameMessage.Content.ReadFromJsonAsync<GetByUsername.Response>();
        getByUsernameResponse.Should().Be(new GetByUsername.Response(
            registerResponse.Id,
            registerRequest.Username,
            registerRequest.DisplayName,
            registerRequest.AvatarUrl));

        var userCreatedEvent = await consumer.ConsumeOne();
        userCreatedEvent.Should().Be(new UserCreatedEvent(
            registerResponse.Id.ToString(),
            registerRequest.Username,
            registerRequest.DisplayName,
            registerResponse.AvatarUrl));
    }

    [Fact(Timeout = 5000)]
    public async Task UpdateUser_ShouldChange_User()
    {
        var client = _fixture.Create<HttpClient>();
        var broker = _fixture.Create<RabbitMqMessageBroker>();
        var consumer = new AwaitableConsumer<UserUpdatedEvent>();
        broker.Users.Subscribe(consumer);

        var registerRequest = new Register.Command("zanli_0", "Степной ишак", "qwerty123!", null);
        var registerMessage = await client.PostAsJsonAsync("/api/users/register", registerRequest);
        var registerResponse = await registerMessage.Content.ReadFromJsonAsync<Register.Response>()!;

        var updateRequest = new Update.Command(
            Id: registerResponse!.Id,
            Username: "akavi",
            DisplayName: "Akavi",
            AvatarUrl: "https://example.com/avatar.jpg",
            OldPassword: null,
            NewPassword: null);

        var updateMessage = await client.PutAsJsonAsync($"/api/users/{updateRequest.Id}", updateRequest);
        updateMessage.Should().HaveStatusCode(HttpStatusCode.OK);

        var updateResponse = await updateMessage.Content.ReadFromJsonAsync<Update.Response>();
        updateResponse.Should().Be(new Update.Response(
            registerResponse.Id,
            updateRequest.Username!,
            updateRequest.DisplayName!,
            updateRequest.AvatarUrl));

        var userUpdatedEvent = await consumer.ConsumeOne();
        userUpdatedEvent.Should().Be(new UserUpdatedEvent(
            updateResponse!.Id.ToString(),
            updateResponse.Username,
            updateResponse.DisplayName,
            updateResponse.AvatarUrl));
    }

    [Fact(Timeout = 5000)]
    public async Task UpdatePassword_ShouldChange_OnlyPassword()
    {
        var client = _fixture.Create<HttpClient>();
        var broker = _fixture.Create<RabbitMqMessageBroker>();
        var consumer = new AwaitableConsumer<UserUpdatedEvent>();
        broker.Users.Subscribe(consumer);

        var registerRequest = new Register.Command("zen_mode", "Степной ишак", "qwerty123!", "https://example.com/avatar.jpg");
        var registerMessage = await client.PostAsJsonAsync("/api/users/register", registerRequest);
        var registerResponse = await registerMessage.Content.ReadFromJsonAsync<Register.Response>()!;

        var updateRequest = new Update.Command(
            Id: registerResponse!.Id,
            Username: null,
            DisplayName: null,
            AvatarUrl: null,
            OldPassword: registerRequest.Password,
            NewPassword: "someNewPassword!");

        var updateMessage = await client.PutAsJsonAsync($"/api/users/{updateRequest.Id}", updateRequest);
        updateMessage.Should().HaveStatusCode(HttpStatusCode.OK);

        var updateResponse = await updateMessage.Content.ReadFromJsonAsync<Update.Response>();
        updateResponse.Should().Be(new Update.Response(
            registerResponse.Id,
            registerRequest.Username,
            registerRequest.DisplayName,
            registerRequest.AvatarUrl));

        var userUpdatedEvent = await consumer.ConsumeOne();
        userUpdatedEvent.Should().Be(new UserUpdatedEvent(
            updateResponse!.Id.ToString(),
            updateResponse.Username,
            updateResponse.DisplayName,
            updateResponse.AvatarUrl));
    }

    [Fact(Timeout = 5000)]
    public async Task NormalizedUsernameDuplication_ShouldReturn_Conflict()
    {
        var client = _fixture.Create<HttpClient>();
        var registerRequest = new Register.Command("USSERname", "display name", "password3", null);
        var registerRequestDuplicate = new Register.Command("us55e3rn44me3333e", "display name 2", "password123", null);

        await client.PostAsJsonAsync("/api/users/register", registerRequest);
        var duplicateMessage = await client.PostAsJsonAsync("/api/users/register", registerRequestDuplicate);

        duplicateMessage.Should().HaveStatusCode(HttpStatusCode.Conflict);
    }
}
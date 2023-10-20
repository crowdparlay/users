using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CrowdParlay.Communication;
using CrowdParlay.Users.Application.Features.Users.Commands;
using CrowdParlay.Users.Application.Features.Users.Queries;
using CrowdParlay.Users.IntegrationTests.Configurations;
using CrowdParlay.Users.IntegrationTests.Extensions;
using CrowdParlay.Users.IntegrationTests.Fixtures;
using FluentAssertions;
using MassTransit.Testing;

namespace CrowdParlay.Users.IntegrationTests.Tests;

[Collection("CommunicationAffective")]
public class UsersControllerTests : IClassFixture<WebApplicationContext>
{
    private readonly HttpClient _client;
    private readonly ITestHarness _harness;

    public UsersControllerTests(WebApplicationContext context)
    {
        _client = context.Client;
        _harness = context.Harness;
    }

    [Fact(DisplayName = "Register user returns new user and publishes event")]
    public async Task Register_Positive()
    {
        var registerRequest = new Register.Command("undrcrxwnkkkj", "Степной ишак", "qwerty123!", "https://example.com/avatar.jpg");
        var registerMessage = await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest, JsonConfigurations.JsonOptions);
        var registerResponse = await registerMessage.Content.ReadFromJsonAsync<Register.Response>(JsonConfigurations.JsonOptions);

        registerResponse.Should().Be(new Register.Response(
            registerResponse!.Id,
            registerRequest.Username,
            registerRequest.DisplayName,
            registerRequest.AvatarUrl));

        var userCreatedEvent = await _harness.Published.LastOrDefaultAsync<UserCreatedEvent>();
        userCreatedEvent.Should().Be(new UserCreatedEvent(
            registerResponse.Id.ToString(),
            registerRequest.Username,
            registerRequest.DisplayName,
            registerResponse.AvatarUrl));
    }

    [Fact(DisplayName = "Register users with look-alike usernames returns failure", Timeout = 5000)]
    public async Task Register_Negative()
    {
        var registerRequest = new Register.Command("username", "display name 1", "password1", "https://example.com/avatar1.jpg");
        await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest, JsonConfigurations.JsonOptions);

        var registerRequestDuplicate = new Register.Command("us55e3rn44me3333e", "display name 2", "password2", "https://example.com/avatar2.jpg");
        var duplicateMessage = await _client.PostAsJsonAsync("/api/v1/users/register", registerRequestDuplicate, JsonConfigurations.JsonOptions);

        duplicateMessage.Should().HaveStatusCode(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "Get user by ID returns user", Timeout = 5000)]
    public async Task GetById_Positive()
    {
        var registerRequest = new Register.Command("undrcrxwn", "Степной ишак", "qwerty123!", "https://example.com/avatar.jpg");
        var registerMessage = await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest, JsonConfigurations.JsonOptions);
        var registerResponse = await registerMessage.Content.ReadFromJsonAsync<Register.Response>(JsonConfigurations.JsonOptions);

        var getByIdMessage = await _client.GetAsync($"/api/v1/users/{registerResponse!.Id}");
        getByIdMessage.Should().HaveStatusCode(HttpStatusCode.OK);

        var getByIdResponse = await getByIdMessage.Content.ReadFromJsonAsync<GetById.Response>(JsonConfigurations.JsonOptions);
        getByIdResponse.Should().Be(new GetById.Response(
            registerResponse.Id,
            registerRequest.Username,
            registerRequest.DisplayName,
            registerRequest.AvatarUrl));
    }

    [Fact(DisplayName = "Get user by username returns user", Timeout = 5000)]
    public async Task GetByUsername_Positive()
    {
        var registerRequest = new Register.Command("compartmental", "Степной ишак", "qwerty123!", "https://example.com/avatar.jpg");
        var registerMessage = await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest, JsonConfigurations.JsonOptions);
        var registerResponse = await registerMessage.Content.ReadFromJsonAsync<Register.Response>(JsonConfigurations.JsonOptions);

        var getByUsernameMessage = await _client.GetAsync($"/api/v1/users/resolve?username={registerResponse!.Username}");
        getByUsernameMessage.Should().HaveStatusCode(HttpStatusCode.OK);

        var getByUsernameResponse = await getByUsernameMessage.Content.ReadFromJsonAsync<GetByUsername.Response>(JsonConfigurations.JsonOptions);
        getByUsernameResponse.Should().Be(new GetByUsername.Response(
            registerResponse.Id,
            registerRequest.Username,
            registerRequest.DisplayName,
            registerRequest.AvatarUrl));
    }

    [Fact(DisplayName = "Update user changes user and publishes event", Timeout = 5000)]
    public async Task Update_Positive()
    {
        var registerRequest = new Register.Command("zanli_0", "Степной ишак", "qwerty123!", avatarUrl: null);
        var registerMessage = await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest, JsonConfigurations.JsonOptions);
        var registerResponse = await registerMessage.Content.ReadFromJsonAsync<Register.Response>(JsonConfigurations.JsonOptions);

        var exchangeData = new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "username", "zanli_0" },
            { "password", "qwerty123!" },
            { "scope", "offline_access"}
        };
        var exchangeMessage = await _client.PostAsync("/connect/token", new FormUrlEncodedContent(exchangeData));
        var jsonDoc = await JsonDocument.ParseAsync(await exchangeMessage.Content.ReadAsStreamAsync());
        var token = jsonDoc.RootElement.GetProperty("access_token").ToString();
        
        var updateRequest = new Update.Command(
            registerResponse!.Id,
            Username: "akavi",
            DisplayName: "Akavi",
            AvatarUrl: "https://example.com/avatar.jpg",
            OldPassword: null,
            NewPassword: null);
        
        var serializedRequest = JsonSerializer.Serialize(updateRequest, JsonConfigurations.JsonOptions);
        
        var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/users/{updateRequest.Id}")
        {
            Content = new StringContent(serializedRequest, Encoding.UTF8, "application/json"),
            Headers = { { "Authorization", $"Bearer {token}" } }
        };
        
        var updateMessage = await _client.SendAsync(requestMessage);
        updateMessage.Should().HaveStatusCode(HttpStatusCode.OK);

        var updateResponse = await updateMessage.Content.ReadFromJsonAsync<Update.Response>(JsonConfigurations.JsonOptions);
        updateResponse.Should().Be(new Update.Response(
            updateRequest.Id,
            updateRequest.Username!,
            updateRequest.DisplayName!,
            updateRequest.AvatarUrl));

        var userUpdatedEvent = await _harness.Published.LastOrDefaultAsync<UserUpdatedEvent>();
        userUpdatedEvent.Should().Be(new UserUpdatedEvent(
            updateResponse!.Id.ToString(),
            updateResponse.Username,
            updateResponse.DisplayName,
            updateResponse.AvatarUrl));

        var getByIdMessage = await _client.GetAsync($"/api/v1/users/{registerResponse.Id}");
        getByIdMessage.Should().HaveStatusCode(HttpStatusCode.OK);

        var getByIdResponse = await getByIdMessage.Content.ReadFromJsonAsync<GetById.Response>(JsonConfigurations.JsonOptions);
        getByIdResponse.Should().Be(new GetById.Response(
            updateRequest.Id,
            updateRequest.Username!,
            updateRequest.DisplayName!,
            updateRequest.AvatarUrl));
    }

    [Fact(DisplayName = "Update user password changes user's password and publishes event", Timeout = 5000)]
    public async Task UpdatePassword_Positive()
    {
        var registerRequest = new Register.Command("zen_mode", "Степной ишак", "qwerty123!", "https://example.com/avatar.jpg");
        var registerMessage = await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest, JsonConfigurations.JsonOptions);
        var registerResponse = await registerMessage.Content.ReadFromJsonAsync<Register.Response>(JsonConfigurations.JsonOptions);
        
        var exchangeData = new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "username", "zen_mode" },
            { "password", "qwerty123!" },
            { "scope", "offline_access"}
        };
        var exchangeMessage = await _client.PostAsync("/connect/token", new FormUrlEncodedContent(exchangeData));
        var jsonDoc = await JsonDocument.ParseAsync(await exchangeMessage.Content.ReadAsStreamAsync());
        var token = jsonDoc.RootElement.GetProperty("access_token").ToString();
        
        var updateRequest = new Update.Command(
            Id: registerResponse!.Id,
            Username: null,
            DisplayName: null,
            AvatarUrl: null,
            OldPassword: registerRequest.Password,
            NewPassword: "someNewPassword!");

        var serializedRequest = JsonSerializer.Serialize(updateRequest, JsonConfigurations.JsonOptions);
        
        var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/users/{updateRequest.Id}")
        {
            Content = new StringContent(serializedRequest, Encoding.UTF8, "application/json"),
            Headers = { { "Authorization", $"Bearer {token}" } }
        };
        
        var updateMessage = await _client.SendAsync(requestMessage);
        updateMessage.Should().HaveStatusCode(HttpStatusCode.OK);

        var updateResponse = await updateMessage.Content.ReadFromJsonAsync<Update.Response>(JsonConfigurations.JsonOptions);
        updateResponse.Should().Be(new Update.Response(
            registerResponse.Id,
            registerRequest.Username,
            registerRequest.DisplayName,
            registerRequest.AvatarUrl));

        var userUpdatedEvent = await _harness.Published.LastOrDefaultAsync<UserUpdatedEvent>();
        userUpdatedEvent.Should().Be(new UserUpdatedEvent(
            updateResponse!.Id.ToString(),
            updateResponse.Username,
            updateResponse.DisplayName,
            updateResponse.AvatarUrl));
    }
}
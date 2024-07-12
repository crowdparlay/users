using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CrowdParlay.Communication;
using CrowdParlay.Users.Api;
using CrowdParlay.Users.Application.Features.Users.Commands;
using CrowdParlay.Users.Application.Features.Users.Queries;
using CrowdParlay.Users.IntegrationTests.Extensions;
using CrowdParlay.Users.IntegrationTests.Fixtures;
using FluentAssertions;
using MassTransit.Testing;

namespace CrowdParlay.Users.IntegrationTests.Tests;

public class UsersControllerTests : IAssemblyFixture<WebApplicationFixture>
{
    private readonly HttpClient _client;
    private readonly ITestHarness _harness;

    public UsersControllerTests(WebApplicationFixture fixture)
    {
        _client = fixture.WebApplicationFactory.CreateClient();
        _harness = fixture.Services.GetTestHarness();
    }

    [Theory(DisplayName = "Register users")]
    [InlineData("", HttpStatusCode.BadRequest)]
    [InlineData("A", HttpStatusCode.BadRequest)]
    [InlineData("Abc", HttpStatusCode.BadRequest)]
    [InlineData("Abcde", HttpStatusCode.OK)]
    [InlineData("Abcdefg", HttpStatusCode.OK)]
    [InlineData("/", HttpStatusCode.BadRequest)]
    [InlineData("a/bcdef", HttpStatusCode.BadRequest)]
    [InlineData("a.bcdef", HttpStatusCode.BadRequest)]
    [InlineData("______", HttpStatusCode.BadRequest)]
    [InlineData("123456", HttpStatusCode.BadRequest)]
    [InlineData("123___", HttpStatusCode.BadRequest)]
    [InlineData("a_b_c_d_e", HttpStatusCode.OK)]
    [InlineData("xxxxxx", HttpStatusCode.OK)]
    [InlineData("a4a4a4", HttpStatusCode.OK)]
    [InlineData("кириллица", HttpStatusCode.BadRequest)]
    public async Task RegisterUsernames_Positive(string username, HttpStatusCode expectedStatusCode)
    {
        var request = new Register.Command(
            username: username,
            displayName: "Display name",
            email: Guid.NewGuid().ToString("N") + "@example.com",
            password: "qwerty123!",
            avatarUrl: null);

        var response = await _client.PostAsJsonAsync("/api/v1/users/register", request, GlobalSerializerOptions.SnakeCase);
        response.Should().HaveStatusCode(expectedStatusCode);
    }

    [Fact(DisplayName = "Register user returns new user and publishes event")]
    public async Task Register_Positive()
    {
        var registerRequest = new Register.Command("undrcrxwnkkkj", "nieeee@tt.tt", "Степной ишак", "qwerty123!", "https://example.com/avatar.jpg");
        var registerMessage = await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest, GlobalSerializerOptions.SnakeCase);
        var registerResponse = await registerMessage.Content.ReadFromJsonAsync<Register.Response>(GlobalSerializerOptions.SnakeCase);

        registerResponse.Should().Be(new Register.Response(
            registerResponse!.Id,
            registerRequest.Username,
            registerRequest.Email,
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
        var registerRequest = new Register.Command("username", "uraaa@goto.wy", "display name 1", "password1", "https://example.com/avatar1.jpg");
        await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest, GlobalSerializerOptions.SnakeCase);

        var registerRequestDuplicate = new Register.Command("us55e3rn44me3333e", "meily@tup.ye", "display name 2", "password2!", null);
        var duplicateMessage = await _client.PostAsJsonAsync("/api/v1/users/register", registerRequestDuplicate, GlobalSerializerOptions.SnakeCase);

        duplicateMessage.Should().HaveStatusCode(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Register user with invalid username returns validation failures", Timeout = 5000)]
    public async Task RegisterValidation_Negative()
    {
        var registerRequest = new Register.Command(string.Empty, string.Empty, string.Empty, "Password", null);
        var registerMessage = await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest, GlobalSerializerOptions.SnakeCase);
        registerMessage.Should().HaveStatusCode(HttpStatusCode.BadRequest);

        var validationProblem = await registerMessage.Content.ReadFromJsonAsync<ValidationProblem>(GlobalSerializerOptions.SnakeCase);
        validationProblem!.ValidationErrors.Should().ContainKey("username").WhoseValue.Should().HaveCount(3);
        validationProblem.ValidationErrors.Should().ContainKey("display_name").WhoseValue.Should().ContainSingle();
    }

    [Fact(DisplayName = "Get user by ID returns user", Timeout = 5000)]
    public async Task GetById_Positive()
    {
        var registerRequest = new Register.Command("undrcrxwn", "pis@atb.eti", "Степной ишак", "qwerty123!", "https://example.com/avatar.jpg");
        var registerMessage = await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest, GlobalSerializerOptions.SnakeCase);
        var registerResponse = await registerMessage.Content.ReadFromJsonAsync<Register.Response>(GlobalSerializerOptions.SnakeCase);

        var getByIdMessage = await _client.GetAsync($"/api/v1/users/{registerResponse!.Id}");
        getByIdMessage.Should().HaveStatusCode(HttpStatusCode.OK);

        var getByIdResponse = await getByIdMessage.Content.ReadFromJsonAsync<GetById.Response>(GlobalSerializerOptions.SnakeCase);
        getByIdResponse.Should().Be(new GetById.Response(
            registerResponse.Id,
            registerRequest.Username,
            registerRequest.DisplayName,
            registerRequest.AvatarUrl));
    }

    [Fact(DisplayName = "Get user by username returns user", Timeout = 5000)]
    public async Task GetByUsername_Positive()
    {
        var registerRequest = new Register.Command("compartmental", "jaZae@bal.sya", "Степной ишак", "qwerty123!", "https://example.com/avatar.jpg");
        var registerMessage = await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest, GlobalSerializerOptions.SnakeCase);
        var registerResponse = await registerMessage.Content.ReadFromJsonAsync<Register.Response>(GlobalSerializerOptions.SnakeCase);

        var getByUsernameMessage = await _client.GetAsync($"/api/v1/users/resolve?username={registerResponse!.Username}");
        getByUsernameMessage.Should().HaveStatusCode(HttpStatusCode.OK);

        var getByUsernameResponse = await getByUsernameMessage.Content.ReadFromJsonAsync<GetByUsername.Response>(GlobalSerializerOptions.SnakeCase);
        getByUsernameResponse.Should().Be(new GetByUsername.Response(
            registerResponse.Id,
            registerRequest.Username,
            registerRequest.DisplayName,
            registerRequest.AvatarUrl));
    }

    [Fact(DisplayName = "Update user changes user and publishes event")]
    public async Task Update_Positive()
    {
        var registerRequest = new Register.Command("zanli_0", "pesokJ@naja.com", "Степной ишак", "qwerty123!", avatarUrl: null);
        var registerMessage = await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest, GlobalSerializerOptions.SnakeCase);
        var registerResponse = await registerMessage.Content.ReadFromJsonAsync<Register.Response>(GlobalSerializerOptions.SnakeCase);

        var updateRequest = new Update.Command(
            registerResponse!.Id,
            Username: "akavi",
            DisplayName: "Akavi",
            Email: "privetKarlik@terpi.davaj",
            AvatarUrl: "https://example.com/avatar.jpg",
            OldPassword: null,
            NewPassword: null);

        var accessToken = await _client.AcquireAccessToken(registerRequest.Username, registerRequest.Password);
        var serializedRequest = JsonSerializer.Serialize(updateRequest, GlobalSerializerOptions.SnakeCase);
        var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/users/{updateRequest.Id}")
        {
            Content = new StringContent(serializedRequest, Encoding.UTF8, "application/json"),
            Headers = { { "Authorization", $"Bearer {accessToken}" } }
        };

        var updateMessage = await _client.SendAsync(requestMessage);
        updateMessage.Should().HaveStatusCode(HttpStatusCode.OK);

        var updateResponse = await updateMessage.Content.ReadFromJsonAsync<Update.Response>(GlobalSerializerOptions.SnakeCase);
        updateResponse.Should().Be(new Update.Response(
            updateRequest.Id,
            updateRequest.Username!,
            updateRequest.DisplayName!,
            updateRequest.Email!,
            updateRequest.AvatarUrl));

        var userUpdatedEvent = await _harness.Published.LastOrDefaultAsync<UserUpdatedEvent>();
        userUpdatedEvent.Should().Be(new UserUpdatedEvent(
            updateResponse!.Id.ToString(),
            updateResponse.Username,
            updateResponse.DisplayName,
            updateResponse.AvatarUrl));

        var getByIdMessage = await _client.GetAsync($"/api/v1/users/{registerResponse.Id}");
        getByIdMessage.Should().HaveStatusCode(HttpStatusCode.OK);

        var getByIdResponse = await getByIdMessage.Content.ReadFromJsonAsync<GetById.Response>(GlobalSerializerOptions.SnakeCase);
        getByIdResponse.Should().Be(new GetById.Response(
            updateRequest.Id,
            updateRequest.Username!,
            updateRequest.DisplayName!,
            updateRequest.AvatarUrl));
    }

    [Fact(DisplayName = "Update user password changes user's password and publishes event", Timeout = 5000)]
    public async Task UpdatePassword_Positive()
    {
        var registerRequest = new Register.Command("zen_mode", "uzumuka@gmail.com", "Степной ишак", "qwerty123!", "https://example.com/avatar.jpg");
        var registerMessage = await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest, GlobalSerializerOptions.SnakeCase);
        var registerResponse = await registerMessage.Content.ReadFromJsonAsync<Register.Response>(GlobalSerializerOptions.SnakeCase);

        var updateRequest = new Update.Command(
            Id: registerResponse!.Id,
            Username: null,
            DisplayName: null,
            Email: null,
            AvatarUrl: null,
            OldPassword: registerRequest.Password,
            NewPassword: "someNewPassword!");

        var accessToken = await _client.AcquireAccessToken(registerRequest.Username, registerRequest.Password);
        var serializedRequest = JsonSerializer.Serialize(updateRequest, GlobalSerializerOptions.SnakeCase);
        var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/users/{updateRequest.Id}")
        {
            Content = new StringContent(serializedRequest, Encoding.UTF8, "application/json"),
            Headers = { { "Authorization", $"Bearer {accessToken}" } }
        };

        var updateMessage = await _client.SendAsync(requestMessage);
        updateMessage.Should().HaveStatusCode(HttpStatusCode.OK);

        var updateResponse = await updateMessage.Content.ReadFromJsonAsync<Update.Response>(GlobalSerializerOptions.SnakeCase);
        updateResponse.Should().Be(new Update.Response(
            registerResponse.Id,
            registerRequest.Username,
            registerRequest.DisplayName,
            registerRequest.Email,
            registerRequest.AvatarUrl));

        var userUpdatedEvent = await _harness.Published.LastOrDefaultAsync<UserUpdatedEvent>();
        userUpdatedEvent.Should().Be(new UserUpdatedEvent(
            updateResponse!.Id.ToString(),
            updateResponse.Username,
            updateResponse.DisplayName,
            updateResponse.AvatarUrl));
    }
}
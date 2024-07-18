using System.Net;
using System.Net.Http.Json;
using CrowdParlay.Users.Application.Features.Users.Commands;
using CrowdParlay.Users.IntegrationTests.Extensions;
using CrowdParlay.Users.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;

namespace CrowdParlay.Users.IntegrationTests.Tests;

public class AuthenticationTests : IAssemblyFixture<WebApplicationFixture>
{
    private readonly CookieContainer _cookies;
    private readonly HttpClient _client;

    public AuthenticationTests(WebApplicationFixture fixture)
    {
        _cookies = new CookieContainer();
        _client = fixture.WebApplicationFactory.CreateDefaultClient(new CookieContainerHandler(_cookies));
    }

    [Fact(DisplayName = "Authenticate with Cookies")]
    public async Task Authenticate_Cookie_Positive()
    {
        var registerRequest = new Register.Command("krowlia", "aylbaylb43@aylb.cdf", "Display name", "qwerty123!", "https://example.com/avatar.jpg");
        var registerResponseMessage = await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest, GlobalSerializerOptions.SnakeCase);
        var registerResponse = await registerResponseMessage.Content.ReadFromJsonAsync<Register.Response>();

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["usernameOrEmail"] = registerRequest.Username,
            ["password"] = registerRequest.Password
        });

        var beforeSignInResponse = await _client.DeleteAsync($"/api/v1/users/{registerResponse!.Id}");
        beforeSignInResponse.Should().HaveStatusCode(HttpStatusCode.Unauthorized);

        var signInResponse = await _client.PostAsync("/api/v1/authentication/sign-in", content);
        signInResponse.Should().BeSuccessful();

        var afterSignInResponse = await _client.DeleteAsync($"/api/v1/users/{registerResponse.Id}");
        afterSignInResponse.Should().BeSuccessful();

        var signOutResponse = await _client.PostAsync("/api/v1/authentication/sign-out", content: null);
        signOutResponse.Should().BeSuccessful();

        var afterSignOutResponse = await _client.DeleteAsync($"/api/v1/users/{registerResponse.Id}");
        afterSignOutResponse.Should().HaveStatusCode(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "Authenticate with email and password returns access token")]
    public async Task Authenticate_EmailAndPassword_Positive()
    {
        var registerRequest = new Register.Command("krowlia", "aylbaylb43@aylb.cdf", "Display name", "qwerty123!", "https://example.com/avatar.jpg");
        await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest, GlobalSerializerOptions.SnakeCase);

        var acquireAccessToken = async () => await _client.AcquireAccessToken(registerRequest.Email, registerRequest.Password);
        await acquireAccessToken.Should().NotThrowAsync();
    }

    [Fact(DisplayName = "Authenticate with Google ID returns Cookie")]
    public async Task Authenticate_Google_Positive()
    {
        var registerRequest = new Register.Command("hlgfasdl", "test@gmail.com", "Daniel", "qwerty123!", "https://example.com/avatar.jpg");
        await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest, GlobalSerializerOptions.SnakeCase);

        var originUri = new Uri(_client.BaseAddress!, "originally-requested-resource");
        var query = new QueryBuilder
        {
            { "state", originUri.ToString() },
            { "code", "4/0AcvDMrC5NNjWpWyo_nloFLPLhMjdt1_JgRlz_6B6fag93Ls3kb_e2qGHMoRC6739PDOv4g" },
            { "scope", "email profile" }
        };

        var signInResponse = await _client.GetAsync($"/api/v1/authentication/sign-in-google-callback{query}");
        signInResponse.Should().BeRedirection();
        signInResponse.Headers.Location.Should().Be(originUri);
        _cookies.GetAllCookies().Should().Contain(cookie => cookie.Name == ".CrowdParlay.Authentication");

        var signOutResponse = await _client.PostAsync("/api/v1/authentication/sign-out", content: null);
        signOutResponse.Should().BeSuccessful();
    }
}
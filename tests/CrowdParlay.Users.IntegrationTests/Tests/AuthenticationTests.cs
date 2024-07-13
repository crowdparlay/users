using System.Net;
using System.Net.Http.Json;
using CrowdParlay.Users.Application.Features.Users.Commands;
using CrowdParlay.Users.IntegrationTests.Extensions;
using CrowdParlay.Users.IntegrationTests.Fixtures;
using FluentAssertions;
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
        const string googleIdToken =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJodHRwczovL2FjY291bnRzLmdvb2dsZS5jb20iLCJhenAiOiI2MDYwNzM5MjU2Ny1ocTRoM2ptcjR2ZzE3NDBub" +
            "25pYzVuZXRnY21xbWpsYy5hcHBzLmdvb2dsZXVzZXJjb250ZW50LmNvbSIsImF1ZCI6IjYwNjA3MzkyNTY3LWhxNGgzam1yNHZnMTc0MG5vbmljNW5ldGdjbXFtamxjLmFwcHM" +
            "uZ29vZ2xldXNlcmNvbnRlbnQuY29tIiwic3ViIjoiMzExODU1ODI0Njg5Mjk3MTQyMjYzIiwiZW1haWwiOiJ0ZXN0QGdtYWlsLmNvbSIsImVtYWlsX3ZlcmlmaWVkIjoidHJ1Z" +
            "SIsIm5iZiI6IjE3MjA3MjAwNzAiLCJuYW1lIjoi0KHRgtC10L_QvdC-0Lkg0LjRiNCw0LoiLCJwaWN0dXJlIjoiaHR0cHM6Ly9saDMuZ29vZ2xldXNlcmNvbnRlbnQuY29tL2E" +
            "vQUNnOG9jTEJGY3BzOWNFN2lIN1Y4MGVaYjRaQzVpTHZKX2MxWEQ3d184b0NfNjB5UjBMOTBkZz1zOTYtYyIsImdpdmVuX25hbWUiOiLQodGC0LXQv9C90L7QuSIsImZhbWlse" +
            "V9uYW1lIjoi0LjRiNCw0LoiLCJpYXQiOiIxNzIwNzIwMzcwIiwiZXhwIjoiOTk5OTk5OTk5OSIsImp0aSI6IjZhYzgyYzRkYWQ1ZGI2ZmVhMjZkNzNhYjg5YTZkMjJmNWE3YTQ" +
            "zOGQiLCJhbGciOiJSUzI1NiIsImtpZCI6Ijg3YmJlMDgxNWIwNjRlNmQ0NDljYWM5OTlmMGU1MGU3MmEzZTQzNzQiLCJ0eXAiOiJKV1QifQ.JiD-ySKsD-ZdMK2YEjH8l2Wq05" +
            "aPyLKalyE27iMIVsM";

        var registerRequest = new Register.Command("hlgfasdl", "test@gmail.com", "Daniel", "qwerty123!", "https://example.com/avatar.jpg");
        await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest, GlobalSerializerOptions.SnakeCase);

        var originUri = new Uri(_client.BaseAddress!, "originally-requested-resource");
        var signInRequest = new Dictionary<string, string>
        {
            ["clientId"] = "60607392567-hq4h3jmr4vg1740nonic5netgcmqmjlc.apps.googleusercontent.com",
            ["client_id"] = "60607392567-hq4h3jmr4vg1740nonic5netgcmqmjlc.apps.googleusercontent.com",
            ["credential"] = googleIdToken,
            ["state"] = originUri.ToString(),
            ["select_by"] = "btn",
            ["g_csrf_token"] = "7d9f9222f033d69d"
        };

        var signInResponse = await _client.PostAsync("/api/v1/authentication/sign-in-google-callback", new FormUrlEncodedContent(signInRequest));
        signInResponse.Should().BeRedirection();
        signInResponse.Headers.Location.Should().Be(originUri);
        _cookies.GetAllCookies().Should().Contain(cookie => cookie.Name == ".CrowdParlay.Authentication");

        var signOutResponse = await _client.PostAsync("/api/v1/authentication/sign-out", content: null);
        signOutResponse.Should().BeSuccessful();
    }
}
using System.Net.Http.Json;
using CrowdParlay.Users.Application.Features.Users.Commands;
using CrowdParlay.Users.IntegrationTests.Extensions;
using CrowdParlay.Users.IntegrationTests.Fixtures;
using FluentAssertions;

namespace CrowdParlay.Users.IntegrationTests.Tests;

public class AuthenticationControllerTests : IClassFixture<WebApplicationContext>
{
    private readonly HttpClient _client;

    public AuthenticationControllerTests(WebApplicationContext context) =>
        _client = context.Client;

    [Fact(DisplayName = "Exchange password with email returns token")]
    public async Task Exchange_Password_Positive()
    {
        var registerRequest = new Register.Command("krowlia", "aylbaylbaylb", "dadada@tt.tt", "qwerty123!", "https://example.com/avatar.jpg");
        await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest, GlobalSerializerOptions.SnakeCase);
    
        var accessToken = await _client.AcquireAccessToken(registerRequest.Email, registerRequest.Password);
        accessToken.Should().NotBeNull();
    }
}
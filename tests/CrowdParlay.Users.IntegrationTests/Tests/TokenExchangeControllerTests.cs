using System.Net.Http.Json;
using CrowdParlay.Users.Application.Features.Users.Commands;
using CrowdParlay.Users.IntegrationTests.Extensions;
using CrowdParlay.Users.IntegrationTests.Fixtures;
using FluentAssertions;

namespace CrowdParlay.Users.IntegrationTests.Tests;

public class TokenExchangeControllerTests : IAssemblyFixture<WebApplicationFixture>
{
    private readonly HttpClient _client;

    public TokenExchangeControllerTests(WebApplicationFixture fixture) =>
        _client = fixture.Client;

    [Fact(DisplayName = "Exchange password with email returns token")]
    public async Task Exchange_Password_Positive()
    {
        var registerRequest = new Register.Command("krowlia", "aylbaylb43@aylb.cdf", "Display name", "qwerty123!", "https://example.com/avatar.jpg");
        await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest, GlobalSerializerOptions.SnakeCase);

        var acquireAccessToken = async () => await _client.AcquireAccessToken(registerRequest.Email, registerRequest.Password);
        await acquireAccessToken.Should().NotThrowAsync();
    }
}
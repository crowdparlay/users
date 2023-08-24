using System.ComponentModel;
using System.Net;
using CrowdParlay.Users.IntegrationTests.Fixtures;
using FluentAssertions;

namespace CrowdParlay.Users.IntegrationTests.Tests;

public class HealthChecksTests : IClassFixture<WebApplicationContext>
{
    private readonly HttpClient _client;

    public HealthChecksTests(WebApplicationContext context) => _client = context.Client;

    [Fact(DisplayName = "Get health returns healthy", Timeout = 5000)]
    public async Task GetHealth_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");
        response.Should().HaveStatusCode(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().BeEquivalentTo("healthy");
    }
}
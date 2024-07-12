using System.Net;
using CrowdParlay.Users.IntegrationTests.Fixtures;
using FluentAssertions;

namespace CrowdParlay.Users.IntegrationTests.Tests;

public class HealthChecksTests : IAssemblyFixture<WebApplicationFixture>
{
    private readonly HttpClient _client;

    public HealthChecksTests(WebApplicationFixture fixture) => _client = fixture.WebApplicationFactory.CreateClient();

    [Fact(DisplayName = "Get health returns healthy", Timeout = 5000)]
    public async Task GetHealth_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/healthz");
        response.Should().HaveStatusCode(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().BeEquivalentTo("healthy");
    }
}
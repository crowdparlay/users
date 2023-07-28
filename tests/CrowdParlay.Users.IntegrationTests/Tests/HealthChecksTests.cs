using System.Net;
using CrowdParlay.Users.IntegrationTests.Fixtures;
using FluentAssertions;

namespace CrowdParlay.Users.IntegrationTests.Tests;

public class HealthChecksTests : IClassFixture<WebApplicationContext>
{
    private readonly IFixture _fixture;

    public HealthChecksTests(WebApplicationContext context) => _fixture = context.Fixture;

    [Fact(Timeout = 5000)]
    public async Task HealthCheck_DatabaseAvailable_ShouldBeHealthy()
    {
        // Arrange
        var client = _fixture.Create<HttpClient>();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        response.Should().HaveStatusCode(HttpStatusCode.OK);
        content.Should().BeEquivalentTo("healthy");
    }
}
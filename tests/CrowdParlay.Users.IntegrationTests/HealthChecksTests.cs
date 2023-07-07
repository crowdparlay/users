using CrowdParlay.Users.IntegrationTests.Attribute;
using CrowdParlay.Users.IntegrationTests.Setups;
using FluentAssertions;

namespace CrowdParlay.Users.IntegrationTests;

public class HealthChecksTests
{
    [Theory(Timeout = 5000), Setups(typeof(TestContainersSetup), typeof(ServerSetup))]
    public async Task HealthCheck_DatabaseAvailable_ShouldBeHealthy(HttpClient client)
    {
        // Act
        var response = await client.GetAsync("/health");
        
        // Assert
        var content = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();
        content.Should().Contain("Healthy");
    }
}
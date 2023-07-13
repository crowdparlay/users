using System.Net;
using CrowdParlay.Users.IntegrationTests.Attributes;
using FluentAssertions;
using Testcontainers.PostgreSql;

namespace CrowdParlay.Users.IntegrationTests.Tests;

public class HealthChecksTests
{
    [Theory(Timeout = 5000), ApiSetup]
    public async Task HealthCheck_DatabaseAvailable_ShouldBeHealthy(HttpClient client)
    {
        // Act
        var response = await client.GetAsync("/health");
        
        // Assert
        var content = await response.Content.ReadAsStringAsync();
        response.Should().HaveStatusCode(HttpStatusCode.OK);
        content.Should().BeEquivalentTo("healthy");
    }
    
    [Theory(Timeout = 5000), ApiSetup]
    public async Task HealthCheck_DatabaseNotAvailable_ShouldBeUnhealthy(HttpClient client, PostgreSqlContainer postgres)
    {
        // Arrange
        await postgres.StopAsync();
        
        // Act
        var response = await client.GetAsync("/health");
        
        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.ServiceUnavailable);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().BeEquivalentTo("unhealthy");
    }
}
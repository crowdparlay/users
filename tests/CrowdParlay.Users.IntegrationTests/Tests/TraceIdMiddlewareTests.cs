using System.Text;
using CrowdParlay.Users.Application.Features.Users.Commands;
using CrowdParlay.Users.IntegrationTests.Attributes;
using FluentAssertions;
using Newtonsoft.Json;

namespace CrowdParlay.Users.IntegrationTests.Tests;

public class TraceIdMiddlewareTests
{
    [Theory(Timeout = 5000), ApiSetup]
    public async Task ResponseCheck_CommonResponse_ShouldContainTraceId(HttpClient client)
    {
        // Arrange
        var command = new Register.Command("username", "display name", "password", false);
        var jsonContent = new StringContent(JsonConvert.SerializeObject(command), Encoding.UTF8, "application/json");
        
        // Act
        var response = await client.PostAsync("/api/users/register", jsonContent);
        
        // Assert
        response.Headers.Should().Contain(header => header.Key == "X-TraceId");
    }
}
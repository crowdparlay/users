using System.Net.Http.Json;
using System.Text;
using CrowdParlay.Users.Application.Features.Users.Commands;
using CrowdParlay.Users.IntegrationTests.Attributes;
using FluentAssertions;
using Newtonsoft.Json;

namespace CrowdParlay.Users.IntegrationTests.Tests;

public class TraceIdMiddlewareTests
{
    [Theory(Timeout = 5000), ApiSetup]
    public async Task SuccessResponse_ShouldContain_TraceIdHeader(HttpClient client)
    {
        // Arrange
        var command = new Register.Command("username", "display name", "password", false);

        // Act
        var response = await client.PostAsJsonAsync("/api/users/register", command);
        
        // Assert
        response.Headers.Should().Contain(header => header.Key == "X-TraceId");
    }
}
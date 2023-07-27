using System.Net.Http.Json;
using CrowdParlay.Users.Application.Features.Users.Commands;
using CrowdParlay.Users.IntegrationTests.Attributes;
using FluentAssertions;

namespace CrowdParlay.Users.IntegrationTests.Tests;

[Collection("RabbitMqAffective")]
public class UsernameNormalizedTests
{
    [Theory(Timeout = 5000), ApiSetup]
    public async Task SameNormalizedUsernames_ShouldReturn_Exception(HttpClient client)
    {
        // Arrange
        var command = new Register.Command("username", "display name", "password3", null);
        var badCommand = new Register.Command("us55e3rn44me3333e", "display name 2", "password123", null);

        // Act
        await client.PostAsJsonAsync("/api/users/register", command);
        var response = await client.PostAsJsonAsync("/api/users/register", badCommand);
        
        // Assert
        response.Should().HaveError();
    }
}
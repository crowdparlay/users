using System.Net.Http.Json;
using CrowdParlay.Users.Application.Features.Users.Commands;
using CrowdParlay.Users.IntegrationTests.Attributes;
using FluentAssertions;

namespace CrowdParlay.Users.IntegrationTests.Tests;

public class TraceIdMiddlewareTests
{
    private const string TraceIdHeaderName = "X-TraceId";

    [Theory(Timeout = 5000), ApiSetup]
    public async Task SuccessResponse_ShouldContain_TraceIdHeader(HttpClient client)
    {
        // Arrange
        var command = new Register.Command("username", "display name", "password");

        // Act
        var response = await client.PostAsJsonAsync("/api/users/register", command);
        
        // Assert
        response.Headers.Should().Contain(header => header.Key == TraceIdHeaderName);
    }

    [Theory(Timeout = 5000), ApiSetup]
    public async Task FailureResponse_ShouldContain_TraceIdHeader(HttpClient client)
    {
        // Arrange
        var command = new Register.Command(string.Empty, string.Empty, string.Empty);

        // Act
        var response = await client.PostAsJsonAsync("/api/users/register", command);
        
        // Assert
        response.Headers.Should().Contain(header => header.Key == TraceIdHeaderName);
        response.Should().HaveClientError();
    }

    [Theory(Timeout = 5000), ApiSetup]
    public async Task MultipleResponses_ShouldContain_UniqueTraceIdHeaders(HttpClient client)
    {
        // Arrange
        var command = new Register.Command("username", "display name", "password");

        // Act
        var postResponse = await client.PostAsJsonAsync("/api/users/register", command);
        var errorResponse = await client.PostAsJsonAsync("/api/users/register", command);

        // Assert
        postResponse.Headers.Should().Contain(header => header.Key == TraceIdHeaderName);
        errorResponse.Headers.Should().Contain(header => header.Key == TraceIdHeaderName);
        errorResponse.Should().HaveClientError();

        var postTraceId = postResponse.Headers.GetValues(TraceIdHeaderName).ToList();
        var errorTraceId = errorResponse.Headers.GetValues(TraceIdHeaderName).ToList();

        postTraceId.Should().NotContainEquivalentOf(errorTraceId);
    }
}
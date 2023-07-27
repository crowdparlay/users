using System.Net.Http.Json;
using CrowdParlay.Users.Application.Features.Users.Commands;
using CrowdParlay.Users.IntegrationTests.Fixtures;
using FluentAssertions;

namespace CrowdParlay.Users.IntegrationTests.Tests;

[Collection("RabbitMqAffective")]
public class TraceIdMiddlewareTests : IClassFixture<WebApplicationContext>
{
    private const string TraceIdHeaderName = "X-TraceId";
    private readonly IFixture _fixture;

    public TraceIdMiddlewareTests(WebApplicationContext context) => _fixture = context.Fixture;

    [Fact(Timeout = 5000)]
    public async Task SuccessResponse_ShouldContain_TraceIdHeader()
    {
        // Arrange
        var client = _fixture.Create<HttpClient>();
        var registerRequest = new Register.Command("username", "display name", "password");

        // Act
        var registerMessage = await client.PostAsJsonAsync("/api/users/register", registerRequest);

        // Assert
        registerMessage.Headers.Should().Contain(header => header.Key == TraceIdHeaderName);
        
        // Teardown
        var registerResponse = await registerMessage.Content.ReadFromJsonAsync<Register.Response>();
        await client.DeleteAsync($"/api/users/{registerResponse!.Id}");
    }

    [Fact(Timeout = 5000)]
    public async Task FailureResponse_ShouldContain_TraceIdHeader()
    {
        // Arrange
        var client = _fixture.Create<HttpClient>();
        var command = new Register.Command(string.Empty, string.Empty, string.Empty);

        // Act
        var response = await client.PostAsJsonAsync("/api/users/register", command);

        // Assert
        response.Headers.Should().Contain(header => header.Key == TraceIdHeaderName);
        response.Should().HaveClientError();
    }

    [Fact(Timeout = 5000)]
    public async Task MultipleResponses_ShouldContain_UniqueTraceIdHeaders()
    {
        // Arrange
        var client = _fixture.Create<HttpClient>();
        var registerRequest = new Register.Command("username", "display name", "password");

        // Act
        var successMessage = await client.PostAsJsonAsync("/api/users/register", registerRequest);
        var failureMessage = await client.PostAsJsonAsync("/api/users/register", registerRequest);

        // Assert
        successMessage.Headers.Should().Contain(header => header.Key == TraceIdHeaderName);
        failureMessage.Headers.Should().Contain(header => header.Key == TraceIdHeaderName);
        failureMessage.Should().HaveClientError();

        var successMessageTraceId = successMessage.Headers.GetValues(TraceIdHeaderName).ToList();
        var failureMessageTraceId = failureMessage.Headers.GetValues(TraceIdHeaderName).ToList();

        successMessageTraceId.Should().NotContainEquivalentOf(failureMessageTraceId);
        
        // Teardown
        var registerResponse = await successMessage.Content.ReadFromJsonAsync<Register.Response>();
        await client.DeleteAsync($"/api/users/{registerResponse!.Id}");
    }
}
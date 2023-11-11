using CrowdParlay.Users.Application.Features.Users.Commands;
using CrowdParlay.Users.IntegrationTests.Fixtures;
using FluentAssertions;

namespace CrowdParlay.Users.IntegrationTests.Tests;

[Collection("CommunicationAffective")]
public class TraceIdMiddlewareTests : IClassFixture<WebApplicationContext>
{
    private const string TraceIdHeaderName = "X-TraceId";
    private readonly HttpClient _client;

    public TraceIdMiddlewareTests(WebApplicationContext context) => _client = context.Client;

    [Fact(DisplayName = "Register user returns trace ID on success", Timeout = 5000)]
    public async Task RegisterUserOnSuccess_ReturnsTraceId()
    {
        var registerRequest = new Register.Command("username", "display name", "password", "email@gmail.com", null);
        var registerMessage = await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest);

        registerMessage.Headers.Should().Contain(header => header.Key == TraceIdHeaderName);
    }

    [Fact(DisplayName = "Register user returns trace ID on failure", Timeout = 5000)]
    public async Task RegisterUserOnFailure_ReturnsTraceId()
    {
        var command = new Register.Command(string.Empty, string.Empty, string.Empty,string.Empty, null);
        var response = await _client.PostAsJsonAsync("/api/v1/users/register", command);

        response.Headers.Should().Contain(header => header.Key == TraceIdHeaderName);
        response.Should().HaveClientError();
    }

    [Fact(DisplayName = "Register users returns unique trace IDs", Timeout = 5000)]
    public async Task RegisterUser_ReturnsUniqueTraceIds()
    {
        var registerRequest = new Register.Command("username", "display name", "emaeMamaZvonit@gmail.com", "password",  null);

        var successMessage = await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest);
        var failureMessage = await _client.PostAsJsonAsync("/api/v1/users/register", registerRequest);

        successMessage.Headers.Should().Contain(header => header.Key == TraceIdHeaderName);
        failureMessage.Headers.Should().Contain(header => header.Key == TraceIdHeaderName);
        failureMessage.Should().HaveClientError();

        var successMessageTraceId = successMessage.Headers.GetValues(TraceIdHeaderName).ToList();
        var failureMessageTraceId = failureMessage.Headers.GetValues(TraceIdHeaderName).ToList();

        successMessageTraceId.Should().NotContainEquivalentOf(failureMessageTraceId);
    }
}
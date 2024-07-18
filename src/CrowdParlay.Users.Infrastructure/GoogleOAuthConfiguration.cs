namespace CrowdParlay.Users.Infrastructure;

public sealed class GoogleOAuthConfiguration
{
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
}
namespace CrowdParlay.Auth.Application.Abstractions.Identity;

/// <summary>
/// User abstraction declaring infrastructure-dependent identity concerns.
/// </summary>
public interface IUser
{
    public Guid Id { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string DisplayName { get; set; }
}
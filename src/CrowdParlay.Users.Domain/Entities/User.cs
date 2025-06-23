namespace CrowdParlay.Users.Domain.Entities;

public class User : EntityBase<Guid>
{
    public required string Username { get; set; }
    public required string DisplayName { get; set; }
    public required string Email { get; set; }
    public required string? AvatarUrl { get; set; }
    public required string? PasswordHash { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
}
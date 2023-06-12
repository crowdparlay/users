using Dodo.Primitives;

namespace CrowdParlay.Users.Domain.Entities;

public class User : EntityBase<Uuid>
{
    public required string Username { get; set; }
    public required string DisplayName { get; set; }
    public required string PasswordHash { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
}
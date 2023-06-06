using Dodo.Primitives;

namespace CrowdParlay.Users.Domain.Entities;

public class User
{
    public required Uuid Id { get; init; }
    public required string Username { get; set; }
    public required string DisplayName { get; set; }
}
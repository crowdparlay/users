using Dodo.Primitives;

namespace CrowdParlay.Users.Domain.Entities;

public class User : EntityBase<Uuid>
{
    public required string Username { get; set; }
    public required string DisplayName { get; set; }
}
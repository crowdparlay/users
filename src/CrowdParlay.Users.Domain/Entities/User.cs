using Microsoft.AspNetCore.Identity;

namespace CrowdParlay.Users.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public required string DisplayName { get; set; }
}
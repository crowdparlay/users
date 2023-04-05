using CrowdParlay.Auth.Application.Abstractions.Identity;
using Microsoft.AspNetCore.Identity;

namespace CrowdParlay.Auth.Infrastructure.Identity;

public class User : IdentityUser<Guid>, IUser
{
    public required string DisplayName { get; set; }

    public string? Username
    {
        get => UserName;
        set => UserName = value;
    }
}
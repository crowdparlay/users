using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Domain.Entities;

namespace CrowdParlay.Users.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IPasswordHasher _hasher;

    public AuthenticationService(IPasswordHasher hasher) => _hasher = hasher;

    public bool Authenticate(User user, string password) => _hasher.VerifyPassword(user.PasswordHash, password);
}
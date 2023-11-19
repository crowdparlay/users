using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Domain.Entities;

namespace CrowdParlay.Users.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IPasswordService _passwordService;

    public AuthenticationService(IPasswordService passwordService) => _passwordService = passwordService;

    public bool Authenticate(User user, string password) => _passwordService.VerifyPassword(user.PasswordHash, password);
}
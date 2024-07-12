using System.IdentityModel.Tokens.Jwt;
using CrowdParlay.Users.Application.Services;

namespace CrowdParlay.Users.Application.Abstractions;

public interface IGoogleAuthenticationService
{
    public Task<GoogleAuthenticationResult> AuthenticateUserByIdTokenAsync(JwtSecurityToken idToken);
}
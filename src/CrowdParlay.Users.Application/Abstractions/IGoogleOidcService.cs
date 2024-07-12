using System.IdentityModel.Tokens.Jwt;
using CrowdParlay.Users.Application.Models;

namespace CrowdParlay.Users.Application.Abstractions;

public interface IGoogleOidcService
{
    public Task<GoogleUserInfo?> GetUserInfoByIdTokenAsync(JwtSecurityToken idToken);
}
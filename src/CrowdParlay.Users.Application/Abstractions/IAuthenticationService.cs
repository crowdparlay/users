using CrowdParlay.Users.Domain.Entities;

namespace CrowdParlay.Users.Application.Abstractions;

public interface IAuthenticationService
{
    public bool Authenticate(User user, string password);;;;;;
}

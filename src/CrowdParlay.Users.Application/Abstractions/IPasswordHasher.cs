namespace CrowdParlay.Users.Application.Abstractions;

public interface IPasswordHasher
{
    public string HashPassword(string password);
    public bool VerifyPassword(string hashedPassword, string password);
}
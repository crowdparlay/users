namespace CrowdParlay.Users.Application.Abstractions;

public interface IPasswordService
{
    public string HashPassword(string password);
    public bool VerifyPassword(string hashedPassword, string password);
}
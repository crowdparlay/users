namespace CrowdParlay.Users.Application.Models;

public sealed class GoogleUserInfo
{
    public string Id { get; set; }
    public string Email { get; set; }
    public bool IsEmailVerified { get; set; }
    public string Name { get; set; }
    public string AvatarUrl { get; set; }
    public string GivenName { get; set; }
    public string FamilyName { get; set; }
}
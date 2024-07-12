namespace CrowdParlay.Users.Domain.Entities;

public class ExternalLoginProvider : EntityBase<string>
{
    public string DisplayName { get; set; }
    public string LogoUrl { get; set; }
}
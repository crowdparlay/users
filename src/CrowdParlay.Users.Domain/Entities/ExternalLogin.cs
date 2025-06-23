namespace CrowdParlay.Users.Domain.Entities;

public class ExternalLogin : EntityBase<Guid>
{
    public Guid UserId { get; set; }
    public string ProviderId { get; set; }
    public string Identity { get; set; }
}
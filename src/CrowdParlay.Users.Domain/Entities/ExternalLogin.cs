using Dodo.Primitives;

namespace CrowdParlay.Users.Domain.Entities;

public class ExternalLogin : EntityBase<Uuid>
{
    public Uuid UserId { get; set; }
    public string ProviderId { get; set; }
    public string Identity { get; set; }
}
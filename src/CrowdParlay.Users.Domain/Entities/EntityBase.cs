namespace CrowdParlay.Users.Domain.Entities;

public abstract class EntityBase<TKey>
{
    public required TKey Id { get; init; }
}
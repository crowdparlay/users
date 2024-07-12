namespace CrowdParlay.Users.Domain.Entities;

public abstract class EntityBase<TKey> : IEquatable<EntityBase<TKey>> where TKey : notnull
{
    public required TKey Id { get; init; }

    public bool Equals(EntityBase<TKey>? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return EqualityComparer<TKey>.Default.Equals(Id, other.Id);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((EntityBase<TKey>)obj);
    }

    public override int GetHashCode() => EqualityComparer<TKey>.Default.GetHashCode(Id);
}
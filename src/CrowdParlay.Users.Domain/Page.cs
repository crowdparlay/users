namespace CrowdParlay.Users.Domain;

public record Page<TItem>(int TotalCount, IEnumerable<TItem> Items);
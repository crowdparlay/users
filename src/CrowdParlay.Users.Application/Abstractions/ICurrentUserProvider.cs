namespace CrowdParlay.Users.Application.Abstractions;

public interface ICurrentUserProvider
{
    public bool IsAuthenticated { get; }
    public int? AccountId { get; }
}
using CrowdParlay.Users.Domain.Entities;
using Dodo.Primitives;

namespace CrowdParlay.Users.Domain.Abstractions;

public interface IUsersRepository : IAsyncGenericRepository<User, Uuid> { }
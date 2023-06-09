using Microsoft.EntityFrameworkCore;

namespace CrowdParlay.Users.Infrastructure.Persistence;

public class OpenIddictDbContext : DbContext
{
    public OpenIddictDbContext(DbContextOptions<OpenIddictDbContext> options) : base(options) { }
}
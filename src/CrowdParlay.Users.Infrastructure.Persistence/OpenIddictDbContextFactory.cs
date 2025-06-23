using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CrowdParlay.Users.Infrastructure.Persistence;

public class OpenIddictDbContextFactory : IDesignTimeDbContextFactory<OpenIddictDbContext>
{
    public OpenIddictDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OpenIddictDbContext>()
            .UseNpgsql("Host=localhost;Database=dummy;Username=dummy;Password=dummy");
        return new OpenIddictDbContext(optionsBuilder.Options);
    }
}
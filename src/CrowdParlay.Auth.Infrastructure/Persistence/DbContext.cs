using CrowdParlay.Auth.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CrowdParlay.Auth.Infrastructure.Persistence;

public sealed class DbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public DbContext(DbContextOptions<DbContext> options) : base(options) { }
}
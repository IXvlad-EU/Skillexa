using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Skillexa.Engine.Domain;

namespace Skillexa.Engine.Data;

public sealed class EngineDbContext(DbContextOptions<EngineDbContext> options)
    : DbContext(options)
{
    public DbSet<Template> Templates => Set<Template>();

    public DbSet<ProviderQuota> ProviderQuotas => Set<ProviderQuota>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}

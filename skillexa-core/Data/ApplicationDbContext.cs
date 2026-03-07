using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Skillexa.Core.Domain;

namespace Skillexa.Core.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<JobStatus> JobStatuses => Set<JobStatus>();

    public DbSet<Job> Jobs => Set<Job>();

    public DbSet<ProviderUsage> ProviderUsages => Set<ProviderUsage>();

    public DbSet<Template> Templates => Set<Template>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}

using Microsoft.EntityFrameworkCore;
using Skillexa.Core.Data.Repositories.Interfaces;
using Skillexa.Core.Domain;

namespace Skillexa.Core.Data.Repositories.Implementations;

public sealed class ProviderUsageRepository(ApplicationDbContext db) : IProviderUsageRepository
{
    public Task<ProviderUsage?> GetByProviderAndPeriodAsync(string provider, string periodKey, CancellationToken cancellationToken = default)
    {
        return db.ProviderUsages.FirstOrDefaultAsync(
            usage => usage.Provider == provider && usage.PeriodKey == periodKey,
            cancellationToken);
    }

    public async Task AddAsync(ProviderUsage usage, CancellationToken cancellationToken = default)
    {
        await db.ProviderUsages.AddAsync(usage, cancellationToken);
    }
}

using Microsoft.EntityFrameworkCore;
using Skillexa.Core.Data.Repositories.Interfaces;
using Skillexa.Core.Domain;

namespace Skillexa.Core.Data.Repositories.Implementations;

public sealed class ProviderUsageRepository(ApplicationDbContext dbContext) : IProviderUsageRepository
{
    public Task<ProviderUsage?> GetByProviderAndPeriodAsync(string provider, string periodKey, CancellationToken cancellationToken = default)
    {
        return dbContext.ProviderUsages.FirstOrDefaultAsync(
            usage => usage.Provider == provider && usage.PeriodKey == periodKey,
            cancellationToken);
    }

    public async Task AddAsync(ProviderUsage usage, CancellationToken cancellationToken = default)
    {
        await dbContext.ProviderUsages.AddAsync(usage, cancellationToken);
    }
}

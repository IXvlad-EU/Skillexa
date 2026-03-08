using Microsoft.EntityFrameworkCore;
using Skillexa.Engine.Data.Repositories.Interfaces;
using Skillexa.Engine.Domain;

namespace Skillexa.Engine.Data.Repositories.Implementations;

public sealed class ProviderQuotaRepository(EngineDbContext db) : IProviderQuotaRepository
{
    public Task<ProviderQuota?> GetAsync(string provider, DateOnly dayKey, CancellationToken cancellationToken = default)
    {
        return db.ProviderQuotas.FirstOrDefaultAsync(
            quota => quota.Provider == provider && quota.DayKey == dayKey,
            cancellationToken);
    }

    /// <summary>
    /// Atomically increments the <c>used</c> counter if it is below the <c>limit</c>.
    /// Returns <c>true</c> if the decrement succeeded (quota available), <c>false</c> if exhausted.
    /// </summary>
    public async Task<bool> TryDecrementAsync(string provider, DateOnly dayKey, CancellationToken cancellationToken = default)
    {
        var rowsAffected = await db.ProviderQuotas
            .Where(quota => quota.Provider == provider
                && quota.DayKey == dayKey
                && quota.Used < quota.Limit)
            .ExecuteUpdateAsync(
                setter => setter
                    .SetProperty(quota => quota.Used, quota => quota.Used + 1)
                    .SetProperty(quota => quota.UpdatedAt, DateTime.UtcNow),
                cancellationToken);

        return rowsAffected > 0;
    }

    public async Task AddAsync(ProviderQuota quota, CancellationToken cancellationToken = default)
    {
        await db.ProviderQuotas.AddAsync(quota, cancellationToken);
    }
}

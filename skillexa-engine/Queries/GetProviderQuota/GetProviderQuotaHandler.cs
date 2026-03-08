using Microsoft.EntityFrameworkCore;
using Skillexa.Engine.Data;

namespace Skillexa.Engine.Queries.GetProviderQuota;

public class GetProviderQuotaHandler(EngineDbContext db)
    : IQueryHandler<GetProviderQuotaQuery, GetProviderQuotaResult?>
{
    public async Task<GetProviderQuotaResult?> HandleAsync(
        GetProviderQuotaQuery query, CancellationToken cancellationToken = default)
    {
        return await db.ProviderQuotas
            .AsNoTracking()
            .Where(quota => quota.Provider == query.Provider && quota.DayKey == query.DayKey)
            .Select(quota => new GetProviderQuotaResult(
                quota.Id,
                quota.Provider,
                quota.DayKey,
                quota.Used,
                quota.Limit))
            .FirstOrDefaultAsync(cancellationToken);
    }
}

using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Skillexa.Core.Data;

namespace Skillexa.Core.Queries.GetUsage;

public class GetUsageHandler(ApplicationDbContext dbContext)
    : IQueryHandler<GetUsageQuery, GetUsageResult?>
{
    public async Task<GetUsageResult?> HandleAsync(
        GetUsageQuery query, CancellationToken cancellationToken = default)
    {
        var currentPeriod = DateTime.UtcNow.ToString("yyyy-MM", CultureInfo.InvariantCulture);

        return await dbContext.ProviderUsages
            .AsNoTracking()
            .Where(usage => usage.Provider == "theirstack" && usage.PeriodKey == currentPeriod)
            .Select(usage => new GetUsageResult(
                usage.Provider,
                usage.PeriodKey,
                usage.Used,
                usage.Remaining))
            .FirstOrDefaultAsync(cancellationToken);
    }
}

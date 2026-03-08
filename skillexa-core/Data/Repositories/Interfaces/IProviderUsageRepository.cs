using Skillexa.Core.Domain;

namespace Skillexa.Core.Data.Repositories.Interfaces;

public interface IProviderUsageRepository
{
    Task<ProviderUsage?> GetByProviderAndPeriodAsync(string provider, string periodKey, CancellationToken cancellationToken = default);
    Task AddAsync(ProviderUsage usage, CancellationToken cancellationToken = default);
}

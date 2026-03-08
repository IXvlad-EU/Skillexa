using Skillexa.Engine.Domain;

namespace Skillexa.Engine.Data.Repositories.Interfaces;

public interface IProviderQuotaRepository
{
    Task<ProviderQuota?> GetAsync(string provider, DateOnly dayKey, CancellationToken cancellationToken = default);
    Task<bool> TryDecrementAsync(string provider, DateOnly dayKey, CancellationToken cancellationToken = default);
    Task AddAsync(ProviderQuota quota, CancellationToken cancellationToken = default);
}

using Skillexa.Engine.Data.Repositories.Interfaces;

namespace Skillexa.Engine.Data.UnitOfWork.Interfaces;

public interface IUnitOfWork
{
    IProviderQuotaRepository ProviderQuotas { get; }
    ITemplateRepository Templates { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

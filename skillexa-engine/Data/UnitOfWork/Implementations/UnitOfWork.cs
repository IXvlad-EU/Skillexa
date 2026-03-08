using Skillexa.Engine.Data.Repositories.Interfaces;
using Skillexa.Engine.Data.UnitOfWork.Interfaces;

namespace Skillexa.Engine.Data.UnitOfWork.Implementations;

public sealed class UnitOfWork(
    EngineDbContext db,
    IProviderQuotaRepository providerQuotas,
    ITemplateRepository templates) : IUnitOfWork
{
    public IProviderQuotaRepository ProviderQuotas => providerQuotas;
    public ITemplateRepository Templates => templates;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return db.SaveChangesAsync(cancellationToken);
    }
}

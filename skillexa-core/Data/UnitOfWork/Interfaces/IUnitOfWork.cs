using Skillexa.Core.Data.Repositories.Interfaces;

namespace Skillexa.Core.Data.UnitOfWork.Interfaces;

public interface IUnitOfWork
{
    IDocumentRepository Documents { get; }
    IUserRepository Users { get; }
    ITemplateRepository Templates { get; }
    IOutboxRepository OutboxMessages { get; }
    IProviderUsageRepository ProviderUsages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

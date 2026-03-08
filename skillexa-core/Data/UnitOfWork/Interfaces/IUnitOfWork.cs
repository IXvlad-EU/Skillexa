using Skillexa.Core.Data.Repositories.Interfaces;

namespace Skillexa.Core.Data.UnitOfWork.Interfaces;

public interface IUnitOfWork
{
    IJobRepository Jobs { get; }
    IUserRepository Users { get; }
    ITemplateRepository Templates { get; }
    IOutboxRepository OutboxMessages { get; }
    IProviderUsageRepository ProviderUsages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

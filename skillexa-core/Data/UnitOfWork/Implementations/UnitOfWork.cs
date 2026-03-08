using Skillexa.Core.Data.Repositories.Interfaces;
using Skillexa.Core.Data.UnitOfWork.Interfaces;

namespace Skillexa.Core.Data.UnitOfWork.Implementations;

public sealed class UnitOfWork(
    ApplicationDbContext db,
    IJobRepository jobs,
    IUserRepository users,
    ITemplateRepository templates,
    IOutboxRepository outboxMessages,
    IProviderUsageRepository providerUsages) : IUnitOfWork
{
    public IJobRepository Jobs => jobs;
    public IUserRepository Users => users;
    public ITemplateRepository Templates => templates;
    public IOutboxRepository OutboxMessages => outboxMessages;
    public IProviderUsageRepository ProviderUsages => providerUsages;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return db.SaveChangesAsync(cancellationToken);
    }
}

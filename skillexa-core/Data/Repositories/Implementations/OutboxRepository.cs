using Microsoft.EntityFrameworkCore;
using Skillexa.Core.Data.Repositories.Interfaces;
using Skillexa.Core.Domain;

namespace Skillexa.Core.Data.Repositories.Implementations;

public sealed class OutboxRepository(ApplicationDbContext dbContext) : IOutboxRepository
{
    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        await dbContext.OutboxMessages.AddAsync(message, cancellationToken);
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetUnpublishedAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        return await dbContext.OutboxMessages
            .Where(message => message.PublishedAt == null)
            .OrderBy(message => message.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkPublishedAsync(long id, CancellationToken cancellationToken = default)
    {
        await dbContext.OutboxMessages
            .Where(message => message.Id == id)
            .ExecuteUpdateAsync(
                setter => setter.SetProperty(message => message.PublishedAt, DateTime.UtcNow),
                cancellationToken);
    }
}

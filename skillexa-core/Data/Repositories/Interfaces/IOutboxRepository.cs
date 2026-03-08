using Skillexa.Core.Domain;

namespace Skillexa.Core.Data.Repositories.Interfaces;

public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OutboxMessage>> GetUnpublishedAsync(int batchSize, CancellationToken cancellationToken = default);
    Task MarkPublishedAsync(long id, CancellationToken cancellationToken = default);
}

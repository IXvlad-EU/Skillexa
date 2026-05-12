using Skillexa.Core.Domain;

namespace Skillexa.Core.Data.Repositories.Interfaces;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Document?> GetByIdForUserAsync(long id, long userId, CancellationToken cancellationToken = default);
    Task AddAsync(Document document, CancellationToken cancellationToken = default);
    Task<bool> ExistsByIdempotencyKeyAsync(long idempotencyKey, CancellationToken cancellationToken = default);
}

using Microsoft.EntityFrameworkCore;
using Skillexa.Core.Data.Repositories.Interfaces;
using Skillexa.Core.Domain;

namespace Skillexa.Core.Data.Repositories.Implementations;

public sealed class DocumentRepository(ApplicationDbContext dbContext) : IDocumentRepository
{
    public Task<Document?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return dbContext.Documents.FirstOrDefaultAsync(document => document.Id == id, cancellationToken);
    }

    public Task<Document?> GetByIdForUserAsync(long id, long userId, CancellationToken cancellationToken = default)
    {
        return dbContext.Documents.FirstOrDefaultAsync(document => document.Id == id && document.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(Document document, CancellationToken cancellationToken = default)
    {
        await dbContext.Documents.AddAsync(document, cancellationToken);
    }

    public Task<bool> ExistsByIdempotencyKeyAsync(long idempotencyKey, CancellationToken cancellationToken = default)
    {
        return dbContext.Documents.AnyAsync(document => document.IdempotencyKey == idempotencyKey, cancellationToken);
    }
}

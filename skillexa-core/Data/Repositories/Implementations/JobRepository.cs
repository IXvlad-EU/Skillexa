using Microsoft.EntityFrameworkCore;
using Skillexa.Core.Data.Repositories.Interfaces;
using Skillexa.Core.Domain;

namespace Skillexa.Core.Data.Repositories.Implementations;

public sealed class JobRepository(ApplicationDbContext db) : IJobRepository
{
    public Task<Job?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return db.Jobs.FirstOrDefaultAsync(job => job.Id == id, cancellationToken);
    }

    public Task<Job?> GetByIdForUserAsync(long id, long userId, CancellationToken cancellationToken = default)
    {
        return db.Jobs.FirstOrDefaultAsync(job => job.Id == id && job.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(Job job, CancellationToken cancellationToken = default)
    {
        await db.Jobs.AddAsync(job, cancellationToken);
    }

    public Task<bool> ExistsByIdempotencyKeyAsync(long idempotencyKey, CancellationToken cancellationToken = default)
    {
        return db.Jobs.AnyAsync(job => job.IdempotencyKey == idempotencyKey, cancellationToken);
    }
}

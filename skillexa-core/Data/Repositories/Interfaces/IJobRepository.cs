using Skillexa.Core.Domain;

namespace Skillexa.Core.Data.Repositories.Interfaces;

public interface IJobRepository
{
    Task<Job?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Job?> GetByIdForUserAsync(long id, long userId, CancellationToken cancellationToken = default);
    Task AddAsync(Job job, CancellationToken cancellationToken = default);
    Task<bool> ExistsByIdempotencyKeyAsync(long idempotencyKey, CancellationToken cancellationToken = default);
}

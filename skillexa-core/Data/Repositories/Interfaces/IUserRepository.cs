using Skillexa.Core.Domain;

namespace Skillexa.Core.Data.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEntraIdAsync(string entraObjectId, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
}

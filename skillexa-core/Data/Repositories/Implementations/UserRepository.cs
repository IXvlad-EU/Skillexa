using Microsoft.EntityFrameworkCore;
using Skillexa.Core.Data.Repositories.Interfaces;
using Skillexa.Core.Domain;

namespace Skillexa.Core.Data.Repositories.Implementations;

public sealed class UserRepository(ApplicationDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByEntraIdAsync(string entraObjectId, CancellationToken cancellationToken = default)
    {
        return dbContext.Users.FirstOrDefaultAsync(user => user.EntraObjectId == entraObjectId, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
    }
}

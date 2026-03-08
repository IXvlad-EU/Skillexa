using Skillexa.Core.Domain;

namespace Skillexa.Core.Data.Repositories.Interfaces;

public interface ITemplateRepository
{
    Task<Template?> GetActiveByKeyAsync(string templateKey, CancellationToken cancellationToken = default);
    Task<Template?> GetByKeyAndVersionAsync(string templateKey, int version, CancellationToken cancellationToken = default);
    Task<int> GetNextVersionAsync(string templateKey, CancellationToken cancellationToken = default);
    Task AddAsync(Template entity, CancellationToken cancellationToken = default);
    Task DeactivateAllVersionsAsync(string templateKey, CancellationToken cancellationToken = default);
}

using Skillexa.Engine.Domain;

namespace Skillexa.Engine.Data.Repositories.Interfaces;

public interface ITemplateRepository
{
    Task<Template?> GetByKeyAndVersionAsync(string templateKey, int version, CancellationToken cancellationToken = default);
    Task<Template?> GetActiveByKeyAsync(string templateKey, CancellationToken cancellationToken = default);
}

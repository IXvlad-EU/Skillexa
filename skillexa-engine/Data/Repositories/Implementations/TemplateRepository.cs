using Microsoft.EntityFrameworkCore;
using Skillexa.Engine.Data.Repositories.Interfaces;
using Skillexa.Engine.Domain;

namespace Skillexa.Engine.Data.Repositories.Implementations;

public sealed class TemplateRepository(EngineDbContext db) : ITemplateRepository
{
    public Task<Template?> GetByKeyAndVersionAsync(string templateKey, int version, CancellationToken cancellationToken = default)
    {
        return db.Templates.FirstOrDefaultAsync(
            template => template.TemplateKey == templateKey && template.Version == version,
            cancellationToken);
    }

    public Task<Template?> GetActiveByKeyAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        return db.Templates.FirstOrDefaultAsync(
            template => template.TemplateKey == templateKey && template.IsActive,
            cancellationToken);
    }
}

using Microsoft.EntityFrameworkCore;
using Skillexa.Core.Data.Repositories.Interfaces;
using Skillexa.Core.Domain;

namespace Skillexa.Core.Data.Repositories.Implementations;

public sealed class TemplateRepository(ApplicationDbContext dbContext) : ITemplateRepository
{
    public Task<Template?> GetActiveByKeyAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        return dbContext.Templates.FirstOrDefaultAsync(
            template => template.TemplateKey == templateKey && template.IsActive,
            cancellationToken);
    }

    public Task<Template?> GetByKeyAndVersionAsync(string templateKey, int version, CancellationToken cancellationToken = default)
    {
        return dbContext.Templates.FirstOrDefaultAsync(
            template => template.TemplateKey == templateKey && template.Version == version,
            cancellationToken);
    }

    public async Task<int> GetNextVersionAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        var maxVersion = await dbContext.Templates
            .Where(template => template.TemplateKey == templateKey)
            .MaxAsync(template => (int?)template.Version, cancellationToken);

        return (maxVersion ?? 0) + 1;
    }

    public async Task AddAsync(Template entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Templates.AddAsync(entity, cancellationToken);
    }

    public async Task DeactivateAllVersionsAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        await dbContext.Templates
            .Where(template => template.TemplateKey == templateKey && template.IsActive)
            .ExecuteUpdateAsync(
                setter => setter.SetProperty(template => template.IsActive, false),
                cancellationToken);
    }
}

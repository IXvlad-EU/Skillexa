using Microsoft.EntityFrameworkCore;
using Skillexa.Engine.Data;

namespace Skillexa.Engine.Queries.GetTemplateByKeyAndVersion;

public class GetTemplateByKeyAndVersionHandler(EngineDbContext db)
    : IQueryHandler<GetTemplateByKeyAndVersionQuery, GetTemplateByKeyAndVersionResult?>
{
    public async Task<GetTemplateByKeyAndVersionResult?> HandleAsync(
        GetTemplateByKeyAndVersionQuery query, CancellationToken cancellationToken = default)
    {
        return await db.Templates
            .AsNoTracking()
            .Where(template => template.TemplateKey == query.TemplateKey
                && template.Version == query.Version)
            .Select(template => new GetTemplateByKeyAndVersionResult(
                template.Id,
                template.TemplateKey,
                template.Version,
                template.IsActive,
                template.Content))
            .FirstOrDefaultAsync(cancellationToken);
    }
}

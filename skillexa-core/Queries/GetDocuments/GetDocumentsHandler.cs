using Microsoft.EntityFrameworkCore;
using Skillexa.Core.Data;

namespace Skillexa.Core.Queries.GetDocuments;

public class GetDocumentsHandler(ApplicationDbContext dbContext)
    : IQueryHandler<GetDocumentsQuery, IReadOnlyList<GetDocumentsResult>>
{
    public async Task<IReadOnlyList<GetDocumentsResult>> HandleAsync(
        GetDocumentsQuery query, CancellationToken cancellationToken = default)
    {
        return await dbContext.Documents
            .AsNoTracking()
            .Where(document => document.UserId == query.UserId)
            .OrderByDescending(document => document.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(document => new GetDocumentsResult(
                document.Id,
                document.Status.Name,
                document.TemplateKey,
                document.ErrorCode,
                document.CreatedAt,
                document.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}

using Microsoft.EntityFrameworkCore;
using Skillexa.Core.Data;

namespace Skillexa.Core.Queries.GetDocumentById;

public class GetDocumentByIdHandler(ApplicationDbContext db)
    : IQueryHandler<GetDocumentByIdQuery, GetDocumentByIdResult?>
{
    public async Task<GetDocumentByIdResult?> HandleAsync(
        GetDocumentByIdQuery query, CancellationToken cancellationToken = default)
    {
        return await db.Documents
            .AsNoTracking()
            .Where(document => document.Id == query.DocumentId && document.UserId == query.UserId)
            .Select(document => new GetDocumentByIdResult(
                document.Id,
                document.Status.Name,
                document.TemplateKey,
                document.TemplateVersion,
                document.PdfStorageKey,
                document.SnapshotStorageKey,
                document.ErrorCode,
                document.ErrorMessage,
                document.CreatedAt,
                document.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}

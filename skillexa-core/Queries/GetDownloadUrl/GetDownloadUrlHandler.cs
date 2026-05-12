using Microsoft.EntityFrameworkCore;
using Skillexa.Core.Data;
using Skillexa.Core.Domain;

namespace Skillexa.Core.Queries.GetDownloadUrl;

public class GetDownloadUrlHandler(ApplicationDbContext db)
    : IQueryHandler<GetDownloadUrlQuery, GetDownloadUrlResult>
{
    public async Task<GetDownloadUrlResult> HandleAsync(
        GetDownloadUrlQuery query, CancellationToken cancellationToken = default)
    {
        var document = await db.Documents
            .AsNoTracking()
            .Where(document => document.Id == query.DocumentId && document.UserId == query.UserId)
            .Select(document => new { document.StatusId, document.PdfStorageKey })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"Document '{query.DocumentId}' not found.");

        if (document.StatusId != DocumentStatuses.Succeeded)
        {
            throw new InvalidOperationException($"Document '{query.DocumentId}' is not in 'Succeeded' status.");
        }

        if (string.IsNullOrEmpty(document.PdfStorageKey))
        {
            throw new InvalidOperationException($"Document '{query.DocumentId}' has no PDF storage key.");
        }

        // TODO: Inject IObjectStorage and call GenerateSignedUrlAsync when storage layer is implemented.
        // For now, return the storage key as a placeholder URL.
        var expiresIn = 600; // 10 minutes
        var placeholderUrl = $"https://storage.placeholder/{document.PdfStorageKey}?expiresIn={expiresIn}";

        return new GetDownloadUrlResult(placeholderUrl, expiresIn);
    }
}

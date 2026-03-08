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
        var job = await db.Jobs
            .AsNoTracking()
            .Where(job => job.Id == query.JobId && job.UserId == query.UserId)
            .Select(job => new { job.StatusId, job.PdfStorageKey })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"Job '{query.JobId}' not found.");

        if (job.StatusId != JobStatuses.Succeeded)
        {
            throw new InvalidOperationException($"Job '{query.JobId}' is not in 'Succeeded' status.");
        }

        if (string.IsNullOrEmpty(job.PdfStorageKey))
        {
            throw new InvalidOperationException($"Job '{query.JobId}' has no PDF storage key.");
        }

        // TODO: Inject IObjectStorage and call GenerateSignedUrlAsync when storage layer is implemented.
        // For now, return the storage key as a placeholder URL.
        var expiresIn = 600; // 10 minutes
        var placeholderUrl = $"https://storage.placeholder/{job.PdfStorageKey}?expiresIn={expiresIn}";

        return new GetDownloadUrlResult(placeholderUrl, expiresIn);
    }
}

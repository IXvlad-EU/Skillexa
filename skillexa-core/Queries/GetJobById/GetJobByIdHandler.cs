using Microsoft.EntityFrameworkCore;
using Skillexa.Core.Data;

namespace Skillexa.Core.Queries.GetJobById;

public class GetJobByIdHandler(ApplicationDbContext db)
    : IQueryHandler<GetJobByIdQuery, GetJobByIdResult?>
{
    public async Task<GetJobByIdResult?> HandleAsync(
        GetJobByIdQuery query, CancellationToken cancellationToken = default)
    {
        return await db.Jobs
            .AsNoTracking()
            .Where(job => job.Id == query.JobId && job.UserId == query.UserId)
            .Select(job => new GetJobByIdResult(
                job.Id,
                job.Status.Name,
                job.TemplateKey,
                job.TemplateVersion,
                job.PdfStorageKey,
                job.SnapshotStorageKey,
                job.ErrorCode,
                job.ErrorMessage,
                job.CreatedAt,
                job.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}

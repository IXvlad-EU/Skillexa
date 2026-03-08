using Microsoft.EntityFrameworkCore;
using Skillexa.Core.Data;

namespace Skillexa.Core.Queries.GetJobs;

public class GetJobsHandler(ApplicationDbContext db)
    : IQueryHandler<GetJobsQuery, IReadOnlyList<GetJobsResult>>
{
    public async Task<IReadOnlyList<GetJobsResult>> HandleAsync(
        GetJobsQuery query, CancellationToken cancellationToken = default)
    {
        return await db.Jobs
            .AsNoTracking()
            .Where(job => job.UserId == query.UserId)
            .OrderByDescending(job => job.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(job => new GetJobsResult(
                job.Id,
                job.Status.Name,
                job.TemplateKey,
                job.ErrorCode,
                job.CreatedAt,
                job.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}

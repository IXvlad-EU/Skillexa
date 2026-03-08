namespace Skillexa.Core.Queries.GetJobs;

public record GetJobsQuery(long UserId, int Page = 1, int PageSize = 20) : IQuery<IReadOnlyList<GetJobsResult>>;

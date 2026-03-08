namespace Skillexa.Core.Queries.GetUsage;

public record GetUsageQuery(long UserId) : IQuery<GetUsageResult?>;

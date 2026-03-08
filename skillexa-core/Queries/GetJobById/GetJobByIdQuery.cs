namespace Skillexa.Core.Queries.GetJobById;

public record GetJobByIdQuery(long JobId, long UserId) : IQuery<GetJobByIdResult?>;

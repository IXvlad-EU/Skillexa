namespace Skillexa.Core.Queries.GetDownloadUrl;

public record GetDownloadUrlQuery(long JobId, long UserId) : IQuery<GetDownloadUrlResult>;

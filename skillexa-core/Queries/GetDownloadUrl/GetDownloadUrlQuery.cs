namespace Skillexa.Core.Queries.GetDownloadUrl;

public record GetDownloadUrlQuery(long DocumentId, long UserId) : IQuery<GetDownloadUrlResult>;

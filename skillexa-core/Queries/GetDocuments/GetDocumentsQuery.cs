namespace Skillexa.Core.Queries.GetDocuments;

public record GetDocumentsQuery(long UserId, int Page = 1, int PageSize = 20) : IQuery<IReadOnlyList<GetDocumentsResult>>;

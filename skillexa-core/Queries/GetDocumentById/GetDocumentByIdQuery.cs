namespace Skillexa.Core.Queries.GetDocumentById;

public record GetDocumentByIdQuery(long DocumentId, long UserId) : IQuery<GetDocumentByIdResult?>;

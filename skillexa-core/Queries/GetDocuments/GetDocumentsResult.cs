namespace Skillexa.Core.Queries.GetDocuments;

public record GetDocumentsResult(
    long Id,
    string Status,
    string TemplateKey,
    string? ErrorCode,
    DateTime CreatedAt,
    DateTime UpdatedAt);

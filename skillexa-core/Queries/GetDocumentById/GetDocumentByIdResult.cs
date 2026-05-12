namespace Skillexa.Core.Queries.GetDocumentById;

public record GetDocumentByIdResult(
    long Id,
    string Status,
    string TemplateKey,
    int TemplateVersion,
    string? PdfStorageKey,
    string? SnapshotStorageKey,
    string? ErrorCode,
    string? ErrorMessage,
    DateTime CreatedAt,
    DateTime UpdatedAt);

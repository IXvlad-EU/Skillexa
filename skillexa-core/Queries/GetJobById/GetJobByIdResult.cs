namespace Skillexa.Core.Queries.GetJobById;

public record GetJobByIdResult(
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

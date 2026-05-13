namespace Skillexa.Core.Commands.UpdateDocumentStatus;

public record UpdateDocumentStatusCommand(
    long DocumentId,
    string Status,
    string? PdfStorageKey,
    string? SnapshotStorageKey,
    string? ErrorCode,
    string? ErrorMessage) : ICommand<UpdateDocumentStatusResult>;

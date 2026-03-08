namespace Skillexa.Engine.Commands.ProcessGeneratePdf;

public record ProcessGeneratePdfResult(
    long JobId,
    string Status,
    string? PdfStorageKey = null,
    string? SnapshotStorageKey = null,
    string? ErrorCode = null,
    string? ErrorMessage = null);

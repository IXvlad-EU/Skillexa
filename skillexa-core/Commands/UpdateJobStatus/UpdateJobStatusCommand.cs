namespace Skillexa.Core.Commands.UpdateJobStatus;

public record UpdateJobStatusCommand(
    long JobId,
    string Status,
    string? PdfStorageKey,
    string? SnapshotStorageKey,
    string? ErrorCode,
    string? ErrorMessage) : ICommand<UpdateJobStatusResult>;

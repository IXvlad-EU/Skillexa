using Skillexa.Core.Data.UnitOfWork.Interfaces;
using Skillexa.Core.Domain;

namespace Skillexa.Core.Commands.UpdateDocumentStatus;

public class UpdateDocumentStatusHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateDocumentStatusCommand, UpdateDocumentStatusResult>
{
    private static readonly Dictionary<string, int> StatusMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Queued"] = DocumentStatuses.Queued,
        ["Processing"] = DocumentStatuses.Processing,
        ["Succeeded"] = DocumentStatuses.Succeeded,
        ["Failed"] = DocumentStatuses.Failed,
    };

    public async Task<UpdateDocumentStatusResult> HandleAsync(
        UpdateDocumentStatusCommand command, CancellationToken cancellationToken = default)
    {
        var document = await unitOfWork.Documents.GetByIdAsync(command.DocumentId, cancellationToken)
            ?? throw new InvalidOperationException($"Document '{command.DocumentId}' not found.");

        if (!StatusMap.TryGetValue(command.Status, out var statusId))
        {
            throw new InvalidOperationException($"Unknown document status '{command.Status}'.");
        }

        document.StatusId = statusId;
        document.PdfStorageKey = command.PdfStorageKey ?? document.PdfStorageKey;
        document.SnapshotStorageKey = command.SnapshotStorageKey ?? document.SnapshotStorageKey;
        document.ErrorCode = command.ErrorCode ?? document.ErrorCode;
        document.ErrorMessage = command.ErrorMessage ?? document.ErrorMessage;
        document.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateDocumentStatusResult(document.Id, command.Status);
    }
}

using Skillexa.Core.Data.UnitOfWork.Interfaces;
using Skillexa.Core.Domain;

namespace Skillexa.Core.Commands.UpdateJobStatus;

public class UpdateJobStatusHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateJobStatusCommand, UpdateJobStatusResult>
{
    private static readonly Dictionary<string, int> StatusMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Queued"] = JobStatuses.Queued,
        ["Processing"] = JobStatuses.Processing,
        ["Succeeded"] = JobStatuses.Succeeded,
        ["Failed"] = JobStatuses.Failed,
    };

    public async Task<UpdateJobStatusResult> HandleAsync(
        UpdateJobStatusCommand command, CancellationToken cancellationToken = default)
    {
        var job = await unitOfWork.Jobs.GetByIdAsync(command.JobId, cancellationToken)
            ?? throw new InvalidOperationException($"Job '{command.JobId}' not found.");

        if (!StatusMap.TryGetValue(command.Status, out var statusId))
        {
            throw new InvalidOperationException($"Unknown job status '{command.Status}'.");
        }

        job.StatusId = statusId;
        job.PdfStorageKey = command.PdfStorageKey ?? job.PdfStorageKey;
        job.SnapshotStorageKey = command.SnapshotStorageKey ?? job.SnapshotStorageKey;
        job.ErrorCode = command.ErrorCode ?? job.ErrorCode;
        job.ErrorMessage = command.ErrorMessage ?? job.ErrorMessage;
        job.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateJobStatusResult(job.Id, command.Status);
    }
}

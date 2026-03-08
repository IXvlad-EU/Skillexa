using Microsoft.Extensions.Logging;
using Skillexa.Engine.Data.UnitOfWork.Interfaces;

namespace Skillexa.Engine.Commands.ProcessGeneratePdf;

public class ProcessGeneratePdfHandler(
    IUnitOfWork unitOfWork,
    ILogger<ProcessGeneratePdfHandler> logger)
    : ICommandHandler<ProcessGeneratePdfCommand, ProcessGeneratePdfResult>
{
    public async Task<ProcessGeneratePdfResult> HandleAsync(
        ProcessGeneratePdfCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing GeneratePdf for JobId={JobId}, CorrelationId={CorrelationId}",
            command.JobId, command.CorrelationId);

        // 1. Check / decrement provider quota
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var quotaAvailable = await unitOfWork.ProviderQuotas.TryDecrementAsync(
            "theirstack", today, cancellationToken);

        if (!quotaAvailable)
        {
            logger.LogWarning(
                "Quota exhausted for provider 'theirstack' on {DayKey}, JobId={JobId}",
                today, command.JobId);

            return new ProcessGeneratePdfResult(
                command.JobId,
                Status: "Failed",
                ErrorCode: "QuotaExceeded",
                ErrorMessage: $"Daily quota for provider 'theirstack' exhausted on {today}.");
        }

        // 2. Load template
        var template = await unitOfWork.Templates.GetByKeyAndVersionAsync(
            command.TemplateKey, command.TemplateVersion, cancellationToken);

        if (template is null)
        {
            logger.LogError(
                "Template '{TemplateKey}' version {TemplateVersion} not found, JobId={JobId}",
                command.TemplateKey, command.TemplateVersion, command.JobId);

            return new ProcessGeneratePdfResult(
                command.JobId,
                Status: "Failed",
                ErrorCode: "TemplateNotFound",
                ErrorMessage: $"Template '{command.TemplateKey}' version {command.TemplateVersion} not found.");
        }

        // 3. TODO: Call TheirStack API (inject ITheirStackClient when service layer is implemented)
        logger.LogInformation(
            "TheirStack API call placeholder for JobId={JobId}", command.JobId);

        // 4. TODO: Render PDF (inject IPdfRenderingService when implemented)
        logger.LogInformation(
            "PDF rendering placeholder for JobId={JobId}", command.JobId);

        // 5. TODO: Upload artefacts to blob storage (inject IObjectStorage when implemented)
        var pdfStorageKey = $"pdf/{command.JobId}.pdf";
        var snapshotStorageKey = $"snapshots/{command.JobId}.json";
        logger.LogInformation(
            "Blob upload placeholder for JobId={JobId}, PdfKey={PdfKey}, SnapshotKey={SnapshotKey}",
            command.JobId, pdfStorageKey, snapshotStorageKey);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Successfully processed GeneratePdf for JobId={JobId}", command.JobId);

        return new ProcessGeneratePdfResult(
            command.JobId,
            Status: "Succeeded",
            PdfStorageKey: pdfStorageKey,
            SnapshotStorageKey: snapshotStorageKey);
    }
}

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
            "Processing GeneratePdf for DocumentId={DocumentId}, CorrelationId={CorrelationId}",
            command.DocumentId, command.CorrelationId);

        // 1. Check / decrement provider quota
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var quotaAvailable = await unitOfWork.ProviderQuotas.TryDecrementAsync(
            "theirstack", today, cancellationToken);

        if (!quotaAvailable)
        {
            logger.LogWarning(
                "Quota exhausted for provider 'theirstack' on {DayKey}, DocumentId={DocumentId}",
                today, command.DocumentId);

            return new ProcessGeneratePdfResult(
                command.DocumentId,
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
                "Template '{TemplateKey}' version {TemplateVersion} not found, DocumentId={DocumentId}",
                command.TemplateKey, command.TemplateVersion, command.DocumentId);

            return new ProcessGeneratePdfResult(
                command.DocumentId,
                Status: "Failed",
                ErrorCode: "TemplateNotFound",
                ErrorMessage: $"Template '{command.TemplateKey}' version {command.TemplateVersion} not found.");
        }

        // 3. TODO: Call TheirStack API (inject ITheirStackClient when service layer is implemented)
        logger.LogInformation(
            "TheirStack API call placeholder for DocumentId={DocumentId}", command.DocumentId);

        // 4. TODO: Render PDF (inject IPdfRenderingService when implemented)
        logger.LogInformation(
            "PDF rendering placeholder for DocumentId={DocumentId}", command.DocumentId);

        // 5. TODO: Upload artefacts to blob storage (inject IObjectStorage when implemented)
        var pdfStorageKey = $"pdf/{command.DocumentId}.pdf";
        var snapshotStorageKey = $"snapshots/{command.DocumentId}.json";
        logger.LogInformation(
            "Blob upload placeholder for DocumentId={DocumentId}, PdfKey={PdfKey}, SnapshotKey={SnapshotKey}",
            command.DocumentId, pdfStorageKey, snapshotStorageKey);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Successfully processed GeneratePdf for DocumentId={DocumentId}", command.DocumentId);

        return new ProcessGeneratePdfResult(
            command.DocumentId,
            Status: "Succeeded",
            PdfStorageKey: pdfStorageKey,
            SnapshotStorageKey: snapshotStorageKey);
    }
}

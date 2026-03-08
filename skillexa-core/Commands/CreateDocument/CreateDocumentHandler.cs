using System.Text.Json;
using Skillexa.Core.Data.UnitOfWork.Interfaces;
using Skillexa.Core.Domain;

namespace Skillexa.Core.Commands.CreateDocument;

public class CreateDocumentHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<CreateDocumentCommand, CreateDocumentResult>
{
    public async Task<CreateDocumentResult> HandleAsync(
        CreateDocumentCommand command, CancellationToken cancellationToken = default)
    {
        var correlationId = GenerateCorrelationId();
        var idempotencyKey = correlationId;

        if (await unitOfWork.Jobs.ExistsByIdempotencyKeyAsync(idempotencyKey, cancellationToken))
        {
            throw new InvalidOperationException($"A job with idempotency key '{idempotencyKey}' already exists.");
        }

        var templateVersion = command.TemplateVersion
            ?? (await unitOfWork.Templates.GetActiveByKeyAsync(command.TemplateKey, cancellationToken))?.Version
            ?? throw new InvalidOperationException($"No active template found for key '{command.TemplateKey}'.");

        var job = new Job
        {
            UserId = command.UserId,
            StatusId = JobStatuses.Queued,
            TemplateKey = command.TemplateKey,
            TemplateVersion = templateVersion,
            Payload = command.PayloadJson,
            CorrelationId = correlationId,
            IdempotencyKey = idempotencyKey,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await unitOfWork.Jobs.AddAsync(job, cancellationToken);

        var generatePdfPayload = new
        {
            messageType = "GeneratePdf",
            messageVersion = 1,
            jobId = job.Id,
            userId = command.UserId,
            templateKey = command.TemplateKey,
            templateVersion,
            payload = command.PayloadJson,
            correlationId,
            idempotencyKey,
        };

        await unitOfWork.OutboxMessages.AddAsync(new OutboxMessage
        {
            Type = "GeneratePdf",
            PayloadJson = JsonSerializer.Serialize(generatePdfPayload),
            CreatedAt = DateTime.UtcNow,
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateDocumentResult(job.Id, "Queued");
    }

    private static long GenerateCorrelationId()
    {
        return BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);
    }
}

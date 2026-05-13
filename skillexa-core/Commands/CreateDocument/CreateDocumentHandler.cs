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

        if (await unitOfWork.Documents.ExistsByIdempotencyKeyAsync(idempotencyKey, cancellationToken))
        {
            throw new InvalidOperationException($"A document with idempotency key '{idempotencyKey}' already exists.");
        }

        var templateVersion = command.TemplateVersion
            ?? (await unitOfWork.Templates.GetActiveByKeyAsync(command.TemplateKey, cancellationToken))?.Version
            ?? throw new InvalidOperationException($"No active template found for key '{command.TemplateKey}'.");

        var document = new Document
        {
            UserId = command.UserId,
            StatusId = DocumentStatuses.Queued,
            TemplateKey = command.TemplateKey,
            TemplateVersion = templateVersion,
            Payload = command.PayloadJson,
            CorrelationId = correlationId,
            IdempotencyKey = idempotencyKey,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await unitOfWork.Documents.AddAsync(document, cancellationToken);

        var generatePdfPayload = new
        {
            messageType = "GeneratePdf",
            messageVersion = 1,
            documentId = document.Id,
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

        return new CreateDocumentResult(document.Id, "Queued");
    }

    private static long GenerateCorrelationId()
    {
        return BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0);
    }
}

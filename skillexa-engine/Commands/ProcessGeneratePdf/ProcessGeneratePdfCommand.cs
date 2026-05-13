namespace Skillexa.Engine.Commands.ProcessGeneratePdf;

public record ProcessGeneratePdfCommand(
    long DocumentId,
    long UserId,
    string TemplateKey,
    int TemplateVersion,
    string PayloadJson,
    long CorrelationId,
    long IdempotencyKey) : ICommand<ProcessGeneratePdfResult>;

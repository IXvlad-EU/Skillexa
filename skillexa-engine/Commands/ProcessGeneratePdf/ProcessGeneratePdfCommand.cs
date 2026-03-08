namespace Skillexa.Engine.Commands.ProcessGeneratePdf;

public record ProcessGeneratePdfCommand(
    long JobId,
    long UserId,
    string TemplateKey,
    int TemplateVersion,
    string PayloadJson,
    long CorrelationId,
    long IdempotencyKey) : ICommand<ProcessGeneratePdfResult>;

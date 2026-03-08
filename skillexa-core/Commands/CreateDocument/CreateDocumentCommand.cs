namespace Skillexa.Core.Commands.CreateDocument;

public record CreateDocumentCommand(
    long UserId,
    string TemplateKey,
    int? TemplateVersion,
    string PayloadJson) : ICommand<CreateDocumentResult>;

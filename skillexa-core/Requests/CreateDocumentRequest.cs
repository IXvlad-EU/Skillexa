namespace Skillexa.Core.Requests;

public record CreateDocumentRequest(
    string TemplateKey,
    int? TemplateVersion,
    string PayloadJson);

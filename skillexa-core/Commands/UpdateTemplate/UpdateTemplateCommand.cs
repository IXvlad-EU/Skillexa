namespace Skillexa.Core.Commands.UpdateTemplate;

public record UpdateTemplateCommand(
    string TemplateKey,
    int Version,
    string Content) : ICommand<UpdateTemplateResult>;

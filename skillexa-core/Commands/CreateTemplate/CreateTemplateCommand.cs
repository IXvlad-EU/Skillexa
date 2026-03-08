namespace Skillexa.Core.Commands.CreateTemplate;

public record CreateTemplateCommand(
    string TemplateKey,
    string Content) : ICommand<CreateTemplateResult>;

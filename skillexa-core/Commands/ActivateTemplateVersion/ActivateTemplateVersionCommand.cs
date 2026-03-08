namespace Skillexa.Core.Commands.ActivateTemplateVersion;

public record ActivateTemplateVersionCommand(
    string TemplateKey,
    int Version) : ICommand<ActivateTemplateVersionResult>;

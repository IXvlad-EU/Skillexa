using Skillexa.Core.Data.UnitOfWork.Interfaces;

namespace Skillexa.Core.Commands.ActivateTemplateVersion;

public class ActivateTemplateVersionHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<ActivateTemplateVersionCommand, ActivateTemplateVersionResult>
{
    public async Task<ActivateTemplateVersionResult> HandleAsync(
        ActivateTemplateVersionCommand command, CancellationToken cancellationToken = default)
    {
        var template = await unitOfWork.Templates.GetByKeyAndVersionAsync(
                command.TemplateKey, command.Version, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Template '{command.TemplateKey}' version {command.Version} not found.");

        await unitOfWork.Templates.DeactivateAllVersionsAsync(command.TemplateKey, cancellationToken);

        template.IsActive = true;
        template.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ActivateTemplateVersionResult(template.Id, template.TemplateKey, template.Version);
    }
}

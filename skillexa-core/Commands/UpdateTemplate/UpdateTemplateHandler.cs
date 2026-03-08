using Skillexa.Core.Data.UnitOfWork.Interfaces;

namespace Skillexa.Core.Commands.UpdateTemplate;

public class UpdateTemplateHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateTemplateCommand, UpdateTemplateResult>
{
    public async Task<UpdateTemplateResult> HandleAsync(
        UpdateTemplateCommand command, CancellationToken cancellationToken = default)
    {
        var template = await unitOfWork.Templates.GetByKeyAndVersionAsync(
                command.TemplateKey, command.Version, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Template '{command.TemplateKey}' version {command.Version} not found.");

        template.Content = command.Content;
        template.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateTemplateResult(template.Id, template.TemplateKey, template.Version);
    }
}

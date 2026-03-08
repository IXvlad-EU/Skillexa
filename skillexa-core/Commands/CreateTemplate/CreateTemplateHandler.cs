using Skillexa.Core.Data.UnitOfWork.Interfaces;
using Skillexa.Core.Domain;

namespace Skillexa.Core.Commands.CreateTemplate;

public class CreateTemplateHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<CreateTemplateCommand, CreateTemplateResult>
{
    public async Task<CreateTemplateResult> HandleAsync(
        CreateTemplateCommand command, CancellationToken cancellationToken = default)
    {
        var nextVersion = await unitOfWork.Templates.GetNextVersionAsync(command.TemplateKey, cancellationToken);

        var template = new Template
        {
            TemplateKey = command.TemplateKey,
            Version = nextVersion,
            IsActive = false,
            Content = command.Content,
            UpdatedAt = DateTime.UtcNow,
        };

        await unitOfWork.Templates.AddAsync(template, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateTemplateResult(template.Id, template.TemplateKey, template.Version);
    }
}

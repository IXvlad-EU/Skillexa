using Skillexa.Core.Data.UnitOfWork.Interfaces;
using Skillexa.Core.Domain;

namespace Skillexa.Core.Commands.ProvisionUser;

public class ProvisionUserHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<ProvisionUserCommand, ProvisionUserResult>
{
    public async Task<ProvisionUserResult> HandleAsync(
        ProvisionUserCommand command, CancellationToken cancellationToken = default)
    {
        var existingUser = await unitOfWork.Users.GetByEntraIdAsync(command.EntraObjectId, cancellationToken);

        if (existingUser is not null)
        {
            return new ProvisionUserResult(existingUser.Id, IsNewUser: false);
        }

        var user = new User
        {
            EntraObjectId = command.EntraObjectId,
            Email = command.Email,
            DisplayName = command.DisplayName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await unitOfWork.Users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProvisionUserResult(user.Id, IsNewUser: true);
    }
}

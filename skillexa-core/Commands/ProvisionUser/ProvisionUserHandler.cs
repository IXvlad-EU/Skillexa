using Microsoft.EntityFrameworkCore;
using Skillexa.Core.Data.UnitOfWork.Interfaces;
using Skillexa.Core.Domain;

namespace Skillexa.Core.Commands.ProvisionUser;

public class ProvisionUserHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<ProvisionUserCommand, ProvisionUserResult>
{
    public async Task<ProvisionUserResult> HandleAsync(
        ProvisionUserCommand command, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(command.Email);
        var displayName = string.IsNullOrWhiteSpace(command.DisplayName)
            ? email
            : command.DisplayName.Trim();

        var existingUser = await unitOfWork.Users.GetByEmailAsync(email, cancellationToken);

        if (existingUser is not null)
        {
            return new ProvisionUserResult(existingUser.Id, IsNewUser: false);
        }

        var user = new User
        {
            Email = email,
            DisplayName = displayName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await unitOfWork.Users.AddAsync(user, cancellationToken);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            var concurrentlyCreatedUser = await unitOfWork.Users.GetByEmailAsync(email, cancellationToken);
            if (concurrentlyCreatedUser is not null)
            {
                return new ProvisionUserResult(concurrentlyCreatedUser.Id, IsNewUser: false);
            }

            throw;
        }

        return new ProvisionUserResult(user.Id, IsNewUser: true);
    }

    private static string NormalizeEmail(string email)
    {
        return string.IsNullOrWhiteSpace(email)
            ? throw new InvalidOperationException("Email is required for user provisioning.")
            : email.Trim().ToLowerInvariant();
    }
}

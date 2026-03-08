namespace Skillexa.Core.Commands.ProvisionUser;

public record ProvisionUserCommand(
    string EntraObjectId,
    string Email,
    string DisplayName) : ICommand<ProvisionUserResult>;

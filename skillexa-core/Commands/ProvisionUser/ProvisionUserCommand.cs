namespace Skillexa.Core.Commands.ProvisionUser;

public record ProvisionUserCommand(
    string Email,
    string DisplayName) : ICommand<ProvisionUserResult>;

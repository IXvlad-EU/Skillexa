// ── REPLACE: MyAction, MyActionResult, MyNamespace ───────────────────────────

namespace MyNamespace.Commands.MyAction;

// 1. Command — immutable record carrying the intent
public record MyActionCommand(
    long UserId
    /* add parameters */) : ICommand<MyActionResult>;

// 2. Result — thin DTO, never a domain entity
public record MyActionResult(long Id /* , ... */);

// 3. Handler — single unit of work, single commit
public sealed class MyActionHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<MyActionCommand, MyActionResult>
{
    public async Task<MyActionResult> HandleAsync(
        MyActionCommand command, CancellationToken cancellationToken = default)
    {
        // Validate inputs
        ArgumentNullException.ThrowIfNull(command);

        // Build domain entity
        var entity = new MyEntity
        {
            UserId    = command.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        // Stage entity via repository
        await unitOfWork.MyEntities.AddAsync(entity, cancellationToken);

        // Stage outbox message for async side-effects (if needed)
        // await unitOfWork.OutboxMessages.AddAsync(new OutboxMessage { ... }, cancellationToken);

        // Atomic commit — all writes in one transaction
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new MyActionResult(entity.Id);
    }
}

namespace Skillexa.Core.Domain;

/// <summary>
/// Well-known document status IDs matching the seeded <see cref="DocumentStatus"/> lookup table.
/// </summary>
public static class DocumentStatuses
{
    public const int Queued = 1;
    public const int Processing = 2;
    public const int Succeeded = 3;
    public const int Failed = 4;
}

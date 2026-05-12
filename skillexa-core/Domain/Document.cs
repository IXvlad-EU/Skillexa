namespace Skillexa.Core.Domain;

public class Document : IEntity
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public int StatusId { get; set; }

    public string TemplateKey { get; set; } = string.Empty;

    public int TemplateVersion { get; set; }

    public string Payload { get; set; } = "{}";

    public string? PdfStorageKey { get; set; }

    public string? SnapshotStorageKey { get; set; }

    public string? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }

    public long CorrelationId { get; set; }

    public long IdempotencyKey { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;

    public DocumentStatus Status { get; set; } = null!;
}

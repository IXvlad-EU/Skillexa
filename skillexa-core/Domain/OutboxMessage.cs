namespace Skillexa.Core.Domain;

public class OutboxMessage : IEntity
{
    public long Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }
}

namespace Skillexa.Core.Domain;

public class Template : IEntity
{
    public long Id { get; set; }

    public string TemplateKey { get; set; } = string.Empty;

    public int Version { get; set; }

    public bool IsActive { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; }
}

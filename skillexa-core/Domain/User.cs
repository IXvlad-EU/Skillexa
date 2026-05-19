namespace Skillexa.Core.Domain;

public class User : IEntity
{
    public long Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<Document> Documents { get; set; } = [];
}

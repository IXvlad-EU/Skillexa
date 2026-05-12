namespace Skillexa.Core.Domain;

public class DocumentStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<Document> Documents { get; set; } = [];
}

namespace Skillexa.Core.Domain;

public class JobStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<Job> Jobs { get; set; } = [];
}

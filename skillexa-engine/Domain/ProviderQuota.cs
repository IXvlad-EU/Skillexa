namespace Skillexa.Engine.Domain;

public class ProviderQuota : IEntity
{
    public long Id { get; set; }

    public string Provider { get; set; } = string.Empty;

    public DateOnly DayKey { get; set; }

    public int Used { get; set; }

    public int Limit { get; set; }

    public DateTime UpdatedAt { get; set; }
}

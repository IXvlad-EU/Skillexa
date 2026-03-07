namespace Skillexa.Core.Domain;

public class ProviderUsage : IEntity
{
    public long Id { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string PeriodKey { get; set; } = string.Empty;

    public int Used { get; set; }

    public int Remaining { get; set; }

    public DateTime UpdatedAt { get; set; }
}

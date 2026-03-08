namespace Skillexa.Engine.Queries.GetProviderQuota;

public record GetProviderQuotaResult(
    long Id,
    string Provider,
    DateOnly DayKey,
    int Used,
    int Limit);

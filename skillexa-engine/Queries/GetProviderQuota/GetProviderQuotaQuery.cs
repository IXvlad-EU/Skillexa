namespace Skillexa.Engine.Queries.GetProviderQuota;

public record GetProviderQuotaQuery(
    string Provider,
    DateOnly DayKey) : IQuery<GetProviderQuotaResult?>;

namespace Skillexa.Core.Queries.GetUsage;

public record GetUsageResult(
    string Provider,
    string PeriodKey,
    int Used,
    int Remaining);

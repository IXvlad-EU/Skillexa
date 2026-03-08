namespace Skillexa.Core.Queries.GetJobs;

public record GetJobsResult(
    long Id,
    string Status,
    string TemplateKey,
    string? ErrorCode,
    DateTime CreatedAt,
    DateTime UpdatedAt);

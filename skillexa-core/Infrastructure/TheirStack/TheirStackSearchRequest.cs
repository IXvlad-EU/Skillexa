using System.Text.Json.Serialization;

namespace Skillexa.Core.Infrastructure.TheirStack;

public record TheirStackSearchRequest
{
    [JsonPropertyName("job_technology_slug_or")]
    public string[] JobTechnologySlugOr { get; init; } = [];

    [JsonPropertyName("url_domain_or")]
    public string[] UrlDomainOr { get; init; } = [];

    [JsonPropertyName("page")]
    public int Page { get; init; }

    [JsonPropertyName("limit")]
    public int Limit { get; init; }

    [JsonPropertyName("posted_at_max_age_days")]
    public int PostedAtMaxAgeDays { get; init; } = 30;
}

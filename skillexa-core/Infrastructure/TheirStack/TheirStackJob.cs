using System.Text.Json.Serialization;

namespace Skillexa.Core.Infrastructure.TheirStack;

public record TheirStackJob
{
    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("job_title")]
    public string JobTitle { get; init; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; init; } = string.Empty;

    [JsonPropertyName("source_url")]
    public string? SourceUrl { get; init; }

    [JsonPropertyName("company")]
    public string? Company { get; init; }

    [JsonPropertyName("remote")]
    public bool Remote { get; init; }

    [JsonPropertyName("salary_string")]
    public string? SalaryString { get; init; }

    [JsonPropertyName("technology_slugs")]
    public string[] TechnologySlugs { get; init; } = [];

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("short_location")]
    public string? ShortLocation { get; init; }

    [JsonPropertyName("date_posted")]
    public string DatePosted { get; init; } = string.Empty;

    [JsonPropertyName("company_object")]
    public TheirStackCompany? CompanyObject { get; init; }
}

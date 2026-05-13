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

    [JsonPropertyName("job_title_or")]
    public string[] JobTitleOr { get; init; } = [];

    [JsonPropertyName("job_description_contains_or")]
    public string[] JobDescriptionContainsOr { get; init; } = [];

    [JsonPropertyName("remote")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Remote { get; init; }

    [JsonPropertyName("job_seniority_or")]
    public string[] JobSeniorityOr { get; init; } = [];

    [JsonPropertyName("employment_statuses_or")]
    public string[] EmploymentStatusesOr { get; init; } = [];

    [JsonPropertyName("job_country_code_or")]
    public string[] JobCountryCodeOr { get; init; } = [];

    [JsonPropertyName("min_salary_usd")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? MinSalaryUsd { get; init; }

    [JsonPropertyName("max_salary_usd")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? MaxSalaryUsd { get; init; }

    [JsonPropertyName("company_name_partial_match_or")]
    public string[] CompanyNamePartialMatchOr { get; init; } = [];
}

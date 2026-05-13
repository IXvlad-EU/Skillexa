using Skillexa.Core.Queries;

namespace Skillexa.Core.Queries.SearchJobListings;

public record SearchJobListingsQuery(
    string[] Skills,
    string[] SourceDomains,
    int Page,
    int PageSize,
    string[]? JobTitles,
    string[]? DescriptionKeywords,
    bool? Remote,
    string[]? Seniorities,
    string[]? EmploymentTypes,
    string[]? Countries,
    decimal? MinSalaryUsd,
    decimal? MaxSalaryUsd,
    int PostedWithinDays,
    string[]? CompanyNames) : IQuery<IReadOnlyList<SearchJobListingsResult>>;

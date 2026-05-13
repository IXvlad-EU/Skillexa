namespace Skillexa.Core.Queries.SearchJobListings;

public record SearchJobListingsResult(
    long Id,
    string Title,
    string CompanyName,
    string? CompanyLogoUrl,
    string? Location,
    bool Remote,
    string? SalaryString,
    string[] TechnologySlugs,
    string? Description,
    string Url,
    string? SourceUrl,
    string DatePosted);

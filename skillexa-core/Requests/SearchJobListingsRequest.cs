namespace Skillexa.Core.Requests;

public record SearchJobListingsRequest(
    string[] Skills,
    string[] SourceDomains,
    int Page = 0,
    int PageSize = 25,
    string[]? JobTitles = null,
    string[]? DescriptionKeywords = null,
    bool? Remote = null,
    string[]? Seniorities = null,
    string[]? EmploymentTypes = null,
    string[]? Countries = null,
    decimal? MinSalaryUsd = null,
    decimal? MaxSalaryUsd = null,
    int PostedWithinDays = 30,
    string[]? CompanyNames = null);

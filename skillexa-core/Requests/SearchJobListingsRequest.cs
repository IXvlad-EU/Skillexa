namespace Skillexa.Core.Requests;

public record SearchJobListingsRequest(
    string[] Skills,
    string[] SourceDomains,
    int Page = 0,
    int PageSize = 25);

using Skillexa.Core.Queries;

namespace Skillexa.Core.Queries.SearchJobListings;

public record SearchJobListingsQuery(
    string[] Skills,
    string[] SourceDomains,
    int Page,
    int PageSize) : IQuery<IReadOnlyList<SearchJobListingsResult>>;

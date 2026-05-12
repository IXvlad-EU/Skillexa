using Skillexa.Core.Infrastructure.TheirStack;

namespace Skillexa.Core.Queries.SearchJobListings;

public class SearchJobListingsHandler(ITheirStackClient client)
    : IQueryHandler<SearchJobListingsQuery, IReadOnlyList<SearchJobListingsResult>>
{
    public async Task<IReadOnlyList<SearchJobListingsResult>> HandleAsync(
        SearchJobListingsQuery query, CancellationToken cancellationToken = default)
    {
        var request = new TheirStackSearchRequest
        {
            JobTechnologySlugOr = query.Skills,
            UrlDomainOr = query.SourceDomains,
            Page = query.Page,
            Limit = query.PageSize,
            PostedAtMaxAgeDays = 30,
        };

        var response = await client.SearchAsync(request, cancellationToken);

        return response.Data
            .Select(job => new SearchJobListingsResult(
                job.Id,
                job.JobTitle,
                job.CompanyObject?.Name ?? job.Company ?? string.Empty,
                job.CompanyObject?.Logo,
                job.ShortLocation,
                job.Remote,
                job.SalaryString,
                job.TechnologySlugs,
                job.Description,
                job.Url,
                job.SourceUrl,
                job.DatePosted))
            .ToList();
    }
}

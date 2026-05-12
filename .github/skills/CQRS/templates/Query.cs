// ── REPLACE: MyQuery, MyResult, MyNamespace ───────────────────────────────────

namespace MyNamespace.Queries.MyQuery;

// 1. Query — immutable record carrying filter / pagination params
public record MyQueryQuery(
    long UserId,
    int Page     = 1,
    int PageSize = 20) : IQuery<IReadOnlyList<MyQueryResult>>;

// 2. Result — read-optimized DTO, never a domain entity
public record MyQueryResult(
    long     Id,
    string   Status,
    DateTime CreatedAt
    /* , ... */);

// 3. Handler — read-only, AsNoTracking, no SaveChangesAsync
public sealed class MyQueryHandler(ApplicationDbContext dbContext)
    : IQueryHandler<MyQueryQuery, IReadOnlyList<MyQueryResult>>
{
    public async Task<IReadOnlyList<MyQueryResult>> HandleAsync(
        MyQueryQuery query, CancellationToken cancellationToken = default)
    {
        return await dbContext.MyEntities
            .AsNoTracking()
            .Where(e => e.UserId == query.UserId)
            .OrderByDescending(e => e.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(e => new MyQueryResult(
                e.Id,
                e.Status.Name,
                e.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}

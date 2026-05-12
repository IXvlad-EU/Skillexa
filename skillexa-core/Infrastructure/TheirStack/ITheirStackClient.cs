namespace Skillexa.Core.Infrastructure.TheirStack;

public interface ITheirStackClient
{
    Task<TheirStackSearchResponse> SearchAsync(
        TheirStackSearchRequest request, CancellationToken cancellationToken = default);
}

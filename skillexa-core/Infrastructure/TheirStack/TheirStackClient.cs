using System.Net.Http.Json;
using System.Text.Json;

namespace Skillexa.Core.Infrastructure.TheirStack;

public class TheirStackClient(HttpClient httpClient) : ITheirStackClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public async Task<TheirStackSearchResponse> SearchAsync(
        TheirStackSearchRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync(
            "/v1/jobs/search", request, SerializerOptions, cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<TheirStackSearchResponse>(
            SerializerOptions, cancellationToken);

        return result!;
    }
}

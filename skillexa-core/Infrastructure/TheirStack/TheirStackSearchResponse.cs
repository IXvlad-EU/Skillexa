using System.Text.Json.Serialization;

namespace Skillexa.Core.Infrastructure.TheirStack;

public record TheirStackSearchResponse
{
    [JsonPropertyName("metadata")]
    public TheirStackMetadata Metadata { get; init; } = default!;

    [JsonPropertyName("data")]
    public IReadOnlyList<TheirStackJob> Data { get; init; } = [];
}

public record TheirStackMetadata
{
    [JsonPropertyName("total_results")]
    public int TotalResults { get; init; }

    [JsonPropertyName("truncated_results")]
    public int TruncatedResults { get; init; }

    [JsonPropertyName("truncated_companies")]
    public int TruncatedCompanies { get; init; }

    [JsonPropertyName("total_companies")]
    public int TotalCompanies { get; init; }
}

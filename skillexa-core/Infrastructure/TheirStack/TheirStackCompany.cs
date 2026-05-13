using System.Text.Json.Serialization;

namespace Skillexa.Core.Infrastructure.TheirStack;

public record TheirStackCompany
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("logo")]
    public string? Logo { get; init; }
}

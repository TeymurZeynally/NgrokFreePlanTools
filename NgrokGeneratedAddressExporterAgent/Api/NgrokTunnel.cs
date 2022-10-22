using System.Text.Json.Serialization;

namespace NgrokGeneratedAddressExporterAgent.Api;

public class NgrokTunnel
{
    [JsonPropertyName("public_url")]
    public string PublicUrl { get; init; }
}
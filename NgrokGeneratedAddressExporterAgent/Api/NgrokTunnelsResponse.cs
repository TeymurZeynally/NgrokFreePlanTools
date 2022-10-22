using System.Text.Json.Serialization;

namespace NgrokGeneratedAddressExporterAgent.Api;

public class NgrokTunnelsResponse
{
    [JsonPropertyName("tunnels")]
    public NgrokTunnel[] Tunnels { get; set; }
}
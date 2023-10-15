using System.Text.Json.Serialization;

namespace poise.services.Authentication.Jwk.Models;

public record AzureJwksResponseModel
{
	[JsonPropertyName("keys")]
	public List<AzureJwkEntryResponseModel> Keys { get; set; }
}
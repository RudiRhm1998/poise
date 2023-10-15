using System.Text.Json.Serialization;

namespace poise.services.Authentication.Jwk.Models;

public record AzureJwkEntryResponseModel
{
	[JsonPropertyName("kty")]
	public string Kty { get; set; }

	[JsonPropertyName("use")]
	public string Use { get; set; }

	[JsonPropertyName("kid")]
	public string Kid { get; set; }

	[JsonPropertyName("x5t")]
	public string X5T { get; set; }

	[JsonPropertyName("n")]
	public string N { get; set; }

	[JsonPropertyName("e")]
	public string E { get; set; }

	[JsonPropertyName("x5c")]
	public List<string> X5C { get; set; }
}
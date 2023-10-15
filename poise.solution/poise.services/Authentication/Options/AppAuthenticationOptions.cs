using System.ComponentModel.DataAnnotations;

namespace poise.services.Authentication.Options;

public class AppAuthenticationOptions
{
	[Required]
	[MinLength(16)]
	public string JwtSecret { get; set; }

	[Required]
	public string AzureAdClientId { get; set; }

	[Required]
	public string AzureAdTenantId { get; set; }

	[Required]
	public string AzureAdAuthRedirect { get; set; }
}
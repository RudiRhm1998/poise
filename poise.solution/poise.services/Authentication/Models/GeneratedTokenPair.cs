namespace poise.services.Authentication.Models;

public record GeneratedTokenPair
{
	public string AuthToken { get; set; }
	public string? RefreshToken { get; set; }
}
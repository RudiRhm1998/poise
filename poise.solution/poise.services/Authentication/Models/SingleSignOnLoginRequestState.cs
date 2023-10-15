namespace poise.services.Authentication.Models;

public record SingleSignOnLoginRequestState
{
	public DateTimeOffset ValidUntilUtc { get; set; }
	public string Nonce { get; set; }
}
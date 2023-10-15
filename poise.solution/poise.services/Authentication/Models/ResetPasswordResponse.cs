namespace poise.services.Authentication.Models;

public class ResetPasswordResponse
{
	public string Token { get; set; }
	public long UserId { get; set; }
}
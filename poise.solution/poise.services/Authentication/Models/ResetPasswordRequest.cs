namespace poise.services.Authentication.Models;

public class ResetPasswordRequest
{
	public long UserId { get; set; }
	public string Token { get; set; }
	public string NewPassword { get; set; }
}
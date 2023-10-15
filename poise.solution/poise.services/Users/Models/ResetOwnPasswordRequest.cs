using System.ComponentModel.DataAnnotations;

namespace poise.services.Users.Models;

public class ResetOwnPasswordRequest
{
	public long? UserId { get; set; }

	[EmailAddress]
	public string? Email { get; set; }
}
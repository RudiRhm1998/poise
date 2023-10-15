using System.ComponentModel.DataAnnotations;

namespace poise.services.Users.Models;

public class NewUserCommunicationOptions
{
	[EmailAddress]
	[Required]
	public string FromEmail { get; set; }

	[Required]
	public string FromName { get; set; }
}
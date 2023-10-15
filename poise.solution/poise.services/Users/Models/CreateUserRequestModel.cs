using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using poise.services.Users.Exceptions;

namespace poise.services.Users.Models;

public class CreateUserRequestModel
{
	[MinLength(1)]
	[MaxLength(256)]
	public string Username { get; set; }
	public string Email { get; set; }
	public List<long> Teams { get; set; }
	public long RoleId { get; set; }

	public void Validate()
	{
		try { _ = new MailAddress(this.Email); } catch { throw new InvalidEmailException(); }
	}
}
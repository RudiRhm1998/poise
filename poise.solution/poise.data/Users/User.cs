using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace poise.data.Users;

public class User : IdentityUser<long>
{
	public string? AzureIdUniqueIdentifier { get; set; }
	[MinLength(1)]
	[MaxLength(256)]
	public string DisplayName { get; set; }
	public long? RoleId { get; set; }
	public Role? Role { get; set; }
	public bool MustResetPassword { get; set; }
}

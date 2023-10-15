using System.ComponentModel.DataAnnotations;

namespace poise.services.Users.Models;

public class UpdateUserRequestModel
{
	public List<long>? Teams { get; set; }
	public long? RoleId { get; set; }
	[MinLength(1)]
	[MaxLength(256)]
	public string? DisplayName { get; set; }
}
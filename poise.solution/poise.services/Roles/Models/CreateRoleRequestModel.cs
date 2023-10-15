using System.ComponentModel.DataAnnotations;

namespace poise.services.Roles.Models;

public class CreateRoleRequestModel
{
	[MinLength(1)]
	[MaxLength(256)]
	public string Name { get; set; }
}
using System.ComponentModel.DataAnnotations.Schema;

namespace poise.data.Users;

[NotMapped]
public record RoleCheckResult
{
	public bool Result { get; set; }
}
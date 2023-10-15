using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace poise.data.Users;

public class Role
{
	public long Id { get; set; }
	[MinLength(1)]
	[MaxLength(256)]
	public string Name { get; set; }

	public BitArray PermissionBitmap { get; set; } = new(256);

	public List<User> Users { get; set; }
}
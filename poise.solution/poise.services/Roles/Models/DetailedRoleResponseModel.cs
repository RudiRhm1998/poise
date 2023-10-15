using poise.services.Users.Models;

namespace poise.services.Roles.Models;

public class DetailedRoleResponseModel : RoleResponseModel
{
	public string PermissionBitmap { get; set; }
	public List<UserResponseModel> Users { get; set; }
}
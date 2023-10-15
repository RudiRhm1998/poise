using poise.services.Roles.Models;

namespace poise.services.Roles;

public interface IRoleService
{
	Task<bool> IsUserAuthorizedForActionAsync(long userId, params Permissions[] permissions);
	Task<List<RoleResponseModel>> QuickListAsync();
	Task CreateAsync(CreateRoleRequestModel body);
	Task Update(long id, UpdateRoleRequestModel body);
	Task UpdatePermission(long roleId, UpdateRolePermissionModel body);
	Task DeleteAsync(long id);
}
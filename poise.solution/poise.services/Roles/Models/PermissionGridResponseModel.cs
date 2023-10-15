namespace poise.services.Roles.Models;

public class PermissionGridResponseModel
{
	public string ActionName { get; set; }
	public string GroupName { get; set; }
	public int PermissionId { get; set; }

	public List<RoleAssignmentResponseModel> RoleAssignment { get; set; }
}
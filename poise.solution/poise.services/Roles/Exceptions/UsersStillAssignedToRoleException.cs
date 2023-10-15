using poise.services.API;

namespace poise.services.Roles.Exceptions;

public class UsersStillAssignedToRoleException : ApiException
{
	public UsersStillAssignedToRoleException(long id) : base(new ApiErrorResponse
	{
		Code = "USERS_STILL_ASSIGNED_TO_ROLE",
		Reason = $"There are still users assigned to role with id {id}. Please remove them first before continuing this action"
	}, 418)
	{
	}
}
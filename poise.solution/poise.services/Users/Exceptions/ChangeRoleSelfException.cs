using poise.services.API;

namespace poise.services.Users.Exceptions;

public class ChangeRoleSelfException : ApiException
{
	public ChangeRoleSelfException() : base(new ApiErrorResponse
	{
		Code = "CHANGE_ROLE_SELF",
		Reason = "You are not allowed to change your own role. Ask another admin to do this action for you"
	}, 418)
	{
	}
}
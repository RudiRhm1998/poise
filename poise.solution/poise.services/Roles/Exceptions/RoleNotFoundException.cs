using poise.services.API;

namespace poise.services.Roles.Exceptions;

public class RoleNotFoundException : ApiException
{
	public RoleNotFoundException(long id) : base(new ApiErrorResponse
	{
		Code = "ROLE_NOT_FOUND",
		Reason = $"Role with id {id} could not be found..."
	}, 418)
	{
	}
}
using poise.services.API;

namespace poise.services.Users.Exceptions;

public class UserNotFoundException : ApiException
{
	public UserNotFoundException(long id) : base(new ApiErrorResponse
	{
		Code = "USER_NOT_FOUND",
		Reason = $"User with id {id} could not be found..."
	}, 418)
	{
	}

	public UserNotFoundException(string email) : base(new ApiErrorResponse
	{
		Code = "USER_NOT_FOUND",
		Reason = $"User with email {email} could not be found..."
	})
	{
	}
}
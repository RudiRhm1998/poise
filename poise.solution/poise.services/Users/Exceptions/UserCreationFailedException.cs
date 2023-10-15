using poise.services.API;

namespace poise.services.Users.Exceptions;

public class UserCreationFailedException : ApiException
{
	public UserCreationFailedException() : base(new ApiErrorResponse() { Code = "USER_CREATE_ERROR" }, 418, false)
	{
	}
}
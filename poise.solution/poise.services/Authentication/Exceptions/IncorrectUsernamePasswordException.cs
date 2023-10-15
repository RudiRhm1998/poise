using poise.services.API;

namespace poise.services.Authentication.Exceptions;

public class IncorrectUsernamePasswordException : ApiException
{
	public IncorrectUsernamePasswordException() : base(new ApiErrorResponse()
	{
		Code = "NON_MATCHING_USERNAME_PASSWORD",
		Reason = "Username and password do not match"
	}, 418, false)
	{
	}
}
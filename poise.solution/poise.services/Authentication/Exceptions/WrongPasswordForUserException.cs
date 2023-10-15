using poise.services.API;

namespace poise.services.Authentication.Exceptions;

public class WrongPasswordForUserException : ApiException
{
	public WrongPasswordForUserException(string? reason = null) : base(new ApiErrorResponse()
	{
		Code = "WRONG_PASS_FOR_USER",
		Reason = reason ?? "The password is invalid."
	}, 418, false)
	{
	}
}
using poise.services.API;

namespace poise.services.Authentication.Exceptions;

public class InvalidPasswordException : ApiException
{
	public InvalidPasswordException(string? reason = null) : base(new ApiErrorResponse()
	{
		Code = "INVALID_PASSWORD",
		Reason = reason ?? "The password is invalid."
	}, 418, false)
	{
	}
}
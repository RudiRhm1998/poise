using poise.services.API;

namespace poise.services.Authentication.Exceptions;

public class InvalidPasswordResetTokenException : ApiException
{
	public InvalidPasswordResetTokenException() : base(new ApiErrorResponse()
	{
		Code = "INVALID_PASS_RESET_TOKEN",
		Reason = "Token is malformed or expired."
	}, 418, false)
	{
	}
}
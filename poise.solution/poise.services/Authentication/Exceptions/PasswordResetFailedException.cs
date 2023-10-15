using poise.services.API;

namespace poise.services.Authentication.Exceptions;

public class PasswordResetFailedException : ApiException
{
	public PasswordResetFailedException() : base(new ApiErrorResponse()
	{
		Code = "PASS_RESET_FAILED",
		Reason = "The password reset has failed."
	}, 418, false)
	{
	}
}
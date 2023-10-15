using poise.services.API;

namespace poise.services.Authentication.Exceptions;

public class MustResetPasswordException : ApiException
{
	public MustResetPasswordException() : base(new ApiErrorResponse()
	{
		Code = "MUST_RESET_PASSWORD",
		Reason = "User must reset password"
	}, 418, false)
	{
	}
}
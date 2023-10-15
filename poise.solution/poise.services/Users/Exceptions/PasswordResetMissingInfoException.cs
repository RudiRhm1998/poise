using poise.services.API;

namespace poise.services.Users.Exceptions;

public class PasswordResetMissingInfoException : ApiException
{
	public PasswordResetMissingInfoException() : base(new ApiErrorResponse
	{
		Code = "RESET_REQUEST_MISSING_INFO",
		Reason = "Submitted info misses both email and id"
	}, 418)
	{
	}
}
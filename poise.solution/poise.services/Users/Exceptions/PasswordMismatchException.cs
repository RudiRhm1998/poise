using poise.services.API;

namespace poise.services.Users.Exceptions;

public class PasswordMismatchException : ApiException
{
	public PasswordMismatchException() : base(new ApiErrorResponse
	{
		Code = "PASSWORD_MISMATCH",
		Reason = "Submitted passwords do not match"
	}, 418)
	{
	}
}
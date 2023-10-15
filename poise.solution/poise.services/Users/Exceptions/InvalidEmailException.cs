using poise.services.API;

namespace poise.services.Users.Exceptions;

public class InvalidEmailException : ApiException
{
	public InvalidEmailException() : base(new ApiErrorResponse
	{
		Code = "INVALID_EMAIL",
		Reason = "Email is not valid"
	}, 418)
	{
	}
}
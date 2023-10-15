using poise.services.API;

namespace poise.services.Users.Exceptions;

public class CustomerMissingException : ApiException
{
	public CustomerMissingException() : base(new ApiErrorResponse
	{
		Code = "CUSTOMER_MISSING_EXCEPTION",
		Reason = "Not all received customers could be found..."
	}, 418)
	{
	}
}
using poise.services.API;

namespace poise.services.Authentication.Exceptions;

public class AttemptPasswordResetOnAzureAccountException : ApiException
{
	public AttemptPasswordResetOnAzureAccountException() : base(new ApiErrorResponse()
	{
		Code = "NO_AZURE_AD_PASS_RESET",
		Reason = "Azure AD created users cannot reset their password."
	}, 418, false)
	{
	}
}
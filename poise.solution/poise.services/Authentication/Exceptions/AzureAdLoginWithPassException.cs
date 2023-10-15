using poise.services.API;

namespace poise.services.Authentication.Exceptions;

public class AzureAdLoginWithPassException : ApiException
{
	public AzureAdLoginWithPassException() : base(new ApiErrorResponse()
	{
		Code = "NO_AZURE_AD_LOGIN_WITH_PASS",
		Reason = "Azure AD created users cannot login with a password."
	}, 418, false)
	{
	}
}
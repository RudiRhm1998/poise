using Microsoft.AspNetCore.Http;
using poise.services.Authentication.Models;

namespace poise.services.Authentication;

public interface IAppAuthenticationService
{
	string RequestLogin();

	Task<IList<AuthCookieResponse>> ProcessLoginCallback(Dictionary<string, string> aadForm, HttpRequest request);

	Task<AuthCookieResponse> HandleTokenRefresh(string refreshToken, HttpRequest request);

	Task<UserProfileResponseModel> GetSelf();

	CookieOptions GetDefaultCookieOptions(HttpRequest request, double? expires = null, bool refreshToken = false);

	Task<IList<AuthCookieResponse>> Login(LoginRequestModel model, HttpRequest request);

	Task<IList<AuthCookieResponse>> ResetPassword(long userId, string token, string password, HttpRequest request);
}
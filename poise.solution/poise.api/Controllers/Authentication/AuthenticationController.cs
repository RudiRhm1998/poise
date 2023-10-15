using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using poise.Middleware;
using poise.services.Authentication;
using poise.services.Authentication.Models;

namespace poise.Controllers.Authentication;

[ApiController]
[Route("api/auth")]
[Authorize]
public class AuthenticationController : ControllerBase
{
	private readonly IAppAuthenticationService _appAuthenticationService;
	public AuthenticationController(IAppAuthenticationService appAuthenticationService)
	{
		_appAuthenticationService = appAuthenticationService;
	}

	[HttpPost("login")]
	[AllowAnonymous]
	public async Task<IActionResult> Login([FromBody] LoginRequestModel model)
	{
		var result = await _appAuthenticationService.Login(model, HttpContext.Request);

		foreach (var cookie in result)
		{
			Response.Cookies.Append(cookie.Key, cookie.Value, cookie.Options);
		}

		return Ok();
	}

	[HttpGet("login-azure")]
	[AllowAnonymous]
	public IActionResult RequestLogin()
	{
		var redirectUrl = _appAuthenticationService.RequestLogin();
		return Redirect(redirectUrl);
	}

	[HttpPost("refresh")]
	[AllowAnonymous]
	public async Task<IActionResult> RefreshToken()
	{
		if (Request.Cookies.TryGetValue(AppAuthenticationService.RefreshTokenCookieName, out var rToken))
		{
			try
			{
				var refreshToken = await _appAuthenticationService.HandleTokenRefresh(rToken, Request);
				Response.Cookies.Append(refreshToken.Key, refreshToken.Value, refreshToken.Options);

				return Ok();
			}
			catch
			{
				return Unauthorized();
			}
		}
		return Unauthorized();
	}

	[HttpPost("sso-return")]
	[AllowAnonymous]
	public async Task<IActionResult> LoginCallback([FromForm] Dictionary<string, string> aadForm)
	{
		var tokenPair = await _appAuthenticationService.ProcessLoginCallback(aadForm, Request);
		foreach (var cookie in tokenPair)
		{
			Response.Cookies.Append(cookie.Key, cookie.Value, cookie.Options);
		}
		return Redirect("/");
	}

	[HttpGet("logout")]
	[AllowAnonymous]
	public IActionResult RequestLogout()
	{
		Response.Cookies.Delete(AppAuthenticationService.AuthTokenCookieName,
			_appAuthenticationService.GetDefaultCookieOptions(Request));
		Response.Cookies.Delete(AppAuthenticationService.RefreshTokenCookieName,
			_appAuthenticationService.GetDefaultCookieOptions(Request, null, true));
		return Ok();
	}

	[HttpGet("me")]
	[Authorize]
	[UserId]
	public async Task<IActionResult> WhoAmI()
	{
		var me = await _appAuthenticationService.GetSelf();
		return Ok(me);
	}

	[HttpPost("reset-password")]
	[AllowAnonymous]
	public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
	{
		var result =
			await _appAuthenticationService.ResetPassword(request.UserId, request.Token, request.NewPassword,
				HttpContext.Request);

		foreach (var cookie in result)
		{
			Response.Cookies.Append(cookie.Key, cookie.Value, cookie.Options);
		}
		return Ok();
	}
}
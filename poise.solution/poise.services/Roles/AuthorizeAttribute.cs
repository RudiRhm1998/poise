using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using poise.services.API;
using poise.services.Authentication;

namespace poise.services.Roles;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAsyncActionFilter
{
	public readonly Permissions[] Scopes;

	public AuthorizeAttribute(params Permissions[] scopes)
	{
		Scopes = scopes;
	}

	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		var jwt = context.HttpContext.Request.Cookies[AppAuthenticationService.AuthTokenCookieName];
		if (string.IsNullOrEmpty(jwt))
		{
			context.Result = new ContentResult
			{
				StatusCode = StatusCodes.Status401Unauthorized,
				ContentType = "application/json",
				Content = JsonSerializer.Serialize(new ApiResponse<object>
				{
					Error = new ApiErrorResponse
					{
						Code = "AUTHORIZATION_FAILED",
						Reason = "No Authorization token found..."
					},
					Data = null,
					Success = false
				})
			};

			return;
		}

		var userId = GetUserId(context.HttpContext.User);
		if (!userId.HasValue)
		{
			context.Result = new ContentResult
			{
				StatusCode = StatusCodes.Status401Unauthorized,
				ContentType = "application/json",
				Content = JsonSerializer.Serialize(new ApiResponse<object>
				{
					Error = new ApiErrorResponse
					{
						Code = "AUTHORIZATION_FAILED",
						Reason = "No user principal found..."
					},
					Data = null,
					Success = false
				})
			};

			return;
		}

		var roleService = context.HttpContext.RequestServices.GetRequiredService<IRoleService>();
		if (!await roleService.IsUserAuthorizedForActionAsync(userId.Value, Scopes))
		{
			context.Result = new ContentResult
			{
				StatusCode = StatusCodes.Status403Forbidden,
				ContentType = "application/json",
				Content = JsonSerializer.Serialize(new ApiResponse<object>
				{
					Error = new ApiErrorResponse
					{
						Code = "FORBIDDEN",
						Reason = "Action is not allowed for this user..."
					},
					Data = null,
					Success = false
				})
			};

			return;
		}

		await next();
	}

	private long? GetUserId(ClaimsPrincipal User)
	{
		return long.TryParse(User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value ?? "none", out var uid)
			? uid
			: null;
	}
}
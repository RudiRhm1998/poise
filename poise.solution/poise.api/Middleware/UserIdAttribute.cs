using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using poise.services.Utils.Middleware;

namespace poise.Middleware;

[AttributeUsage(AttributeTargets.Method)]
public class UserIdAttribute : Attribute, IAsyncActionFilter
{
	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		var jwt = await context.HttpContext.GetTokenAsync("access_token");
		if (string.IsNullOrEmpty(jwt))
		{
			context.Result = new ContentResult()
			{
				StatusCode = StatusCodes.Status401Unauthorized,
			};
		}
		else
		{
			var handler = new JwtSecurityTokenHandler();
			var tData = handler.ReadJwtToken(jwt);
			if (long.TryParse(tData.Claims.FirstOrDefault(x => x.Type == "nameid")?.Value ?? "none", out var userId))
			{
				var userService = context.HttpContext.RequestServices.GetRequiredService<CurrentUserService>();
				userService.UserId = userId;
				await next();
			}
			else
			{
				context.Result = new ContentResult()
				{
					StatusCode = StatusCodes.Status401Unauthorized,
				};
			}
		}
	}
}
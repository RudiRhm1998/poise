using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;
using poise.services.API;
using Serilog;

namespace poise.Middleware;

public static class ExceptionMiddlewareModule
{
	public static void SetupExceptionHandler(WebApplication app)
	{
		app.UseExceptionHandler(err =>
		{
			err.Run(async ctx =>
			{
				var exception = ctx.Features.Get<IExceptionHandlerPathFeature>()?.Error;
				if (exception is ApiException apiException)
				{
					if (apiException.WriteErrorLog)
					{
						// Some errors we want to be alerted to - but usually not.
						Log.Logger.Error(apiException,
							$"{apiException.ErrorResponse?.Code}: {apiException?.ErrorResponse?.Reason}");
					}
					else
					{
						// We don't have alerting on Warnings.
						Log.Logger.Warning(apiException,
							$"{apiException.ErrorResponse?.Code}: {apiException?.ErrorResponse?.Reason}");
					}

					ctx.Response.StatusCode = apiException.ResponseCode;
					await ctx.Response.WriteAsJsonAsync(new ApiResponse<object>
					{
						Error = apiException.ErrorResponse,
						Data = null,
						Success = false
					});
				}
				else
				{
					Log.Logger.Error(exception, $"An unhandled exception was thrown. Returning 500: {exception?.Message}");

					ctx.Response.StatusCode = 500;
					await ctx.Response.WriteAsJsonAsync(new ApiResponse<object>
					{
						Error = new ApiErrorResponse
						{
							Code = "UNEXPECTED_ERROR",
#if DEBUG
							Reason = "An unexpected error occured\n\n" + exception?.Message + "\n\n" + exception?.StackTrace
#else
							Reason = "An unexpected error occured - please contact us"
#endif
						},
						Success = false,
						Data = null
					});
				}
			});
		});
	}
}

public class ValidationErrorExceptionFilter : IExceptionFilter
{
	public void OnException(ExceptionContext context)
	{
		if (context.Exception is ValidationException exc)
		{
			var tException = new ApiValidationException(exc.Errors);
			context.ExceptionHandled = true;
			context.HttpContext.Response.StatusCode = tException.ResponseCode;
			context.HttpContext.Response.WriteAsJsonAsync(new ApiResponse<object>
			{
				Data = null,
				Error = tException.ErrorResponse,
				Success = false
			});
		}
	}
}
using System.Net;
using FluentValidation.Results;

namespace poise.services.API;

public class ApiErrorResponse
{
	public string Code { get; set; } = string.Empty;
	public string Reason { get; set; } = string.Empty;
	public object? Data { get; init; }
}

public class ApiValidationException : ApiException
{
	public ApiValidationException(IEnumerable<ValidationFailure> result)
		: base(new ApiErrorResponse
		{
			Code = "VALIDATION_ERRORS",
			Reason = "",
			Data = result.Select(x => new ApiValidationErrorModel
			{
				Property = x.PropertyName,
				Code = x.ErrorCode,
				Error = x.ErrorMessage
			})
		}, (int) HttpStatusCode.UnprocessableEntity)
	{
	}
}

public record ApiValidationErrorModel
{
	public string Property { get; set; }
	public string Code { get; set; }
	public string Error { get; set; }
}
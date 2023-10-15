namespace poise.services.API;

public class ApiResponse<T>
{
	public bool Success { get; init; } = true;
	public ApiErrorResponse? Error { get; set; }
	public T? Data { get; set; }

	public static ApiResponse<T> GenericOkayResponse => new()
	{
		Success = true,
		Data = default,
		Error = null
	};
}

using System.Net;

namespace poise.services.API;

public class ApiException : Exception
{
	public ApiException(ApiErrorResponse errorResponse,
		int responseCode = (int)HttpStatusCode.BadRequest,
		bool writeErrorLog = false)
	{
		ErrorResponse = errorResponse;
		ResponseCode = responseCode;
		WriteErrorLog = writeErrorLog;
	}

	public ApiErrorResponse ErrorResponse { get; }
	public int ResponseCode { get; }
	public bool WriteErrorLog { get; }
}

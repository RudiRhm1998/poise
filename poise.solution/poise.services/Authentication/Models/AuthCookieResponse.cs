using Microsoft.AspNetCore.Http;

namespace poise.services.Authentication.Models;

public record AuthCookieResponse
{
	public string Key { get; set; }
	public string Value { get; set; }
	public CookieOptions Options { get; set; }
}
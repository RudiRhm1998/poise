using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using poise.services.Authentication.Jwk.Models;
using poise.services.Authentication.Options;

namespace poise.services.Authentication.Jwk;

public class AzureJwkStore : IAzureJwkStore
{
	private readonly AppAuthenticationOptions _authenticationOptions;
	private readonly HttpClient _httpClient;
	private readonly ILogger<AzureJwkStore> _logger;
	private readonly SemaphoreSlim _refreshLock = new(1, 1);

	public AzureJwkStore(IOptions<AppAuthenticationOptions> authenticationOptions, HttpClient httpClient,
		ILogger<AzureJwkStore> logger)
	{
		_httpClient = httpClient;
		_logger = logger;
		_authenticationOptions = authenticationOptions.Value;
	}
	private AzureJwksResponseModel? _keys { get; set; }
	private DateTimeOffset LastRetrievedOn { get; set; } = DateTimeOffset.MinValue;

	public async Task<bool> ValidateTokenWithAzureKeys(string token)
	{
		var result = false;

		try
		{
			if ((DateTimeOffset.UtcNow - LastRetrievedOn).TotalSeconds > TimeSpan.FromHours(1).TotalSeconds
				|| (!_keys?.Keys.Any() ?? true))
			{
				await RefreshJwks();
			}

			var tokenHandler = new JwtSecurityTokenHandler();
			var dToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
			var matchingJwk = _keys.Keys.FirstOrDefault(x => x.Kid == dToken?.Header.Kid);
			if (matchingJwk != null)
			{
				tokenHandler.ValidateToken(token, new TokenValidationParameters
				{
					ValidateLifetime = true,
					ValidateIssuer = true,
					ValidateIssuerSigningKey = true,
					IssuerSigningKeys = _keys.Keys.Select(x => new JsonWebKey(JsonSerializer.Serialize(x))),
					ValidateAudience = true,
					ValidAudiences = new[] { _authenticationOptions.AzureAdClientId },
					ValidIssuers = new[] { $"https://login.microsoftonline.com/{_authenticationOptions.AzureAdTenantId}/v2.0" }
				}, out var _validatedToken);

				result = DateTimeOffset.UtcNow < _validatedToken.ValidTo;
			}
			else
			{
				result = false;
			}
		}
		catch
		{
			result = false;
		}

		return result;
	}

	private async Task RefreshJwks()
	{

		if (await _refreshLock.WaitAsync(TimeSpan.FromSeconds(30)))
		{
			try
			{
				var url =
					$"https://login.microsoftonline.com/{_authenticationOptions.AzureAdTenantId}/discovery/keys?appid={_authenticationOptions.AzureAdClientId}";
				if ((DateTimeOffset.UtcNow - LastRetrievedOn).TotalSeconds > 300)
				{
					var result = await _httpClient.GetAsync(url);
					var resultBody = await result.Content.ReadAsStringAsync();
					var keys = JsonSerializer.Deserialize<AzureJwksResponseModel>(resultBody);

					_keys = keys;
					LastRetrievedOn = DateTimeOffset.UtcNow;
					_logger.LogInformation("Refreshed JWK cache");
				}
			}
			finally
			{
				_refreshLock.Release();
			}
		}
	}
}
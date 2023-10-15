namespace poise.services.Authentication.Jwk;

public interface IAzureJwkStore
{
	Task<bool> ValidateTokenWithAzureKeys(string token);
}
namespace poise.services.Authentication.Models;

public record UserProfileResponseModel
{
	public long Id { get; set; }
	public string Name { get; set; }
	public string Email { get; set; }
	public string PermissionsMap { get; set; }
	public bool IsAzureAccount { get; set; }
}
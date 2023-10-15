namespace poise.services.Users.Models;

public class UserResponseModel
{
	public long Id { get; set; }
	public string DisplayName { get; set; }
	public string Email { get; set; }
	public string RoleName { get; set; }
}
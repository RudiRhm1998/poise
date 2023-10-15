using poise.services.Users.Models;

namespace poise.services.Users;

public interface IUserService
{
	Task<List<UserResponseModel>> QuickListAsync();
	Task<DetailedUserResponseModel> GetAsync(long id);
	Task UpdateAsync(UpdateUserRequestModel body, long? id);
	Task CreateAsync(CreateUserRequestModel createRequestModel);
	Task RequestResetPassword(long userId);
	Task RequestResetPassword(string email);
}
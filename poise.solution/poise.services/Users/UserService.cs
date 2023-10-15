using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using poise.data.Context;
using poise.data.Users;
using poise.services.Authentication.Exceptions;
using poise.services.Roles.Exceptions;
using poise.services.Users.Exceptions;
using poise.services.Users.Models;
using poise.services.Utils.Helper;
using poise.services.Utils.Middleware;

namespace poise.services.Users;

public class UserService : IUserService
{
	private readonly PoiseContext _context;
	private readonly CurrentUserService _currentUserService;
	private readonly UserManager<User> _userManager;
	private readonly ILogger<UserService> _logger;

	public UserService(PoiseContext context, UserManager<User> userManager, ILogger<UserService> logger,
		CurrentUserService currentUserService)
	{
		_context = context;
		_userManager = userManager;
		_logger = logger;
		_currentUserService = currentUserService;
	}

	public Task<List<UserResponseModel>> QuickListAsync()
	{
		return _context.Users.Select(x => new UserResponseModel
		{
			Id = x.Id,
			DisplayName = x.DisplayName,
			Email = x.Email ?? string.Empty
		}).ToListAsync();
	}

	public async Task<DetailedUserResponseModel> GetAsync(long id)
	{
		var user = await _context.Users
			.Select(x => new DetailedUserResponseModel
			{
				Id = x.Id,
				DisplayName = x.DisplayName,
				RoleId = x.RoleId ?? -1,
				Email = x.Email ?? string.Empty,
			})
			.FirstOrDefaultAsync(x => x.Id == id);
		if (user == null)
		{
			throw new UserNotFoundException(id);
		}

		return user;
	}

	public async Task UpdateAsync(UpdateUserRequestModel body, long? id)
	{
		var userId = id ?? _currentUserService.UserId;
		var user = await _context.Users.AsTracking().FirstOrDefaultAsync(x => x.Id == userId);
		if (user == null)
		{
			throw new UserNotFoundException(userId);
		}

		if (body.RoleId.HasValue)
		{
			if (user.Id == _currentUserService.UserId && user.RoleId != body.RoleId.Value)
			{
				throw new ChangeRoleSelfException();
			}

			var role = await _context.AuthorizationRoles.FirstOrDefaultAsync(x => x.Id == body.RoleId);
			if (role == null)
			{
				throw new RoleNotFoundException(body.RoleId.Value);
			}
			user.RoleId = body.RoleId;
		}

		if (!body.DisplayName.IsNullOrEmpty())
		{
			user.DisplayName = body.DisplayName!;
		}

		await _context.SaveChangesAsync();
	}

	public async Task CreateAsync(CreateUserRequestModel createRequestModel)
	{
		createRequestModel.Validate();
		var role = await _context.AuthorizationRoles.FirstOrDefaultAsync(x => x.Id == createRequestModel.RoleId);
		if (role == default)
		{
			_logger.LogError($"Trying to create a user with non-existing role: {createRequestModel.RoleId}");
			throw new RoleNotFoundException(createRequestModel.RoleId);
		}

		var userToCreate = new User()
		{
			DisplayName = createRequestModel.Username,
			// Username has to be unique, and we only use displayName in the frontend anyways.
			UserName = Guid.NewGuid().ToString(),
			Email = createRequestModel.Email,
			RoleId = createRequestModel.RoleId,
			// First signin, create a password
			MustResetPassword = true
		};

		var identityResult = await _userManager.CreateAsync(userToCreate);

		if (identityResult.Succeeded)
		{
			// await _emailService.SendUserCreatedEmail(userToCreate, await _userManager.GeneratePasswordResetTokenAsync(userToCreate));
			_logger.LogInformation($"Created new user with id: {userToCreate.Id} and role {role.Name}");
		}
		else
		{
			_logger.LogError($"Failed to create user with email: {createRequestModel.Email}, reason: {JsonSerializer.Serialize(identityResult.Errors)}");
			throw new UserCreationFailedException();
		}
	}

	public async Task RequestResetPassword(long userId)
	{
		var sw = new Stopwatch();
		sw.Start();

		await AuthenticationHelpers.RandomDelayAsync(sw);

		var user = await _context.Users.AsTracking().FirstOrDefaultAsync(x => x.Id == userId);
		if (user == null)
		{
			// We don't want to leak information about the existence of a user.
			_logger.LogInformation($"Trying to reset password for non-existing user with id: {userId}");
			return;
		}

		await this.ResetPassword(user);
	}

	public async Task RequestResetPassword(string email)
	{
		var sw = new Stopwatch();
		sw.Start();

		var user = await _context.Users.AsTracking().FirstOrDefaultAsync(x => x.NormalizedEmail == email.ToUpper());
		await AuthenticationHelpers.RandomDelayAsync(sw);

		if (user == null)
		{
			// We don't want to leak information about the existence of a user.
			_logger.LogInformation($"Trying to reset password for non-existing user with email: {email}");
			return;
		}

		await this.ResetPassword(user);
	}

	private async Task ResetPassword(User user)
	{
		if (user.AzureIdUniqueIdentifier != null)
		{
			_logger.LogInformation($"Attempted to reset password for Azure user with id: {user.Id}");
			throw new AttemptPasswordResetOnAzureAccountException();
		}

		var token = await _userManager.GeneratePasswordResetTokenAsync(user);

		// await _emailService.SendPasswordResetEmail(user, token);
	}
}
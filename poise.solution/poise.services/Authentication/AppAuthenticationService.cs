using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using poise.services.Authentication.Exceptions;
using poise.services.Authentication.Jwk;
using poise.services.Authentication.Models;
using poise.services.Authentication.Options;
using poise.data.Context;
using poise.data.Users;
using poise.services.Roles.Extensions;
using poise.services.Users.Exceptions;
using poise.services.Utils.Helper;
using poise.services.Utils.Middleware;

namespace poise.services.Authentication;

public class AppAuthenticationService : IAppAuthenticationService
{
	public static readonly string AuthTokenCookieName = "X-Authentication-Token";
	public static readonly string RefreshTokenCookieName = "X-Refresh-Token";

	private static readonly int AuthTokenLifetimeSeconds = (int) TimeSpan.FromHours(1).TotalSeconds;
	private static readonly int RefreshTokenLifetimeSeconds = (int) TimeSpan.FromDays(7).TotalSeconds;

	private static readonly Random Random = new();

	private readonly IAzureJwkStore _azureJwkStore;
	private readonly PoiseContext _db;
	private readonly CurrentUserService _currentUserService;
	private readonly ILogger<AppAuthenticationService> _logger;

	private readonly ITimeLimitedDataProtector _loginStateProtector;
	private readonly AppAuthenticationOptions _options;
	private readonly UserManager<User> _userManager;

	public AppAuthenticationService(ILogger<AppAuthenticationService> logger, PoiseContext db,
		IOptions<AppAuthenticationOptions> options, IDataProtectionProvider dataProtectionProvider,
		UserManager<User> userManager, IAzureJwkStore azureJwkStore, CurrentUserService currentUserService)
	{
		_logger = logger;
		_db = db;
		_currentUserService = currentUserService;
		_userManager = userManager;
		_azureJwkStore = azureJwkStore;
		_options = options.Value;

		_loginStateProtector = dataProtectionProvider.CreateProtector("LoginState").ToTimeLimitedDataProtector();
	}

	public string RequestLogin()
	{
		var random = new Random();
		var nonce = random.Next(0, int.MaxValue);
		var state = new SingleSignOnLoginRequestState
		{
			ValidUntilUtc = DateTimeOffset.UtcNow.AddMinutes(15),
			Nonce = nonce.ToString()
		};
		var qb = new QueryBuilder
		{
			{
				"response_type", "id_token"
			},
			{
				"redirect_uri", _options.AzureAdAuthRedirect
			},
			{
				"response_mode", "form_post"
			},
			{
				"scope", "openid profile email"
			},
			{
				"nonce", nonce.ToString()
			},
			{
				"client_id", _options.AzureAdClientId
			},
			{
				"state", _loginStateProtector.Protect(JsonSerializer.Serialize(state), TimeSpan.FromMinutes(15))
			}
		};

		return $"https://login.microsoftonline.com/{_options.AzureAdTenantId}/oauth2/v2.0/authorize{qb}";
	}
	public async Task<IList<AuthCookieResponse>> ProcessLoginCallback(Dictionary<string, string> aadForm,
		HttpRequest request)
	{
		var stateSuccess = aadForm.TryGetValue("state", out var state);
		var idTokenSuccess = aadForm.TryGetValue("id_token", out var idToken);
		if (!stateSuccess || !idTokenSuccess)
		{
			_logger.LogError("User tried to login - but callback lacks state or idToken");
			throw new Exception("TODO: Nice error + Redirect");
		}

		var deserializedState =
			JsonSerializer.Deserialize<SingleSignOnLoginRequestState>(_loginStateProtector.Unprotect(state));
		var jwtHandler = new JwtSecurityTokenHandler();
		var token = jwtHandler.ReadToken(idToken) as JwtSecurityToken;

		var valid = await _azureJwkStore.ValidateTokenWithAzureKeys(idToken);
		if (!valid)
		{
			_logger.LogError("Could not validate provided token with any azure JWK");
			throw new Exception();
		}

		if (deserializedState == null)
		{
			_logger.LogError("Received state from single sign on callback, but could not unprotect/deserialize it!");
			throw new Exception("TODO");
		}
		if (deserializedState.ValidUntilUtc < DateTimeOffset.UtcNow)
		{
			_logger.LogWarning("Token is no longer valid! User most login again");
			throw new Exception("TODO");
		}

		// Do nonce check to prevent forgeries
		var tokenNonce = token.Claims.FirstOrDefault(x => x.Type == "nonce")?.Value;
		if (tokenNonce != deserializedState.Nonce)
		{
			_logger.LogError(
				$"Presented token has no nonce, or nonce did not match: {tokenNonce} != {deserializedState.Nonce}");
			throw new Exception("TODO");
		}

		// TODO: Check if nonce is a repeat!

		// Check if user already exists. If not, we will provision a new user with the appropriate role (if we can find one. Otherwise bounce the user?)
		var userAzureId = token.Claims.FirstOrDefault(x => x.Type == "sub")?.Value ?? string.Empty;
		if (string.IsNullOrWhiteSpace(userAzureId))
		{
			_logger.LogError("User token did not contain a sub claim! Cannot continue");
			throw new Exception("TODO");
		}

		var existingUser = await _db.Users.FirstOrDefaultAsync(x => x.AzureIdUniqueIdentifier == userAzureId);
		if (existingUser == null)
		{
			_logger.LogInformation(
				"User hit single sign on callback - but no user account has been issued yet. Issuing a new one");

			var desiredEmail = token.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
			var desiredFirstName = token.Claims.FirstOrDefault(x => x.Type == "given_name");
			var desiredFamilyName = token.Claims.FirstOrDefault(x => x.Type == "family_name");

			if (string.IsNullOrWhiteSpace(desiredEmail))
			{
				_logger.LogError("Did not get email claim from Azure AD login! Can't continue");
				throw new Exception("TODO");
			}

			if (new[] { desiredFirstName, desiredFamilyName }.Any(y => string.IsNullOrWhiteSpace(y?.Value)))
			{
				_logger.LogError("Did not get First name or Last name from AAD claims!");
				throw new Exception("TODO");
			}

			_logger.LogInformation($"Account will be issued for id = {userAzureId} and email = {desiredEmail}");

			existingUser = new User
			{
				Email = desiredEmail,
				NormalizedEmail = desiredEmail.ToUpperInvariant(),
				UserName = userAzureId,
				NormalizedUserName = userAzureId.ToUpperInvariant(),
				DisplayName = (desiredFirstName!.Value[0] + "." + desiredFamilyName!.Value).ToLowerInvariant(),
				AzureIdUniqueIdentifier = userAzureId,
				RoleId = 1 // Default role is "User"
			};

			var createResult = await _userManager.CreateAsync(existingUser);
			if (!createResult.Succeeded)
			{
				_logger.LogError("Could not issue a new user: " + JsonSerializer.Serialize(createResult.Errors));
				throw new Exception("TODO");
			}
		}

		_logger.LogDebug($"User logging in with existing user: {existingUser.Id}");

		var tokenPair = await GenerateTokenPair(existingUser, true);

		var aCookie = new AuthCookieResponse
		{
			Key = AuthTokenCookieName,
			Value = tokenPair.AuthToken,
			Options = GetDefaultCookieOptions(request, AuthTokenLifetimeSeconds)
		};

		var rCookie = new AuthCookieResponse
		{
			Key = RefreshTokenCookieName,
			Value = tokenPair.RefreshToken,
			Options = GetDefaultCookieOptions(request, RefreshTokenLifetimeSeconds, true)
		};

		return new[] { aCookie, rCookie };
	}

	public async Task<AuthCookieResponse> HandleTokenRefresh(string refreshToken, HttpRequest request)
	{
		var handler = new JwtSecurityTokenHandler();
		var token = handler.ReadToken(refreshToken) as JwtSecurityToken;

		var uId = token.Claims.FirstOrDefault(x => x.Type == "nameid")?.Value;
		if (string.IsNullOrWhiteSpace(uId) || !long.TryParse(uId, out var numericUId))
		{
			_logger.LogError("Users Refresh Token did not contain a nameid");
			// todo: clear auth & refresh token cookies for this user and let them re-auth
			throw new Exception("Missing nameid");
		}

		var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == numericUId);
		if (user == null)
		{
			_logger.LogError($"Could not find user with Id {numericUId} for refresh");
			throw new Exception();
		}

		var tokenPair = await GenerateTokenPair(user, false);

		var aCookie = new AuthCookieResponse
		{
			Key = AuthTokenCookieName,
			Value = tokenPair.AuthToken,
			Options = GetDefaultCookieOptions(request, AuthTokenLifetimeSeconds)
		};

		return aCookie;
	}

	public async Task<UserProfileResponseModel> GetSelf()
	{
		var user = await _db.Users
			.Select(x => new UserProfileResponseModel
			{
				Id = x.Id,
				Email = x.Email,
				Name = x.DisplayName,
				PermissionsMap = x.Role!.PermissionBitmap.ToBitArrayString(),
				IsAzureAccount = !x.AzureIdUniqueIdentifier.IsNullOrEmpty(),
			}).FirstOrDefaultAsync(x => x.Id == _currentUserService.UserId);

		if (user == null)
		{
			throw new UserNotFoundException(_currentUserService.UserId);
		}

		return user;
	}

	private async Task<GeneratedTokenPair> GenerateTokenPair(User user, bool withRefresh)
	{
		var handler = new JwtSecurityTokenHandler();
		var key = Encoding.ASCII.GetBytes(_options.JwtSecret);
		var maxDate = new DateTimeOffset(2038, 1, 19, 0, 0, 0, 0, TimeSpan.Zero).DateTime;

		var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, user.Id.ToString())
		};

		var descriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(claims),
			Audience = "SBJ.TimeTracker.UI",
			Issuer = "SBJ.TimeTracker.API",
			Expires = new[] { DateTime.UtcNow.AddSeconds(AuthTokenLifetimeSeconds), maxDate }.Min(),
			IssuedAt = DateTime.UtcNow,
			SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
		};

		var aToken = handler.WriteToken(handler.CreateToken(descriptor));
		string? rToken = null;
		if (withRefresh)
		{
			var refreshDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new Claim[]
				{
					new(ClaimTypes.Role, "Refresh"),
					new(ClaimTypes.NameIdentifier, user.Id.ToString())
				}),
				Audience = "SBJ.TimeTracker.UI",
				Issuer = "SBJ.TimeTracker.API",
				Expires = new[] { DateTime.UtcNow.AddSeconds(RefreshTokenLifetimeSeconds), maxDate }.Min(),
				IssuedAt = DateTime.UtcNow,
				SigningCredentials =
					new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
			};

			rToken = handler.WriteToken(handler.CreateToken(refreshDescriptor));
		}

		return new GeneratedTokenPair
		{
			AuthToken = aToken,
			RefreshToken = rToken
		};
	}

	public CookieOptions GetDefaultCookieOptions(HttpRequest request, double? expires = null, bool refreshToken = false)
	{
		var options = new CookieOptions
		{
			Secure = true,
			HttpOnly = true,
			Domain = request.Host.Host,
			Path = refreshToken ? "/api/auth/refresh" : "/",
			SameSite = SameSiteMode.Strict,
			IsEssential = true
		};

		if (expires.HasValue)
		{
			options.Expires = DateTimeOffset.UtcNow.AddSeconds(expires.Value);
		}

		return options;
	}
	public async Task<IList<AuthCookieResponse>> Login(LoginRequestModel model, HttpRequest request)
	{
		var sw = new Stopwatch();
		sw.Start();

		var user = await _userManager.FindByEmailAsync(model.Email);

		if (user == default)
		{
			_logger.LogInformation($"Failed log in attempt for non-existant user with email {model.Email}.");

			await AuthenticationHelpers.RandomDelayAsync(sw);

			throw new IncorrectUsernamePasswordException();
		}

		// Azure AD users can't log in with a password
		if (!user.AzureIdUniqueIdentifier.IsNullOrEmpty())
		{
			_logger.LogInformation($"Azure AD user with email {model.Email} tried to log in with a password.");

			await AuthenticationHelpers.RandomDelayAsync(sw);

			throw new AzureAdLoginWithPassException();
		}

		// If password is supplied, check it.
		if (!await _userManager.CheckPasswordAsync(user, model.Password))
		{
			_logger.LogInformation($"Failed log in attempt for user with email {model.Email}.");

			await AuthenticationHelpers.RandomDelayAsync(sw);

			throw new IncorrectUsernamePasswordException();
		}

		if (user.MustResetPassword)
		{
			await AuthenticationHelpers.RandomDelayAsync(sw);

			_logger.LogTrace($"User {user.Id} tried to log in, but has to reset their password first.");

			// Password is correct, but user must reset their password.
			throw new MustResetPasswordException();
		}

		return await GenerateAuthCookieResponsesForUser(user, request);
	}


	public async Task<IList<AuthCookieResponse>> ResetPassword(long userId, string token, string password, HttpRequest request)
	{
		// Decode token from base64.
		token = Base64Helpers.Base64Decode(token);

		var user = await _userManager.FindByIdAsync(userId.ToString());

		if (user == default)
		{
			throw new UserNotFoundException(userId);
		}

		if (!user.AzureIdUniqueIdentifier.IsNullOrEmpty())
		{
			_logger.LogInformation($"Azure AD user with email {user.Email} tried to reset their password.");
			throw new AttemptPasswordResetOnAzureAccountException();
		}

		var res = await _userManager.ResetPasswordAsync(user, token, password);
		if (res.Succeeded)
		{
			_logger.LogInformation($"User {user.Id} has reset their password");
			user.MustResetPassword = false;
			await _userManager.UpdateAsync(user);
		}
		else if (res.Errors.Any())
		{
			_logger.LogError($"Could not reset password for user {user.Id}: {JsonSerializer.Serialize(res.Errors)}");
			throw AuthenticationHelpers.PasswordResetErrorToException(res.Errors.First());
		}
		else
		{
			_logger.LogError($"Could not reset password for user {user.Id}: Unknown error");
			throw AuthenticationHelpers.PasswordResetErrorToException(null);
		}

		return await GenerateAuthCookieResponsesForUser(user, request);
	}

	private async Task<IList<AuthCookieResponse>> GenerateAuthCookieResponsesForUser(User user, HttpRequest request)
	{
		var tokenPair = await GenerateTokenPair(user, true);

		var aCookie = new AuthCookieResponse
		{
			Key = AuthTokenCookieName,
			Value = tokenPair.AuthToken,
			Options = GetDefaultCookieOptions(request, AuthTokenLifetimeSeconds)
		};

		var rCookie = new AuthCookieResponse
		{
			Key = RefreshTokenCookieName,
			Value = tokenPair.RefreshToken,
			Options = GetDefaultCookieOptions(request, RefreshTokenLifetimeSeconds, true)
		};

		return new[] { aCookie, rCookie };
	}
}
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using poise.services.Authentication.Exceptions;

namespace poise.services.Utils.Helper;

public static class AuthenticationHelpers
{
	private static readonly Random Random = new Random();

	public static Exception PasswordResetErrorToException(IdentityError? error)
	{
		// src: https://github.com/dotnet/aspnetcore/blob/main/src/Identity/Extensions.Core/src/IdentityErrorDescriber.cs
		return error?.Code switch
		{
			"PasswordToShort" => new InvalidPasswordException(error.Code),
			"PasswordRequiresDigit" => new InvalidPasswordException(error.Code),
			"PasswordRequiresLower" => new InvalidPasswordException(error.Code),
			"PasswordRequiresUpper" => new InvalidPasswordException(error.Code),
			"PasswordRequiresNonAlphanumeric" => new InvalidPasswordException(error.Code),
			"PasswordRequiresUniqueChars" => new InvalidPasswordException(error.Code),
			"InvalidToken" => new InvalidPasswordResetTokenException(),
			_ => new PasswordResetFailedException()
		};
	}

	public static async Task RandomDelayAsync(Stopwatch sw)
	{
		// Introduce random timing so there's no information disclosure whether this user exists or not
		sw.Stop();
		if (sw.ElapsedMilliseconds < 250)
		{
			var remaining = 250 - sw.ElapsedMilliseconds;
			await Task.Delay((int) remaining + Random.Next(25, 150));
		}
	}
}
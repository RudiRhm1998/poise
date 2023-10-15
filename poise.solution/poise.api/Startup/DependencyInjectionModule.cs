using System.Net;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using poise.data.Context;
using poise.data.Users;
using poise.services.API;
using poise.services.Authentication;
using poise.services.Authentication.Jwk;
using poise.services.Authentication.Options;
using poise.services.Roles;
using poise.services.Users;
using poise.services.Users.Models;
using poise.services.Utils.Middleware;

namespace poise.Startup;

public class DependencyInjectionModule
{
	public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddHttpClient();
		services.AddTransient<IUserService, UserService>();
		services.AddTransient<IRoleService, RoleService>();
		services.AddScoped<CurrentUserService>();
		services.AddSingleton<IAzureJwkStore, AzureJwkStore>();
		
		services.AddDataProtection()
			.PersistKeysToDbContext<PoiseContext>();
		services.AddIdentityCore<User>()
			.AddEntityFrameworkStores<PoiseContext>()
			.AddDefaultTokenProviders();
		
		services.AddTransient<IAppAuthenticationService, AppAuthenticationService>();
		
		services.AddAuthentication()
			.AddJwtBearer(options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ClockSkew = TimeSpan.Zero,
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidIssuer = "poise.api",
					ValidAudience = "poise.UI",
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Authentication:JwtSecret"]))
				};

				options.Events = new JwtBearerEvents
				{
					OnMessageReceived = context =>
					{
						if (context.Request.Cookies.ContainsKey(AppAuthenticationService.AuthTokenCookieName))
						{
							context.Token = context.Request.Cookies[AppAuthenticationService.AuthTokenCookieName];
						}
						return Task.CompletedTask;
					}
				};
			});
		services.Configure<IdentityOptions>(options =>
		{
			options.User.RequireUniqueEmail = true;
		});
	}
	
	public static void RegisterDatabase(IServiceCollection services, IConfiguration configuration)
	{
		// Add services to the container.
		var connectionString = configuration["Postgres:ConnectionString"];
		services.AddDbContext<PoiseContext>(options =>
		{
			var vMaj = configuration["Postgres:VersionMajor"];
			var vMin = configuration["Postgres:VersionMajor"];
			if (string.IsNullOrEmpty(vMaj) || string.IsNullOrEmpty(vMin) || string.IsNullOrEmpty(connectionString))
			{
				throw new NullReferenceException("Postgres connection string or version is not set");
			}

			options.UseNpgsql(connectionString,
					o =>
					{
						o.SetPostgresVersion(int.Parse(vMaj), int.Parse(vMin));
						o.EnableRetryOnFailure();
						o.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
					})
				.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
		});
	}
	
	public static void RegisterOptions(IServiceCollection services, IConfiguration configuration)
	{
		services.AddOptions<AppAuthenticationOptions>()
			.Bind(configuration.GetSection("Authentication"))
			.ValidateDataAnnotations()
			.ValidateOnStart();
		services
			.AddOptions<NewUserCommunicationOptions>()
			.Bind(configuration.GetSection("Email:NewUserCommunication"))
			.ValidateDataAnnotations()
			.ValidateOnStart();
		services
			.AddOptions<HostOptions>()
			.Bind(configuration.GetSection("HostOptions"))
			.ValidateDataAnnotations()
			.ValidateOnStart();
	}
	public static void RegisterValidationExceptionHandler(IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<ApiBehaviorOptions>(o =>
		{
			o.InvalidModelStateResponseFactory = context =>
			{
				context.HttpContext.Response.StatusCode = (int) HttpStatusCode.UnprocessableEntity;
				return new ObjectResult(new ApiResponse<object>
				{
					Success = false,
					Data = null,
					Error = new ApiErrorResponse
					{
						Code = "VALIDATION_ERRORS",
						Reason = "Model could not be validated, please see error data",
						Data = context.ModelState.Keys.Select(x => new
						{
							Field = x,
							Errors = context.ModelState.FirstOrDefault(y => y.Key == x).Value?.Errors.Select(y => y.ErrorMessage)
							         ?? new List<string>()
						})
					}
				});
			};
		});

	}
}

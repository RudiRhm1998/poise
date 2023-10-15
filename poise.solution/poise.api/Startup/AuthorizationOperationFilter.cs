using Microsoft.OpenApi.Models;
using poise.services.Roles;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace poise.Startup;

public class AuthorizationOperationFilter : IOperationFilter
{
	public void Apply(OpenApiOperation operation, OperationFilterContext ctx)
	{
		var attribute = (AuthorizeAttribute?) ctx.ApiDescription.CustomAttributes().FirstOrDefault(a => a is AuthorizeAttribute);
		if (attribute == null)
		{
			return;
		}

		operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
		operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

		var permissionList = attribute.Scopes
			.Select(permission => Enum.GetName(typeof(Permissions), permission) ?? string.Empty)
			.Where(x => !string.IsNullOrEmpty(x))
			.ToList();
		operation.Security.Add(new OpenApiSecurityRequirement
		{
			[new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			}] = permissionList
		});
	}
}
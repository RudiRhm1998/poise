using System.Collections;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using poise.data.Context;
using poise.data.Users;
using poise.services.Roles.Exceptions;
using poise.services.Roles.Extensions;
using poise.services.Roles.Models;
using poise.services.Users.Models;

namespace poise.services.Roles;

public class RoleService : IRoleService
{
	private PoiseContext _context;
	public RoleService(PoiseContext context)
	{
		_context = context;
	}

	public async Task<bool> IsUserAuthorizedForActionAsync(long userId, params Permissions[] permissions)
	{
		var bitCheck = new BitArray(256, false);
		foreach (var permission in permissions)
		{
			bitCheck.Set((int) permission, true);
		}

		var sql = @"SELECT ((AR.""PermissionBitmap""::bit(256) & @bitmapCheck::bit(256)) = @bitmapCheck::bit(256)) as Result from ""AspNetUsers"" as users
JOIN ""AuthorizationRoles"" AR on users.""RoleId"" = AR.""Id""
WHERE users.""Id"" = @userId";

		IList<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

		var userIdParameter = new NpgsqlParameter("@userId", userId);
		var bitmapParameter = new NpgsqlParameter("@bitmapCheck", bitCheck.ToBitArrayString());

		parameters.Add(userIdParameter);
		parameters.Add(bitmapParameter);

		var result = (await _context.RoleCheckResults
				.FromSqlRaw(sql, parameters.ToArray())
				.ToListAsync())
			.FirstOrDefault();

		return result?.Result ?? false;
	}
	
	public Task<List<RoleResponseModel>> QuickListAsync()
	{
		return _context.AuthorizationRoles.Select(x => new RoleResponseModel
		{
			Id = x.Id,
			Name = x.Name
		})
			.OrderBy(x => x.Id)
			.ToListAsync();
	}

	public async Task<DetailedRoleResponseModel> GetAsync(long id)
	{
		var role = await _context.AuthorizationRoles
			.Select(x => new DetailedRoleResponseModel
			{
				Id = x.Id,
				Name = x.Name,
				PermissionBitmap = x.PermissionBitmap.ToBitArrayString(),
				Users = x.Users.Select(y => new UserResponseModel
				{
					Id = y.Id,
					DisplayName = y.DisplayName
				}).ToList()
			})
			.FirstOrDefaultAsync(x => x.Id == id);

		if (role == null)
		{
			throw new RoleNotFoundException(id);
		}

		return role;
	}

	public async Task CreateAsync(CreateRoleRequestModel body)
	{
		var role = new Role
		{
			Name = body.Name,
			Users = new List<User>()
		};

		_context.AuthorizationRoles.Add(role);
		await _context.SaveChangesAsync();
	}

	public async Task Update(long id, UpdateRoleRequestModel body)
	{
		var role = await _context.AuthorizationRoles
			.AsTracking()
			.Include(x => x.Users)
			.FirstOrDefaultAsync(x => x.Id == id);

		if (role == null)
		{
			throw new RoleNotFoundException(id);
		}

		role.Name = body.Name;

		await _context.SaveChangesAsync();
	}

	public async Task DeleteAsync(long id)
	{
		var role = await _context.AuthorizationRoles
			.AsTracking()
			.Include(x => x.Users)
			.FirstOrDefaultAsync(x => x.Id == id);

		if (role == null)
		{
			throw new RoleNotFoundException(id);
		}
		if (role.Users.Count > 0)
		{
			throw new UsersStillAssignedToRoleException(id);
		}

		_context.AuthorizationRoles.Remove(role);
		await _context.SaveChangesAsync();
	}

	public async Task UpdatePermission(long roleId, UpdateRolePermissionModel body)
	{
		var role = await _context.AuthorizationRoles
			.AsTracking()
			.Include(x => x.Users)
			.FirstOrDefaultAsync(x => x.Id == roleId);

		if (role == null)
		{
			throw new RoleNotFoundException(roleId);
		}

		var startArray = (BitArray) role.PermissionBitmap.Clone();
		startArray.Set(body.PermissionId, body.Enabled);
		role.PermissionBitmap = startArray;

		await _context.SaveChangesAsync();
	}
}
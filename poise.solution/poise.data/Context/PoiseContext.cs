using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using poise.data.Users;

namespace poise.data.Context;

public class PoiseContext : IdentityDbContext<User, IdentityRole<long>, long>, IDataProtectionKeyContext
{
    public PoiseContext(DbContextOptions options) : base (options)
    {
        
    }
    public DbSet<Role> AuthorizationRoles { get; set; }
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    public DbSet<RoleCheckResult> RoleCheckResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<RoleCheckResult>()
            .HasNoKey();
        
        modelBuilder.Entity<User>();
    }
}
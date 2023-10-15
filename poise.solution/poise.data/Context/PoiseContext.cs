using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using poise.data.Users;

namespace poise.data.Context;

public class PoiseContext : IdentityDbContext<User, IdentityRole<long>, long>
{
    public PoiseContext(DbContextOptions options) : base (options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>();
    }
}
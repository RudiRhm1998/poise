using Microsoft.EntityFrameworkCore;

namespace poise.data.Context;

public class PoiseContext : DbContext
{
    public PoiseContext(DbContextOptions<PoiseContext> options) : base (options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }

    public DbSet<WeatherForecast> WeatherForecasts { get; set; }
}
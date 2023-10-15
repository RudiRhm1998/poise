using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace poise.data.Context;

public class PoiseContextFactory : IDesignTimeDbContextFactory<PoiseContext>
{
	public PoiseContext CreateDbContext(string[] args)
	{
		var optionsBuilder = new DbContextOptionsBuilder();
		optionsBuilder.UseNpgsql(
			"Host=localhost;Port=12651;Database=poise;Username=poise;Password=poise;Include Error Detail=true;Log Parameters=true",
			o =>
			{
				o.SetPostgresVersion(15, 3);
			});

		return new PoiseContext(optionsBuilder.Options);
	}
}

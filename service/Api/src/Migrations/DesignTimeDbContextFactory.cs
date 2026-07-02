using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using Module;

using Shared.Operational.Persistence.Data;

namespace Api.Migrations;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        var connectionString = "Host=localhost;Database=resys_shop;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.UseVector();
            npgsqlOptions.MigrationsAssembly("Api.Migrations");
        });

        optionsBuilder.UseSnakeCaseNamingConvention();

        ApplicationDbContext.AdditionalConfigurationsAssemblies =
        [
            typeof(IModuleMarker).Assembly
        ];

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}

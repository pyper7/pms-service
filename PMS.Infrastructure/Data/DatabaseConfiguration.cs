using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PMS.Infrastructure.Data;

public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseProvider = configuration["Database:Provider"]?.ToLowerInvariant() ?? "sqlserver";
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        switch (databaseProvider)
        {
            case "postgresql":
            case "postgres":
                services.AddDbContext<PmsDbContext>(options =>
                    options.UseNpgsql(connectionString, npgsqlOptions =>
                    {
                        npgsqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorCodesToAdd: null);
                    }));
                break;

            case "sqlserver":
            default:
                services.AddDbContext<PmsDbContext>(options =>
                    options.UseSqlServer(connectionString, sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    }));
                break;
        }

        return services;
    }

    public static string GetDatabaseProvider(this IConfiguration configuration)
    {
        return configuration["Database:Provider"]?.ToLowerInvariant() ?? "sqlserver";
    }

    public static bool IsPostgreSQL(this IConfiguration configuration)
    {
        var provider = configuration.GetDatabaseProvider();
        return provider == "postgresql" || provider == "postgres";
    }

    public static bool IsSqlServer(this IConfiguration configuration)
    {
        var provider = configuration.GetDatabaseProvider();
        return provider == "sqlserver";
    }
}

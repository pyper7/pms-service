using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.IO;

namespace PMS.Infrastructure.Data;

public class PmsDbContextFactory : IDesignTimeDbContextFactory<PmsDbContext>
{
    public PmsDbContext CreateDbContext(string[] args)
    {
        var envConn = System.Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        var connectionString = !string.IsNullOrWhiteSpace(envConn)
            ? envConn
            : "Server=(localdb)\\MSSQLLocalDB;Database=PmsDb;Trusted_Connection=True;MultipleActiveResultSets=true";

        var optionsBuilder = new DbContextOptionsBuilder<PmsDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new PmsDbContext(optionsBuilder.Options);
    }
}



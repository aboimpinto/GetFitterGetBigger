using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GetFitterGetBigger.API.Models;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FitnessDbContext>
{
    public FitnessDbContext CreateDbContext(string[] args)
    {
        // Build configuration
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        // Get connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Create DbContext options
        var optionsBuilder = new DbContextOptionsBuilder<FitnessDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new FitnessDbContext(optionsBuilder.Options);
    }
}

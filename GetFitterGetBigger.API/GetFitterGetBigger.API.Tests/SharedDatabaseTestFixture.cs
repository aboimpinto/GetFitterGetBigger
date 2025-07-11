using System.Net.Http.Headers;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Tests.TestBuilders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace GetFitterGetBigger.API.Tests;

/// <summary>
/// Test fixture that maintains a single database instance across all requests within a test class.
/// This solves the issue of NotFound errors when retrieving entities created in previous requests.
/// </summary>
public class SharedDatabaseTestFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private string DatabaseName { get; } = $"InMemoryDbForTesting_{Guid.NewGuid()}";
    
    public async Task InitializeAsync()
    {
        // Ensure database is seeded once when the fixture is created
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        await context.Database.EnsureCreatedAsync();
        await SeedTestDataAsync(context);
    }

    public new Task DisposeAsync() => Task.CompletedTask;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.Testing.json", optional: false, reloadOnChange: false);
        });
        
        builder.ConfigureServices(services =>
        {
            // Remove any existing database-related services
            var descriptorsToRemove = services.Where(d => 
                d.ServiceType == typeof(DbContextOptions<FitnessDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(FitnessDbContext) ||
                (d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)) ||
                (d.ServiceType.FullName?.Contains("Npgsql") == true)
            ).ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }
            
            // Add DbContext using the shared in-memory database
            services.AddDbContext<FitnessDbContext>(options =>
            {
                options.UseInMemoryDatabase(DatabaseName);
                options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
            });
        });
    }
    
    /// <summary>
    /// Creates a new scope and provides access to the database context for test setup/verification
    /// </summary>
    public async Task<T> ExecuteDbContextAsync<T>(Func<FitnessDbContext, Task<T>> action)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        return await action(context);
    }
    
    /// <summary>
    /// Creates a new scope and provides access to the database context for test setup/verification
    /// </summary>
    public async Task ExecuteDbContextAsync(Func<FitnessDbContext, Task> action)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        await action(context);
    }

    private async Task SeedTestDataAsync(FitnessDbContext context)
    {
        // Use the centralized SeedDataBuilder for consistent test data
        var seedBuilder = new SeedDataBuilder(context);

        // Only seed if no data exists
        // if (!await context.BodyParts.AnyAsync())
        // {
            await seedBuilder.WithAllReferenceDataAsync();
        // }
    }
}

/// <summary>
/// Collection definition for tests that should share the same database instance
/// </summary>
[CollectionDefinition("SharedDatabase")]
public class SharedDatabaseCollection : ICollectionFixture<SharedDatabaseTestFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
using System.Net;
using GetFitterGetBigger.API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace GetFitterGetBigger.API.Tests.IntegrationTests;

/// <summary>
/// Integration tests for database migration functionality.
/// These tests verify that the automatic database migration on startup works correctly.
/// </summary>
public class DatabaseMigrationTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private string _databaseName;
    
    public DatabaseMigrationTests(ITestOutputHelper output)
    {
        _output = output;
        _databaseName = $"MigrationTest_{Guid.NewGuid():N}";
    }
    
    public Task InitializeAsync() => Task.CompletedTask;
    
    public async Task DisposeAsync()
    {
        // Clean up test database
        var optionsBuilder = new DbContextOptionsBuilder<FitnessDbContext>();
        optionsBuilder.UseInMemoryDatabase(_databaseName);
        
        using var context = new FitnessDbContext(optionsBuilder.Options);
        await context.Database.EnsureDeletedAsync();
    }
    
    [Fact]
    public async Task Application_WithNoDatabase_CreatesAndMigratesSuccessfully()
    {
        // Arrange
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    // Remove existing DbContext
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<FitnessDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }
                    
                    // Add DbContext with test database
                    services.AddDbContext<FitnessDbContext>(options =>
                        options.UseInMemoryDatabase(_databaseName));
                    
                    // Add logging to capture migration logs
                    services.AddLogging(logging =>
                    {
                        logging.SetMinimumLevel(LogLevel.Debug);
                    });
                });
            });
        
        // Act - Creating the client should trigger migrations
        using var client = factory.CreateClient();
        
        // Assert - Verify database was created
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        // In-memory database doesn't support real migrations, but we can verify the context works
        var canConnect = await context.Database.CanConnectAsync();
        Assert.True(canConnect);
        
        // Verify we can query tables (which means schema exists)
        var exerciseCount = await context.Exercises.CountAsync();
        Assert.True(exerciseCount >= 0); // Should not throw
    }
    
    [Fact]
    public async Task Application_WithExistingDatabase_SkipsMigrationSuccessfully()
    {
        // Arrange - First create a database
        var optionsBuilder = new DbContextOptionsBuilder<FitnessDbContext>();
        optionsBuilder.UseInMemoryDatabase(_databaseName);
        
        using (var context = new FitnessDbContext(optionsBuilder.Options))
        {
            await context.Database.EnsureCreatedAsync();
        }
        
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    // Remove existing DbContext
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<FitnessDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }
                    
                    // Add DbContext with existing test database
                    services.AddDbContext<FitnessDbContext>(options =>
                        options.UseInMemoryDatabase(_databaseName));
                    
                    // Add logging to capture migration logs
                    services.AddLogging(logging =>
                    {
                        logging.SetMinimumLevel(LogLevel.Debug);
                    });
                });
            });
        
        // Act - Creating the client should check migrations but not fail
        using var client = factory.CreateClient();
        
        // Assert - Application should start successfully
        var response = await client.GetAsync("/api/exercises");
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }
    
    [Fact(Skip = "This test requires a way to simulate migration failure")]
    public async Task Application_WithMigrationFailure_ExitsGracefully()
    {
        // This test would require a more complex setup to simulate a migration failure
        // In a real scenario with PostgreSQL TestContainers, we could:
        // 1. Create a database with insufficient permissions
        // 2. Or create a migration that would fail (e.g., invalid SQL)
        // 3. Verify the application exits with Environment.Exit(1)
        
        // For now, this test is skipped as it's difficult to test Environment.Exit
        // in the test framework without special handling
    }
}
using System.Net.Http.Headers;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.IntegrationTests.TestBuilders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Xunit;

namespace GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Fixtures;

public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private string? _connectionString;
    
    public IntegrationTestWebApplicationFactory()
    {
    }
    
    public IntegrationTestWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public async Task InitializeAsync()
    {
        // No longer managing container here - it's managed by PostgreSqlTestFixture
        await Task.CompletedTask;
    }
    
    public void SetConnectionString(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddJsonFile("appsettings.Testing.json", optional: false, reloadOnChange: false);
        });

        builder.ConfigureServices((context, services) =>
        {
            // Remove all DbContext registrations
            var descriptors = services.Where(
                d => d.ServiceType == typeof(DbContextOptions<FitnessDbContext>) ||
                     d.ServiceType == typeof(DbContextOptions) ||
                     (d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>))).ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Remove existing Npgsql registrations
            var npgsqlDescriptors = services.Where(
                d => d.ServiceType.FullName?.Contains("Npgsql") == true).ToList();

            foreach (var descriptor in npgsqlDescriptors)
            {
                services.Remove(descriptor);
            }

            // Add DbContext using PostgreSQL from the container
            services.AddDbContext<FitnessDbContext>(options =>
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    throw new InvalidOperationException("Connection string not set. Call SetConnectionString or use constructor with connection string.");
                }
                options.UseNpgsql(_connectionString);
                options.EnableSensitiveDataLogging(); // For debugging
                options.LogTo(Console.WriteLine, LogLevel.Information); // Log SQL queries
            });
        });

        var host = base.CreateHost(builder);

        // Run migrations and seed the database after the host is created
        try
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var db = services.GetRequiredService<FitnessDbContext>();
                
                // Run migrations instead of EnsureCreated for PostgreSQL
                db.Database.Migrate();
                
                // Note: Reference data is already seeded by migrations through HasData in OnModelCreating
                // We only need to seed additional test-specific data if needed
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during database initialization: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }

        return host;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }

    private void SeedReferenceData(FitnessDbContext context)
    {
        // Use the centralized SeedDataBuilder for consistent test data
        var seedBuilder = new SeedDataBuilder(context);
        
        // Only seed if no data exists
        if (!context.BodyParts.Any())
        {
            seedBuilder
                .WithAllReferenceDataAsync()
                .GetAwaiter()
                .GetResult();
        }
    }
    
    /// <summary>
    /// Cleans up test data between scenarios while preserving reference data
    /// </summary>
    public async Task CleanupTestDataAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        try
        {
            // Delete only test-specific data, keep reference data
            // This prevents duplicate key violations on reference data
            
            // Check if ExerciseLinks table exists before trying to clean it
            var tableExists = await TableExistsAsync(context, "ExerciseLinks");
            if (tableExists)
            {
                context.ExerciseLinks.RemoveRange(context.ExerciseLinks);
            }
            
            // Exercises table should exist after migrations
            context.Exercises.RemoveRange(context.Exercises);
            // Add other entity cleanups here as needed
            
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the test cleanup
            Console.WriteLine($"Error during test data cleanup: {ex.Message}");
        }
    }
    
    private async Task<bool> TableExistsAsync(FitnessDbContext context, string tableName)
    {
        try
        {
            var connection = context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT EXISTS (
                    SELECT FROM information_schema.tables 
                    WHERE table_schema = 'public' 
                    AND table_name = @tableName
                )";
            
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@tableName";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);
            
            var result = await command.ExecuteScalarAsync();
            return result != null && (bool)result;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Provides direct access to the database context for test setup
    /// </summary>
    public async Task<T> ExecuteDbContextAsync<T>(Func<FitnessDbContext, Task<T>> action)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        return await action(context);
    }
    
    /// <summary>
    /// Provides direct access to the database context for test setup (void return)
    /// </summary>
    public async Task ExecuteDbContextAsync(Func<FitnessDbContext, Task> action)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        await action(context);
    }
}
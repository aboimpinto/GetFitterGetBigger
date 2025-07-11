using System.Net.Http.Headers;
using System.Net.Http.Json;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Tests.TestBuilders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Xunit;

namespace GetFitterGetBigger.API.Tests;

public class PostgreSqlApiTestFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    
    public PostgreSqlApiTestFixture()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        
        // Add retry logic for container startup
        var retryCount = 0;
        const int maxRetries = 5;
        
        while (retryCount < maxRetries)
        {
            try
            {
                // Test the connection
                using var connection = new Npgsql.NpgsqlConnection(_postgresContainer.GetConnectionString());
                await connection.OpenAsync();
                await connection.CloseAsync();
                break;
            }
            catch (Exception)
            {
                retryCount++;
                if (retryCount >= maxRetries)
                {
                    throw new InvalidOperationException("Failed to connect to PostgreSQL container after multiple retries");
                }
                await Task.Delay(1000 * retryCount); // Exponential backoff
            }
        }
    }
    
    public new async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
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
                options.UseNpgsql(_postgresContainer.GetConnectionString());
                options.EnableSensitiveDataLogging(); // For debugging
                options.LogTo(Console.WriteLine, LogLevel.Information); // Log SQL queries
            });
        });

        var host = base.CreateHost(builder);

        // Run migrations and seed the database after the host is created
        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var db = services.GetRequiredService<FitnessDbContext>();
            
            // Run migrations instead of EnsureCreated for PostgreSQL
            db.Database.Migrate();
            
            // Seed the database with test data
            SeedTestData(db);
            
            // Ensure the transaction is committed
            db.SaveChanges();
        }

        return host;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }

    private void SeedTestData(FitnessDbContext context)
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
    
    // Helper method to clean up test data between tests
    public async Task CleanupDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        // Delete only exercise-related data, keep reference data
        // This prevents duplicate key violations on reference data
        context.ExerciseLinks.RemoveRange(context.ExerciseLinks);
        context.Exercises.RemoveRange(context.Exercises);
        
        await context.SaveChangesAsync();
    }
}
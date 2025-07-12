using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Helpers;
using GetFitterGetBigger.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Fixtures;

/// <summary>
/// Manages PostgreSQL container lifecycle for integration tests.
/// Implements IAsyncLifetime to properly handle container startup and cleanup.
/// </summary>
public class PostgreSqlTestFixture : IAsyncLifetime, IDisposable
{
    private readonly PostgreSqlContainer _postgresContainer;
    private IntegrationTestWebApplicationFactory? _factory;
    
    public IntegrationTestWebApplicationFactory Factory => _factory ?? throw new InvalidOperationException("Factory not initialized. Ensure InitializeAsync has been called.");
    
    public PostgreSqlTestFixture()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("bddtestdb")
            .WithUsername("bddtestuser")
            .WithPassword("bddtestpass")
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        // Start the container
        await _postgresContainer.StartAsync();
        
        // Wait for the container to be ready
        await WaitForContainerReadyAsync();
        
        // Create the factory after container is ready
        _factory = new IntegrationTestWebApplicationFactory();
        await _factory.InitializeAsync();
    }
    
    public async Task DisposeAsync()
    {
        if (_factory != null)
        {
            await _factory.DisposeAsync();
        }
        
        await _postgresContainer.DisposeAsync();
    }
    
    public void Dispose()
    {
        DisposeAsync().GetAwaiter().GetResult();
    }
    
    /// <summary>
    /// Gets the connection string for the PostgreSQL container
    /// </summary>
    public string GetConnectionString() => _postgresContainer.GetConnectionString();
    
    /// <summary>
    /// Executes migrations and seeds the database with reference data
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        await Factory.ExecuteDbContextAsync(async context =>
        {
            await context.Database.MigrateAsync();
            
            // Seed reference data if needed
            // Check Equipment instead of BodyParts since BodyParts are seeded by DbContext migrations
            // but Equipment is only seeded by SeedDataBuilder
            if (!context.Equipment.Any())
            {
                var seeder = new TestDatabaseSeeder(context);
                await seeder.SeedReferenceDataAsync();
            }
        });
    }
    
    /// <summary>
    /// Cleans the database by removing all test data while preserving reference data
    /// </summary>
    public async Task CleanDatabaseAsync()
    {
        await Factory.CleanupTestDataAsync();
    }
    
    /// <summary>
    /// Creates a new HttpClient for making API requests
    /// </summary>
    public HttpClient CreateClient()
    {
        return Factory.CreateClient();
    }
    
    /// <summary>
    /// Provides direct access to the database context for test setup
    /// </summary>
    public async Task<T> ExecuteDbContextAsync<T>(Func<FitnessDbContext, Task<T>> action)
    {
        return await Factory.ExecuteDbContextAsync(action);
    }
    
    /// <summary>
    /// Provides direct access to the database context for test setup (void return)
    /// </summary>
    public async Task ExecuteDbContextAsync(Func<FitnessDbContext, Task> action)
    {
        await Factory.ExecuteDbContextAsync(action);
    }
    
    private async Task WaitForContainerReadyAsync()
    {
        var retryCount = 0;
        const int maxRetries = 10;
        
        while (retryCount < maxRetries)
        {
            try
            {
                using var connection = new Npgsql.NpgsqlConnection(_postgresContainer.GetConnectionString());
                await connection.OpenAsync();
                await connection.CloseAsync();
                return;
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
}
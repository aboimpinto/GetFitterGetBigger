using System;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using Xunit;

namespace GetFitterGetBigger.API.Tests;

public class PostgreSqlTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    
    public string ConnectionString => _postgresContainer.GetConnectionString();
    
    public PostgreSqlTestFixture()
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
                using var connection = new Npgsql.NpgsqlConnection(ConnectionString);
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
    
    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }
}
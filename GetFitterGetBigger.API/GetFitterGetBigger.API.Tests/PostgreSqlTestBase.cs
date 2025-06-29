using Xunit;

namespace GetFitterGetBigger.API.Tests;

[Collection("PostgreSQL Integration Tests")]
public abstract class PostgreSqlTestBase : IAsyncLifetime
{
    protected PostgreSqlApiTestFixture Factory { get; }
    protected HttpClient Client { get; private set; } = null!;
    
    protected PostgreSqlTestBase(PostgreSqlApiTestFixture factory)
    {
        Factory = factory;
    }
    
    public virtual async Task InitializeAsync()
    {
        // Clean up database before each test
        await Factory.CleanupDatabaseAsync();
        
        // Create a new client for each test
        Client = Factory.CreateClient();
    }
    
    public virtual Task DisposeAsync()
    {
        Client?.Dispose();
        return Task.CompletedTask;
    }
}

// Collection definition to ensure tests don't run in parallel
[CollectionDefinition("PostgreSQL Integration Tests")]
public class PostgreSqlTestCollection : ICollectionFixture<PostgreSqlApiTestFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
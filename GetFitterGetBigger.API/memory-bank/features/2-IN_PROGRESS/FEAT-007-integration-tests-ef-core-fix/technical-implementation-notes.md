# Technical Implementation Notes

## PostgreSQL TestContainers Implementation Details

### Test Fixture Example
```csharp
public class PostgreSqlTestFixture : IAsyncLifetime
{
    private PostgreSqlContainer _postgresContainer;
    
    public async Task InitializeAsync()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("testdb")
            .WithUsername("test")
            .WithPassword("test")
            .Build();
        
        await _postgresContainer.StartAsync();
    }
    
    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }
    
    public string ConnectionString => _postgresContainer.GetConnectionString();
}
```

### ApiTestFixture Modifications
```csharp
// Replace the existing UseInMemoryDatabase with:
services.AddDbContext<FitnessDbContext>(options =>
{
    options.UseNpgsql(postgresFixture.ConnectionString);
    options.EnableSensitiveDataLogging(); // For debugging
});

// After host creation, run migrations:
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
    await db.Database.MigrateAsync(); // Use migrations instead of EnsureCreated
}
```

## Test Data Cleanup Options

### Option 1: Transaction Rollback
```csharp
public class TransactionalTest : IDisposable
{
    private readonly IDbContextTransaction _transaction;
    
    public TransactionalTest(FitnessDbContext context)
    {
        _transaction = context.Database.BeginTransaction();
    }
    
    public void Dispose()
    {
        _transaction.Rollback();
        _transaction.Dispose();
    }
}
```

### Option 2: Respawn Library
```csharp
// Install-Package Respawn
private static Respawner _respawner;

// In fixture initialization:
_respawner = await Respawner.CreateAsync(connectionString, new RespawnerOptions
{
    SchemasToInclude = new[] { "public" },
    TablesToIgnore = new[] { "__EFMigrationsHistory" }
});

// After each test:
await _respawner.ResetAsync(connectionString);
```

## Performance Optimizations

### Container Reuse Pattern
```csharp
public class SharedPostgreSqlFixture : IAsyncLifetime
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private static PostgreSqlContainer _sharedContainer;
    private static int _referenceCount;
    
    public async Task InitializeAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_sharedContainer == null)
            {
                _sharedContainer = new PostgreSqlBuilder()
                    .WithImage("postgres:15-alpine")
                    .Build();
                await _sharedContainer.StartAsync();
            }
            _referenceCount++;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
```

## CI/CD Considerations

### GitHub Actions Example
```yaml
services:
  postgres:
    image: postgres:15-alpine
    env:
      POSTGRES_USER: test
      POSTGRES_PASSWORD: test
      POSTGRES_DB: testdb
    options: >-
      --health-cmd pg_isready
      --health-interval 10s
      --health-timeout 5s
      --health-retries 5
```

### Docker Compose for Local Development
```yaml
version: '3.8'
services:
  test-db:
    image: postgres:15-alpine
    environment:
      POSTGRES_USER: test
      POSTGRES_PASSWORD: test
      POSTGRES_DB: testdb
    ports:
      - "5433:5432"
```

## Troubleshooting

### Common Issues:

1. **Container startup timeout**
   - Increase timeout in TestContainers configuration
   - Check Docker daemon is running

2. **Port conflicts**
   - TestContainers handles random port assignment automatically
   - No manual port configuration needed

3. **Migration failures**
   - Ensure all migrations are PostgreSQL-compatible
   - Check for SQL Server specific syntax

4. **Performance issues**
   - Use container reuse pattern
   - Consider parallel test execution limits
   - Monitor Docker resource allocation
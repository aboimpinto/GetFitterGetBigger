# Integration Testing with PostgreSQL and TestContainers

This document describes how to run and maintain integration tests for the GetFitterGetBigger API using PostgreSQL TestContainers.

## Prerequisites

### Docker Installation
Integration tests require Docker to be installed and running on your machine:

- **Windows**: Install [Docker Desktop](https://www.docker.com/products/docker-desktop)
- **macOS**: Install [Docker Desktop](https://www.docker.com/products/docker-desktop)
- **Linux**: Install Docker using your distribution's package manager

Verify Docker is installed and running:
```bash
docker --version
docker ps
```

## Running Integration Tests

### Running All Tests
```bash
dotnet test
```

### Running Only PostgreSQL Integration Tests
```bash
dotnet test --filter "FullyQualifiedName~PostgreSql"
```

### Running a Specific Test
```bash
dotnet test --filter "FullyQualifiedName=GetFitterGetBigger.API.Tests.Controllers.ExercisesControllerPostgreSqlTests.GetExercises_ReturnsPagedListOfExercises"
```

## Test Architecture

### Key Components

1. **PostgreSqlTestFixture**: Manages the PostgreSQL container lifecycle
   - Creates a PostgreSQL 15-alpine container for each test collection
   - Handles container startup and cleanup
   - Provides connection string to tests

2. **PostgreSqlApiTestFixture**: Extends WebApplicationFactory with PostgreSQL support
   - Replaces in-memory database with PostgreSQL
   - Runs EF Core migrations
   - Seeds test data

3. **PostgreSqlTestBase**: Base class for all PostgreSQL integration tests
   - Implements test isolation with database cleanup between tests
   - Provides shared test infrastructure

## Troubleshooting

### Common Issues

#### 1. Docker Not Running
**Error**: "Cannot connect to the Docker daemon"
**Solution**: Start Docker Desktop or the Docker service

#### 2. Port Conflicts
**Error**: "bind: address already in use"
**Solution**: TestContainers automatically assigns random ports, but if you have issues:
- Check for running PostgreSQL containers: `docker ps`
- Stop conflicting containers: `docker stop <container-id>`

#### 3. Slow Test Startup
**Issue**: Tests take a long time to start
**Explanation**: The first test run downloads the PostgreSQL image. Subsequent runs will be faster.

#### 4. Test Failures After Migration Changes
**Solution**: 
- Delete and recreate the test database
- Ensure migrations are applied in the correct order
- Check for PostgreSQL-specific SQL in migrations

### Debugging Tips

1. **View SQL Queries**: Tests log all SQL queries to the console
2. **Container Logs**: `docker logs <container-id>`
3. **Connect to Test Database**: Use the connection string from test logs with a PostgreSQL client

## Performance Considerations

- **Container Startup**: ~2-3 seconds per test collection
- **Test Isolation**: Each test cleans and reseeds the database
- **Parallel Execution**: Tests in the same collection run sequentially to avoid conflicts
- **Resource Usage**: Each container uses ~100MB RAM

## Best Practices

1. **Use Collection Fixtures**: Group related tests to share container instances
2. **Clean Test Data**: Always clean up test data between tests
3. **Avoid Hard-Coded IDs**: Use the seeded test data IDs
4. **Test Real Scenarios**: Test actual PostgreSQL features (ILIKE, arrays, JSON)

## CI/CD Configuration

For GitHub Actions or other CI/CD pipelines, ensure:

1. Docker is available in the build environment
2. Add Docker service to your workflow:

```yaml
services:
  docker:
    image: docker:dind
    options: --privileged
```

3. Or use a runner with Docker pre-installed

## Future Improvements

- [ ] Add test data factories for easier test setup
- [ ] Implement parallel test execution with multiple containers
- [ ] Add performance benchmarks
- [ ] Create integration test templates

## Related Documentation

- [TestContainers Documentation](https://testcontainers.com/)
- [EF Core Testing Documentation](https://docs.microsoft.com/en-us/ef/core/testing/)
- [PostgreSQL Docker Image](https://hub.docker.com/_/postgres)
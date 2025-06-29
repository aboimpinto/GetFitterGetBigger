# GetFitterGetBigger API

Backend service for the GetFitterGetBigger fitness application ecosystem.

## Prerequisites

- .NET 9.0 SDK
- PostgreSQL 15+
- Docker (for integration tests)

## Getting Started

### Database Setup
1. Ensure PostgreSQL is running locally
2. Update connection string in `appsettings.Development.json`
3. Run migrations:
   ```bash
   dotnet ef database update
   ```

### Running the API
```bash
dotnet run --project GetFitterGetBigger.API
```

The API will be available at:
- Development: http://localhost:5214
- Swagger: http://localhost:5214/swagger

## Testing

### Prerequisites for Testing
- Docker must be installed and running (required for integration tests)
- See [INTEGRATION-TESTING.md](INTEGRATION-TESTING.md) for detailed setup

### Running Tests
```bash
# Run all tests
dotnet test

# Run only unit tests
dotnet test --filter "FullyQualifiedName!~PostgreSql"

# Run only integration tests
dotnet test --filter "FullyQualifiedName~PostgreSql"
```

## Architecture

- **API Layer**: ASP.NET Core Web API with JWT authentication
- **Data Layer**: Entity Framework Core with PostgreSQL
- **Caching**: In-memory caching with configurable TTL
- **Testing**: xUnit with TestContainers for integration tests

## Key Features

- JWT-based authentication
- Role-based authorization (Admin, Personal Trainer, Client)
- Comprehensive exercise management system
- Reference data management
- Server-side caching
- PostgreSQL with custom ID types

## Documentation

- [Integration Testing Guide](INTEGRATION-TESTING.md)
- [Memory Bank](memory-bank/README.md) - Project knowledge base
- API Documentation: Run the project and visit `/swagger`

## Development Workflow

1. Check memory bank for existing patterns and processes
2. Follow FEATURE_IMPLEMENTATION_PROCESS.md for new features
3. Follow BUG_IMPLEMENTATION_PROCESS.md for bug fixes
4. Ensure all tests pass before committing
5. Update memory bank documentation as needed
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
3. Database migrations are applied automatically on startup
   - The application will create the database if it doesn't exist
   - All pending migrations will be applied automatically
   - If migration fails, the application will exit to prevent data corruption
   
   **Manual migration (optional):**
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
- See [INTEGRATION-TESTING.md](memory-bank/INTEGRATION-TESTING.md) for detailed setup

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

- [Integration Testing Guide](memory-bank/INTEGRATION-TESTING.md)
- [Memory Bank](memory-bank/README.md) - Project knowledge base
- API Documentation: Run the project and visit `/swagger`

## Development Workflow

1. Check memory bank for existing patterns and processes
2. Follow FEATURE_IMPLEMENTATION_PROCESS.md for new features
3. Follow BUG_IMPLEMENTATION_PROCESS.md for bug fixes
4. Ensure all tests pass before committing
5. Update memory bank documentation as needed

## Database Migration Troubleshooting

### Automatic Migration Behavior
- Migrations run automatically on application startup
- Database is created if it doesn't exist
- All pending migrations are applied in order
- Application exits if migration fails (prevents schema mismatch)

### Common Issues
1. **Application won't start**
   - Check PostgreSQL is running
   - Verify connection string is correct
   - Check logs for migration errors
   - Run `dotnet ef database update` manually to see detailed errors

2. **Migration failures**
   - Each migration runs in its own transaction
   - Failed migrations are automatically rolled back
   - Check for schema conflicts or invalid SQL

3. **Debugging migrations**
   - Enable EF Core debug logging in appsettings.json:
     ```json
     "Logging": {
       "LogLevel": {
         "Microsoft.EntityFrameworkCore.Migrations": "Debug"
       }
     }
     ```
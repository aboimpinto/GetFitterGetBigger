# Technical Context

> For a comprehensive overview of the technology stack across the entire ecosystem, please refer to the [Shared Memory Bank](/Shared/memory-bank/techContext.md).

## API-Specific Technology Stack

The GetFitterGetBigger API Application is built using the following technologies:

### Core Technologies
- **C# Minimal API**: Lightweight framework for building HTTP APIs
- **ASP.NET Core**: Web framework for building web APIs
- **.NET 9.0**: The latest version of Microsoft's development platform
- **Swagger/OpenAPI**: For API documentation and testing

### Data Access
- **Entity Framework Core**: ORM for database access
  - **Record-Based Entities**: Using C# records for entity models
  - **Specialized ID Types**: Type-safe GUID wrappers with domain-specific string representation
  - **Handler Pattern**: Static Handler classes for entity creation and manipulation
- **SQL Server** (anticipated): For data storage
- **Repository Pattern**: For data access abstraction
- **UnitOfWork Pattern**: For transaction management (planned integration)

### Security
- **JWT Authentication**: For secure authentication
- **Role-Based Authorization**: For access control
- **Data Validation**: For input validation and security

## Key Technical Components

### API Endpoints
- **RESTful Design**: Following REST principles for API design
- **Resource-Based Organization**: Endpoints organized around resources
- **Versioning**: Support for API versioning
- **Pagination**: For handling large datasets

### Authentication/Authorization
- **JWT Token Generation**: For secure authentication
- **Role-Based Access Control**: For authorization
- **Secure Password Handling**: For user security

### Data Persistence
- **Entity Framework Core**: For database operations
- **Migration Support**: For database schema evolution
- **Seed Data**: For initial data population

### Documentation
- **Swagger Integration**: For interactive API documentation
- **XML Comments**: For code documentation
- **API Documentation Files**: In the api-docs folder

## Technical Constraints

### Performance
- **Response Time**: APIs should respond within acceptable timeframes
- **Scalability**: Support for growing user base and data volume
- **Caching**: Where appropriate for performance optimization

### Security
- **Data Protection**: Secure handling of sensitive data
- **Input Validation**: To prevent injection attacks
- **Rate Limiting**: To prevent abuse

### Compatibility
- **Cross-Platform**: Support for various client platforms
- **Backward Compatibility**: For API evolution
- **Standard Formats**: JSON for data exchange

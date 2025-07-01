# System Patterns

> For a comprehensive overview of the entire ecosystem architecture, please refer to the [Shared Memory Bank](/Shared/memory-bank/systemPatterns.md).

## API-Specific Architectural Patterns

The GetFitterGetBigger API Application serves as the central hub for all data operations in the ecosystem. It implements several key architectural patterns specific to its role:

### Core Patterns

- **API-First Design**: The system is built around this central API that serves as the backbone for all operations
- **Three-Tier Architecture**: 
  1. Presentation Layer (Mobile App and Admin App)
  2. Business Logic Layer (API - this project)
  3. Data Access Layer (Database, accessed only through API)
- **Separation of Concerns**: Clear division between client applications and server-side logic

### Design Patterns

- **Repository Pattern**: For data access abstraction
- **Service Layer Pattern**: MANDATORY - All business logic must reside in service layer
- **Dependency Injection**: For loose coupling and testability
- **Unit of Work Pattern**: For managing database transactions
- **CQRS (Command Query Responsibility Segregation)**: Potentially for separating read and write operations
- **Mediator Pattern**: Potentially for handling communication between components

### Architectural Rules (MANDATORY)

#### Controller Layer Rules
- **Controllers MUST NOT directly access repositories** - This is FORBIDDEN
- **Controllers MUST NOT directly access UnitOfWork (ReadOnly or Writable)** - This is FORBIDDEN
- **Controllers MUST ONLY communicate with Service layer**
- **Controllers are responsible for:**
  - HTTP request/response handling
  - Input validation via attributes
  - Authorization checks
  - Calling appropriate service methods
  - Mapping service results to HTTP responses

#### Service Layer Rules
- **Services are the ONLY components that access repositories**
- **Services are the ONLY components that create UnitOfWork instances**
- **Services decide whether to use ReadOnly or Writable UnitOfWork**
- **Services are responsible for:**
  - Business logic implementation
  - Transaction management via UnitOfWork
  - Calling multiple repositories within a single transaction
  - Data validation beyond basic input validation
  - Business rule enforcement

#### Repository Layer Rules
- **Repositories MUST be accessed through UnitOfWork**
- **Repositories handle ONLY data access logic**
- **No business logic in repositories**
- **Repositories are responsible for:**
  - CRUD operations
  - Query operations
  - Data persistence logic

#### Transaction Management
- **UnitOfWork manages database transactions**
- **Services MUST call CommitAsync() on Writable UnitOfWork**
- **Multiple operations can be wrapped in a single UnitOfWork transaction**
- **Example pattern:**
```csharp
using (var unitOfWork = _unitOfWorkProvider.CreateWritable())
{
    var repository = unitOfWork.GetRepository<IRepository>();
    // Perform operations
    await unitOfWork.CommitAsync();
}
```

## Communication Patterns

- **RESTful API**: For standardized communication between clients and server
- **Request-Response**: Standard HTTP communication pattern
- **JWT Authentication**: For secure authentication and authorization
- **Federated Authentication and Claims-Based Authorization**: A detailed system for handling user identity and permissions via federated providers and a local claims store. [Details](/memory-bank/features/federated-authentication.md)

## Data Patterns

- **Data Transfer Objects (DTOs)**: For transferring data between API and client applications
- **Entity Models**: For database representation
  - **Record-Based Entities**: Using C# records for immutable entity models
  - **Specialized ID Types**: Type-safe ID wrappers around GUIDs with domain-specific string representation
  - **Handler Pattern**: Static Handler classes within entities for creation and manipulation
- **Data Validation**: Input validation at API boundaries
- **Shared Models**: Using models from the Shared project for consistency
- **Entity Framework Core**: For ORM-based database access with specialized ID type conversions

## API Endpoint Organization

- **Resource-Based Endpoints**: Organized around resources (exercises, workouts, plans)
- **Versioned APIs**: To support backward compatibility as the system evolves
- **Consistent Response Formats**: Standardized success and error responses
- **Pagination**: For handling large datasets efficiently

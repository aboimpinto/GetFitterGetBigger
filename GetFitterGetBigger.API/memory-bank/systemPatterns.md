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
- **Dependency Injection**: For loose coupling and testability
- **CQRS (Command Query Responsibility Segregation)**: Potentially for separating read and write operations
- **Mediator Pattern**: Potentially for handling communication between components

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

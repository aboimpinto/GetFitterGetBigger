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

#### ⚠️ CRITICAL: ReadOnly vs Writable UnitOfWork Usage

**This is a MANDATORY architectural rule that MUST be followed:**

1. **Use ReadOnlyUnitOfWork for ALL validation and query operations**
   - Checking if related entities exist
   - Validating data before updates
   - Any operation that doesn't modify data
   
2. **Use WritableUnitOfWork ONLY for actual data modifications**
   - Creating new entities
   - Updating existing entities
   - Deleting entities

**Why this matters:**
- Using WritableUnitOfWork for queries causes Entity Framework to track entities
- Tracked entities can lead to unwanted database updates
- Example: Validating a BodyPart exists before updating a MuscleGroup can cause BOTH to be updated if using WritableUnitOfWork

**Correct pattern:**
```csharp
public async Task<ResultDto> UpdateEntityAsync(string id, UpdateDto request)
{
    // STEP 1: Validation with ReadOnlyUnitOfWork
    using (var readOnlyUow = _unitOfWorkProvider.CreateReadOnly())
    {
        var validationRepo = readOnlyUow.GetRepository<IValidationRepository>();
        var relatedEntity = await validationRepo.GetByIdAsync(request.RelatedId);
        if (relatedEntity == null)
            throw new ArgumentException("Related entity not found");
    }
    
    // STEP 2: Update with WritableUnitOfWork
    using var writableUow = _unitOfWorkProvider.CreateWritable();
    var repository = writableUow.GetRepository<IMainRepository>();
    var entity = await repository.GetByIdAsync(id);
    var updated = Entity.Handler.Update(entity, request.NewValue);
    await repository.UpdateAsync(updated);
    await writableUow.CommitAsync();
    
    return MapToDto(updated);
}
```

## Communication Patterns

### External Communication
- **RESTful API**: For standardized communication between clients and server
- **Request-Response**: Standard HTTP communication pattern
- **JWT Authentication**: For secure authentication and authorization
- **Federated Authentication and Claims-Based Authorization**: A detailed system for handling user identity and permissions via federated providers and a local claims store. [Details](/memory-bank/features/federated-authentication.md)

### Service-to-Service Communication (NEW - FEAT-020)
- **Single Repository Rule**: Each service only accesses its own repository directly
- **Service Dependencies**: Services depend on other services, not repositories
- **Validation Pattern**: Services expose validation methods (ExistsAsync, AllExistAsync) for cross-service validation
- **Transactional Pattern**: Services accept IWritableUnitOfWork parameters for participating in transactions initiated by other services

Example of service-to-service communication:
```csharp
// MuscleGroupService needs to validate a BodyPart exists
public async Task<MuscleGroupDto> CreateAsync(CreateMuscleGroupRequest request)
{
    // Use IBodyPartService instead of IBodyPartRepository
    if (!await _bodyPartService.ExistsAsync(request.BodyPartId))
    {
        throw new InvalidOperationException($"Body part with ID {request.BodyPartId} not found");
    }
    
    // Continue with creation...
}
```

Example of transactional pattern:
```csharp
// AuthService creates user and claim in same transaction
public async Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request)
{
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    
    // Create user
    var user = new User { Id = UserId.New(), Email = request.Email };
    await userRepository.AddUserAsync(user);
    
    // Pass unitOfWork to ClaimService for transactional consistency
    await _claimService.CreateUserClaimAsync(user.Id, "Free-Tier", unitOfWork);
    
    await unitOfWork.CommitAsync();
}
```

## Data Patterns

- **Data Transfer Objects (DTOs)**: For transferring data between API and client applications
- **Entity Models**: For database representation
  - **Record-Based Entities**: Using C# records for immutable entity models
  - **Specialized ID Types**: Type-safe ID wrappers around GUIDs with domain-specific string representation
  - **Handler Pattern**: Static Handler classes within entities for creation and manipulation
- **Data Validation**: Input validation at API boundaries
- **Shared Models**: Using models from the Shared project for consistency
- **Entity Framework Core**: For ORM-based database access with specialized ID type conversions

### Reference Data Patterns
- **Reference Tables**: Lookup tables with relatively static data
  - **Pure Reference Data**: Inherits from ReferenceDataBase (BodyParts, DifficultyLevels, etc.)
  - **Dynamic Reference Data**: Custom implementation with CRUD support (MuscleGroups, Equipment)
  - **Cache Integration**: Direct cache integration in services (see `/memory-bank/CACHE_INTEGRATION_PATTERN.md`)
  - **CRUD Conversion Process**: Converting read-only tables to CRUD (see `/memory-bank/REFERENCE_TABLE_CRUD_PROCESS.md`)
- **Caching Strategy**:
  - **IEternalCacheService**: 365-day cache for pure reference data
  - **ICacheService**: 1-hour cache for dynamic reference data
  - **Direct Integration**: Services own their caching logic, no wrapper classes
- **Related Documentation**:
  - `/memory-bank/reference-tables-overview.md` - Complete list of reference tables
  - `/memory-bank/CodeQualityGuidelines/CacheIntegrationPattern.md` - Cache integration implementation

## API Endpoint Organization

- **Resource-Based Endpoints**: Organized around resources (exercises, workouts, plans)
- **Versioned APIs**: To support backward compatibility as the system evolves
- **Consistent Response Formats**: Standardized success and error responses
- **Pagination**: For handling large datasets efficiently

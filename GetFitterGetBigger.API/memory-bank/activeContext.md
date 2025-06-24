# Active Context

## Application Overview
GetFitterGetBigger is a fitness application ecosystem consisting of:
1. A C# Minimal API with Swagger (this application)
2. A mobile app for clients to perform workouts
3. An admin app for Personal Trainers to manage content

## Current Status
This API serves as the central data processing hub for the GetFitterGetBigger ecosystem. It handles all database operations since neither the mobile app nor the admin app have direct database access.

## Key Responsibilities
- Process, record, retrieve, and update fitness data
- Serve as the intermediary between client applications and the database
- Provide endpoints for workout management
- Support exercise and workout plan creation by trainers
- Enable clients to access and track their fitness activities

## Database Model Implementation
A standardized approach for implementing database models using Entity Framework Core has been documented in `databaseModelPattern.md`. Key aspects include:

- Entities implemented as C# records with immutable properties
- Specialized ID types for each entity (e.g., WorkoutId, ExerciseId) that wrap GUIDs
- Static Handler classes within each entity for creation and manipulation
- Consistent coding standards including underscore prefix for private fields and explicit `this.` qualifier usage
- DbContext configuration for handling specialized ID types
- Each file should contain only one class or enum (Single Responsibility Principle)
- Enums are placed in the Models/Enums directory
- Specialized ID types are placed in the Models/SpecializedIds directory
- All unused using directives should be removed before considering a task complete
- Handler.Create methods should use expression-bodied members with target-typed new expressions:
  ```csharp
  public static Entity Create(...) =>
      new()
      {
          Property = value,
          ...
      };
  ```

## Data Access Patterns
The application uses the Unit of Work and Repository patterns for data access, which have been documented in `unitOfWorkPattern.md`. Key aspects include:

- The Olimpo.EntityFramework.Persistency library provides a generic implementation of the Unit of Work and Repository patterns
- The Unit of Work pattern ensures that all operations within a unit either succeed or fail as a whole
- The Repository pattern abstracts the data access layer from the rest of the application
- Reference data repositories are implemented using a base repository interface and implementation
- Each reference data entity has its own repository interface and implementation
- The ReferenceTablesController provides endpoints for retrieving reference data
- Two types of Unit of Work are available:
  - ReadOnlyUnitOfWork: For read-only operations
  - WritableUnitOfWork: For operations that modify data, with transaction support

### API Endpoints for Reference Data
The application provides separate controllers for each reference data type, all under the `/api/ReferenceTables` route. These controllers are grouped together in the Swagger UI under a "ReferenceTables" section for better organization.

#### Controller Organization
All reference table controllers inherit from a base `ReferenceTablesBaseController` class, which provides common functionality and ensures consistent API design. The Swagger UI is configured to group these controllers together using a custom document processor.

#### Available Endpoints

##### DifficultyLevelsController
- `GET /api/ReferenceTables/DifficultyLevels` - Get all active difficulty levels
- `GET /api/ReferenceTables/DifficultyLevels/{id}` - Get a difficulty level by ID
- `GET /api/ReferenceTables/DifficultyLevels/ByValue/{value}` - Get a difficulty level by value

##### KineticChainTypesController
- `GET /api/ReferenceTables/KineticChainTypes` - Get all active kinetic chain types
- `GET /api/ReferenceTables/KineticChainTypes/{id}` - Get a kinetic chain type by ID
- `GET /api/ReferenceTables/KineticChainTypes/ByValue/{value}` - Get a kinetic chain type by value

##### BodyPartsController
- `GET /api/ReferenceTables/BodyParts` - Get all active body parts
- `GET /api/ReferenceTables/BodyParts/{id}` - Get a body part by ID
- `GET /api/ReferenceTables/BodyParts/ByValue/{value}` - Get a body part by value

##### MuscleRolesController
- `GET /api/ReferenceTables/MuscleRoles` - Get all active muscle roles
- `GET /api/ReferenceTables/MuscleRoles/{id}` - Get a muscle role by ID
- `GET /api/ReferenceTables/MuscleRoles/ByValue/{value}` - Get a muscle role by value

##### EquipmentController
- `GET /api/ReferenceTables/Equipment` - Get all equipment
- `GET /api/ReferenceTables/Equipment/{id}` - Get equipment by ID
- `GET /api/ReferenceTables/Equipment/ByName/{name}` - Get equipment by name
- `GET /api/ReferenceTables/Equipment/ByValue/{value}` - Get equipment by value (name)

##### MetricTypesController
- `GET /api/ReferenceTables/MetricTypes` - Get all metric types
- `GET /api/ReferenceTables/MetricTypes/{id}` - Get a metric type by ID
- `GET /api/ReferenceTables/MetricTypes/ByName/{name}` - Get a metric type by name
- `GET /api/ReferenceTables/MetricTypes/ByValue/{value}` - Get a metric type by value (name)

##### MovementPatternsController
- `GET /api/ReferenceTables/MovementPatterns` - Get all movement patterns
- `GET /api/ReferenceTables/MovementPatterns/{id}` - Get a movement pattern by ID
- `GET /api/ReferenceTables/MovementPatterns/ByName/{name}` - Get a movement pattern by name
- `GET /api/ReferenceTables/MovementPatterns/ByValue/{value}` - Get a movement pattern by value (name)

##### MuscleGroupsController
- `GET /api/ReferenceTables/MuscleGroups` - Get all muscle groups
- `GET /api/ReferenceTables/MuscleGroups/{id}` - Get a muscle group by ID
- `GET /api/ReferenceTables/MuscleGroups/ByName/{name}` - Get a muscle group by name
- `GET /api/ReferenceTables/MuscleGroups/ByValue/{value}` - Get a muscle group by value (name)
- `GET /api/ReferenceTables/MuscleGroups/ByBodyPart/{bodyPartId}` - Get all muscle groups for a specific body part

This approach follows the Single Responsibility Principle, with each controller responsible for a single reference data type, while providing a unified API experience through the common base controller and Swagger UI grouping.

### Non-ReferenceDataBase Entities

Some entities in the system do not inherit from ReferenceDataBase but are still treated as reference data for API purposes. These include:

- Equipment
- MetricType
- MovementPattern
- MuscleGroup

These entities have their own controllers that follow the same pattern as the ReferenceDataBase controllers but implement the mapping to ReferenceDataDto directly rather than inheriting from ReferenceTablesBaseController. This ensures a consistent API experience while accommodating the different entity structures.

### API Behavior

#### Case-Insensitive Value Searches
All endpoints that search by value (e.g., `/api/ReferenceTables/BodyParts/ByValue/{value}`) perform case-insensitive searches. This means that searching for "chest", "CHEST", or "ChEsT" will all return the same result as searching for "Chest". This behavior is consistent across all reference data endpoints to provide a more user-friendly API.

#### DTO Structure
The API endpoints return reference data using a standardized `ReferenceDataDto` class with the following properties:

- `id`: A string formatted as `<referencetable>-<guid>` to ensure strong typing on the client side
- `value`: The display name or value of the reference data item
- `description`: An optional description of the reference data item

Internal properties like `DisplayOrder` and `IsActive` are intentionally excluded from the DTO to maintain a clean API contract and hide implementation details from clients.

Example response:

```json
{
  "id": "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4",
  "value": "Compound",
  "description": "Exercises that work multiple muscle groups"
}
```

The `ReferenceTablesBaseController` provides a common `MapToDto` method that handles the conversion from entity objects to DTOs, ensuring consistent formatting of IDs and property selection across all reference data endpoints.

For non-ReferenceDataBase entities, the controllers implement their own mapping to ReferenceDataDto to maintain a consistent API contract:

```csharp
// Example from EquipmentController
var result = equipment.Select(e => new ReferenceDataDto
{
    Id = e.Id.ToString(),
    Value = e.Name
});
```

This approach ensures that all reference data endpoints return data in a consistent format, regardless of the underlying entity structure.

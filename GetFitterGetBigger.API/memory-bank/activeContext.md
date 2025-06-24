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

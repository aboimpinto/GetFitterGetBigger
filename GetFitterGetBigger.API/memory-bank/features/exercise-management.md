# Exercise Management Feature

## 1. Feature Overview

This feature enables the creation and management of exercises, which are the fundamental building blocks of workouts. Personal Trainers (PTs) will use the Admin application to create a comprehensive library of exercises, including detailed instructions and visual aids.

This document outlines the API and database-level implementation for the Exercise Management feature.

### Key Features:
- Full CRUD operations (Create, Read, Update, Delete)
- Pagination support (default 10 items per page, configurable)
- Filtering capabilities (by name and other fields)
- Soft delete with conditional hard delete
- Admin-only access via authorization

---

## 2. Database Schema

A new `Exercise` entity will be added to the database. It will be implemented following the existing `databaseModelPattern.md`.

### `Exercise` Entity

Represents a single exercise in the system.

**Location**: `Models/Entities/Exercise.cs`
**Specialized ID**: `Models/SpecializedIds/ExerciseId.cs`

| Field          | Type             | Constraints                               | Description                                                                 |
| :------------- | :--------------- | :---------------------------------------- | :-------------------------------------------------------------------------- |
| `Id`           | `ExerciseId`     | Primary Key                               | The unique identifier for the exercise.                                     |
| `Name`         | `string`         | Required, Unique Index                    | The common name of the exercise (e.g., "Barbell Back Squat").               |
| `Description`  | `string`         | Required                                  | A concise summary of the exercise.                                          |
| `Instructions` | `string`         | Required                                  | Detailed, step-by-step instructions for performing the movement.            |
| `VideoUrl`     | `string`         | Nullable                                  | A link to a hosted video demonstrating the exercise.                        |
| `ImageUrl`     | `string`         | Nullable                                  | A link to a hosted image of the exercise.                                   |
| `IsUnilateral` | `bool`           | Required                                  | Indicates if the exercise is performed on one side of the body at a time.   |
| `IsActive`     | `bool`           | Required, Default: `true`                 | A flag for soft-deleting exercises.                                         |
| `DifficultyId` | `DifficultyLevelId` | Foreign Key to `DifficultyLevels` table | The skill level required for the exercise (e.g., 'Beginner', 'Advanced').   |

### Many-to-Many Relationships

The `Exercise` entity will have many-to-many relationships with the following reference tables:
- `MuscleGroups` (via `ExerciseMuscleGroups` join table)
- `Equipment` (via `ExerciseEquipment` join table)
- `BodyParts` (via `ExerciseBodyParts` join table)
- `MovementPatterns` (via `ExerciseMovementPatterns` join table)

These relationships enable advanced filtering and categorization of exercises.

---

## 3. API Endpoints

A new `ExercisesController` will be created to expose endpoints for managing exercises.

### Endpoints:

#### GET /api/exercises
Get a paginated list of exercises with filtering.
- Query Parameters:
  - `page` (int, default: 1): Page number
  - `pageSize` (int, default: 10): Items per page
  - `name` (string, optional): Filter by exercise name (case-insensitive, partial match)
  - `difficultyId` (string, optional): Filter by difficulty level
  - `muscleGroupIds` (string[], optional): Filter by muscle groups
  - `equipmentIds` (string[], optional): Filter by equipment
  - `movementPatternIds` (string[], optional): Filter by movement patterns
  - `isActive` (bool, default: true): Include inactive exercises
- Response: `PagedResponse<ExerciseDto>`

#### GET /api/exercises/{id}
Get a single exercise by its ID.
- Response: `ExerciseDto`

#### POST /api/exercises
Create a new exercise.
- Request Body: `CreateExerciseRequest`
- Response: `ExerciseDto`
- Authorization: Admin only

#### PUT /api/exercises/{id}
Update an existing exercise.
- Request Body: `UpdateExerciseRequest`
- Response: `ExerciseDto`
- Authorization: Admin only

#### DELETE /api/exercises/{id}
Delete an exercise (soft or hard delete based on references).
- Response: 204 No Content
- Authorization: Admin only
- Business Rule: Hard delete only if no workout references exist

---

## 4. Business Logic

- **Uniqueness:** The `Name` of the exercise must be unique (case-insensitive). The service layer will enforce this constraint.
- **Deletion Rules:**
  - Check if exercise is referenced in any WorkoutLog or UserExercise (when these entities exist)
  - If referenced: Soft delete only (`IsActive = false`)
  - If not referenced: Hard delete from database
- **Validation Rules:**
  - Name: Required, max 200 characters, unique
  - Description: Required, max 1000 characters
  - Instructions: Required, max 5000 characters
  - VideoUrl/ImageUrl: Optional, must be valid URLs when provided
  - At least one MuscleGroup must be specified
  - DifficultyLevel must exist in reference table
- **Dependencies:** 
  - DifficultyLevels reference table
  - MuscleGroups, Equipment, BodyParts, MovementPatterns for categorization

---

## 5. Implementation Tasks

- Create the `ExerciseId` specialized ID type.
- Create the `Exercise` entity record.
- Update `FitnessDbContext` with the `Exercises` DbSet and configure the entity.
- Create and apply a new database migration.
- Implement `IExerciseRepository` and `ExerciseRepository`.
- Implement `IExerciseService` and `ExerciseService` to contain business logic.
- Implement the `ExercisesController` with all the required endpoints.
- Add unit and integration tests for the new repositories, services, and controllers.

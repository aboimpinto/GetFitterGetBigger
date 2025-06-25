# Exercise Management Feature

## 1. Feature Overview

This feature enables the creation and management of exercises, which are the fundamental building blocks of workouts. Personal Trainers (PTs) will use the Admin application to create a comprehensive library of exercises, including detailed instructions and visual aids.

This document outlines the API and database-level implementation for the Exercise Management feature.

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

### Open Questions

- **Relationships to other reference tables:** Should the `Exercise` entity have many-to-many relationships with other reference tables like `MuscleGroups`, `Equipment`, `BodyParts`, and `MovementPatterns`? This would allow for more advanced filtering and categorization.

---

## 3. API Endpoints

A new `ExercisesController` will be created to expose endpoints for managing exercises.

- `GET /api/exercises`: Get a paginated list of exercises with filtering.
- `GET /api/exercises/{id}`: Get a single exercise by its ID.
- `POST /api/exercises`: Create a new exercise.
- `PUT /api/exercises/{id}`: Update an existing exercise.
- `DELETE /api/exercises/{id}`: Deactivate (soft delete) an exercise.

The implementation will follow the patterns established in the `api-docs/exercises.md` file.

---

## 4. Business Logic

- **Uniqueness:** The `Name` of the exercise must be unique. The service layer will enforce this constraint.
- **Deletion:** When a `DELETE` request is received, the system will first check if the exercise is part of any existing `Workout`.
  - If it is, the exercise will be marked as inactive (`IsActive = false`).
  - If it is not, the exercise can be permanently deleted from the database. (Note: This rule may be refined later).
- **Dependencies:** The `Exercise` entity will have a dependency on the `DifficultyLevels` reference table.

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

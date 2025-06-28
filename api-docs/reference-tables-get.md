# Get Reference Tables API

This document outlines the API endpoints for retrieving reference data. Reference data consists of relatively static lookup tables used throughout the application, suchs as lists of body parts, difficulty levels, and equipment.

---
used_by:
  - admin
  - client
  - shared
---

## Common API Patterns

All reference table endpoints follow a set of common patterns to ensure consistency and ease of use.

### Base URL

The base URL for all reference table endpoints is:
`/api/ReferenceTables`

### ID Format

A critical convention is the format of the `id` field in the response. It is a structured string designed to provide type safety on the client side:

`"<ReferenceTable>-<Guid>"`

**Example:** `bodypart-a1b2c3d4-e5f6-7890-1234-567890abcdef`

Clients should rely on this full string for identification and referencing.

### Standard Response DTO

All endpoints return a `ReferenceDataDto` object or an array of them. The structure is as follows:

```json
{
  "id": "string",
  "value": "string",
  "description": "string"
}
```

### Case-Insensitive Searches

All endpoints that allow searching by a string value (e.g., `ByValue` or `ByName`) perform **case-insensitive** searches. For example, searching for "chest", "CHEST", or "ChEsT" will all yield the same result.

### Authentication

Bearer token required in the Authorization header for all endpoints.

---

## Endpoints by Reference Table

### BodyParts

*   **Volatility:** Static. Body parts are not expected to change.
*   **Endpoints:**
    *   `GET /api/ReferenceTables/BodyParts`: Get all active body parts.
    *   `GET /api/ReferenceTables/BodyParts/{id}`: Get a body part by its structured ID.
    *   `GET /api/ReferenceTables/BodyParts/ByValue/{value}`: Get a body part by its value (name).

### DifficultyLevels

*   **Volatility:** Static.
*   **Endpoints:**
    *   `GET /api/ReferenceTables/DifficultyLevels`: Get all active difficulty levels.
    *   `GET /api/ReferenceTables/DifficultyLevels/{id}`: Get a difficulty level by ID.
    *   `GET /api/ReferenceTables/DifficultyLevels/ByValue/{value}`: Get a difficulty level by value.

### Equipment

*   **Volatility:** Dynamic. New equipment can be added over time.
*   **Endpoints:**
    *   `GET /api/ReferenceTables/Equipment`: Get all equipment.
    *   `GET /api/ReferenceTables/Equipment/{id}`: Get equipment by ID.
    *   `GET /api/ReferenceTables/Equipment/ByName/{name}`: Get equipment by name.
    *   `GET /api/ReferenceTables/Equipment/ByValue/{value}`: Get equipment by value (name).

### KineticChainTypes

*   **Volatility:** Static.
*   **Endpoints:**
    *   `GET /api/ReferenceTables/KineticChainTypes`: Get all active kinetic chain types.
    *   `GET /api/ReferenceTables/KineticChainTypes/{id}`: Get a kinetic chain type by ID.
    *   `GET /api/ReferenceTables/KineticChainTypes/ByValue/{value}`: Get a kinetic chain type by value.

### MetricTypes

*   **Volatility:** Low-maintenance. May change infrequently.
*   **Endpoints:**
    *   `GET /api/ReferenceTables/MetricTypes`: Get all metric types.
    *   `GET /api/ReferenceTables/MetricTypes/{id}`: Get a metric type by ID.
    *   `GET /api/ReferenceTables/MetricTypes/ByName/{name}`: Get a metric type by name.
    *   `GET /api/ReferenceTables/MetricTypes/ByValue/{value}`: Get a metric type by value (name).

### MovementPatterns

*   **Volatility:** Low-maintenance.
*   **Endpoints:**
    *   `GET /api/ReferenceTables/MovementPatterns`: Get all movement patterns.
    *   `GET /api/ReferenceTables/MovementPatterns/{id}`: Get a movement pattern by ID.
    *   `GET /api/ReferenceTables/MovementPatterns/ByName/{name}`: Get a movement pattern by name.
    *   `GET /api/ReferenceTables/MovementPatterns/ByValue/{value}`: Get a movement pattern by value (name).

### MuscleGroups

*   **Volatility:** Low-maintenance.
*   **Endpoints:**
    *   `GET /api/ReferenceTables/MuscleGroups`: Get all muscle groups.
    *   `GET /api/ReferenceTables/MuscleGroups/{id}`: Get a muscle group by ID.
    *   `GET /api/ReferenceTables/MuscleGroups/ByName/{name}`: Get a muscle group by name.
    *   `GET /api/ReferenceTables/MuscleGroups/ByValue/{value}`: Get a muscle group by value (name).
    *   `GET /api/ReferenceTables/MuscleGroups/ByBodyPart/{bodyPartId}`: Get all muscle groups for a specific body part.

### MuscleRoles

*   **Volatility:** Static.
*   **Endpoints:**
    *   `GET /api/ReferenceTables/MuscleRoles`: Get all active muscle roles.
    *   `GET /api/ReferenceTables/MuscleRoles/{id}`: Get a muscle role by ID.
    *   `GET /api/ReferenceTables/MuscleRoles/ByValue/{value}`: Get a muscle role by value.

### ExerciseTypes

*   **Volatility:** Static. Exercise types are predefined: Warmup, Workout, Cooldown, Rest.
*   **Business Rule:** "Rest" type cannot be combined with other types.
*   **Endpoints:**
    *   `GET /api/ReferenceTables/ExerciseTypes`: Get all exercise types.
    *   `GET /api/ReferenceTables/ExerciseTypes/{id}`: Get an exercise type by ID.
    *   `GET /api/ReferenceTables/ExerciseTypes/ByValue/{value}`: Get an exercise type by value.

# ExerciseWeightType API Documentation

## Overview

The ExerciseWeightType reference table defines how weight is handled for different types of exercises. This table helps enforce proper weight validation rules during workout logging.

## Endpoints

### GET /api/ReferenceTables/ExerciseWeightTypes

Retrieves all exercise weight types.

**Response:**
```json
[
  {
    "id": "exerciseweighttype-b2a4c5d7-6b8c-5d9e-0f1a-2b3c4d5e6f7a",
    "value": "Bodyweight Only",
    "description": "Exercises that only use bodyweight with no option for additional weight"
  },
  {
    "id": "exerciseweighttype-a1b3c5d7-5b7c-4d8e-9f0a-1b2c3d4e5f6a",
    "value": "Bodyweight Optional",
    "description": "Exercises that can be performed with just bodyweight or with added weight"
  },
  {
    "id": "exerciseweighttype-c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a",
    "value": "Weight Required",
    "description": "Exercises that must have external weight specified"
  },
  {
    "id": "exerciseweighttype-d4c6b5a7-8c9d-7e0f-1a2b-3c4d5e6f7a8b",
    "value": "Machine Weight",
    "description": "Exercises performed on machines with weight stacks or plates"
  },
  {
    "id": "exerciseweighttype-e5d7a8b9-9d0e-8f1a-2b3c-4d5e6f7a8b9c",
    "value": "No Weight",
    "description": "Exercises that don't involve weight (stretching, mobility, etc.)"
  }
]
```

### GET /api/ReferenceTables/ExerciseWeightTypes/{id}

Retrieves a specific exercise weight type by ID.

**Parameters:**
- `id` (string, required): The exercise weight type ID

**Response:**
```json
{
  "id": "exerciseweighttype-b2a4c5d7-6b8c-5d9e-0f1a-2b3c4d5e6f7a",
  "value": "Bodyweight Only",
  "description": "Exercises that only use bodyweight with no option for additional weight"
}
```

**Error Responses:**
- `404 Not Found`: Weight type not found

### GET /api/ReferenceTables/ExerciseWeightTypes/ByValue/{value}

Retrieves a specific exercise weight type by its value (case-insensitive).

**Parameters:**
- `value` (string, required): The exercise weight type value

**Example:**
```
GET /api/ReferenceTables/ExerciseWeightTypes/ByValue/bodyweight%20only
```

**Response:**
```json
{
  "id": "exerciseweighttype-b2a4c5d7-6b8c-5d9e-0f1a-2b3c4d5e6f7a",
  "value": "Bodyweight Only",
  "description": "Exercises that only use bodyweight with no option for additional weight"
}
```

**Error Responses:**
- `404 Not Found`: Weight type not found

### GET /api/ReferenceTables/ExerciseWeightTypes/ByCode/{code}

Retrieves a specific exercise weight type by its code (case-sensitive).

**Parameters:**
- `code` (string, required): The exercise weight type code

**Example:**
```
GET /api/ReferenceTables/ExerciseWeightTypes/ByCode/BODYWEIGHT_ONLY
```

**Response:**
```json
{
  "id": "exerciseweighttype-b2a4c5d7-6b8c-5d9e-0f1a-2b3c4d5e6f7a",
  "value": "Bodyweight Only",
  "description": "Exercises that only use bodyweight with no option for additional weight"
}
```

**Error Responses:**
- `404 Not Found`: Weight type not found

## Weight Type Codes and Validation Rules

| Code | Value | Validation Rule |
|------|-------|----------------|
| `BODYWEIGHT_ONLY` | Bodyweight Only | Weight must be null or 0 |
| `BODYWEIGHT_OPTIONAL` | Bodyweight Optional | Weight can be null, 0, or any positive value |
| `WEIGHT_REQUIRED` | Weight Required | Weight must be > 0 |
| `MACHINE_WEIGHT` | Machine Weight | Weight must be > 0 |
| `NO_WEIGHT` | No Weight | Weight must be null or 0 |

## Caching

All ExerciseWeightType endpoints implement a 24-hour cache duration to optimize performance. The cache is automatically invalidated when data changes (though this table is read-only in the current implementation).

## Authorization

Currently, these endpoints do not require authentication. When authorization is implemented, they will require valid JWT tokens with appropriate claims.

## Integration with Exercises

When creating or updating exercises, you can optionally specify an `exerciseWeightTypeId`:

```json
{
  "name": "Barbell Bench Press",
  "description": "Classic chest exercise",
  "exerciseWeightTypeId": "exerciseweighttype-c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a",
  // ... other fields
}
```

The exercise response will include the weight type information:

```json
{
  "id": "exercise-123...",
  "name": "Barbell Bench Press",
  "exerciseWeightType": {
    "id": "exerciseweighttype-c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a",
    "value": "Weight Required",
    "description": "Exercises that must have external weight specified"
  },
  // ... other fields
}
```

## Future Enhancements

- Weight validation will be enforced when creating workout log sets
- Custom weight types may be supported for Personal Trainers
- Additional validation rules may be added (e.g., weight increments for machines)
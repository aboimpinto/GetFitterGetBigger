# Exercise Weight Type API Endpoints

## Overview
This document details the API endpoints for the Exercise Weight Type reference data feature. These endpoints provide read-only access to the system-defined exercise weight types.

## Base URL
- Development: `http://localhost:5214/api`
- Production: TBD

## Endpoints

### 1. List All Exercise Weight Types
Retrieves all available exercise weight types in the system.

**Endpoint**: `GET /exercise-weight-types`

**Authentication**: None required (public endpoint)

**Query Parameters**: None

**Request Headers**:
```http
Accept: application/json
```

**Success Response**: 200 OK
```json
[
  {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "code": "BODYWEIGHT_ONLY",
    "name": "Bodyweight Only",
    "description": "Exercises that cannot have external weight added",
    "isActive": true,
    "displayOrder": 1
  },
  {
    "id": "223e4567-e89b-12d3-a456-426614174001",
    "code": "BODYWEIGHT_OPTIONAL",
    "name": "Bodyweight Optional",
    "description": "Exercises that can be performed with or without additional weight",
    "isActive": true,
    "displayOrder": 2
  },
  {
    "id": "323e4567-e89b-12d3-a456-426614174002",
    "code": "WEIGHT_REQUIRED",
    "name": "Weight Required",
    "description": "Exercises that must have external weight specified",
    "isActive": true,
    "displayOrder": 3
  },
  {
    "id": "423e4567-e89b-12d3-a456-426614174003",
    "code": "MACHINE_WEIGHT",
    "name": "Machine Weight",
    "description": "Exercises performed on machines with weight stacks",
    "isActive": true,
    "displayOrder": 4
  },
  {
    "id": "523e4567-e89b-12d3-a456-426614174004",
    "code": "NO_WEIGHT",
    "name": "No Weight",
    "description": "Exercises that do not use weight as a metric",
    "isActive": true,
    "displayOrder": 5
  }
]
```

**Error Responses**: None expected for this endpoint

**Usage Notes**:
- Results are ordered by `displayOrder`
- Only active weight types are returned by default
- This endpoint is commonly used to populate dropdown selectors in exercise management interfaces

---

### 2. Get Exercise Weight Type by ID
Retrieves a specific exercise weight type by its unique identifier.

**Endpoint**: `GET /exercise-weight-types/{id}`

**Authentication**: None required (public endpoint)

**Path Parameters**:
- `id` (string, required): The unique identifier of the exercise weight type

**Request Headers**:
```http
Accept: application/json
```

**Success Response**: 200 OK
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "code": "BODYWEIGHT_ONLY",
  "name": "Bodyweight Only",
  "description": "Exercises that cannot have external weight added",
  "isActive": true,
  "displayOrder": 1
}
```

**Error Responses**:

404 Not Found
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Exercise weight type with id '999e4567-e89b-12d3-a456-426614174999' not found"
}
```

400 Bad Request (Invalid GUID format)
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Invalid id format. Expected a valid GUID."
}
```

**Usage Notes**:
- Used when displaying exercise details to show the associated weight type
- Can be used for validation before exercise updates

---

## Integration with Exercise Endpoints

### Exercise Creation/Update
When creating or updating an exercise, the weight type is specified using the `exerciseWeightTypeId` field:

**Request Body Example**:
```json
{
  "name": "Push-up",
  "description": "Standard push-up exercise",
  "exerciseWeightTypeId": "123e4567-e89b-12d3-a456-426614174000",
  "muscleGroups": ["chest", "triceps", "shoulders"]
}
```

### Exercise Response
Exercise responses include the weight type information:

```json
{
  "id": "abc123",
  "name": "Push-up",
  "exerciseWeightType": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "code": "BODYWEIGHT_ONLY",
    "name": "Bodyweight Only"
  }
}
```

---

## Weight Validation Rules

The API enforces weight validation based on the exercise weight type when creating workout exercises:

| Weight Type Code | Valid Weight Values | Validation Error Message |
|-----------------|-------------------|-------------------------|
| BODYWEIGHT_ONLY | null or 0 | "Bodyweight exercises cannot have external weight" |
| NO_WEIGHT | null or 0 | "This exercise type does not use weight" |
| BODYWEIGHT_OPTIONAL | null, 0, or > 0 | N/A (all values valid) |
| WEIGHT_REQUIRED | > 0 | "Weight is required for this exercise" |
| MACHINE_WEIGHT | > 0 | "Machine weight must be specified" |

---

## Caching Recommendations

Since exercise weight types are reference data that rarely changes:
- Client applications should cache the list response for the session duration
- Use ETags or Last-Modified headers for cache validation
- Refresh cache on application startup or after 24 hours

---

## Migration Endpoints (Admin Only)

These endpoints are only available during data migration and require admin authentication:

### Bulk Update Exercise Weight Types
**Endpoint**: `PATCH /admin/exercises/weight-types/bulk-update`
**Authentication**: Required (Admin-Tier claim)

**Request Body**:
```json
{
  "updates": [
    {
      "exerciseId": "exercise-guid-1",
      "exerciseWeightTypeId": "weight-type-guid"
    },
    {
      "exerciseId": "exercise-guid-2",
      "exerciseWeightTypeId": "weight-type-guid"
    }
  ]
}
```

**Response**: 200 OK
```json
{
  "updated": 2,
  "failed": 0,
  "errors": []
}
```
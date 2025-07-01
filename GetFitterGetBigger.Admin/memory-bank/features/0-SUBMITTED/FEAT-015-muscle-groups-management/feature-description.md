# Muscle Groups CRUD API Integration

This document provides the API specifications for Muscle Groups CRUD operations that need to be integrated into the Admin application.

---
feature: muscle-groups-management
status: 0-SUBMITTED
created: 2025-07-01
api_version: 1.0
---

## Overview

Muscle Groups is a dynamic reference table that administrators can manage through full CRUD operations. This feature allows Personal Trainers to add, update, and remove muscle groups that can be associated with exercises and specific body parts.

### Business Context
- Muscle groups are used when creating or editing exercises to specify targeted muscles
- Each muscle group is associated with a specific body part
- Only administrators (PT-Tier or Admin-Tier) can modify muscle groups
- Muscle groups use soft delete to maintain referential integrity with existing exercises

## Authentication Requirements

- **Read Operations**: Any authenticated user
- **Write Operations**: Bearer token with `PT-Tier` or `Admin-Tier` claims

## Endpoints

### 1. Get All Muscle Groups
**URL**: `GET /api/ReferenceTables/MuscleGroups`
**Headers**: `Authorization: Bearer {token}`
**Response**: 200 OK
```json
[
  {
    "id": "musclegroup-123e4567-e89b-12d3-a456-426614174000",
    "value": "Biceps",
    "description": "Biceps brachii muscle"
  }
]
```

### 2. Get Muscle Groups by Body Part
**URL**: `GET /api/ReferenceTables/MuscleGroups/ByBodyPart/{bodyPartId}`
**Headers**: `Authorization: Bearer {token}`
**Path Parameters**:
- `bodyPartId`: Body part ID in format `bodypart-{guid}`

**Response**: 200 OK
```json
[
  {
    "id": "musclegroup-123e4567-e89b-12d3-a456-426614174000",
    "value": "Biceps",
    "description": "Biceps brachii muscle"
  }
]
```

### 3. Create Muscle Group
**URL**: `POST /api/ReferenceTables/MuscleGroups`
**Headers**: 
- `Authorization: Bearer {token}`
- `Content-Type: application/json`

**Request Body**:
```json
{
  "name": "string",        // Required, 1-100 characters
  "bodyPartId": "string"   // Required, format: bodypart-{guid}
}
```

**Validation Rules**:
- Name is required and cannot be empty
- Name must be unique within the same body part (case-insensitive)
- BodyPartId must reference an active body part
- Leading/trailing whitespace is automatically trimmed

**Success Response**: 201 Created
```json
{
  "id": "musclegroup-123e4567-e89b-12d3-a456-426614174000",
  "name": "Deltoids",
  "bodyPartId": "bodypart-456e7890-e89b-12d3-a456-426614174000",
  "isActive": true,
  "createdAt": "2025-01-30T12:00:00Z",
  "updatedAt": null
}
```

**Error Responses**:
- 400 Bad Request: Invalid input or body part ID
- 404 Not Found: Body part doesn't exist
- 409 Conflict: Muscle group name already exists for this body part

### 4. Update Muscle Group
**URL**: `PUT /api/ReferenceTables/MuscleGroups/{id}`
**Headers**: 
- `Authorization: Bearer {token}`
- `Content-Type: application/json`

**Path Parameters**:
- `id`: Muscle group ID in format `musclegroup-{guid}`

**Request Body**:
```json
{
  "name": "string",        // Required, 1-100 characters
  "bodyPartId": "string"   // Required, format: bodypart-{guid}
}
```

**Validation Rules**:
- Name is required and cannot be empty
- Name must be unique within the body part (excluding current muscle group)
- BodyPartId must reference an active body part
- Muscle group must exist and be active

**Success Response**: 200 OK
```json
{
  "id": "musclegroup-123e4567-e89b-12d3-a456-426614174000",
  "name": "Anterior Deltoids",
  "bodyPartId": "bodypart-456e7890-e89b-12d3-a456-426614174000",
  "isActive": true,
  "createdAt": "2025-01-30T12:00:00Z",
  "updatedAt": "2025-01-30T13:00:00Z"
}
```

**Error Responses**:
- 400 Bad Request: Invalid ID format or body part ID
- 404 Not Found: Muscle group or body part doesn't exist or is inactive
- 409 Conflict: Name already exists for this body part

### 5. Delete Muscle Group
**URL**: `DELETE /api/ReferenceTables/MuscleGroups/{id}`
**Headers**: `Authorization: Bearer {token}`

**Path Parameters**:
- `id`: Muscle group ID in format `musclegroup-{guid}`

**Important Notes**:
- Implements soft delete (sets IsActive = false)
- Cannot delete muscle groups referenced by exercises
- Deleted muscle groups won't appear in GET endpoints
- Existing exercises maintain their references to deleted muscle groups

**Success Response**: 204 No Content

**Error Responses**:
- 400 Bad Request: Invalid ID format
- 404 Not Found: Muscle group doesn't exist or already inactive
- 409 Conflict: Muscle group is in use by exercises

## Data Models

### MuscleGroupDto
- **id**: string (Format: musclegroup-{guid})
- **name**: string
- **bodyPartId**: string (Format: bodypart-{guid})
- **bodyPartName**: string (optional, included in detailed responses)
- **isActive**: boolean
- **createdAt**: string (ISO 8601 datetime)
- **updatedAt**: string or null

### CreateMuscleGroupDto
- **name**: string (Required, 1-100 characters)
- **bodyPartId**: string (Required, must be valid body part ID)

### UpdateMuscleGroupDto
- **name**: string (Required, 1-100 characters)
- **bodyPartId**: string (Required, must be valid body part ID)

### ReferenceDataDto (for list operations)
- **id**: string (Format: musclegroup-{guid})
- **value**: string (The muscle group name)
- **description**: string (Extended description)

## Implementation Guidelines

### UI Requirements
1. Muscle groups management section in admin panel
2. List view with search/filter capabilities
3. Filter by body part functionality
4. Add/Edit forms with validation
5. Body part selection dropdown in forms
6. Delete confirmation dialog
7. Show error when trying to delete muscle groups in use

### State Management
- Cache muscle groups list for performance
- Invalidate cache after mutations
- Handle optimistic updates with rollback on error
- Maintain body parts list for dropdown selection

### Error Handling
- Display user-friendly messages for conflicts
- Show which exercises use a muscle group (on delete conflict)
- Handle invalid body part references
- Handle network errors gracefully

### Business Rules
1. Muscle group names must be unique within each body part (case-insensitive)
2. Cannot delete muscle groups referenced by exercises
3. All timestamps are in UTC
4. Soft deletes maintain referential integrity
5. Body part association cannot be null

## Dependencies
- Body Parts reference table (for foreign key relationship)
- Must refresh muscle groups list after CRUD operations
- Exercise forms depend on active muscle groups list
- Consider implementing local search/filtering by body part

## Performance Considerations
- Muscle groups list is cached on the server
- Implement client-side caching
- Use debouncing for search operations
- Consider lazy loading muscle groups by body part

## Related Features
- Exercise CRUD operations depend on muscle groups
- Reference table inline creation (FEAT-013) may affect UI implementation
- Body parts are read-only reference data
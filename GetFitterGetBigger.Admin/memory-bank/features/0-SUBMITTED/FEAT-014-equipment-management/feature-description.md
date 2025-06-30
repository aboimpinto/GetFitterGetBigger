# Equipment CRUD API Integration

This document provides the API specifications for Equipment CRUD operations that need to be integrated into the Admin application.

---
feature: equipment-management
status: 0-SUBMITTED
created: 2025-06-30
api_version: 1.0
---

## Overview

Equipment is a dynamic reference table that administrators can manage through full CRUD operations. This feature allows Personal Trainers to add, update, and remove equipment options that can be associated with exercises.

### Business Context
- Equipment items are used when creating or editing exercises
- Only administrators (PT-Tier or Admin-Tier) can modify equipment
- Equipment uses soft delete to maintain referential integrity

## Authentication Requirements

- **Read Operations**: Any authenticated user
- **Write Operations**: Bearer token with `PT-Tier` or `Admin-Tier` claims

## Endpoints

### 1. Get All Equipment
**URL**: `GET /api/ReferenceTables/Equipment`
**Headers**: `Authorization: Bearer {token}`
**Response**: 200 OK
```json
[
  {
    "id": "equipment-123e4567-e89b-12d3-a456-426614174000",
    "name": "Barbell",
    "isActive": true,
    "createdAt": "2025-01-30T12:00:00Z",
    "updatedAt": null
  }
]
```

### 2. Create Equipment
**URL**: `POST /api/ReferenceTables/Equipment`
**Headers**: 
- `Authorization: Bearer {token}`
- `Content-Type: application/json`

**Request Body**:
```json
{
  "name": "string"  // Required, 1-100 characters
}
```

**Validation Rules**:
- Name is required and cannot be empty
- Name must be unique (case-insensitive)
- Leading/trailing whitespace is automatically trimmed

**Success Response**: 201 Created
```json
{
  "id": "equipment-123e4567-e89b-12d3-a456-426614174000",
  "name": "Dumbbell",
  "isActive": true,
  "createdAt": "2025-01-30T12:00:00Z",
  "updatedAt": null
}
```

**Error Responses**:
- 400 Bad Request: Invalid input
- 409 Conflict: Equipment name already exists

### 3. Update Equipment
**URL**: `PUT /api/ReferenceTables/Equipment/{id}`
**Headers**: 
- `Authorization: Bearer {token}`
- `Content-Type: application/json`

**Path Parameters**:
- `id`: Equipment ID in format `equipment-{guid}`

**Request Body**:
```json
{
  "name": "string"  // Required, 1-100 characters
}
```

**Validation Rules**:
- Name is required and cannot be empty
- Name must be unique (excluding current equipment)
- Equipment must exist and be active

**Success Response**: 200 OK
```json
{
  "id": "equipment-123e4567-e89b-12d3-a456-426614174000",
  "name": "Olympic Barbell",
  "isActive": true,
  "createdAt": "2025-01-30T12:00:00Z",
  "updatedAt": "2025-01-30T13:00:00Z"
}
```

**Error Responses**:
- 400 Bad Request: Invalid ID format
- 404 Not Found: Equipment doesn't exist or is inactive
- 409 Conflict: Name already exists

### 4. Delete Equipment
**URL**: `DELETE /api/ReferenceTables/Equipment/{id}`
**Headers**: `Authorization: Bearer {token}`

**Path Parameters**:
- `id`: Equipment ID in format `equipment-{guid}`

**Important Notes**:
- Implements soft delete (sets IsActive = false)
- Cannot delete equipment referenced by exercises
- Deleted equipment won't appear in GET endpoints

**Success Response**: 204 No Content

**Error Responses**:
- 400 Bad Request: Invalid ID format
- 404 Not Found: Equipment doesn't exist or already inactive
- 409 Conflict: Equipment is in use by exercises

## Data Models

### EquipmentDto
```typescript
interface EquipmentDto {
  id: string;           // Format: equipment-{guid}
  name: string;
  isActive: boolean;
  createdAt: string;    // ISO 8601 datetime
  updatedAt: string | null;
}
```

### CreateEquipmentDto
```typescript
interface CreateEquipmentDto {
  name: string;  // Required, 1-100 characters
}
```

### UpdateEquipmentDto
```typescript
interface UpdateEquipmentDto {
  name: string;  // Required, 1-100 characters
}
```

## Implementation Guidelines

### UI Requirements
1. Equipment management section in admin panel
2. List view with search/filter capabilities
3. Add/Edit forms with validation
4. Delete confirmation dialog
5. Show error when trying to delete equipment in use

### State Management
- Cache equipment list for performance
- Invalidate cache after mutations
- Handle optimistic updates with rollback on error

### Error Handling
- Display user-friendly messages for conflicts
- Show which exercises use equipment (on delete conflict)
- Handle network errors gracefully

### Business Rules
1. Equipment names must be unique (case-insensitive)
2. Cannot delete equipment referenced by exercises
3. All timestamps are in UTC
4. Soft deletes maintain referential integrity

## Dependencies
- Must refresh equipment list after CRUD operations
- Exercise forms depend on active equipment list
- Consider implementing local search/filtering

## Performance Considerations
- Equipment list is cached on the server
- Implement client-side caching
- Use debouncing for search operations
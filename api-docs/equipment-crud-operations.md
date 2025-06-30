# Equipment CRUD Operations API

This document details the CRUD (Create, Read, Update, Delete) operations available for Equipment management.

---
used_by:
  - admin
---

## Overview

Equipment is a dynamic reference table that supports full CRUD operations. Unlike static reference tables (e.g., BodyParts, DifficultyLevels), equipment can be added, updated, or removed by administrators.

## Authentication

All write operations (POST, PUT, DELETE) require:
- Bearer token with `PT-Tier` or `Admin-Tier` claims
- Read operations (GET) require any valid authentication

## Endpoints

### Create Equipment

**Endpoint:** `POST /api/ReferenceTables/Equipment`

**Request Body:**
```json
{
  "name": "string"
}
```

**Validation Rules:**
- `name` is required and cannot be empty
- `name` must be unique (case-insensitive)
- Leading/trailing whitespace is automatically trimmed

**Success Response (201 Created):**
```json
{
  "id": "equipment-{guid}",
  "name": "Barbell",
  "isActive": true,
  "createdAt": "2025-01-30T12:00:00Z",
  "updatedAt": null
}
```

**Error Responses:**
- `400 Bad Request`: Invalid input (empty name, validation errors)
- `409 Conflict`: Equipment with the same name already exists

### Update Equipment

**Endpoint:** `PUT /api/ReferenceTables/Equipment/{id}`

**URL Parameters:**
- `id`: Equipment ID in format `equipment-{guid}`

**Request Body:**
```json
{
  "name": "string"
}
```

**Validation Rules:**
- `name` is required and cannot be empty
- `name` must be unique (excluding current equipment)
- Equipment must exist and be active

**Success Response (200 OK):**
```json
{
  "id": "equipment-{guid}",
  "name": "Olympic Barbell",
  "isActive": true,
  "createdAt": "2025-01-30T12:00:00Z",
  "updatedAt": "2025-01-30T13:00:00Z"
}
```

**Error Responses:**
- `400 Bad Request`: Invalid ID format or validation errors
- `404 Not Found`: Equipment doesn't exist or is inactive
- `409 Conflict`: Another equipment with the same name exists

### Delete Equipment

**Endpoint:** `DELETE /api/ReferenceTables/Equipment/{id}`

**URL Parameters:**
- `id`: Equipment ID in format `equipment-{guid}`

**Important Notes:**
- Implements soft delete (sets `IsActive = false`)
- Cannot delete equipment that is referenced by exercises
- Deleted equipment won't appear in GET endpoints

**Success Response:** `204 No Content`

**Error Responses:**
- `400 Bad Request`: Invalid ID format
- `404 Not Found`: Equipment doesn't exist or is already inactive
- `409 Conflict`: Equipment is in use by exercises

## DTOs

### CreateEquipmentDto
```csharp
{
  "name": "string" // Required, 1-100 characters
}
```

### UpdateEquipmentDto
```csharp
{
  "name": "string" // Required, 1-100 characters
}
```

### EquipmentDto
```csharp
{
  "id": "string",      // Format: equipment-{guid}
  "name": "string",
  "isActive": boolean,
  "createdAt": "datetime",
  "updatedAt": "datetime?" // Nullable
}
```

## Business Rules

1. **Unique Names**: Equipment names must be unique (case-insensitive)
2. **Soft Deletes**: Equipment is never physically deleted, only marked as inactive
3. **Referential Integrity**: Cannot delete equipment that is referenced by exercises
4. **Audit Trail**: All equipment records maintain creation and update timestamps
5. **Cache Invalidation**: All write operations invalidate relevant caches

## Example Usage

### Creating New Equipment
```bash
curl -X POST https://api.example.com/api/ReferenceTables/Equipment \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"name": "Resistance Bands"}'
```

### Updating Equipment
```bash
curl -X PUT https://api.example.com/api/ReferenceTables/Equipment/equipment-123e4567-e89b-12d3-a456-426614174000 \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"name": "Heavy Resistance Bands"}'
```

### Deleting Equipment
```bash
curl -X DELETE https://api.example.com/api/ReferenceTables/Equipment/equipment-123e4567-e89b-12d3-a456-426614174000 \
  -H "Authorization: Bearer {token}"
```

## Implementation Notes

- Equipment CRUD operations follow the established patterns from MuscleGroups CRUD
- All timestamps use UTC
- Repository methods include built-in SaveChangesAsync calls
- Integration tests demonstrate full CRUD flow (some skipped due to in-memory DB limitations)
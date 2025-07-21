# Muscle Groups API Contracts

## Data Transfer Objects (DTOs)

### MuscleGroupDto
```typescript
interface MuscleGroupDto {
  id: string;           // Format: "musclegroup-{guid}"
  name: string;         // Unique muscle group name
  bodyPartId: string;   // Format: "bodypart-{guid}"
  bodyPartName?: string;// Associated body part name
  isActive: boolean;    // Soft delete status
  createdAt: string;    // ISO 8601 datetime
  updatedAt?: string;   // ISO 8601 datetime (nullable)
}
```

### CreateMuscleGroupDto
```typescript
interface CreateMuscleGroupDto {
  name: string;         // Required, must be unique
  bodyPartId: string;   // Required, must be valid body part ID
}
```

### UpdateMuscleGroupDto
```typescript
interface UpdateMuscleGroupDto {
  name: string;         // Required, must be unique
  bodyPartId: string;   // Required, must be valid body part ID
}
```

## API Endpoints

### Get All Muscle Groups
**Endpoint:** `GET /api/ReferenceTables/MuscleGroups`
**Authorization:** Any authenticated user
**Query Parameters:**
- `pageNumber`: number (default: 1)
- `pageSize`: number (default: 10, max: 100)
- `searchTerm`: string (optional)
- `sortBy`: string (default: "name")
- `sortDirection`: "asc" | "desc" (default: "asc")
- `includeInactive`: boolean (default: false)

**Response:**
```json
{
  "items": [
    {
      "id": "musclegroup-123e4567-e89b-12d3-a456-426614174000",
      "name": "Quadriceps",
      "bodyPartId": "bodypart-456e7890-a12b-34c5-d678-901234567890",
      "bodyPartName": "Legs",
      "isActive": true,
      "createdAt": "2025-07-01T10:00:00Z",
      "updatedAt": "2025-07-01T12:30:00Z"
    }
  ],
  "totalCount": 15,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 2,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### Get Muscle Group by ID
**Endpoint:** `GET /api/ReferenceTables/MuscleGroups/{id}`
**Authorization:** Any authenticated user
**Response:** Single `MuscleGroupDto`

### Get Muscle Group by Name
**Endpoint:** `GET /api/ReferenceTables/MuscleGroups/ByValue/{value}`
**Authorization:** Any authenticated user
**Response:** Single `MuscleGroupDto`

### Get Muscle Groups by Body Part
**Endpoint:** `GET /api/ReferenceTables/MuscleGroups/ByBodyPart/{bodyPartId}`
**Authorization:** Any authenticated user
**Response:** Array of `MuscleGroupDto[]`

### Create Muscle Group
**Endpoint:** `POST /api/ReferenceTables/MuscleGroups`
**Authorization:** Admin only (`PT-Tier` or `Admin-Tier`)
**Request Body:** `CreateMuscleGroupDto`
**Response:** Created `MuscleGroupDto` with 201 status

### Update Muscle Group
**Endpoint:** `PUT /api/ReferenceTables/MuscleGroups/{id}`
**Authorization:** Admin only (`PT-Tier` or `Admin-Tier`)
**Request Body:** `UpdateMuscleGroupDto`
**Response:** Updated `MuscleGroupDto`

### Delete Muscle Group
**Endpoint:** `DELETE /api/ReferenceTables/MuscleGroups/{id}`
**Authorization:** Admin only (`PT-Tier` or `Admin-Tier`)
**Response:** 204 No Content

## Error Responses

### 400 Bad Request
- Invalid input data
- Name already exists (for create/update)

### 404 Not Found
- Muscle group not found
- Body part not found

### 409 Conflict
- Cannot delete muscle group in use by exercises

## Business Rules

1. **Unique Names**: Muscle group names must be unique (case-insensitive)
2. **Body Part Association**: Every muscle group must be associated with a valid body part
3. **Soft Delete**: Muscle groups are soft deleted to maintain referential integrity
4. **Exercise Dependencies**: Cannot delete muscle groups that are in use by exercises
5. **ID Format**: All muscle group IDs follow the pattern `musclegroup-{guid}`

## Breaking Changes from Previous Implementation

**Previous (ReferenceDataDto):**
```json
{
  "id": "musclegroup-{guid}",
  "value": "Quadriceps",
  "description": "Front thigh muscles"
}
```

**Current (MuscleGroupDto):**
- `value` field renamed to `name`
- `description` field removed
- Added `bodyPartId` and `bodyPartName` fields
- Added `isActive`, `createdAt`, and `updatedAt` fields

## Cache Considerations

- Muscle groups are reference data that changes infrequently
- Implement client-side caching with appropriate TTL
- Invalidate cache on any CRUD operations
- Consider using ETags for efficient cache validation
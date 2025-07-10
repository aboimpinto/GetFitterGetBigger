# Exercise Management API Specification

## Overview
The Exercise Management API provides comprehensive CRUD operations for managing exercises, which are the fundamental building blocks of workouts in the GetFitterGetBigger platform.

## Data Models

### Exercise Entity
The Exercise entity represents a single exercise with all its metadata and relationships.

**Core Properties:**
- `id` - Unique identifier (format: "exercise-{guid}")
- `name` - Exercise name (max 100 characters)
- `description` - Detailed description
- `coachNotes` - Array of coaching instructions
- `exerciseTypes` - Array of exercise type references
- `videoUrl` - Optional video demonstration URL
- `imageUrl` - Optional image URL
- `isUnilateral` - Boolean indicating if exercise works one side at a time
- `isActive` - Boolean for soft delete functionality
- `difficulty` - Reference to difficulty level
- `muscleGroups` - Array of muscle group assignments with roles
- `equipment` - Array of required equipment references
- `movementPatterns` - Array of movement pattern references
- `bodyParts` - Array of body part references
- `kineticChain` - Optional kinetic chain type (required for non-REST exercises)
- `exerciseWeightType` - Defines how exercise handles weight assignments

**Related Entities:**
- `CoachNote` - Individual coaching instruction with text and order
- `MuscleGroupAssignment` - Links muscle groups with their role (Primary/Secondary/Stabilizer)

**Exercise Weight Types:**
- `BODYWEIGHT_ONLY` - No external weight allowed
- `BODYWEIGHT_OPTIONAL` - Can add weight to bodyweight
- `WEIGHT_REQUIRED` - Must specify external weight
- `MACHINE_WEIGHT` - Machine-based weight
- `NO_WEIGHT` - Weight not applicable (e.g., stretches)

## Endpoints

### 1. List Exercises
```http
GET /api/exercises
```

**Query Parameters:**
- `page` (number, optional): Page number (default: 1)
- `pageSize` (number, optional): Items per page (default: 10, max: 50)
- `name` (string, optional): Filter by name (case-insensitive, partial match)
- `difficultyId` (string, optional): Filter by difficulty level ID

**Response:**
```json
{
  "items": Exercise[],
  "currentPage": number,
  "pageSize": number,
  "totalCount": number,
  "totalPages": number,
  "hasPreviousPage": boolean,
  "hasNextPage": boolean
}
```

### 2. Get Exercise by ID
```http
GET /api/exercises/{id}
```

**Response:** Exercise object

### 3. Create Exercise
```http
POST /api/exercises
```

**Request Body:**
```json
{
  "name": string,
  "description": string,
  "coachNotes": [{ "text": string, "order": number }],
  "exerciseTypeIds": string[],
  "videoUrl": string?,
  "imageUrl": string?,
  "isUnilateral": boolean,
  "difficultyId": string,
  "kineticChainId": string?, // Required for non-REST exercises
  "muscleGroups": [{ "muscleGroupId": string, "muscleRoleId": string }],
  "equipmentIds": string[],
  "bodyPartIds": string[],
  "movementPatternIds": string[]
}
```

**Response:**
- Status: 201 Created
- Location header: `/api/exercises/{id}`
- Body: Created Exercise object

### 4. Update Exercise
```http
PUT /api/exercises/{id}
```

**Request Body:** Same as Create Exercise

**Response:** 204 No Content

### 5. Delete Exercise
```http
DELETE /api/exercises/{id}
```

**Business Logic:**
- Soft delete: Sets `isActive = false`
- Maintains referential integrity
- Exercise remains in database but excluded from active listings

**Response:** 204 No Content

## Business Logic

### Validation Rules
1. **Name**: Required, max 100 characters, must be unique among active exercises
2. **Description**: Required
3. **Exercise Types**: 
   - Optional array
   - REST type cannot be combined with other types
4. **Unilateral Flag**: Required boolean
5. **Difficulty**: Required reference to existing difficulty level
6. **Kinetic Chain**:
   - Required for non-REST exercises
   - Must be null for REST exercises
7. **URLs**: Must be valid URLs when provided
8. **Reference Data**: All IDs must reference existing, active entities

### Exercise Types Logic
- An exercise can have multiple types (Warmup, Workout, Cooldown)
- REST type is exclusive and cannot be combined
- Types determine valid link relationships (see Exercise Linking feature)

### Soft Delete Strategy
- Exercises are never hard deleted
- `isActive = false` removes from active listings
- Preserves workout history and data integrity

## Security

### Authentication
- All endpoints require Bearer token authentication
- Token must be included in Authorization header

### Authorization
- **Read Access**: All authenticated users
- **Create/Update/Delete**: Requires `PT-Tier` or `Admin-Tier` claim

### Data Access Rules
- Users can only access exercises from their organization
- Admin users can access all exercises
- Audit logging for all modifications

## Integration Points

### Reference Tables
Required reference data endpoints:
- `/api/reference-tables/difficulty-levels`
- `/api/reference-tables/muscle-groups`
- `/api/reference-tables/muscle-roles`
- `/api/reference-tables/equipment`
- `/api/reference-tables/body-parts`
- `/api/reference-tables/movement-patterns`
- `/api/reference-tables/exercise-types`
- `/api/reference-tables/kinetic-chain-types`

### Related Features
- **Exercise Linking**: Manages relationships between exercises
- **Workout Builder**: Consumes exercises for workout creation
- **Media Upload**: Handles exercise images and videos

### External Services
- CDN for image/video storage
- Video streaming service for exercise demonstrations

## Error Handling

### Standard Error Response
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Error Title",
  "status": 400,
  "errors": {
    "fieldName": ["Error message"]
  }
}
```

### Common Error Scenarios
- 400: Validation errors, business rule violations
- 401: Missing or invalid authentication
- 403: Insufficient permissions
- 404: Exercise not found
- 409: Name conflict (duplicate)
- 500: Server errors

## Performance Considerations

### Caching Strategy
- Reference data: Cache for 24 hours
- Exercise list: Cache for 5 minutes with proper ETags
- Individual exercises: Cache for 1 hour

### Pagination
- Default page size: 10
- Maximum page size: 50
- Use cursor-based pagination for large datasets

### Database Optimization
- Indexes on: name, isActive, difficulty, exercise types
- Eager loading for related entities
- Query optimization for complex filters
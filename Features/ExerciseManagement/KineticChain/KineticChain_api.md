# Kinetic Chain API Specification

## Overview
The Kinetic Chain feature adds biomechanical categorization to exercises, distinguishing between compound movements (multi-muscle) and isolation movements (single-muscle). This enhancement helps trainers and users better understand exercise complexity and plan appropriate workouts.

## Data Models

### KineticChainType Entity
```typescript
interface KineticChainType {
  id: string; // Format: "kineticchaintype-{guid}"
  name: string; // "Compound" or "Isolation"
  description: string;
  order: number;
  isActive: boolean;
}
```

### Exercise Entity Enhancement
```typescript
interface Exercise {
  // ... existing fields ...
  kineticChain?: KineticChainType; // Required for non-REST exercises
}
```

## API Changes

### Exercise Endpoints Enhancement

#### GET /api/exercises
**Response Enhancement:**
```json
{
  "items": [{
    "id": "exercise-123",
    "name": "Bench Press",
    "kineticChain": {
      "id": "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4",
      "name": "Compound",
      "description": "Exercises that work multiple muscle groups",
      "order": 1,
      "isActive": true
    }
    // ... other fields
  }]
}
```

#### POST /api/exercises
**Request Enhancement:**
```json
{
  "name": "Bench Press",
  "kineticChainId": "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4",
  // ... other fields
}
```

#### PUT /api/exercises/{id}
**Request Enhancement:**
- Same as POST - includes `kineticChainId` field

### Reference Table Endpoint
```http
GET /api/referenceTables/kineticChainTypes
```

**Response:**
```json
{
  "data": [
    {
      "id": "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4",
      "name": "Compound",
      "description": "Exercises that work multiple muscle groups",
      "order": 1,
      "isActive": true
    },
    {
      "id": "kineticchaintype-2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b",
      "name": "Isolation",
      "description": "Exercises that work a single muscle group",
      "order": 2,
      "isActive": true
    }
  ]
}
```

## Business Logic

### Validation Rules

#### For Non-REST Exercises
- `kineticChainId` is **REQUIRED**
- Must reference an existing, active KineticChainType
- Validation error (400):
  ```json
  {
    "title": "Validation failed",
    "errors": {
      "KineticChainId": ["Kinetic chain is required for non-rest exercises"]
    }
  }
  ```

#### For REST Exercises
- `kineticChainId` **MUST be null**
- Validation error (400):
  ```json
  {
    "title": "Validation failed",
    "errors": {
      "KineticChainId": ["Kinetic chain must be null for rest exercises"]
    }
  }
  ```

### Business Rules
1. REST exercises cannot have a kinetic chain classification
2. All other exercise types must specify kinetic chain
3. Kinetic chain cannot be changed if exercise is used in active workouts (future enhancement)
4. System maintains two primary types: Compound and Isolation

## Migration Strategy

### Database Migration
1. Add non-nullable foreign key to Exercise table
2. Seed KineticChainType reference data
3. Update existing exercises with appropriate classifications
4. Add database constraints for REST exercise validation

### Data Classification
Default classifications for existing exercises:
- Multi-joint movements → Compound
- Single-joint movements → Isolation
- REST exercises → NULL
- Manual review required for edge cases

## Security
- Read access: All authenticated users
- Modification: Not applicable (reference data)
- Reference data managed by system administrators

## Performance Considerations
- KineticChainType data should be cached (24-hour TTL)
- Include in exercise query eager loading
- Index on Exercise.KineticChainId for filtering

## Integration Points
- Exercise search/filter by kinetic chain
- Workout builder can group by kinetic chain
- Analytics on compound vs isolation exercise usage
- Training program templates based on kinetic chain
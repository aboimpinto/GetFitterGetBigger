# Exercise Kinetic Chain Field

> **⚠️ DEPRECATED**: This documentation has been migrated to the new Features structure.
> Please refer to:
> - API: `/Features/ExerciseManagement/KineticChain/KineticChain_api.md`
> - Admin: `/Features/ExerciseManagement/KineticChain/KineticChain_admin.md`
> - Clients: `/Features/ExerciseManagement/KineticChain/KineticChain_clients.md`
> This file is maintained for backward compatibility only.

## Overview
The Kinetic Chain field has been added to all Exercise endpoints to categorize exercises based on their biomechanical movement patterns. This field helps trainers and clients understand whether an exercise involves compound movements (multi-muscle) or isolation movements (single-muscle).

## Implementation Details
- **API Feature**: FEAT-019
- **Status**: IN_PROGRESS
- **Added**: 2025-07-07

## Field Specification

### Data Type
- **Field Name**: `kineticChain` (in responses) / `kineticChainId` (in requests)
- **Type**: Reference to KineticChainType entity
- **Required**: Yes for non-REST exercises, must be null for REST exercises

### Available Values
1. **Compound**
   - ID: `kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4`
   - Description: "Exercises that work multiple muscle groups"
   - Order: 1

2. **Isolation**
   - ID: `kineticchaintype-2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b`
   - Description: "Exercises that work a single muscle group"
   - Order: 2

## Endpoint Changes

### GET /api/exercises
Response now includes `kineticChain` object:
```json
{
  "data": [{
    "id": "...",
    "name": "Bench Press",
    "kineticChain": {
      "id": "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4",
      "name": "Compound",
      "description": "Exercises that work multiple muscle groups",
      "order": 1,
      "isActive": true
    },
    // ... other fields
  }]
}
```

### GET /api/exercises/{id}
Response includes the same `kineticChain` object structure.

### POST /api/exercises
Request requires `kineticChainId`:
```json
{
  "name": "Bench Press",
  "kineticChainId": "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4",
  // ... other required fields
}
```

### PUT /api/exercises/{id}
Request includes `kineticChainId`:
```json
{
  "name": "Updated Exercise Name",
  "kineticChainId": "kineticchaintype-2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b",
  // ... other fields
}
```

## Validation Rules

### For Non-REST Exercises
- `kineticChainId` is **REQUIRED**
- Must reference an existing, active KineticChainType
- Error response (400 Bad Request):
  ```json
  {
    "title": "Validation failed",
    "errors": {
      "KineticChainId": ["Kinetic chain is required for non-rest exercises"]
    }
  }
  ```

### For REST Exercises
- `kineticChainId` **MUST be null**
- Error response (400 Bad Request):
  ```json
  {
    "title": "Validation failed",
    "errors": {
      "KineticChainId": ["Kinetic chain must be null for rest exercises"]
    }
  }
  ```

## Reference Table Endpoint
Kinetic chain types can be fetched from:
```
GET /api/referenceTables/kineticChainTypes
```

Response format:
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

## Frontend Implementation Requirements

### Admin Application
- Add Kinetic Chain dropdown to Exercise create/edit forms
- Display Kinetic Chain in exercise lists and details
- Implement validation based on exercise type (REST vs non-REST)
- Cache kinetic chain types after initial fetch

### Client Applications
- Display Kinetic Chain information in exercise views
- Use for filtering/categorizing exercises
- Show appropriate icons or visual indicators

## Migration Notes
- Existing exercises will need to be updated with kinetic chain values
- Database migration adds non-nullable foreign key (with initial data population)
- No breaking changes for existing API consumers (field is additive)

## Related Features
- Similar pattern to Difficulty Level implementation
- Part of exercise categorization improvements
- Enhances workout planning capabilities
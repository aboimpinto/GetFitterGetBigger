# Exercise Weight Type Reference Table

## Overview

The Exercise Weight Type reference table is a core system table that defines how exercises handle weight assignments. This is a fixed reference table that should not grow dynamically.

## Table Structure

```sql
CREATE TABLE ExerciseWeightTypes (
    Id INT PRIMARY KEY,
    Code VARCHAR(50) NOT NULL UNIQUE,
    Name VARCHAR(100) NOT NULL,
    Description TEXT,
    UIBehavior TEXT,
    ValidationRules TEXT,
    IsActive BOOLEAN DEFAULT TRUE,
    SortOrder INT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);
```

## Reference Data

```json
[
  {
    "id": 1,
    "code": "BODYWEIGHT_ONLY",
    "name": "Bodyweight Only",
    "description": "Exercises that cannot have external weight added",
    "uiBehavior": "Hide weight input field",
    "validationRules": "TargetWeight must be null or zero",
    "sortOrder": 1,
    "isActive": true
  },
  {
    "id": 2,
    "code": "BODYWEIGHT_OPTIONAL",
    "name": "Bodyweight Optional",
    "description": "Exercises that can be performed with or without additional weight",
    "uiBehavior": "Show optional weight field with + prefix",
    "validationRules": "TargetWeight can be null, zero, or positive",
    "sortOrder": 2,
    "isActive": true
  },
  {
    "id": 3,
    "code": "WEIGHT_REQUIRED",
    "name": "Weight Required",
    "description": "Exercises that must have external weight specified",
    "uiBehavior": "Show required weight field",
    "validationRules": "TargetWeight must be greater than zero",
    "sortOrder": 3,
    "isActive": true
  },
  {
    "id": 4,
    "code": "MACHINE_WEIGHT",
    "name": "Machine Weight",
    "description": "Exercises performed on machines with weight stacks",
    "uiBehavior": "Show weight field with machine context",
    "validationRules": "TargetWeight must be greater than zero",
    "sortOrder": 4,
    "isActive": true
  },
  {
    "id": 5,
    "code": "NO_WEIGHT",
    "name": "No Weight",
    "description": "Exercises that do not use weight as a metric",
    "uiBehavior": "Hide weight input field",
    "validationRules": "TargetWeight must be null or zero",
    "sortOrder": 5,
    "isActive": true
  }
]
```

## API Endpoints

### Get All Exercise Weight Types
```
GET /api/reference/exercise-weight-types
```

Response:
```json
{
    "data": [
        {
            "id": 1,
            "code": "BODYWEIGHT_ONLY",
            "name": "Bodyweight Only",
            "description": "Exercises that cannot have external weight added"
        },
        // ... other types
    ]
}
```

### Get Single Exercise Weight Type
```
GET /api/reference/exercise-weight-types/{id}
```

## Usage in Exercise Entity

### Exercise Model Structure
```json
{
  "Exercise": {
    "id": "number",
    "name": "string",
    "exerciseWeightTypeId": "number",
    "exerciseWeightType": "ExerciseWeightType"
  }
}
```

### ExerciseWeightType Model Structure
```json
{
  "ExerciseWeightType": {
    "id": "number",
    "code": "string",
    "name": "string",
    "description": "string",
    "uiBehavior": "string",
    "validationRules": "string"
  }
}
```

## Validation Logic

### Weight Assignment Validation Rules

```
FUNCTION validateWeightAssignment(exercise, targetWeight):
    weightTypeCode = exercise.exerciseWeightType.code
    
    IF weightTypeCode IN ["BODYWEIGHT_ONLY", "NO_WEIGHT"]:
        RETURN targetWeight IS NULL OR targetWeight = 0
        
    ELSE IF weightTypeCode = "BODYWEIGHT_OPTIONAL":
        RETURN targetWeight IS NULL OR targetWeight >= 0
        
    ELSE IF weightTypeCode IN ["WEIGHT_REQUIRED", "MACHINE_WEIGHT"]:
        RETURN targetWeight > 0
        
    ELSE:
        THROW ERROR "Unknown weight type: " + weightTypeCode
```

### Validation Rules by Type

| Weight Type Code | Valid Target Weight Values |
|-----------------|---------------------------|
| BODYWEIGHT_ONLY | null or 0 |
| NO_WEIGHT | null or 0 |
| BODYWEIGHT_OPTIONAL | null, 0, or any positive number |
| WEIGHT_REQUIRED | positive numbers only (> 0) |
| MACHINE_WEIGHT | positive numbers only (> 0) |

## Admin Interface Requirements

1. **Exercise Management:**
   - Dropdown to select Exercise Weight Type when creating/editing exercises
   - Show current weight type in exercise list views
   - Bulk update functionality for migrating existing exercises

2. **Reference Data Management:**
   - Read-only view of Exercise Weight Types
   - No create/update/delete operations (fixed reference table)
   - Ability to toggle IsActive flag if needed

## Client Interface Requirements

1. **Workout Creation:**
   - Dynamic weight input based on exercise weight type
   - Clear visual indicators of weight requirements
   - Contextual help text

2. **Exercise Selection:**
   - Filter exercises by weight type
   - Show weight type badge/icon in exercise lists
   - Group exercises by weight type in selection interfaces

## Migration Notes

1. **Initial Setup:**
   - Insert reference data as part of deployment
   - No user-configurable additions allowed

2. **Exercise Migration:**
   - Default mapping rules:
     - Exercises with "Barbell", "Dumbbell", "Cable" → WEIGHT_REQUIRED
     - Exercises with "Weighted" prefix → BODYWEIGHT_OPTIONAL
     - Common bodyweight exercises → BODYWEIGHT_ONLY
     - Stretches, mobility work → NO_WEIGHT
     - Machine exercises → MACHINE_WEIGHT

3. **Data Integrity:**
   - Add foreign key constraint after migration
   - Ensure all exercises have valid weight type assigned
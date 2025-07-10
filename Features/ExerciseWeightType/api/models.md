# Exercise Weight Type Data Models

## Overview
This document defines the data structures for the Exercise Weight Type feature, including entities, DTOs, and validation models.

## Core Entity

### ExerciseWeightType
The primary entity representing an exercise weight type in the system.

```json
{
  "ExerciseWeightType": {
    "id": "string (guid)",
    "code": "string",
    "name": "string",
    "description": "string",
    "isActive": "boolean",
    "displayOrder": "integer",
    "createdAt": "datetime",
    "updatedAt": "datetime"
  }
}
```

**Field Specifications**:
- `id`: Unique identifier (GUID format)
- `code`: Immutable string identifier (max 50 chars, uppercase with underscores)
- `name`: Display name (max 100 chars)
- `description`: Detailed description (max 500 chars)
- `isActive`: Whether this type is currently available for use
- `displayOrder`: Integer for sorting in UI (1-based)
- `createdAt`: UTC timestamp of record creation
- `updatedAt`: UTC timestamp of last modification

**Constraints**:
- `code` must be unique across all records
- `code` cannot be modified after creation
- `displayOrder` must be unique when `isActive` is true

## Data Transfer Objects (DTOs)

### ExerciseWeightTypeDto
Used for API responses when returning weight type information.

```json
{
  "id": "string (guid)",
  "code": "string",
  "name": "string",
  "description": "string",
  "isActive": "boolean",
  "displayOrder": "integer"
}
```

### ExerciseWeightTypeSummaryDto
Lightweight DTO used when weight type is included in other entities (e.g., Exercise).

```json
{
  "id": "string (guid)",
  "code": "string",
  "name": "string"
}
```

## Predefined Reference Data

### System Weight Types
The system includes five predefined weight types that cannot be modified or deleted:

```json
[
  {
    "code": "BODYWEIGHT_ONLY",
    "name": "Bodyweight Only",
    "description": "Exercises that cannot have external weight added",
    "displayOrder": 1,
    "isActive": true
  },
  {
    "code": "BODYWEIGHT_OPTIONAL",
    "name": "Bodyweight Optional",
    "description": "Exercises that can be performed with or without additional weight",
    "displayOrder": 2,
    "isActive": true
  },
  {
    "code": "WEIGHT_REQUIRED",
    "name": "Weight Required",
    "description": "Exercises that must have external weight specified",
    "displayOrder": 3,
    "isActive": true
  },
  {
    "code": "MACHINE_WEIGHT",
    "name": "Machine Weight",
    "description": "Exercises performed on machines with weight stacks",
    "displayOrder": 4,
    "isActive": true
  },
  {
    "code": "NO_WEIGHT",
    "name": "No Weight",
    "description": "Exercises that do not use weight as a metric",
    "displayOrder": 5,
    "isActive": true
  }
]
```

## Validation Models

### Weight Assignment Validation
When assigning weight to an exercise in a workout, the following validation applies:

```json
{
  "WorkoutExerciseWeightValidation": {
    "exerciseId": "string (guid)",
    "exerciseWeightTypeCode": "string",
    "assignedWeight": "number (nullable)",
    "validationResult": {
      "isValid": "boolean",
      "errorMessage": "string (nullable)"
    }
  }
}
```

### Weight Type Rules
```json
{
  "WeightTypeRule": {
    "code": "string",
    "allowsNoWeight": "boolean",
    "allowsZeroWeight": "boolean",
    "requiresPositiveWeight": "boolean",
    "maxWeight": "number (nullable)",
    "uiHint": "string"
  }
}
```

**Rule Definitions**:
```json
{
  "BODYWEIGHT_ONLY": {
    "allowsNoWeight": true,
    "allowsZeroWeight": true,
    "requiresPositiveWeight": false,
    "maxWeight": 0,
    "uiHint": "hide_weight_field"
  },
  "BODYWEIGHT_OPTIONAL": {
    "allowsNoWeight": true,
    "allowsZeroWeight": true,
    "requiresPositiveWeight": false,
    "maxWeight": null,
    "uiHint": "show_optional_weight"
  },
  "WEIGHT_REQUIRED": {
    "allowsNoWeight": false,
    "allowsZeroWeight": false,
    "requiresPositiveWeight": true,
    "maxWeight": null,
    "uiHint": "show_required_weight"
  },
  "MACHINE_WEIGHT": {
    "allowsNoWeight": false,
    "allowsZeroWeight": false,
    "requiresPositiveWeight": true,
    "maxWeight": null,
    "uiHint": "show_machine_weight"
  },
  "NO_WEIGHT": {
    "allowsNoWeight": true,
    "allowsZeroWeight": true,
    "requiresPositiveWeight": false,
    "maxWeight": 0,
    "uiHint": "hide_weight_field"
  }
}
```

## Integration Models

### Exercise with Weight Type
When returning exercise information, the weight type is included:

```json
{
  "Exercise": {
    "id": "string (guid)",
    "name": "string",
    "description": "string",
    "exerciseWeightType": {
      "id": "string (guid)",
      "code": "string",
      "name": "string"
    },
    "muscleGroups": ["string"],
    "equipment": ["string"],
    "isActive": "boolean"
  }
}
```

### Workout Exercise Assignment
When creating workout exercises, weight validation is applied:

```json
{
  "WorkoutExercise": {
    "id": "string (guid)",
    "workoutId": "string (guid)",
    "exerciseId": "string (guid)",
    "sets": "integer",
    "reps": "integer",
    "weight": "number (nullable)",
    "restSeconds": "integer",
    "notes": "string (nullable)"
  }
}
```

**Validation Process**:
1. Retrieve exercise's weight type code
2. Apply validation rules based on code
3. Return validation error if weight assignment violates rules

## Database Schema

### ExerciseWeightTypes Table
```json
{
  "tableName": "ExerciseWeightTypes",
  "columns": {
    "Id": {
      "type": "uniqueidentifier",
      "nullable": false,
      "primaryKey": true
    },
    "Code": {
      "type": "nvarchar(50)",
      "nullable": false,
      "unique": true
    },
    "Name": {
      "type": "nvarchar(100)",
      "nullable": false
    },
    "Description": {
      "type": "nvarchar(500)",
      "nullable": true
    },
    "IsActive": {
      "type": "bit",
      "nullable": false,
      "default": true
    },
    "DisplayOrder": {
      "type": "int",
      "nullable": false
    },
    "CreatedAt": {
      "type": "datetime2",
      "nullable": false
    },
    "UpdatedAt": {
      "type": "datetime2",
      "nullable": false
    }
  },
  "indexes": [
    {
      "name": "IX_ExerciseWeightTypes_Code",
      "columns": ["Code"],
      "unique": true
    },
    {
      "name": "IX_ExerciseWeightTypes_DisplayOrder",
      "columns": ["DisplayOrder", "IsActive"]
    }
  ]
}
```

### Exercise Table Modification
```json
{
  "alterTable": "Exercises",
  "addColumn": {
    "ExerciseWeightTypeId": {
      "type": "uniqueidentifier",
      "nullable": false,
      "foreignKey": {
        "references": "ExerciseWeightTypes(Id)",
        "onDelete": "RESTRICT"
      }
    }
  }
}
```

## Migration Support Models

### Bulk Update Request
For migrating existing exercises to use weight types:

```json
{
  "BulkExerciseWeightTypeUpdate": {
    "updates": [
      {
        "exerciseId": "string (guid)",
        "exerciseWeightTypeId": "string (guid)"
      }
    ]
  }
}
```

### Migration Mapping Rules
Default mappings for automatic migration:

```json
{
  "MigrationRules": [
    {
      "condition": "name contains 'Barbell' OR 'Dumbbell' OR 'Cable'",
      "assignWeightTypeCode": "WEIGHT_REQUIRED"
    },
    {
      "condition": "name starts with 'Weighted'",
      "assignWeightTypeCode": "BODYWEIGHT_OPTIONAL"
    },
    {
      "condition": "name contains 'Push-up' OR 'Pull-up' OR 'Dip' OR 'Plank'",
      "assignWeightTypeCode": "BODYWEIGHT_ONLY"
    },
    {
      "condition": "category equals 'Stretching' OR 'Mobility'",
      "assignWeightTypeCode": "NO_WEIGHT"
    },
    {
      "condition": "equipment contains 'Machine'",
      "assignWeightTypeCode": "MACHINE_WEIGHT"
    }
  ]
}
```
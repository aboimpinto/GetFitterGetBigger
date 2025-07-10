# Exercise Weight Type Reference Table

## Overview

The Exercise Weight Type reference table is a core system table that defines how exercises handle weight assignments. This is a fixed reference table that should not grow dynamically.

## Conceptual Data Structure

```
ExerciseWeightType:
    - Identifier (unique)
    - Code (unique, immutable)
    - Name 
    - Description
    - IsActive indicator
    - Display order
    - Audit fields (creation time, last update)
```

## Reference Data Values

### BODYWEIGHT_ONLY
- Code: BODYWEIGHT_ONLY
- Name: Bodyweight Only
- Description: Exercises that cannot have external weight added
- Display Order: 1
- Active: Yes

### BODYWEIGHT_OPTIONAL
- Code: BODYWEIGHT_OPTIONAL
- Name: Bodyweight Optional
- Description: Exercises that can be performed with or without additional weight
- Display Order: 2
- Active: Yes

### WEIGHT_REQUIRED
- Code: WEIGHT_REQUIRED
- Name: Weight Required
- Description: Exercises that must have external weight specified
- Display Order: 3
- Active: Yes

### MACHINE_WEIGHT
- Code: MACHINE_WEIGHT
- Name: Machine Weight
- Description: Exercises performed on machines with weight stacks
- Display Order: 4
- Active: Yes

### NO_WEIGHT
- Code: NO_WEIGHT
- Name: No Weight
- Description: Exercises that do not use weight as a metric
- Display Order: 5
- Active: Yes

## Operations

### Retrieve All Exercise Weight Types
- Operation: List all weight types
- Access: Public (no authentication required)
- Returns: Collection of weight type records

### Retrieve Single Exercise Weight Type
- Operation: Get specific weight type by identifier
- Access: Public (no authentication required)
- Returns: Single weight type record or not found

## Usage in Exercise Entity

### Relationship with Exercise Entity
```
Exercise:
    - Has one ExerciseWeightType (required)
    - References ExerciseWeightType by identifier
    
ExerciseWeightType:
    - Can be referenced by many Exercises
    - Provides weight handling rules for exercises
```

## Validation Logic

**Note:** Weight validation rules and UI behavior are implemented in code based on the `code` field, not stored in the database. This ensures type-safe validation and consistent behavior across all applications.

### Weight Assignment Validation Rules (Implemented in Code)

```
When validating weight for an exercise:
    Get the exercise's weight type code
    
    If code is BODYWEIGHT_ONLY or NO_WEIGHT:
        → Weight must be empty or zero
        
    If code is BODYWEIGHT_OPTIONAL:
        → Weight can be empty, zero, or positive
        
    If code is WEIGHT_REQUIRED or MACHINE_WEIGHT:
        → Weight must be positive (greater than zero)
        
    Otherwise:
        → Invalid weight type code
```

### Validation Rules by Type

| Weight Type Code | Valid Target Weight Values | UI Behavior (Implemented in Code) |
|-----------------|---------------------------|----------------------------------|
| BODYWEIGHT_ONLY | null or 0 | Hide weight input field |
| NO_WEIGHT | null or 0 | Hide weight input field |
| BODYWEIGHT_OPTIONAL | null, 0, or any positive number | Show optional weight field with + prefix |
| WEIGHT_REQUIRED | positive numbers only (> 0) | Show required weight field |
| MACHINE_WEIGHT | positive numbers only (> 0) | Show weight field with machine context |

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
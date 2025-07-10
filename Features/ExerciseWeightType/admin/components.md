# Exercise Weight Type Admin Components

## Overview
This document describes the UI components for managing exercise weight types in the Admin application. These components enable Personal Trainers to assign appropriate weight types to exercises and ensure proper weight validation during workout creation.

## Component Hierarchy

```
ExerciseManagement/
├── ExerciseWeightTypeSelector
│   ├── WeightTypeDropdown
│   └── WeightTypeInfo
├── WeightInputField
│   ├── NumericInput
│   ├── WeightUnitSelector
│   └── ValidationMessage
└── ExerciseWeightTypeBadge
    └── BadgeDisplay
```

## Core Components

### 1. ExerciseWeightTypeSelector
A dropdown component for selecting exercise weight types during exercise creation or editing.

**Purpose**: Allow trainers to assign the appropriate weight type to exercises

**Props**:
```json
{
  "selectedWeightTypeId": "string (guid)",
  "onChange": "function(weightTypeId)",
  "disabled": "boolean",
  "required": "boolean",
  "showDescription": "boolean"
}
```

**Features**:
- Displays all active weight types in a dropdown
- Shows weight type name with optional description tooltip
- Validates selection is made when required
- Preserves selection during form edits

**Visual Design**:
- Standard dropdown with clear labels
- Tooltip icon next to each option showing description
- Visual indicator for currently selected type
- Disabled state styling when not editable

### 2. WeightInputField
Dynamic input field that adapts based on the selected exercise weight type.

**Purpose**: Provide appropriate weight input interface based on exercise weight type rules

**Props**:
```json
{
  "exerciseWeightTypeCode": "string",
  "value": "number (nullable)",
  "onChange": "function(weight)",
  "unit": "string (kg/lbs)",
  "showValidation": "boolean"
}
```

**Behavior by Weight Type**:

| Weight Type | UI Behavior | Placeholder Text |
|-------------|------------|-----------------|
| BODYWEIGHT_ONLY | Hidden | N/A |
| NO_WEIGHT | Hidden | N/A |
| BODYWEIGHT_OPTIONAL | Visible with "+" prefix | "Optional weight" |
| WEIGHT_REQUIRED | Visible with required indicator | "Enter weight" |
| MACHINE_WEIGHT | Visible with machine icon | "Machine setting" |

**Features**:
- Real-time validation based on weight type rules
- Unit conversion support (kg/lbs)
- Decimal input support with appropriate precision
- Clear error messaging for invalid inputs

### 3. ExerciseWeightTypeBadge
A visual indicator showing the weight type of an exercise.

**Purpose**: Quick visual identification of exercise weight requirements

**Props**:
```json
{
  "weightTypeCode": "string",
  "size": "small | medium | large",
  "showTooltip": "boolean"
}
```

**Visual Variants**:
```json
{
  "BODYWEIGHT_ONLY": {
    "color": "blue",
    "icon": "person",
    "text": "BW"
  },
  "BODYWEIGHT_OPTIONAL": {
    "color": "green",
    "icon": "person-plus",
    "text": "BW+"
  },
  "WEIGHT_REQUIRED": {
    "color": "orange",
    "icon": "weight",
    "text": "WT"
  },
  "MACHINE_WEIGHT": {
    "color": "purple",
    "icon": "machine",
    "text": "MCH"
  },
  "NO_WEIGHT": {
    "color": "gray",
    "icon": "none",
    "text": "NW"
  }
}
```

## Integration Components

### 4. ExerciseListWithWeightTypes
Enhanced exercise list showing weight type information.

**Purpose**: Display exercises with their weight type classifications

**Features**:
- Weight type badge in exercise list items
- Filter exercises by weight type
- Sort by weight type
- Bulk weight type update selection

**List Item Display**:
```
[Badge] Exercise Name
        Muscle Groups | Equipment
```

### 5. ExerciseFormWithWeightType
Exercise creation/edit form with integrated weight type selection.

**Purpose**: Manage exercise details including weight type assignment

**Form Fields**:
1. Exercise Name (required)
2. Description (optional)
3. **Weight Type (required)** - Dropdown selector
4. Muscle Groups (required)
5. Equipment (optional)
6. Active Status (checkbox)

**Validation**:
- Weight type must be selected
- Cannot change weight type if exercise is used in active workouts
- Warning message when changing weight type

### 6. WorkoutExerciseWeightInput
Specialized weight input for workout template creation.

**Purpose**: Enter weight for exercises in workout templates with type-aware validation

**Features**:
- Fetches exercise weight type automatically
- Applies appropriate validation rules
- Shows contextual help based on weight type
- Preserves weight values during template edits

**Contextual Help Messages**:
```json
{
  "BODYWEIGHT_ONLY": "This is a bodyweight exercise - no external weight needed",
  "BODYWEIGHT_OPTIONAL": "Add weight for progression (optional)",
  "WEIGHT_REQUIRED": "Please specify the weight to use",
  "MACHINE_WEIGHT": "Enter the machine weight setting",
  "NO_WEIGHT": "This exercise doesn't track weight"
}
```

## Composite Views

### 7. ExerciseWeightTypeManagementPanel
Admin panel for viewing and understanding weight types (read-only).

**Purpose**: Reference panel for understanding weight type system

**Sections**:
1. Weight Type List
   - All weight types with descriptions
   - Visual examples of each type
   - Common exercises for each type

2. Validation Rules Display
   - Table showing weight rules for each type
   - Visual indicators for allowed/prohibited values

3. Migration Tools (Admin only)
   - Bulk update interface
   - Exercise mapping preview
   - Migration progress tracker

## Responsive Design Requirements

### Desktop (≥1024px)
- Full-width forms with labels beside inputs
- Exercise list in table format with all columns visible
- Weight type badges at standard size

### Tablet (768px - 1023px)
- Stacked form layout
- Condensed exercise list with essential columns
- Medium-sized badges

### Mobile (< 768px)
- Single column form layout
- Card-based exercise list
- Small badges with abbreviations
- Full-screen weight type selector

## Accessibility Requirements

1. **Keyboard Navigation**
   - All dropdowns navigable with arrow keys
   - Tab order follows logical flow
   - Enter key submits forms

2. **Screen Reader Support**
   - Descriptive labels for all inputs
   - ARIA labels for badges and icons
   - Error messages announced

3. **Visual Indicators**
   - Not solely reliant on color
   - Icons accompany color coding
   - High contrast mode support

## State Management

### Component States
```json
{
  "ExerciseWeightTypeState": {
    "weightTypes": "array of weight type objects",
    "selectedWeightTypeId": "string (guid)",
    "isLoading": "boolean",
    "error": "string (nullable)"
  },
  
  "WeightInputState": {
    "value": "number (nullable)",
    "unit": "kg | lbs",
    "isValid": "boolean",
    "validationMessage": "string (nullable)"
  }
}
```

### Data Flow
1. Weight types loaded on app initialization
2. Cached for session duration
3. Selected weight type triggers UI updates
4. Validation occurs on weight input change
5. Form submission validates all weight assignments

## Error Handling

### User-Friendly Messages
```json
{
  "WEIGHT_REQUIRED_MISSING": "Please enter a weight for this exercise",
  "BODYWEIGHT_WEIGHT_SPECIFIED": "Bodyweight exercises cannot have external weight",
  "INVALID_WEIGHT_VALUE": "Please enter a valid weight greater than 0",
  "WEIGHT_TYPE_NOT_SELECTED": "Please select a weight type for this exercise"
}
```

### Error Display
- Inline validation messages below inputs
- Toast notifications for save errors
- Form-level error summary for multiple issues
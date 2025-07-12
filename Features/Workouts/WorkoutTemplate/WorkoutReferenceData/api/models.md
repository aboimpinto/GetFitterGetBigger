# Workout Reference Data API Models

This document defines the data models and DTOs used in the Workout Reference Data API. All models follow the established reference data patterns with technology-agnostic JSON serialization.

## Base Reference Data Pattern

All reference tables inherit from the standard reference data base pattern:

```json
{
  "id": "string (guid)",
  "value": "string",
  "description": "string",
  "displayOrder": "number",
  "isActive": "boolean"
}
```

## WorkoutObjective Model

Defines training goals and objectives for workout templates.

### Entity Model
```json
{
  "workoutObjectiveId": "string (guid)",
  "value": "string",
  "description": "string",
  "displayOrder": "number",
  "isActive": "boolean"
}
```

### Field Specifications
| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| workoutObjectiveId | GUID | Required, Primary Key | Unique identifier for the objective |
| value | String | Required, 1-100 chars, Unique | Display name (e.g., "Muscular Strength") |
| description | String | Required, 1-500 chars | Detailed explanation with programming guidance |
| displayOrder | Integer | Required, Positive, Unique | UI ordering sequence |
| isActive | Boolean | Required, Default: true | Visibility flag |

### Standard Values
```json
[
  {
    "value": "Muscular Strength",
    "description": "Develops maximum force production capabilities. Typical programming includes 1-5 reps per set, 3-5 sets total, 3-5 minute rest periods between sets, and 85-100% intensity of 1RM. Focus on heavy compound movements with excellent form and full recovery between efforts.",
    "displayOrder": 1
  },
  {
    "value": "Hypertrophy",
    "description": "Promotes muscle size increase through controlled volume and moderate intensity. Typical programming includes 6-12 reps per set, 3-4 sets total, 1-3 minute rest periods, and 65-85% intensity of 1RM. Emphasizes time under tension and metabolic stress.",
    "displayOrder": 2
  },
  {
    "value": "Power",
    "description": "Develops force production at high velocity for explosive movements. Typical programming includes 1-6 reps per set, 3-5 sets total, 3-5 minute rest periods, with emphasis on explosive movement quality rather than specific intensity percentages. Focus on speed and power output.",
    "displayOrder": 3
  },
  {
    "value": "Muscular Endurance",
    "description": "Builds capacity for sustained muscle contraction over time. Typical programming includes 15+ reps per set, 2-3 sets total, 30-90 second rest periods, and 50-65% intensity of 1RM. Emphasizes work capacity and fatigue resistance.",
    "displayOrder": 4
  },
  {
    "value": "Cardiovascular Conditioning",
    "description": "Improves cardiovascular system efficiency and capacity. Includes both HIIT (High-Intensity Interval Training) and LISS (Low-Intensity Steady State) methodologies. Programming varies widely based on specific protocol but emphasizes heart rate zones and metabolic adaptation.",
    "displayOrder": 5
  },
  {
    "value": "Flexibility & Mobility",
    "description": "Enhances range of motion and movement quality. Includes dynamic warm-ups, static stretching, and mobility work. Typically time-based rather than rep-based, focusing on position holds and movement flow rather than traditional sets and reps.",
    "displayOrder": 6
  },
  {
    "value": "General Fitness",
    "description": "Maintains overall health and fitness across multiple components. Balanced approach combining elements of strength, endurance, mobility, and conditioning. Programming varies to address all fitness components without specializing in any single objective.",
    "displayOrder": 7
  }
]
```

## WorkoutCategory Model

Organizes workouts by primary focus area and muscle groups.

### Entity Model
```json
{
  "workoutCategoryId": "string (guid)",
  "value": "string",
  "description": "string",
  "icon": "string",
  "color": "string",
  "primaryMuscleGroups": "string",
  "displayOrder": "number",
  "isActive": "boolean"
}
```

### Field Specifications
| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| workoutCategoryId | GUID | Required, Primary Key | Unique identifier for the category |
| value | String | Required, 1-50 chars, Unique | Display name (e.g., "Arms", "HIIT") |
| description | String | Required, 1-300 chars | Category overview and muscle groups |
| icon | String | Required, 1-50 chars, Unique | Icon identifier for UI display |
| color | String | Required, Hex color | Color code for UI theming |
| primaryMuscleGroups | String | Required, 1-200 chars | Comma-separated muscle groups |
| displayOrder | Integer | Required, Positive, Unique | UI ordering sequence |
| isActive | Boolean | Required, Default: true | Visibility flag |

### Standard Values
```json
[
  {
    "value": "HIIT",
    "description": "Cardiovascular conditioning with high-intensity bursts and short rest periods. Provides full body engagement through time-based exercises designed to improve cardiovascular efficiency and metabolic conditioning.",
    "icon": "timer-icon",
    "color": "#FF6B35",
    "primaryMuscleGroups": "Full Body",
    "displayOrder": 1
  },
  {
    "value": "Arms",
    "description": "Upper arm muscle development focusing on biceps, triceps, and forearms. Common exercises include curls, extensions, and pressing movements designed to build arm strength and definition.",
    "icon": "bicep-icon",
    "color": "#4ECDC4",
    "primaryMuscleGroups": "Biceps, Triceps, Forearms",
    "displayOrder": 2
  },
  {
    "value": "Legs",
    "description": "Lower body strength and development targeting quadriceps, glutes, hamstrings, and calves. Features compound movements like squats, lunges, and deadlifts for comprehensive lower body training.",
    "icon": "leg-icon",
    "color": "#45B7D1",
    "primaryMuscleGroups": "Quadriceps, Glutes, Hamstrings, Calves",
    "displayOrder": 3
  },
  {
    "value": "Abs & Core",
    "description": "Core stability and abdominal strength development targeting rectus abdominis, obliques, and transverse abdominis. Includes planks, crunches, and rotational movements for functional core strength.",
    "icon": "core-icon",
    "color": "#F7DC6F",
    "primaryMuscleGroups": "Rectus Abdominis, Obliques, Transverse Abdominis",
    "displayOrder": 4
  },
  {
    "value": "Shoulders",
    "description": "Shoulder strength and stability focusing on anterior, medial, and posterior deltoids. Features presses, raises, and rowing movements to develop well-rounded shoulder strength and mobility.",
    "icon": "shoulder-icon",
    "color": "#BB8FCE",
    "primaryMuscleGroups": "Anterior Deltoids, Medial Deltoids, Posterior Deltoids",
    "displayOrder": 5
  },
  {
    "value": "Back",
    "description": "Posterior chain strength and posture improvement targeting latissimus dorsi, trapezius, and rhomboids. Includes pull-ups, rows, and deadlifts for comprehensive back development and postural health.",
    "icon": "back-icon",
    "color": "#85C1E9",
    "primaryMuscleGroups": "Latissimus Dorsi, Trapezius, Rhomboids",
    "displayOrder": 6
  },
  {
    "value": "Chest",
    "description": "Chest muscle development focusing on pectoralis major and minor. Features push-ups, presses, and fly movements designed to build chest strength, size, and definition.",
    "icon": "chest-icon",
    "color": "#F8C471",
    "primaryMuscleGroups": "Pectoralis Major, Pectoralis Minor",
    "displayOrder": 7
  },
  {
    "value": "Full Body",
    "description": "Comprehensive training targeting all major muscle groups through compound movements and functional training patterns. Provides balanced development across the entire body in efficient workout sessions.",
    "icon": "fullbody-icon",
    "color": "#82E0AA",
    "primaryMuscleGroups": "All Major Muscle Groups",
    "displayOrder": 8
  }
]
```

## ExecutionProtocol Model

Standardizes set execution methods and training protocols.

### Entity Model
```json
{
  "executionProtocolId": "string (guid)",
  "code": "string",
  "value": "string", 
  "description": "string",
  "timeBase": "boolean",
  "repBase": "boolean",
  "restPattern": "string",
  "intensityLevel": "string",
  "displayOrder": "number",
  "isActive": "boolean"
}
```

### Field Specifications
| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| executionProtocolId | GUID | Required, Primary Key | Unique identifier for the protocol |
| code | String | Required, 1-50 chars, Unique, Uppercase | Programmatic identifier (e.g., "STANDARD") |
| value | String | Required, 1-50 chars, Unique | Display name (e.g., "Standard") |
| description | String | Required, 1-500 chars | Detailed execution instructions |
| timeBase | Boolean | Required | Whether protocol uses time-based metrics |
| repBase | Boolean | Required | Whether protocol uses repetition-based metrics |
| restPattern | String | Required, Enum | Rest pattern type: "Fixed", "Minimal", "Variable", "Micro-rests", "No rest" |
| intensityLevel | String | Required, Enum | Intensity level: "Low", "Medium", "High" |
| displayOrder | Integer | Required, Positive, Unique | UI ordering sequence |
| isActive | Boolean | Required, Default: true | Visibility flag |

### Rest Pattern Enumeration
```json
{
  "restPatterns": [
    "Fixed",
    "Minimal", 
    "Variable",
    "Micro-rests",
    "No rest"
  ]
}
```

### Intensity Level Enumeration
```json
{
  "intensityLevels": [
    "Low",
    "Medium",
    "High"
  ]
}
```

### Standard Values
```json
[
  {
    "code": "STANDARD",
    "value": "Standard",
    "description": "Traditional sets and repetitions with prescribed rest periods. Perform the specified reps, then rest for the prescribed time before the next set. Used for strength training, hypertrophy, and general fitness with fixed rest patterns.",
    "timeBase": false,
    "repBase": true,
    "restPattern": "Fixed",
    "intensityLevel": "Medium",
    "displayOrder": 1
  },
  {
    "code": "AMRAP",
    "value": "AMRAP",
    "description": "Maximum repetitions within a specified time window. Perform as many complete reps as possible within the time limit while maintaining proper form. Used for conditioning, muscular endurance, and metabolic training with minimal rest.",
    "timeBase": true,
    "repBase": true,
    "restPattern": "Minimal",
    "intensityLevel": "High",
    "displayOrder": 2
  },
  {
    "code": "EMOM",
    "value": "EMOM",
    "description": "Perform specified reps at the start of each minute, rest for the remainder. At minute 0, perform target reps, rest until minute 1, repeat. Used for conditioning, skill development, and pacing practice with variable rest based on work completion speed.",
    "timeBase": true,
    "repBase": true,
    "restPattern": "Variable",
    "intensityLevel": "Medium",
    "displayOrder": 3
  },
  {
    "code": "FOR_TIME",
    "value": "For Time",
    "description": "Complete all prescribed work as quickly as possible while maintaining proper form. Perform all sets and reps in minimum time with minimal rest. Used for competitive fitness, time challenges, and high-intensity training.",
    "timeBase": true,
    "repBase": true,
    "restPattern": "Minimal",
    "intensityLevel": "High",
    "displayOrder": 4
  },
  {
    "code": "TABATA",
    "value": "Tabata",
    "description": "20 seconds maximum effort, 10 seconds rest, repeated 8 times (4 minutes total). Work at maximum intensity for 20 seconds, rest 10 seconds, repeat. Used for high-intensity interval training and cardiovascular conditioning.",
    "timeBase": true,
    "repBase": false,
    "restPattern": "Fixed",
    "intensityLevel": "High",
    "displayOrder": 5
  },
  {
    "code": "CLUSTER",
    "value": "Cluster",
    "description": "Brief rest periods within sets to maintain higher intensity. Perform partial reps, rest 10-15 seconds, continue until set complete. Used for strength training, power development, and heavy loading with micro-rests within sets.",
    "timeBase": true,
    "repBase": true,
    "restPattern": "Micro-rests",
    "intensityLevel": "High",
    "displayOrder": 6
  },
  {
    "code": "DROP_SET",
    "value": "Drop Set",
    "description": "Reduce weight immediately after reaching failure to continue the set. Work to failure, immediately reduce weight 10-20%, continue without rest. Used for hypertrophy, muscle exhaustion, and plateau breaking.",
    "timeBase": false,
    "repBase": true,
    "restPattern": "No rest",
    "intensityLevel": "High",
    "displayOrder": 7
  },
  {
    "code": "REST_PAUSE",
    "value": "Rest Pause",
    "description": "Brief rest after failure to squeeze out additional repetitions. Work to failure, rest 10-15 seconds, perform additional reps, repeat as needed. Used for hypertrophy, intensity techniques, and muscle exhaustion.",
    "timeBase": true,
    "repBase": true,
    "restPattern": "Micro-rests",
    "intensityLevel": "High",
    "displayOrder": 8
  }
]
```

## WorkoutMuscles Model

Links workout templates to specific muscles with engagement levels (future use).

### Entity Model
```json
{
  "workoutTemplateId": "string (guid)",
  "muscleId": "string (guid)",
  "engagementLevel": "string",
  "estimatedLoad": "number"
}
```

### Field Specifications
| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| workoutTemplateId | GUID | Required, Foreign Key | Reference to WorkoutTemplate |
| muscleId | GUID | Required, Foreign Key | Reference to existing Muscle table |
| engagementLevel | String | Required, Enum | Muscle involvement level |
| estimatedLoad | Integer | Required, 1-10 range | Relative load on this muscle |

### Engagement Level Enumeration
```json
{
  "engagementLevels": [
    "Primary",
    "Secondary", 
    "Stabilizer"
  ]
}
```

## Common Response Patterns

### Collection Response
All collection endpoints return arrays of entities:
```json
[
  {
    "entityId": "guid",
    "value": "string",
    "description": "string",
    // ... additional fields
  }
]
```

### Single Entity Response
All single entity endpoints return individual objects:
```json
{
  "entityId": "guid",
  "value": "string", 
  "description": "string",
  // ... additional fields
}
```

### Error Response
All endpoints use consistent error format:
```json
{
  "type": "string (uri)",
  "title": "string",
  "status": "number",
  "detail": "string",
  "instance": "string (optional)"
}
```

## Validation Rules

### Common Validations
- **GUID fields**: Must be valid GUID format
- **String fields**: No null values, trimmed of whitespace
- **Integer fields**: Must be positive numbers
- **Boolean fields**: Explicit true/false values

### Business Rule Validations
- **value**: Must be unique within each reference table
- **displayOrder**: Must be unique within each reference table
- **code**: Must be unique, uppercase with underscores only
- **color**: Must be valid hex color code (#RRGGBB format)
- **icon**: Must reference existing icon in UI icon library

## Database Constraints

### Primary Keys
All entities use GUID primary keys with descriptive names (workoutObjectiveId, workoutCategoryId, executionProtocolId).

### Indexes
- Unique index on `value` field for all reference tables
- Unique index on `code` field for ExecutionProtocol
- Unique index on `displayOrder` field for all reference tables
- Regular index on `isActive` field for filtering

### Referential Integrity
- WorkoutMuscles.muscleId → Muscles.muscleId (existing table)
- WorkoutMuscles.workoutTemplateId → WorkoutTemplate.workoutTemplateId (future table)
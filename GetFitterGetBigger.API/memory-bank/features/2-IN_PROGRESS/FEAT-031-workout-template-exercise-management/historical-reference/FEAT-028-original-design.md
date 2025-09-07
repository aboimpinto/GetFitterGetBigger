# FEAT-028: Original Design (Historical Reference)

**Note**: This document is preserved for historical reference. FEAT-028 has been superseded by FEAT-031 with an improved design approach.

## Original Feature: FEAT-028: Comprehensive Workout Exercise Management

### Overview
This feature extends the WorkoutTemplate core functionality to support comprehensive exercise management within workouts, including complex workout structures like circuits, supersets, EMOM (Every Minute on the Minute), and progressive loading patterns.

### Background
The current WorkoutTemplate implementation (FEAT-026) provides basic CRUD operations for workout templates but lacks the ability to:
- Add and manage exercises within templates
- Support different execution protocols (STANDARD, SUPERSET, DROP_SET, AMRAP)
- Configure exercises based on their MetricType (repetitions, time, distance, weight)
- Create complex workout structures with rounds and circuits
- Manage rest periods as part of the workout flow

### Business Requirements

#### Core Functionality
1. **ExecutionProtocol Integration**
   - Integrate ExecutionProtocol at the WorkoutTemplate level
   - Different protocols determine how exercises are structured and performed
   - Support for STANDARD, SUPERSET, DROP_SET, AMRAP, EMOM protocols

2. **Exercise Management by Zones**
   - Add exercises to specific zones (Warmup, Main, Cooldown)
   - Manage sequence ordering within each zone
   - Support zone-specific exercise recommendations

3. **Set Configuration Based on MetricType**
   - Configure exercises differently based on their MetricType:
     - REPETITIONS: Define rep counts (e.g., "10" or "8-12")
     - TIME: Define duration in seconds (e.g., "60s")
     - DISTANCE: Define distance values (e.g., "400m")
     - WEIGHT: Define weight amounts based on ExerciseWeightType

4. **Complex Workout Structures**
   - Support for rounds/circuits of exercises
   - Group exercises for supersets and circuits
   - Configure rest periods between rounds
   - Support nested structures (rounds within rounds)

### Example Workout Structures

#### Example 1: HIT Circuit (No Rest Between Rounds)
```
ExecutionProtocol: CIRCUIT
Rounds: 2

Round Structure:
- 10 Air Squats (REPETITIONS)
- 10 Crunches (REPETITIONS)
- 10 Push-ups (REPETITIONS)

Configuration:
- No rest between exercises
- No rest between rounds
- Total: 2 rounds × 3 exercises = 60 total reps
```

#### Example 2: Strength Training with Progressive Loading
```
ExecutionProtocol: STANDARD

Round 1: Barbell Squats (Progressive Loading)
- Set 1: 10 reps @ 40kg
- Rest: 2m30s
- Set 2: 10 reps @ 60kg
- Rest: 2m30s
- Set 3: 10 reps @ 80kg
- Rest: 2m30s

Round 2: Core Circuit
- 1m Plank (TIME)
- 50 Walking Lunges (REPETITIONS)

Configuration:
- Rest is implemented as a special exercise entry
- Progressive weight increases per set
- Mixed MetricTypes (TIME and REPETITIONS)
```

#### Example 3: EMOM (Every Minute on the Minute)
```
ExecutionProtocol: EMOM
Duration: 20 minutes (4 rounds of 5-minute cycles)

Minute Structure:
1. 10 Burpees (REPETITIONS)
2. 5 Barbell Hang-Clean @ 60kg (REPETITIONS + WEIGHT)
3. 10 Kettlebell Swings @ 20kg (REPETITIONS + WEIGHT)
4. 10 calories on Assault Bike (DISTANCE/CALORIES)
5. Rest

Configuration:
- Each exercise must be completed within its minute
- Remaining time in each minute is rest
- Pattern repeats for total duration
```

### Technical Requirements

#### Data Model Enhancements
1. **WorkoutTemplate Updates**
   - Add ExecutionProtocolId field
   - Add support for round/circuit configuration
   - Add total rounds count

2. **New Entity: ExerciseGroup**
   - Groups exercises for circuits/supersets
   - Defines group-level configuration (rest between rounds)
   - Supports nested groups

3. **Enhanced SetConfiguration**
   - Support different configuration based on MetricType
   - Progressive loading support
   - Time-based configuration for EMOM/Tabata

4. **Rest Period Management**
   - REST as a special exercise type
   - Configurable rest durations
   - Rest between sets vs rest between rounds

#### API Endpoints
1. **Exercise Management**
   - Already implemented in FEAT-026 but needs testing
   - Add validation based on ExecutionProtocol

2. **New Endpoints**
   - POST `/api/workout-templates/{id}/exercise-groups` - Create exercise group
   - PUT `/api/workout-templates/{id}/exercise-groups/{groupId}` - Update group
   - POST `/api/workout-templates/{id}/execution-protocol` - Set protocol

#### Business Rules
1. **ExecutionProtocol Constraints**
   - STANDARD: Traditional sets and reps, no grouping
   - SUPERSET: Exercises must be in groups of 2+
   - CIRCUIT: All exercises in a round performed sequentially
   - EMOM: Time-based, requires duration configuration
   - AMRAP: Time-based, tracks rounds completed

2. **MetricType Validation**
   - Exercises must be configured according to their supported MetricTypes
   - ExerciseWeightType determines if weight configuration is required
   - Time-based protocols require TIME metric support

3. **Zone Restrictions**
   - Warmup: Light exercises, mobility work
   - Main: Primary workout exercises
   - Cooldown: Stretching, recovery exercises

### Dependencies
- FEAT-026 (WorkoutTemplate Core) must be completed
- Existing Exercise, MetricType, and ExecutionProtocol entities
- ExerciseMetricSupport relationship

### Success Criteria
1. Personal trainers can create workouts with complex structures
2. All example workout types can be fully configured
3. Exercise configuration adapts based on MetricType
4. ExecutionProtocol drives the workout structure UI/UX
5. Complete BDD test coverage for all scenarios

### Implementation Priority
1. ExecutionProtocol integration with WorkoutTemplate
2. Basic exercise addition with MetricType validation
3. ExerciseGroup entity for circuits/supersets
4. Complex workout structure support
5. BDD tests and integration tests

### Notes
- This feature significantly enhances the workout creation capabilities
- UI/UX will need major updates to support these complex structures
- Performance testing is critical with nested structures
- Consider workout preview/simulation functionality

## What Was Extracted for FEAT-031

### Concepts Adopted
1. **ExecutionProtocol** - Using existing entity instead of creating WorkoutType
2. **MetricType for UI Validation** - Using to determine which input fields to show
3. **Workout Examples** - Incorporated similar examples in technical design
4. **Phase Organization** - Warmup, Workout (Main), Cooldown phases

### Concepts Modified
1. **ExerciseGroup Entity** → Replaced with round-based organization and metadata
2. **Complex nested structures** → Simplified to rounds + JSON metadata
3. **Multiple tables approach** → Single flexible table with JSON

### Concepts Not Needed
1. **ExerciseGroup entity** - Rounds and metadata handle grouping
2. **SetConfiguration entity** - JSON metadata is more flexible
3. **Complex zone restrictions** - Simplified phase approach

## Key Improvements in FEAT-031

1. **Simpler Architecture**
   - Single WorkoutTemplateExercise table vs multiple entities
   - JSON metadata provides infinite flexibility
   - No schema changes needed for new workout types

2. **Better Integration**
   - Uses existing ExecutionProtocol
   - Leverages ExerciseLinks for auto-add functionality
   - MetricType for UI hints only, not data storage

3. **More Flexible**
   - Any workout type can be supported
   - Easy to add new types without code changes
   - Metadata adapts to any requirement

## Lessons Learned

1. **Start Simple** - A flexible JSON approach beats complex entity relationships
2. **Use What Exists** - ExecutionProtocol and MetricType were already there
3. **Think UI/Backend Separately** - MetricType for UI, JSON for storage
4. **Rounds Are Powerful** - Most complexity can be expressed as rounds + metadata

---

*This document preserved from FEAT-028 submitted on July 25, 2024*
*Superseded by FEAT-031 on January 7, 2025*
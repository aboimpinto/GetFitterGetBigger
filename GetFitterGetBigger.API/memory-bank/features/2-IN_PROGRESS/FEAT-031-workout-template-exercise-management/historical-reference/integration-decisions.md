# Integration Decisions: FEAT-028 → FEAT-031

## Executive Summary

FEAT-031 supersedes FEAT-028 by incorporating existing codebase entities (ExecutionProtocol, MetricType) while simplifying the overall architecture through a flexible JSON metadata approach.

## Key Integration Decisions

### 1. Use ExecutionProtocol Instead of Creating WorkoutType

**Decision**: Use the existing ExecutionProtocol entity
- Already has: STANDARD, SUPERSET, DROP_SET, AMRAP
- To be added later: EMOM, TABATA, CIRCUIT, LADDER
- Link via ExecutionProtocolId in WorkoutTemplate
- Add ExecutionProtocolConfig (JSON) to WorkoutTemplate for protocol-specific settings

**Rationale**: 
- Entity already exists and is fully integrated
- Has caching, services, repositories, and tests
- Avoids duplication and maintains consistency

### 2. MetricType for UI Validation Only

**Decision**: Use MetricType to determine UI input fields, but store everything as JSON
- Query ExerciseMetricSupport to find supported metrics
- UI shows appropriate fields (reps, time, weight, distance)
- Backend stores all data as flexible JSON metadata

**API Flow**:
```
GET /api/exercises/{exerciseId}/supported-metrics
→ Returns ["REPETITIONS", "WEIGHT"]
→ UI shows reps and weight input fields
→ Data saved as JSON: {"reps": 10, "weight": {"value": 20, "unit": "kg"}}
```

**Rationale**:
- Provides user-friendly UI without rigid backend constraints
- JSON storage allows infinite flexibility
- No schema changes needed for new metric combinations

### 3. ExerciseLink for Warmup/Cooldown Management

**Decision**: Use existing ExerciseLink system
- WARMUP, COOLDOWN, WORKOUT, ALTERNATIVE link types already exist
- Auto-add warmup/cooldown when adding workout exercises
- Auto-cleanup orphaned exercises on removal

**Rationale**:
- System already handles exercise relationships
- Four-way linking provides complete flexibility
- No need for additional relationship tables

### 4. REST as Special Exercise, Not ExerciseType

**Decision**: REST is a regular exercise in the Exercise table
- Has special ExerciseId (e.g., 999)
- Only accepts duration in metadata
- Can be added like any other exercise

**Note**: Need to verify if REST exercise exists or needs to be seeded

**Rationale**:
- Simpler than special-casing REST everywhere
- Consistent with treating everything as exercises
- Easier to manage in rounds

### 5. Single Flexible Table Design

**Decision**: Use single WorkoutTemplateExercise table with JSON metadata

**Structure**:
```sql
WorkoutTemplateExercise
- Id (GUID)
- WorkoutTemplateId
- ExerciseId  
- Phase (Warmup/Workout/Cooldown)
- RoundNumber
- OrderInRound
- Metadata (JSON)
- AddedAutomatically
- SourceExerciseId
```

**Rationale**:
- Simpler than multiple tables per workout type
- JSON metadata provides infinite flexibility
- No migrations needed for new workout types
- Proven to handle all workout scenarios

### 6. ExecutionProtocolConfig in WorkoutTemplate

**Decision**: Add ExecutionProtocolConfig to WorkoutTemplate table

**Examples**:
```json
// EMOM
{
  "interval": 60,
  "totalDuration": 10,
  "durationUnit": "minutes"
}

// TABATA
{
  "workDuration": 20,
  "restDuration": 10,
  "rounds": 8
}
```

**Rationale**:
- Protocol-specific settings at template level
- Separates structure (protocol) from content (exercises)
- Clean separation of concerns

## Migration Path from FEAT-028

### What to Keep
- ✅ ExecutionProtocol concept and entity
- ✅ MetricType for UI validation
- ✅ Phase-based organization (Warmup/Main/Cooldown)
- ✅ Workout examples and use cases

### What to Change
- ❌ ExerciseGroup entity → Use rounds + metadata
- ❌ SetConfiguration entity → Use JSON metadata
- ❌ Multiple tables → Single flexible table

### What to Add
- ➕ ExecutionProtocolConfig field to WorkoutTemplate
- ➕ Flexible JSON metadata structure
- ➕ Auto-linking via ExerciseLinks
- ➕ GUID identification for exercise instances

## Implementation Phases

### Phase 1: Database Schema
1. Add ExecutionProtocolId to WorkoutTemplate
2. Add ExecutionProtocolConfig to WorkoutTemplate
3. Create new WorkoutTemplateExercise table
4. Seed missing ExecutionProtocols (future)

### Phase 2: Core Services
1. WorkoutTemplateExerciseService
2. Auto-linking logic
3. Metadata validation
4. Round management

### Phase 3: API Endpoints
1. Add exercise with auto-linking
2. Remove with cleanup
3. Reorder exercises
4. Copy rounds

### Phase 4: Integration
1. MetricType UI hints endpoint
2. ExecutionProtocol validation
3. Complete testing

## Benefits of This Approach

1. **Leverages Existing Code**
   - Uses ExecutionProtocol (55+ files already integrated)
   - Uses MetricType (fully implemented)
   - Uses ExerciseLinks (four-way linking ready)

2. **Simpler Than FEAT-028**
   - One table vs multiple entities
   - JSON vs rigid schema
   - Rounds vs complex grouping

3. **More Flexible**
   - Supports any workout type
   - No schema changes for new types
   - Metadata adapts to any need

4. **Easier to Implement**
   - Less code to write
   - Fewer tests needed
   - Simpler mental model

## Conclusion

By integrating with existing ExecutionProtocol and MetricType entities while adopting a flexible JSON metadata approach, FEAT-031 provides a simpler, more maintainable solution than FEAT-028 while supporting all the same use cases and more.
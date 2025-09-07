# FEAT-031: Workout Template Exercise Management

## Document Structure

This feature is organized into multiple focused documents to avoid duplication and improve maintainability:

### Core Documents

1. **[feature-description.md](./feature-description.md)**
   - User stories and acceptance criteria
   - High-level requirements
   - Implementation phases

2. **[business-rules.md](./business-rules.md)** 
   - Complete business rules and lifecycle states
   - Permission model and validation rules
   - Error handling and edge cases

3. **[api-design.md](./api-design.md)**
   - API endpoints and contracts
   - Service layer interfaces
   - Business logic flows
   - Error handling

4. **[technical-design-detail.md](./technical-design-detail.md)**
   - Complete workout example ("Leg Burning I")
   - All execution protocols with examples
   - Complex scenarios and edge cases

### Shared Technical Specifications

5. **[database-schema.md](./database-schema.md)**
   - ExecutionProtocol migration (First task!)
   - WorkoutTemplate modifications  
   - WorkoutTemplateExercise table migration
   - Design decisions and relationships

6. **[metadata-patterns.md](./metadata-patterns.md)**
   - JSON metadata structures
   - Pattern examples for all exercise types
   - Validation rules
   - Future extensibility

7. **[auto-linking-logic.md](./auto-linking-logic.md)**
   - Warmup/cooldown auto-management
   - Add/remove flows with SQL examples
   - Edge cases and scenarios
   - Performance considerations

### Implementation Guidance

8. **[implementation-notes.md](./implementation-notes.md)** 📝
   - All clarifications consolidated
   - Key decisions explained
   - Implementation priorities
   - Testing scenarios in Gherkin format

### Historical Reference

9. **[historical-reference/](./historical-reference/)**
   - FEAT-028 original design (superseded)
   - Integration decisions
   - Evolution of the design

## Quick Start

1. **For Requirements**: Start with [feature-description.md](./feature-description.md)
2. **For Business Rules**: Review [business-rules.md](./business-rules.md)
3. **For API Contract**: See [api-design.md](./api-design.md)
4. **For Database**: Review [database-schema.md](./database-schema.md)
5. **For Examples**: Check [technical-design-detail.md](./technical-design-detail.md)
6. **For Clarifications**: Read [implementation-notes.md](./implementation-notes.md)

## Key Features

- **Multiple Execution Protocols**: REPS_AND_SETS, SUPERSET, AMRAP, etc.
- **Round-Based Organization**: Flexible structure for any workout type
- **Auto-Linking**: Intelligent warmup/cooldown management
- **JSON Metadata**: Infinite flexibility without schema changes
- **Integration**: Uses existing ExecutionProtocol and ExerciseLinks

## Implementation Priority

1. ⚠️ **First Task**: Rename "Standard" to "Reps and Sets" in ExecutionProtocol
2. Add ExecutionProtocolId to WorkoutTemplate
3. Migrate WorkoutTemplateExercise table structure
4. Implement core CRUD operations
5. Add auto-linking logic

## Related Features

- **FEAT-032**: Exercise Metric Support (extracted for separation of concerns)
- **FEAT-026**: WorkoutTemplate Core (prerequisite)
- **FEAT-028**: Original design (historical reference)

## Status

**Current**: SUBMITTED  
**Next Step**: Refine using `/refine-feature FEAT-031` to create detailed tasks
# Database Schema: Workout Template Exercise Management

This document contains the complete database schema definition for FEAT-031.

## ExecutionProtocol Integration

### Migration Required (First Task!)

```sql
-- Rename "Standard" to "Reps and Sets" for clarity
UPDATE ExecutionProtocol 
SET Value = 'Reps and Sets', 
    Code = 'REPS_AND_SETS',
    Description = 'Traditional workout with fixed sets and repetitions'
WHERE ExecutionProtocolId = '30000003-3000-4000-8000-300000000001';
```

### Current Protocols (After Migration)
- **REPS_AND_SETS** (Id: 30000003-3000-4000-8000-300000000001) - Renamed from STANDARD
- **SUPERSET** (Id: 30000003-3000-4000-8000-300000000002)
- **DROP_SET** (Id: 30000003-3000-4000-8000-300000000003)
- **AMRAP** (Id: 30000003-3000-4000-8000-300000000004)

### Future Protocols
To be added in future iterations:
- EMOM (Every Minute on the Minute)
- TABATA (20s work, 10s rest, 8 rounds)
- CIRCUIT (Sequential exercises, minimal rest)
- LADDER (Progressive rep scheme)

## Schema Changes

### WorkoutTemplate Table (Modified)

```sql
-- Add these columns to existing WorkoutTemplate table
ALTER TABLE WorkoutTemplate ADD
    ExecutionProtocolId UNIQUEIDENTIFIER NOT NULL 
        FOREIGN KEY REFERENCES ExecutionProtocol(ExecutionProtocolId),
    ExecutionProtocolConfig NVARCHAR(MAX) NULL; -- JSON configuration for execution protocol
```

### WorkoutTemplateExercise Table (Fresh Creation)

**IMPORTANT**: The existing table was never used (wrong initial implementation). We will DROP and CREATE fresh, no migration needed.

#### Old Structure (To Be Dropped)
```csharp
// Old entity that will be removed:
- WorkoutZone enum (Warmup/Main/Cooldown) 
- SequenceOrder (single value)
- SetConfiguration collection
- Notes field
```

#### New Structure (Fresh Creation)
```sql
-- Drop the old unused table
DROP TABLE IF EXISTS WorkoutTemplateExercise;
DROP TABLE IF EXISTS SetConfiguration; -- Also drop related table

-- Create fresh new table
CREATE TABLE WorkoutTemplateExercise (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    WorkoutTemplateId INT NOT NULL FOREIGN KEY REFERENCES WorkoutTemplate(Id),
    ExerciseId INT NOT NULL FOREIGN KEY REFERENCES Exercise(Id),
    Phase NVARCHAR(20) NOT NULL CHECK (Phase IN ('Warmup', 'Workout', 'Cooldown')),
    RoundNumber INT NOT NULL CHECK (RoundNumber > 0),
    OrderInRound INT NOT NULL CHECK (OrderInRound > 0),
    Metadata NVARCHAR(MAX) NOT NULL, -- JSON structure (see metadata-patterns.md)
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
```

#### Final Structure
```sql
WorkoutTemplateExercise (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    WorkoutTemplateId INT NOT NULL FOREIGN KEY REFERENCES WorkoutTemplate(Id),
    ExerciseId INT NOT NULL FOREIGN KEY REFERENCES Exercise(Id),
    Phase NVARCHAR(20) NOT NULL CHECK (Phase IN ('Warmup', 'Workout', 'Cooldown')),
    RoundNumber INT NOT NULL CHECK (RoundNumber > 0),
    OrderInRound INT NOT NULL CHECK (OrderInRound > 0),
    Metadata NVARCHAR(MAX) NOT NULL, -- JSON structure (see metadata-patterns.md)
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- Indexes to be added during migration
CREATE INDEX IX_WorkoutTemplate ON WorkoutTemplateExercise (WorkoutTemplateId, Phase, RoundNumber, OrderInRound);
CREATE INDEX IX_Exercise ON WorkoutTemplateExercise (ExerciseId);
CREATE INDEX IX_Round ON WorkoutTemplateExercise (WorkoutTemplateId, Phase, RoundNumber);
CREATE INDEX IX_WorkoutPhase ON WorkoutTemplateExercise (WorkoutTemplateId, Phase) WHERE Phase = 'Workout';
```

### Recommended Additional Indexes

```sql
-- For efficient orphan detection during exercise removal
CREATE INDEX IX_ExerciseLinks_Source ON ExerciseLinks (SourceExerciseId, LinkType);
CREATE INDEX IX_ExerciseLinks_Target ON ExerciseLinks (TargetExerciseId, LinkType);

-- For template status queries (when template lifecycle is implemented)
CREATE INDEX IX_WorkoutTemplate_Status ON WorkoutTemplate (Status) 
    WHERE Status IN ('Draft', 'Production');
```

## Key Design Decisions

### Why No Tracking Fields?
Initially considered adding:
- `AddedAutomatically BIT` - To track auto-added exercises
- `SourceExerciseId UNIQUEIDENTIFIER` - To link to triggering workout exercise

**Decision**: Removed for simplicity. When removing exercises, we simply check if warmup/cooldown are used by ANY other workout exercise via ExerciseLinks.

### Why JSON Metadata?
- **Flexibility**: Different execution protocols need different data
- **No Schema Changes**: New workout types don't require migrations
- **Simplicity**: One table handles all variations

### Phase-Based Round Numbering
- Rounds restart at 1 for each phase
- Warmup Round 1, Workout Round 1, Cooldown Round 1 (not continuous)
- Cleaner organization and easier UI management

## Relationships

```
WorkoutTemplate
    ├── ExecutionProtocolId → ExecutionProtocol (defines structure)
    └── WorkoutTemplateExercise (multiple)
            ├── ExerciseId → Exercise (the actual exercise)
            └── Metadata (JSON - flexible data)

ExerciseLinks (existing)
    ├── SourceExerciseId → Exercise (workout exercise)
    ├── TargetExerciseId → Exercise (warmup/cooldown)
    └── LinkType (WARMUP, COOLDOWN, WORKOUT, ALTERNATIVE)
```

## Data Integrity Rules

1. **Unique Exercise per Round**: Same exercise cannot appear twice in the same round
2. **Order Sequence**: OrderInRound must be sequential within each round
3. **Valid Phases**: Only 'Warmup', 'Workout', 'Cooldown' allowed
4. **Non-Empty Metadata**: All exercises must have metadata (even if just `{}`)
5. **Valid Exercise References**: ExerciseId must exist and be active
6. **Round Renumbering**: When deleting a round, subsequent rounds must be renumbered (e.g., if round 2 is deleted, round 3 becomes round 2)
7. **Soft Delete for Exercises**: Exercises use soft delete (IsDeleted flag) - never hard delete

## Migration Notes

### Implementation Strategy
**IMPORTANT**: We are creating a FRESH WorkoutTemplateExercise table. The old one was never used and will be dropped.

#### Implementation Steps:
1. **Drop existing tables** (WorkoutTemplateExercise and SetConfiguration)
2. **Create new table structure** with proper design:
   - Phase string for workout zones
   - RoundNumber and OrderInRound for organization
   - JSON Metadata for flexibility
3. **Update entity models** to match new structure
4. **No migration needed** - starting fresh with no data

### Why Fresh Start?
- The initial implementation was wrong and never used
- No data exists in the current tables
- Cleaner to start fresh than to migrate an unused structure
- No backward compatibility concerns
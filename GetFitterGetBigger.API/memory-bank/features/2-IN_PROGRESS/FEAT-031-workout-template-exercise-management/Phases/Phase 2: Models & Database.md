# Phase 2: Models & Database - Estimated: 4h 0m

## Task 2.1: Rename ExecutionProtocol "Standard" to "Reps and Sets"
`[Complete]` (Est: 30m, Actual: 45m) - Completed: 2025-09-07 14:30

**CRITICAL**: This must be the FIRST task to align with feature requirements.

**Implementation:**
- Create EF Core migration to update ExecutionProtocol data
- Update existing "Standard" entry to "Reps and Sets" with Code "REPS_AND_SETS"
```csharp
// NOTE: Do NOT use raw SQL. Use EF Core migration's UpdateData method:
migrationBuilder.UpdateData(
    table: "ExecutionProtocol",
    keyColumn: "ExecutionProtocolId",
    keyValue: new Guid("30000003-3000-4000-8000-300000000001"),
    columns: new[] { "Value", "Code", "Description" },
    values: new object[] { "Reps and Sets", "REPS_AND_SETS", "Traditional workout with fixed sets and repetitions" });
```

**Testing:**
- Create migration test to verify data conversion
- Test that existing references still work
- Verify cache invalidation after migration

**Reference Pattern:**
- Follow migration patterns from `/memory-bank/features/3-COMPLETED/FEAT-030-exercise-link-enhancements/` Phase 2

**✅ IMPLEMENTATION COMPLETED:**
- ✅ Created migration file: `20250907121841_RenameExecutionProtocolStandardToRepsAndSets.cs`
- ✅ Used EF Core UpdateData method (not raw SQL) as specified
- ✅ Updated ID `30000003-3000-4000-8000-300000000001`:
  - Value: "Standard" → "Reps and Sets"
  - Code: "STANDARD" → "REPS_AND_SETS"
  - Description: Updated to "Traditional workout with fixed sets and repetitions"
- ✅ Added rollback capability in Down() method
- ✅ Created comprehensive migration tests in `GetFitterGetBigger.API.Tests/Migrations/ExecutionProtocolMigrationTests.cs`
- ✅ Build verification: 0 errors, 0 warnings
- ✅ Tests verify data conversion and rollback scenarios

## Task 2.2: Add ExecutionProtocol integration to WorkoutTemplate entity
`[Complete]` (Est: 1h, Actual: 1h) - Completed: 2025-09-07 14:35

**Implementation:**
- Modify `/GetFitterGetBigger.API/Models/Entities/WorkoutTemplate.cs`
- Add ExecutionProtocolId and ExecutionProtocolConfig properties:
```csharp
public record WorkoutTemplate : IEmptyEntity<WorkoutTemplate>
{
    // Existing properties...
    public ExecutionProtocolId ExecutionProtocolId { get; init; }
    public string? ExecutionProtocolConfig { get; init; } // JSON for protocol-specific settings
    
    // Navigation property
    public ExecutionProtocol? ExecutionProtocol { get; init; }
}
```

**Entity Modification & Migration:**
```csharp
// 1. Modify WorkoutTemplate entity to add new properties:
public ExecutionProtocolId ExecutionProtocolId { get; init; }
public string? ExecutionProtocolConfig { get; init; } // JSON for protocol-specific settings
public ExecutionProtocol? ExecutionProtocol { get; init; } // Navigation property

// 2. Configure in DbContext for PostgreSQL JSON:
modelBuilder.Entity<WorkoutTemplate>()
    .Property(e => e.ExecutionProtocolConfig)
    .HasColumnType("jsonb"); // PostgreSQL JSON type

// 3. Generate migration: dotnet ef migrations add AddExecutionProtocolToWorkoutTemplate
```

**Unit Tests:**
- Test entity creation with ExecutionProtocol
- Test Handler methods with ExecutionProtocol integration
- Test Empty pattern with new properties
- Test navigation property loading

**Critical Patterns:**
- Follow entity patterns from `/GetFitterGetBigger.API/Models/Entities/ExecutionProtocol.cs`
- Use nullable ExecutionProtocolConfig for optional protocol settings
- Update Empty pattern to include new properties

## Task 2.3: Create new WorkoutTemplateExercise entity design
`[Pending]` (Est: 1h 30m)

**CRITICAL**: This replaces the existing flawed WorkoutTemplateExercise implementation.

**Implementation:**
- **DROP and CREATE fresh** - no migration needed (table never used properly)
- Create new entity at `/GetFitterGetBigger.API/Models/Entities/WorkoutTemplateExercise.cs`:

```csharp
public record WorkoutTemplateExercise : IEmptyEntity<WorkoutTemplateExercise>
{
    public WorkoutTemplateExerciseId Id { get; init; }
    public WorkoutTemplateId WorkoutTemplateId { get; init; }
    public ExerciseId ExerciseId { get; init; }
    public string Phase { get; init; } = string.Empty; // Warmup, Workout, Cooldown
    public int RoundNumber { get; init; }
    public int OrderInRound { get; init; }
    public string Metadata { get; init; } = string.Empty; // JSON structure
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    
    // Navigation properties
    public WorkoutTemplate? WorkoutTemplate { get; init; }
    public Exercise? Exercise { get; init; }
    
    // IEntity implementation
    string IEntity.Id => Id.ToString();
    bool IEntity.IsActive => true;
    public bool IsEmpty => Id.IsEmpty;
    
    public static WorkoutTemplateExercise Empty => new()
    {
        Id = WorkoutTemplateExerciseId.Empty,
        WorkoutTemplateId = WorkoutTemplateId.Empty,
        ExerciseId = ExerciseId.Empty,
        Phase = string.Empty,
        RoundNumber = 0,
        OrderInRound = 0,
        Metadata = "{}",
        CreatedAt = DateTime.MinValue,
        UpdatedAt = DateTime.MinValue
    };
    
    public static class Handler
    {
        public static EntityResult<WorkoutTemplateExercise> CreateNew(
            WorkoutTemplateId workoutTemplateId,
            ExerciseId exerciseId,
            string phase,
            int roundNumber,
            int orderInRound,
            string metadata)
        {
            return Validate.For<WorkoutTemplateExercise>()
                .EnsureNotEmpty(workoutTemplateId, "Workout template ID cannot be empty")
                .EnsureNotEmpty(exerciseId, "Exercise ID cannot be empty")
                .EnsureNotEmpty(phase, "Phase cannot be empty")
                .Ensure(() => IsValidPhase(phase), "Phase must be Warmup, Workout, or Cooldown")
                .EnsureMinValue(roundNumber, 1, "Round number must be at least 1")
                .EnsureMinValue(orderInRound, 1, "Order in round must be at least 1")
                .EnsureNotEmpty(metadata, "Metadata cannot be empty")
                .Ensure(() => IsValidJson(metadata), "Metadata must be valid JSON")
                .OnSuccess(() => new WorkoutTemplateExercise
                {
                    Id = WorkoutTemplateExerciseId.New(),
                    WorkoutTemplateId = workoutTemplateId,
                    ExerciseId = exerciseId,
                    Phase = phase,
                    RoundNumber = roundNumber,
                    OrderInRound = orderInRound,
                    Metadata = metadata,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
        }
        
        private static bool IsValidPhase(string phase) =>
            phase is "Warmup" or "Workout" or "Cooldown";
        
        private static bool IsValidJson(string json)
        {
            try { JsonDocument.Parse(json); return true; }
            catch { return false; }
        }
    }
}
```

**Entity Creation & EF Core Configuration:**
```csharp
// NOTE: Do NOT use raw SQL. Use EF Core Entities and Migrations!

// 1. Create new entity in /Models/Entities/WorkoutTemplateExercise.cs
public record WorkoutTemplateExercise : IEmptyEntity<WorkoutTemplateExercise>
{
    public WorkoutTemplateExerciseId Id { get; init; }
    public WorkoutTemplateId WorkoutTemplateId { get; init; }
    public ExerciseId ExerciseId { get; init; }
    public string Phase { get; init; } // "Warmup", "Workout", "Cooldown"
    public int RoundNumber { get; init; }
    public int OrderInRound { get; init; }
    public string Metadata { get; init; } // JSON string
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    
    // Navigation properties
    public WorkoutTemplate? WorkoutTemplate { get; init; }
    public Exercise? Exercise { get; init; }
    
    // Handler pattern implementation (following existing patterns)
    public static class Handler 
    {
        // CreateNew, Create, Update methods following Entity Handler pattern
    }
}

// 2. Configure in FitnessDbContext.OnModelCreating():
modelBuilder.Entity<WorkoutTemplateExercise>(entity =>
{
    entity.HasKey(e => e.Id);
    
    entity.Property(e => e.Phase)
        .HasMaxLength(20)
        .IsRequired();
    
    entity.Property(e => e.Metadata)
        .HasColumnType("jsonb") // PostgreSQL JSON type
        .IsRequired();
    
    entity.HasIndex(e => new { e.WorkoutTemplateId, e.Phase, e.RoundNumber, e.OrderInRound })
        .HasDatabaseName("IX_WorkoutTemplate");
    
    entity.HasIndex(e => e.ExerciseId)
        .HasDatabaseName("IX_Exercise");
    
    entity.HasIndex(e => new { e.WorkoutTemplateId, e.Phase, e.RoundNumber })
        .HasDatabaseName("IX_Round");
    
    // Foreign key relationships
    entity.HasOne(e => e.WorkoutTemplate)
        .WithMany()
        .HasForeignKey(e => e.WorkoutTemplateId);
    
    entity.HasOne(e => e.Exercise)
        .WithMany()
        .HasForeignKey(e => e.ExerciseId);
});

// 3. Generate migration: dotnet ef migrations add CreateWorkoutTemplateExerciseTable
// 4. The old tables will be dropped automatically if they exist
```

**Unit Tests:**
- Test entity creation with Handler.CreateNew
- Test validation for all fields
- Test JSON metadata validation
- Test Empty pattern implementation
- Test phase validation (Warmup/Workout/Cooldown)

**Critical Patterns:**
- Follow Handler pattern from `/GetFitterGetBigger.API/Models/Entities/ExecutionProtocol.cs`
- Use EntityResult<T> pattern, not exceptions
- Use proper validation with Validate.For<T>()
- Implement IEmptyEntity<T> pattern correctly

## Task 2.4: Update DbContext configuration for new entities
`[Pending]` (Est: 1h)

**Implementation:**
- Update `/GetFitterGetBigger.API/Models/FitnessDbContext.cs`
- Configure new WorkoutTemplateExercise entity:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Existing configurations...
    
    // WorkoutTemplate ExecutionProtocol integration
    modelBuilder.Entity<WorkoutTemplate>()
        .HasOne(wt => wt.ExecutionProtocol)
        .WithMany()
        .HasForeignKey(wt => wt.ExecutionProtocolId)
        .OnDelete(DeleteBehavior.Restrict);
    
    // WorkoutTemplateExercise configuration
    modelBuilder.Entity<WorkoutTemplateExercise>()
        .HasKey(wte => wte.Id);
    
    modelBuilder.Entity<WorkoutTemplateExercise>()
        .Property(wte => wte.Id)
        .HasConversion(
            id => id.Value,
            value => new WorkoutTemplateExerciseId(value));
    
    modelBuilder.Entity<WorkoutTemplateExercise>()
        .HasOne(wte => wte.WorkoutTemplate)
        .WithMany()
        .HasForeignKey(wte => wte.WorkoutTemplateId)
        .OnDelete(DeleteBehavior.Cascade);
        
    modelBuilder.Entity<WorkoutTemplateExercise>()
        .HasOne(wte => wte.Exercise)
        .WithMany()
        .HasForeignKey(wte => wte.ExerciseId)
        .OnDelete(DeleteBehavior.Restrict);
    
    // Phase constraint
    modelBuilder.Entity<WorkoutTemplateExercise>()
        .Property(wte => wte.Phase)
        .HasMaxLength(20);
        
    // JSON metadata column for PostgreSQL
    modelBuilder.Entity<WorkoutTemplateExercise>()
        .Property(wte => wte.Metadata)
        .HasColumnType("jsonb"); // PostgreSQL JSON type
    
    // Indexes
    modelBuilder.Entity<WorkoutTemplateExercise>()
        .HasIndex(wte => new { wte.WorkoutTemplateId, wte.Phase, wte.RoundNumber, wte.OrderInRound })
        .HasDatabaseName("IX_WorkoutTemplate_Phase_Round_Order");
}
```

**Testing:**
- Test entity configuration with EF Core
- Test foreign key constraints
- Test indexes are created
- Test JSON metadata column functionality

**Critical Patterns:**
- Follow EF Core configuration patterns from existing entities
- Use proper foreign key relationships
- Configure specialized ID converters
- Add performance indexes

## CHECKPOINT: Phase 2 Complete - Models & Database
`[IN_PROGRESS]` - Date: 2025-09-07 16:45

**Requirements for Completion:**
- Build: ✅ 0 errors, 0 warnings  
- Tests: ❌ 16 tests failing (due to ServiceValidate violations)
- Migration: ✅ 2 EF Core migrations created and tested

**Implementation Summary:**
- **ExecutionProtocol Migration**: ✅ "Standard" renamed to "Reps and Sets" with UpdateData
- **WorkoutTemplate Enhancement**: ✅ ExecutionProtocolId and ExecutionProtocolConfig added
- **WorkoutTemplateExercise**: ⏳ Entity design pending (Task 2.3 not started)
- **Database Schema**: ✅ WorkoutTemplate table updated with new columns
- **EF Core Configuration**: ✅ Foreign key relationships and JSONB configured

**Test Updates:**
- Migration Tests: ✅ ExecutionProtocolMigrationTests created
- Entity Tests: ✅ WorkoutTemplateTests updated with ExecutionProtocolId
- Test Builders: ✅ WorkoutTemplateDtoBuilder and WorkoutTemplateBuilder updated
- Test Status: ❌ 16 tests failing due to Empty pattern violations

**Code Review Reports:**
1. **Review 1**: `/memory-bank/features/2-IN_PROGRESS/FEAT-031-workout-template-exercise-management/code-reviews/Phase_2_Models_Database/Code-Review-Phase-2-Models-Database-2025-09-07-15-30-REQUIRES_CHANGES.md`
   - Status: REQUIRES_CHANGES (78% quality score)
   - Issues: Magic strings (Rule 10), Single exit points (Rule 1), ServiceResult pattern violations
   
2. **Review 2**: [phase-2-review-2025-09-07.md](../code-reviews/phase-2-review-2025-09-07.md)
   - Status: REQUIRES_CHANGES (72% approval rate)
   - Critical Issues: ServiceValidate pattern violations causing 16 test failures
   - Root Cause: `WhenValidAsync()` returns `default(T)!` instead of Empty objects
   - Action Required: Fix violations before proceeding to Phase 3

**Git Commit**: 
- `ebe620b8` - feat(workout-template): integrate ExecutionProtocol into WorkoutTemplate entity
- `d991dae` - fix(validation): resolve build errors and failing tests in chained validation
- `bf5865fe` - refactor(tooling): restructure FEAT-031 task management into phase-based system

**Status**: ⏳ Phase 2 IN PROGRESS - Critical fixes required for ServiceValidate violations
# FEAT-031: Workout Template Exercise Management System - Implementation Tasks

## Feature Branch: `feature/feat-031-workout-template-exercise-management`
## Estimated Total Time: 24h 0m
## Actual Total Time: [To be calculated at completion]

## 📚 Pre-Implementation Checklist
- [ ] Read `/memory-bank/Overview/SystemPatterns.md` - Architecture rules
- [ ] Read `/memory-bank/CodeQualityGuidelines/ServiceResultPattern.md` - Error handling patterns
- [ ] Read `/memory-bank/PracticalGuides/CommonImplementationPitfalls.md` - Critical mistakes to avoid
- [ ] Read `/memory-bank/CodeQualityGuidelines/ServiceValidatePattern.md` - Validation patterns
- [ ] Read `/memory-bank/Overview/UnitVsIntegrationTests.md` - Test separation rules
- [ ] Read `/memory-bank/DevelopmentGuidelines/Templates/FeatureCheckpointTemplate.md` - Checkpoint format requirements
- [ ] Run baseline health check (`dotnet build` and `dotnet test`)
- [ ] Study existing WorkoutTemplate implementation patterns
- [ ] Review ExecutionProtocol and ExerciseLink integrations

## 🔍 Codebase Study Findings

### Current Implementation Analysis

**Existing WorkoutTemplate Functionality:**
- ✅ Complete CRUD operations (`/GetFitterGetBigger.API/Services/WorkoutTemplate/WorkoutTemplateService.cs`)
- ✅ WorkoutTemplate entity with WorkoutState lifecycle management
- ✅ Existing WorkoutTemplateExerciseService with zone-based organization (Warmup/Main/Cooldown)
- ✅ SetConfiguration system (to be replaced with JSON metadata)
- ✅ 1,750+ passing tests (1,395 unit + 355 integration)
- ✅ ServiceResult<T> pattern for error handling
- ✅ ServiceValidate pattern for validation chains
- ✅ Repository pattern with ReadOnly/Writable UnitOfWork separation
- ✅ Controller endpoints with proper authorization

**ExecutionProtocol Integration Available:**
- ✅ Fully implemented ExecutionProtocol entity (`/GetFitterGetBigger.API/Models/Entities/ExecutionProtocol.cs`)
- ✅ Enum with 4 values: WARMUP(0), COOLDOWN(1), WORKOUT(2), ALTERNATIVE(3)
- ✅ Cached reference data service with eternal caching
- ✅ Controller and repository layers complete
- ✅ String values: "Reps and Sets" (REPS_AND_SETS), SUPERSET, DROP_SET, AMRAP

**ExerciseLink System Available:**
- ✅ Complete four-way linking system (`/GetFitterGetBigger.API/Services/Exercise/Features/Links/ExerciseLinkService.cs`)
- ✅ Bidirectional link creation/deletion with WARMUP→WORKOUT, COOLDOWN→WORKOUT patterns
- ✅ ExerciseLinkType enum: WARMUP, COOLDOWN, WORKOUT, ALTERNATIVE
- ✅ Auto-linking algorithm already implemented
- ✅ Advanced validation with ServiceValidate pattern
- ✅ Repository and data service layers complete

**Current WorkoutTemplateExercise Implementation:**
- ❌ **CRITICAL ISSUE**: Current entity uses old WorkoutZone enum (Warmup/Main/Cooldown) instead of Phase strings
- ❌ Uses SetConfiguration collection instead of JSON metadata
- ❌ Missing ExecutionProtocol integration
- ❌ No auto-linking with ExerciseLinks
- ❌ No round-based organization
- ❌ No GUID-based exercise identification

**Key Patterns to Follow:**
- ServiceResult<T> pattern for all service methods (`/GetFitterGetBigger.API/Services/Results/ServiceResult.cs`)
- ServiceValidate for validation chains (`/GetFitterGetBigger.API/Services/Validation/ServiceValidate.cs`)
- Entity Handler patterns with validation (`/GetFitterGetBigger.API/Models/Entities/ExecutionProtocol.cs` lines 43-105)
- Repository base class pattern (`/GetFitterGetBigger.API/Repositories/RepositoryBase.cs`)
- Empty pattern implementation (`/GetFitterGetBigger.API/Models/Entities/WorkoutTemplateExercise.cs` lines 39-46)

## 🧪 Test Structure Requirements

### Global Acceptance Tests
**Location**: `acceptance-tests/WorkoutTemplateExerciseManagement.feature`
- Test complete workflows: API → Admin and API → Clients
- End-to-end business processes validation
- Cross-project integration scenarios

### Project-Specific Minimal Acceptance Tests
**Location**: `Tests/Features/WorkoutTemplateExercise/WorkoutTemplateExerciseManagementAcceptanceTests.cs`
- BDD format with Given/When/Then
- Focus on critical API project paths
- Based on integration test patterns

### BDD Test Scenarios (MANDATORY)

#### Scenario 1: Add Workout Exercise with Auto-Linking
```gherkin
Given I am authenticated as "PT-Tier"
And a workout template "Leg Burning I" exists with ExecutionProtocol "REPS_AND_SETS"
And a workout exercise "Barbell Squat" exists with warmup exercise "High Knees" linked
When I send a POST request to "/api/workout-templates/{templateId}/exercises" with:
  """
  {
    "exerciseId": "barbell-squat-123",
    "phase": "Workout",
    "roundNumber": 1,
    "metadata": {
      "reps": 10,
      "weight": {"value": 60, "unit": "kg"}
    }
  }
  """
Then the response status should be 201
And the response should contain the added workout exercise
And the warmup exercise "High Knees" should be auto-added to warmup phase
And both exercises should have unique GUIDs
And the database should contain both exercises in the template
```

#### Scenario 2: Remove Workout Exercise with Orphan Cleanup
```gherkin
Given I am authenticated as "PT-Tier"
And a workout template exists with "Barbell Squat" in workout phase
And "High Knees" exists in warmup phase (auto-added)
And no other workout exercise uses "High Knees" as warmup
When I send a DELETE request to "/api/workout-templates/{templateId}/exercises/{exerciseGuid}"
Then the response status should be 200
And both "Barbell Squat" and "High Knees" should be removed
And the remaining exercises should maintain correct order
```

#### Scenario 3: Copy Round with New GUIDs
```gherkin
Given I am authenticated as "PT-Tier"
And a workout template has a workout round with 3 exercises
When I send a POST request to "/api/workout-templates/{templateId}/rounds/copy" with:
  """
  {
    "sourcePhase": "Workout",
    "sourceRoundNumber": 1,
    "targetPhase": "Workout",
    "targetRoundNumber": 2
  }
  """
Then the response status should be 201
And a new round 2 should exist with 3 exercises
And all exercises should have new GUIDs (different from source)
And all metadata should be preserved
```

#### Scenario 4: Reorder Exercise Within Round
```gherkin
Given I am authenticated as "PT-Tier"
And a workout template has exercises with OrderInRound [1,2,3,4]
When I send a PUT request to "/api/workout-templates/{templateId}/exercises/{exerciseGuid}/order" with:
  """
  {
    "newOrderInRound": 1
  }
  """
Then the response status should be 200
And the target exercise should have OrderInRound = 1
And other exercises should be reordered accordingly [2,3,4,5]
```

#### Scenario 5: Get Template Exercises Organized by Phase and Round
```gherkin
Given I am authenticated as "PT-Tier"
And a workout template has exercises across multiple phases and rounds
When I send a GET request to "/api/workout-templates/{templateId}/exercises"
Then the response status should be 200
And the response should be organized by phases: warmup, workout, cooldown
And each phase should contain rounds with exercises in correct order
And each exercise should include ExecutionProtocol-appropriate metadata
```

### Edge Cases to Test:
- [ ] Adding exercise to template in Production state (should fail)
- [ ] Removing exercise that doesn't exist
- [ ] Copying round to existing round number
- [ ] Auto-linking when warmup already exists manually
- [ ] Circular dependency prevention in auto-linking
- [ ] Empty metadata scenarios
- [ ] REST exercise handling with duration-only metadata
- [ ] Multiple rounds with same exercises (different metadata)

## Phase 1: Planning & Analysis - Estimated: 2h 0m

### Task 1.1: Study codebase for similar implementations and patterns
`[Pending]` (Est: 2h)

**Objective:**
- Search for similar entities/services/controllers in the codebase
- Identify patterns to follow and code that can be reused
- Document findings with specific file references
- Note any lessons learned from completed features

**Implementation Steps:**
- Use Grep/Glob tools to find similar implementations
- Analyze existing patterns in Services/, Controllers/, and Models/
- Review `/memory-bank/features/3-COMPLETED/` for similar features
- Document reusable code patterns with file paths

**Study Areas:**
1. **WorkoutTemplate Service Patterns**:
   - `/GetFitterGetBigger.API/Services/WorkoutTemplate/WorkoutTemplateService.cs` - Main service patterns
   - `/GetFitterGetBigger.API/Services/WorkoutTemplate/Features/Exercise/WorkoutTemplateExerciseService.cs` - Exercise management (to be refactored)
   - `/GetFitterGetBigger.API/Controllers/WorkoutTemplatesController.cs` - Controller patterns

2. **ExecutionProtocol Integration Patterns**:
   - `/GetFitterGetBigger.API/Models/Entities/ExecutionProtocol.cs` - Entity patterns
   - `/GetFitterGetBigger.API/Services/ReferenceTables/ExecutionProtocol/ExecutionProtocolService.cs` - Service patterns
   - `/GetFitterGetBigger.API/Controllers/ExecutionProtocolsController.cs` - API patterns

3. **ExerciseLink Integration Patterns**:
   - `/GetFitterGetBigger.API/Services/Exercise/Features/Links/ExerciseLinkService.cs` - Auto-linking logic
   - `/GetFitterGetBigger.API/Services/Exercise/Features/Links/DataServices/` - Repository patterns
   - `/GetFitterGetBigger.API/Models/Entities/ExerciseLink.cs` - Entity relationships

4. **JSON Metadata Storage Examples**:
   - Search for existing JSON field implementations
   - Review PostgreSQL JSON column patterns
   - Document JSON serialization/deserialization patterns

5. **Validation and Error Handling Patterns**:
   - `/GetFitterGetBigger.API/Services/Validation/ServiceValidate.cs` - Validation chains
   - `/GetFitterGetBigger.API/Services/Results/ServiceResult.cs` - Result patterns
   - `/GetFitterGetBigger.API/Constants/ErrorMessages/` - Error message patterns

**Deliverables:**
- List of similar implementations with file paths
- Patterns to follow (ServiceResult, Empty pattern, etc.)
- Code examples that can be adapted
- Critical warnings from lessons learned
- JSON metadata implementation strategy
- Auto-linking integration approach

**Critical Pattern References:**
- Check `/memory-bank/PracticalGuides/CommonImplementationPitfalls.md` Section 1: UnitOfWork usage
- Review ServiceValidate pattern from ExerciseLinkService implementation
- Study Entity Handler patterns from ExecutionProtocol.Handler

## CHECKPOINT: Phase 1 Complete - Planning & Analysis
`[Pending]`

**Requirements for Completion:**
- Build: ✅ 0 errors, 0 warnings
- Tests: ✅ All existing tests passing
- Git Commit Hash: [MANDATORY - To be added after commit]

**Planning Summary:**
- **Codebase Analysis**: Document existing patterns with specific file references
- **Integration Strategy**: ExecutionProtocol and ExerciseLink integration approach
- **Migration Plan**: Current WorkoutTemplateExercise to new flexible system
- **Test Strategy**: BDD scenarios and test structure defined

**Deliverables:**
- Study findings document with file references
- Integration approach for ExecutionProtocol and ExerciseLinks  
- Migration strategy from old to new system
- Risk assessment and mitigation strategies

**Code Review**: Follow `/memory-bank/DevelopmentGuidelines/Templates/FeatureCheckpointTemplate.md`
- **Status**: [To be filled after review]
- **Review Path**: `/memory-bank/features/2-IN_PROGRESS/FEAT-031-workout-template-exercise-management/code-reviews/Phase_1_Planning_Analysis/`

**Git Commit**: [MANDATORY - Phase cannot be marked complete without this]

## Phase 2: Models & Database - Estimated: 4h 0m

### Task 2.1: Rename ExecutionProtocol "Standard" to "Reps and Sets"
`[Pending]` (Est: 30m)

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

### Task 2.2: Add ExecutionProtocol integration to WorkoutTemplate entity
`[Pending]` (Est: 1h)

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

### Task 2.3: Create new WorkoutTemplateExercise entity design
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

### Task 2.4: Update DbContext configuration for new entities
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
`[Pending]`

**Requirements for Completion:**
- Build: ✅ 0 errors, 0 warnings  
- Tests: ✅ All existing tests passing + new entity tests
- Migration: ✅ EF Core migration created and tested
- Git Commit Hash: [MANDATORY - To be added after commit]

**Implementation Summary:**
- **ExecutionProtocol Migration**: "Standard" renamed to "Reps and Sets"
- **WorkoutTemplate Enhancement**: ExecutionProtocolId and Config fields added
- **WorkoutTemplateExercise**: Fresh entity design with JSON metadata
- **Database Schema**: New table created with proper indexes
- **EF Core Configuration**: Entity relationships and constraints configured

**Test Requirements:**
- New Tests Added: Entity creation, validation, JSON metadata
- Migration Tests: Data conversion and rollback
- EF Core Tests: Configuration and relationships
- All Existing Tests: Must remain passing

**Code Review**: Follow `/memory-bank/DevelopmentGuidelines/Templates/FeatureCheckpointTemplate.md`
- **Status**: [To be filled after review]
- **Review Path**: `/memory-bank/features/2-IN_PROGRESS/FEAT-031-workout-template-exercise-management/code-reviews/Phase_2_Models_Database/`

**Git Commit**: [MANDATORY - Phase cannot be marked complete without this]

## Phase 3: Repository Layer - Estimated: 3h 0m

### Task 3.1: Create IWorkoutTemplateExerciseRepository interface
`[Pending]` (Est: 1h)

**Implementation:**
- Create `/GetFitterGetBigger.API/Repositories/Interfaces/IWorkoutTemplateExerciseRepository.cs`
- Follow repository patterns from ExerciseLink implementation:

```csharp
public interface IWorkoutTemplateExerciseRepository
{
    // CRUD Operations
    Task<WorkoutTemplateExercise> GetByIdAsync(WorkoutTemplateExerciseId id);
    Task<List<WorkoutTemplateExercise>> GetByWorkoutTemplateAsync(WorkoutTemplateId workoutTemplateId);
    Task<List<WorkoutTemplateExercise>> GetByTemplateAndPhaseAsync(WorkoutTemplateId workoutTemplateId, string phase);
    Task<List<WorkoutTemplateExercise>> GetByTemplatePhaseAndRoundAsync(WorkoutTemplateId workoutTemplateId, string phase, int roundNumber);
    
    // Auto-linking support queries
    Task<List<WorkoutTemplateExercise>> GetWorkoutExercisesAsync(WorkoutTemplateId workoutTemplateId);
    Task<bool> ExistsInTemplateAsync(WorkoutTemplateId workoutTemplateId, ExerciseId exerciseId);
    Task<bool> ExistsInPhaseAsync(WorkoutTemplateId workoutTemplateId, string phase, ExerciseId exerciseId);
    
    // Order management
    Task<int> GetMaxOrderInRoundAsync(WorkoutTemplateId workoutTemplateId, string phase, int roundNumber);
    Task ReorderExercisesInRoundAsync(WorkoutTemplateId workoutTemplateId, string phase, int roundNumber, Dictionary<WorkoutTemplateExerciseId, int> newOrders);
    
    // Round management
    Task<List<WorkoutTemplateExercise>> GetRoundExercisesAsync(WorkoutTemplateId workoutTemplateId, string phase, int roundNumber);
    Task<int> GetMaxRoundNumberAsync(WorkoutTemplateId workoutTemplateId, string phase);
    
    // Modification operations
    Task AddAsync(WorkoutTemplateExercise exercise);
    Task AddRangeAsync(List<WorkoutTemplateExercise> exercises);
    Task UpdateAsync(WorkoutTemplateExercise exercise);
    Task DeleteAsync(WorkoutTemplateExerciseId id);
    Task DeleteRangeAsync(List<WorkoutTemplateExerciseId> ids);
}
```

**Unit Tests:**
- Test interface contract and method signatures
- Test specialized ID type usage
- Test parameter validation requirements

**Critical Patterns:**
- Follow interface patterns from `/GetFitterGetBigger.API/Repositories/Interfaces/IExerciseLinkRepository.cs`
- Use specialized ID types (WorkoutTemplateExerciseId, WorkoutTemplateId, ExerciseId)
- Support both individual and batch operations
- Include auto-linking support queries

### Task 3.2: Implement WorkoutTemplateExerciseRepository
`[Pending]` (Est: 2h)

**Implementation:**
- Create `/GetFitterGetBigger.API/Repositories/Implementations/WorkoutTemplateExerciseRepository.cs`
- Extend from RepositoryBase pattern:

```csharp
public class WorkoutTemplateExerciseRepository : RepositoryBase<WorkoutTemplateExercise, WorkoutTemplateExerciseId>, IWorkoutTemplateExerciseRepository
{
    public WorkoutTemplateExerciseRepository(FitnessDbContext context) : base(context) { }

    public async Task<List<WorkoutTemplateExercise>> GetByWorkoutTemplateAsync(WorkoutTemplateId workoutTemplateId)
    {
        return await Context.WorkoutTemplateExercises
            .Include(wte => wte.Exercise)
            .Where(wte => wte.WorkoutTemplateId == workoutTemplateId)
            .OrderBy(wte => wte.Phase)
            .ThenBy(wte => wte.RoundNumber)
            .ThenBy(wte => wte.OrderInRound)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<WorkoutTemplateExercise>> GetByTemplateAndPhaseAsync(WorkoutTemplateId workoutTemplateId, string phase)
    {
        return await Context.WorkoutTemplateExercises
            .Include(wte => wte.Exercise)
            .Where(wte => wte.WorkoutTemplateId == workoutTemplateId && wte.Phase == phase)
            .OrderBy(wte => wte.RoundNumber)
            .ThenBy(wte => wte.OrderInRound)
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<bool> ExistsInTemplateAsync(WorkoutTemplateId workoutTemplateId, ExerciseId exerciseId)
    {
        return await Context.WorkoutTemplateExercises
            .AnyAsync(wte => wte.WorkoutTemplateId == workoutTemplateId && 
                           wte.ExerciseId == exerciseId);
    }
    
    public async Task<int> GetMaxOrderInRoundAsync(WorkoutTemplateId workoutTemplateId, string phase, int roundNumber)
    {
        var maxOrder = await Context.WorkoutTemplateExercises
            .Where(wte => wte.WorkoutTemplateId == workoutTemplateId && 
                         wte.Phase == phase && 
                         wte.RoundNumber == roundNumber)
            .Select(wte => wte.OrderInRound)
            .DefaultIfEmpty(0)
            .MaxAsync();
            
        return maxOrder;
    }
    
    public async Task ReorderExercisesInRoundAsync(WorkoutTemplateId workoutTemplateId, string phase, int roundNumber, Dictionary<WorkoutTemplateExerciseId, int> newOrders)
    {
        var exercises = await Context.WorkoutTemplateExercises
            .Where(wte => wte.WorkoutTemplateId == workoutTemplateId && 
                         wte.Phase == phase && 
                         wte.RoundNumber == roundNumber)
            .ToListAsync();
        
        foreach (var exercise in exercises)
        {
            if (newOrders.TryGetValue(exercise.Id, out var newOrder))
            {
                // Use record 'with' syntax to update
                var updatedExercise = exercise with 
                { 
                    OrderInRound = newOrder,
                    UpdatedAt = DateTime.UtcNow
                };
                Context.WorkoutTemplateExercises.Update(updatedExercise);
            }
        }
    }
    
    // Additional methods following same patterns...
}
```

**Unit Tests:**
- Test all repository methods with mock data
- Test Include() navigation property loading
- Test ordering by Phase → Round → Order
- Test efficient queries with AsNoTracking()
- Test batch operations
- Test error scenarios (not found, invalid parameters)

**Critical Patterns:**
- Follow repository patterns from `/GetFitterGetBigger.API/Repositories/Implementations/ExerciseLinkRepository.cs`
- Use AsNoTracking() for query performance
- Use Include() for navigation properties following NavigationLoadingPattern
- Use record 'with' syntax for updates
- Extend from RepositoryBase<T, TId> pattern

## CHECKPOINT: Phase 3 Complete - Repository Layer
`[Pending]`

**Requirements for Completion:**
- Build: ✅ 0 errors, 0 warnings
- Tests: ✅ All repository methods tested + existing tests passing
- Performance: ✅ Query optimization with AsNoTracking() and indexes
- Git Commit Hash: [MANDATORY - To be added after commit]

**Implementation Summary:**
- **IWorkoutTemplateExerciseRepository**: Complete interface with auto-linking support
- **WorkoutTemplateExerciseRepository**: Full implementation with EF Core patterns
- **Query Optimization**: AsNoTracking() and Include() for performance
- **Batch Operations**: Support for bulk add/update/delete operations

**Test Requirements:**
- Repository Tests: All CRUD methods with comprehensive scenarios
- Performance Tests: Query execution time validation
- Integration Tests: Database operations with real DbContext
- Mock Tests: Unit tests with in-memory database

**Code Review**: Follow `/memory-bank/DevelopmentGuidelines/Templates/FeatureCheckpointTemplate.md`
- **Status**: [To be filled after review]
- **Review Path**: `/memory-bank/features/2-IN_PROGRESS/FEAT-031-workout-template-exercise-management/code-reviews/Phase_3_Repository_Layer/`

**Git Commit**: [MANDATORY - Phase cannot be marked complete without this]

## Phase 4: Service Layer - Estimated: 6h 0m

### Task 4.1: Create WorkoutTemplateExerciseService interface
`[Pending]` (Est: 1h)

**CRITICAL**: Before implementing service methods, review `/memory-bank/PracticalGuides/CommonImplementationPitfalls.md` Section 1 about UnitOfWork usage.

**Implementation:**
- Create `/GetFitterGetBigger.API/Services/WorkoutTemplate/Features/Exercise/IWorkoutTemplateExerciseService.cs`
- Follow service patterns from ExerciseLink implementation:

```csharp
public interface IWorkoutTemplateExerciseService
{
    // Core exercise management
    Task<ServiceResult<AddExerciseResultDto>> AddExerciseAsync(WorkoutTemplateId templateId, AddExerciseDto dto);
    Task<ServiceResult<RemoveExerciseResultDto>> RemoveExerciseAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId);
    Task<ServiceResult<UpdateMetadataResultDto>> UpdateExerciseMetadataAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId, string metadata);
    Task<ServiceResult<ReorderResultDto>> ReorderExerciseAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId, int newOrderInRound);
    
    // Round management
    Task<ServiceResult<CopyRoundResultDto>> CopyRoundAsync(WorkoutTemplateId templateId, CopyRoundDto dto);
    
    // Query operations  
    Task<ServiceResult<WorkoutTemplateExercisesDto>> GetTemplateExercisesAsync(WorkoutTemplateId templateId);
    Task<ServiceResult<WorkoutTemplateExerciseDto>> GetExerciseByIdAsync(WorkoutTemplateExerciseId exerciseId);
    
    // Validation
    Task<ServiceResult<BooleanResultDto>> ValidateExerciseMetadataAsync(ExerciseId exerciseId, ExecutionProtocolId protocolId, string metadata);
}
```

**DTOs Required:**
```csharp
public record AddExerciseDto(
    ExerciseId ExerciseId,
    string Phase,
    int RoundNumber,
    string Metadata);

public record AddExerciseResultDto(
    List<WorkoutTemplateExerciseDto> AddedExercises,
    string Message);

public record RemoveExerciseResultDto(
    List<WorkoutTemplateExerciseDto> RemovedExercises,
    string Message);

public record CopyRoundDto(
    string SourcePhase,
    int SourceRoundNumber,
    string TargetPhase,
    int TargetRoundNumber);

public record WorkoutTemplateExercisesDto(
    WorkoutTemplateId TemplateId,
    string TemplateName,
    ExecutionProtocolDto ExecutionProtocol,
    WorkoutPhaseDto Warmup,
    WorkoutPhaseDto Workout,
    WorkoutPhaseDto Cooldown);

public record WorkoutPhaseDto(
    List<RoundDto> Rounds);

public record RoundDto(
    int RoundNumber,
    List<WorkoutTemplateExerciseDto> Exercises);
```

**Unit Tests:**
- Test interface contract definition
- Test DTO structure and validation
- Test ServiceResult<T> return types

**Critical Patterns:**
- Follow interface patterns from `/GetFitterGetBigger.API/Services/Exercise/Features/Links/IExerciseLinkService.cs`
- All methods return ServiceResult<T>
- Use specialized ID types throughout
- Support auto-linking operations

### Task 4.2: Implement core service methods with auto-linking logic
`[Pending]` (Est: 3h)

**CRITICAL**: Read `/memory-bank/PracticalGuides/CommonImplementationPitfalls.md` before starting. Use ReadOnlyUnitOfWork for validation, WritableUnitOfWork ONLY for modifications.

**Implementation:**
- Create `/GetFitterGetBigger.API/Services/WorkoutTemplate/Features/Exercise/WorkoutTemplateExerciseService.cs`
- Follow patterns from ExerciseLinkService with proper UnitOfWork usage:

```csharp
public class WorkoutTemplateExerciseService : IWorkoutTemplateExerciseService
{
    private readonly IUnitOfWorkProvider<FitnessDbContext> _unitOfWorkProvider;
    private readonly IExerciseLinkQueryDataService _exerciseLinkDataService;
    private readonly ILogger<WorkoutTemplateExerciseService> _logger;

    public async Task<ServiceResult<AddExerciseResultDto>> AddExerciseAsync(WorkoutTemplateId templateId, AddExerciseDto dto)
    {
        return await ServiceValidate.Build<AddExerciseResultDto>()
            .EnsureNotEmpty(templateId, WorkoutTemplateExerciseErrorMessages.WorkoutTemplateNotFound)
            .EnsureNotEmpty(dto.ExerciseId, WorkoutTemplateExerciseErrorMessages.InvalidExerciseId)
            .EnsureNotEmpty(dto.Phase, WorkoutTemplateExerciseErrorMessages.InvalidZone)
            .Ensure(() => IsValidPhase(dto.Phase), WorkoutTemplateExerciseErrorMessages.InvalidZoneWarmupMainCooldown)
            .EnsureMinValue(dto.RoundNumber, 1, "Round number must be at least 1")
            .EnsureNotEmpty(dto.Metadata, "Metadata is required for exercise configuration")
            .EnsureAsync(
                async () => await IsTemplateInDraftStateAsync(templateId),
                WorkoutTemplateExerciseErrorMessages.CanOnlyAddExercisesToDraftTemplates)
            .EnsureAsync(
                async () => await IsExerciseActiveAsync(dto.ExerciseId),
                WorkoutTemplateExerciseErrorMessages.ExerciseNotFound)
            .MatchAsync(
                whenValid: async () => await ProcessAddExerciseWithAutoLinkingAsync(templateId, dto),
                whenInvalid: errors => ServiceResult<AddExerciseResultDto>.Failure(
                    AddExerciseResultDto.Empty,
                    errors.FirstOrDefault()));
    }

    private async Task<ServiceResult<AddExerciseResultDto>> ProcessAddExerciseWithAutoLinkingAsync(WorkoutTemplateId templateId, AddExerciseDto dto)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateWritable();
        var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
        var addedExercises = new List<WorkoutTemplateExercise>();

        // Calculate order in round
        var orderInRound = await repository.GetMaxOrderInRoundAsync(templateId, dto.Phase, dto.RoundNumber) + 1;

        // Create main exercise
        var mainExercise = WorkoutTemplateExercise.Handler.CreateNew(
            templateId,
            dto.ExerciseId,
            dto.Phase,
            dto.RoundNumber,
            orderInRound,
            dto.Metadata);

        if (!mainExercise.IsSuccess)
        {
            return ServiceResult<AddExerciseResultDto>.Failure(
                AddExerciseResultDto.Empty,
                ServiceError.ValidationFailed(string.Join(", ", mainExercise.Errors)));
        }

        await repository.AddAsync(mainExercise.Value);
        addedExercises.Add(mainExercise.Value);

        // Auto-link if this is a workout exercise
        if (dto.Phase == "Workout")
        {
            var linkedExercises = await AddAutoLinkedExercisesAsync(repository, templateId, dto.ExerciseId);
            addedExercises.AddRange(linkedExercises);
        }

        await unitOfWork.CommitAsync();

        var resultDto = new AddExerciseResultDto(
            addedExercises.Select(e => MapToDto(e)).ToList(),
            $"Successfully added {addedExercises.Count} exercise(s)");

        return ServiceResult<AddExerciseResultDto>.Success(resultDto);
    }

    private async Task<List<WorkoutTemplateExercise>> AddAutoLinkedExercisesAsync(
        IWorkoutTemplateExerciseRepository repository, 
        WorkoutTemplateId templateId, 
        ExerciseId workoutExerciseId)
    {
        var addedExercises = new List<WorkoutTemplateExercise>();

        // Use ReadOnlyUnitOfWork for querying ExerciseLinks
        using var readOnlyUow = _unitOfWorkProvider.CreateReadOnly();
        
        // Query linked exercises (warmup/cooldown)
        // Note: GetLinkedExercisesAsync doesn't exist, use GetBySourceExerciseAsync for each type
        var warmupLinks = await _exerciseLinkDataService.GetBySourceExerciseAsync(workoutExerciseId, ExerciseLinkType.WARMUP.ToString());
        var cooldownLinks = await _exerciseLinkDataService.GetBySourceExerciseAsync(workoutExerciseId, ExerciseLinkType.COOLDOWN.ToString());
        
        // Combine results
        var allLinks = new List<ExerciseLinkDto>();
        if (warmupLinks.IsSuccess) allLinks.AddRange(warmupLinks.Data);
        if (cooldownLinks.IsSuccess) allLinks.AddRange(cooldownLinks.Data);
        
        var linkedExercises = ServiceResult<List<ExerciseLinkDto>>.Success(allLinks);
        
        if (!linkedExercises.IsSuccess || !linkedExercises.Data.Any())
            return addedExercises;

        foreach (var linkedExercise in linkedExercises.Data)
        {
            var targetPhase = linkedExercise.LinkType == ExerciseLinkType.WARMUP ? "Warmup" : "Cooldown";
            
            // Check if already exists in template
            var alreadyExists = await repository.ExistsInPhaseAsync(templateId, targetPhase, linkedExercise.TargetExerciseId);
            
            if (!alreadyExists)
            {
                // Get max round number for target phase, default to 1
                var maxRound = await repository.GetMaxRoundNumberAsync(templateId, targetPhase);
                var targetRound = Math.Max(1, maxRound);
                
                var orderInPhase = await repository.GetMaxOrderInRoundAsync(templateId, targetPhase, targetRound) + 1;

                var autoLinkedExercise = WorkoutTemplateExercise.Handler.CreateNew(
                    templateId,
                    linkedExercise.TargetExerciseId,
                    targetPhase,
                    targetRound,
                    orderInPhase,
                    "{}"); // Empty metadata - PT must configure

                if (autoLinkedExercise.IsSuccess)
                {
                    await repository.AddAsync(autoLinkedExercise.Value);
                    addedExercises.Add(autoLinkedExercise.Value);
                }
            }
        }

        return addedExercises;
    }

    // Critical: Use ReadOnlyUnitOfWork for validation queries
    private async Task<bool> IsTemplateInDraftStateAsync(WorkoutTemplateId templateId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var templateRepo = unitOfWork.GetRepository<IWorkoutTemplateRepository>();
        
        var template = await templateRepo.GetByIdAsync(templateId);
        return !template.IsEmpty && template.WorkoutState?.Value == "Draft";
    }

    private async Task<bool> IsExerciseActiveAsync(ExerciseId exerciseId)
    {
        using var unitOfWork = _unitOfWorkProvider.CreateReadOnly();
        var exerciseRepo = unitOfWork.GetRepository<IExerciseRepository>();
        
        var exercise = await exerciseRepo.GetByIdAsync(exerciseId);
        return !exercise.IsEmpty && exercise.IsActive;
    }
}
```

**Unit Tests:**
- Test AddExerciseAsync with all validation scenarios
- Test auto-linking logic for workout exercises
- Test proper UnitOfWork usage (ReadOnly vs Writable)
- Test error handling and ServiceResult patterns
- Test ServiceValidate chain execution

**Critical Patterns:**
- **CRITICAL**: Use ReadOnlyUnitOfWork for ALL validation queries
- Only use WritableUnitOfWork for actual data modifications
- Follow ServiceValidate pattern from `/GetFitterGetBigger.API/Services/Exercise/Features/Links/ExerciseLinkService.cs`
- Use ServiceResult<T> pattern consistently
- Check `/memory-bank/PracticalGuides/CommonImplementationPitfalls.md` Section 1

### Task 4.3: Implement remove exercise with orphan cleanup
`[Pending]` (Est: 1h 30m)

**Implementation:**
- Implement RemoveExerciseAsync with orphan detection logic:

```csharp
public async Task<ServiceResult<RemoveExerciseResultDto>> RemoveExerciseAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId)
{
    return await ServiceValidate.Build<RemoveExerciseResultDto>()
        .EnsureNotEmpty(templateId, WorkoutTemplateExerciseErrorMessages.InvalidTemplateId)
        .EnsureNotEmpty(exerciseId, WorkoutTemplateExerciseErrorMessages.InvalidExerciseId)
        .EnsureAsync(
            async () => await IsTemplateInDraftStateAsync(templateId),
            WorkoutTemplateExerciseErrorMessages.TemplateNotInDraftState)
        .EnsureAsync(
            async () => await DoesExerciseExistInTemplateAsync(exerciseId, templateId),
            WorkoutTemplateExerciseErrorMessages.TemplateExerciseNotFound)
        .MatchAsync(
            whenValid: async () => await ProcessRemoveExerciseWithCleanupAsync(templateId, exerciseId));
}

private async Task<ServiceResult<RemoveExerciseResultDto>> ProcessRemoveExerciseWithCleanupAsync(WorkoutTemplateId templateId, WorkoutTemplateExerciseId exerciseId)
{
    using var unitOfWork = _unitOfWorkProvider.CreateWritable();
    var repository = unitOfWork.GetRepository<IWorkoutTemplateExerciseRepository>();
    var removedExercises = new List<WorkoutTemplateExercise>();

    // Get the exercise being removed
    var exerciseToRemove = await repository.GetByIdAsync(exerciseId);
    removedExercises.Add(exerciseToRemove);

    // If it's a workout exercise, find orphaned warmup/cooldown exercises
    if (exerciseToRemove.Phase == "Workout")
    {
        var orphanedExercises = await FindOrphanedExercisesAsync(repository, templateId, exerciseToRemove.ExerciseId);
        removedExercises.AddRange(orphanedExercises);
    }

    // Remove all exercises
    var exerciseIds = removedExercises.Select(e => e.Id).ToList();
    await repository.DeleteRangeAsync(exerciseIds);

    // Reorder remaining exercises in affected rounds
    await ReorderRemainingExercisesAsync(repository, templateId, removedExercises);

    await unitOfWork.CommitAsync();

    var resultDto = new RemoveExerciseResultDto(
        removedExercises.Select(e => MapToDto(e)).ToList(),
        $"Successfully removed {removedExercises.Count} exercise(s)");

    return ServiceResult<RemoveExerciseResultDto>.Success(resultDto);
}

private async Task<List<WorkoutTemplateExercise>> FindOrphanedExercisesAsync(
    IWorkoutTemplateExerciseRepository repository, 
    WorkoutTemplateId templateId, 
    ExerciseId removedWorkoutExerciseId)
{
    var orphanedExercises = new List<WorkoutTemplateExercise>();

    // Use ReadOnlyUnitOfWork for querying ExerciseLinks
    using var readOnlyUow = _unitOfWorkProvider.CreateReadOnly();
    
    // Get all linked exercises for the removed workout exercise
    var warmupLinks = await _exerciseLinkDataService.GetBySourceExerciseAsync(removedWorkoutExerciseId, ExerciseLinkType.WARMUP.ToString());
    var cooldownLinks = await _exerciseLinkDataService.GetBySourceExerciseAsync(removedWorkoutExerciseId, ExerciseLinkType.COOLDOWN.ToString());
    
    // Combine results
    var allLinks = new List<ExerciseLinkDto>();
    if (warmupLinks.IsSuccess) allLinks.AddRange(warmupLinks.Data);
    if (cooldownLinks.IsSuccess) allLinks.AddRange(cooldownLinks.Data);
    
    var linkedExercises = ServiceResult<List<ExerciseLinkDto>>.Success(allLinks);
    
    if (!linkedExercises.IsSuccess) return orphanedExercises;

    foreach (var linkedExercise in linkedExercises.Data)
    {
        // Check if this warmup/cooldown exercise is used by any OTHER workout exercise
        var otherWorkoutExercises = await repository.GetWorkoutExercisesAsync(templateId);
        var stillNeeded = false;

        foreach (var otherWorkout in otherWorkoutExercises.Where(e => e.ExerciseId != removedWorkoutExerciseId))
        {
            // Check if other workout exercises link to this warmup/cooldown
            // Use the specific link type to check for dependencies
            var otherLinks = await _exerciseLinkDataService.GetBySourceExerciseAsync(
                otherWorkout.ExerciseId, 
                linkedExercise.LinkType.ToString());
            
            if (otherLinks.IsSuccess && otherLinks.Data.Any(l => l.TargetExerciseId == linkedExercise.TargetExerciseId))
            {
                stillNeeded = true;
                break;
            }
        }

        if (!stillNeeded)
        {
            // Find this exercise in the template and mark for removal
            var targetPhase = linkedExercise.LinkType == ExerciseLinkType.WARMUP ? "Warmup" : "Cooldown";
            var exercisesInPhase = await repository.GetByTemplateAndPhaseAsync(templateId, targetPhase);
            var orphan = exercisesInPhase.FirstOrDefault(e => e.ExerciseId == linkedExercise.TargetExerciseId);
            
            if (orphan != null)
            {
                orphanedExercises.Add(orphan);
            }
        }
    }

    return orphanedExercises;
}
```

**Unit Tests:**
- Test RemoveExerciseAsync with various scenarios
- Test orphan detection logic
- Test that non-orphaned exercises are preserved
- Test reordering after removal
- Test ServiceValidate chain for removal

**Critical Patterns:**
- Use ReadOnlyUnitOfWork for orphan detection queries
- Use WritableUnitOfWork only for actual deletions
- Follow complex business logic patterns from ExerciseLinkService
- Proper error handling with ServiceResult<T>

### Task 4.4: Implement round management and reordering
`[Pending]` (Est: 30m)

**Implementation:**
- Implement CopyRoundAsync and ReorderExerciseAsync methods following the same patterns
- Include proper validation and ServiceResult<T> return types
- Use appropriate UnitOfWork patterns

**Unit Tests:**
- Test round copying with new GUIDs
- Test exercise reordering within rounds
- Test validation scenarios for round operations

## CHECKPOINT: Phase 4 Complete - Service Layer
`[Pending]`

**Requirements for Completion:**
- Build: ✅ 0 errors, 0 warnings
- Tests: ✅ All service methods tested + existing tests passing
- Patterns: ✅ ServiceValidate and ServiceResult<T> used consistently
- UnitOfWork: ✅ Proper ReadOnly vs Writable usage verified
- Git Commit Hash: [MANDATORY - To be added after commit]

**Implementation Summary:**
- **IWorkoutTemplateExerciseService**: Complete interface with all required methods
- **Auto-linking Logic**: Integration with ExerciseLinks for automatic warmup/cooldown
- **Orphan Cleanup**: Intelligent removal of unused auto-added exercises
- **Round Management**: Copy and reorder functionality
- **Validation**: Comprehensive ServiceValidate chains

**Test Requirements:**
- Service Tests: All methods with comprehensive scenario coverage
- Auto-linking Tests: Complex business logic validation
- UnitOfWork Tests: Proper ReadOnly vs Writable usage
- Integration Tests: End-to-end service functionality

**Code Review**: Follow `/memory-bank/DevelopmentGuidelines/Templates/FeatureCheckpointTemplate.md`
- **Status**: [To be filled after review]
- **Review Path**: `/memory-bank/features/2-IN_PROGRESS/FEAT-031-workout-template-exercise-management/code-reviews/Phase_4_Service_Layer/`

**Git Commit**: [MANDATORY - Phase cannot be marked complete without this]

## Phase 5: API Controllers - Estimated: 3h 0m

### Task 5.1: Create DTOs and request/response models
`[Pending]` (Est: 1h)

**Implementation:**
- Create comprehensive DTO structure in `/GetFitterGetBigger.API/DTOs/WorkoutTemplateExercise/`
- Follow DTO patterns from ExerciseLink feature:

```csharp
// Requests
public record AddExerciseToTemplateRequest(
    string ExerciseId,
    string Phase,
    int RoundNumber,
    JsonDocument Metadata);

public record UpdateExerciseMetadataRequest(
    JsonDocument Metadata);

public record ReorderExerciseRequest(
    int NewOrderInRound);

public record CopyRoundRequest(
    string SourcePhase,
    int SourceRoundNumber,
    string TargetPhase,
    int TargetRoundNumber);

// Responses  
public record WorkoutTemplateExercisesResponseDto(
    bool Success,
    WorkoutTemplateExercisesDto Data,
    string Message = "",
    List<string> Errors = default);

public record AddExerciseResponseDto(
    bool Success,
    AddExerciseResultDto Data,
    string Message = "",
    List<string> Errors = default);

// Core DTOs
public record WorkoutTemplateExerciseDto(
    string Id,
    string ExerciseId,
    string ExerciseName,
    string ExerciseType,
    string Phase,
    int RoundNumber,
    int OrderInRound,
    JsonDocument Metadata,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static WorkoutTemplateExerciseDto Empty => new(
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        string.Empty,
        0,
        0,
        JsonDocument.Parse("{}"),
        DateTime.MinValue,
        DateTime.MinValue);
}
```

**Unit Tests:**
- Test DTO creation and serialization
- Test Empty pattern implementation
- Test JsonDocument metadata handling
- Test request validation attributes

**Critical Patterns:**
- Follow DTO patterns from `/GetFitterGetBigger.API/DTOs/ExerciseLinkDto.cs`
- Use JsonDocument for metadata (not string)
- Implement Empty pattern for all DTOs
- Use record types for immutability

### Task 5.2: Create WorkoutTemplateExercisesController
`[Pending]` (Est: 1h 30m)

**Implementation:**
- Create `/GetFitterGetBigger.API/Controllers/WorkoutTemplateExercisesController.cs`
- Follow controller patterns from existing controllers:

```csharp
[ApiController]
[Route("api/workout-templates/{templateId}/exercises")]
[Authorize]
public class WorkoutTemplateExercisesController : ControllerBase
{
    private readonly IWorkoutTemplateExerciseService _service;

    public WorkoutTemplateExercisesController(IWorkoutTemplateExerciseService service)
    {
        _service = service;
    }

    /// <summary>
    /// Add exercise to workout template with automatic warmup/cooldown linking
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AddExerciseResponseDto), 201)]
    [ProducesResponseType(typeof(ErrorResponseDto), 400)]
    [ProducesResponseType(typeof(ErrorResponseDto), 404)]
    public async Task<IActionResult> AddExercise(
        [FromRoute] string templateId,
        [FromBody] AddExerciseToTemplateRequest request)
    {
        var templateIdParsed = WorkoutTemplateId.ParseOrEmpty(templateId);
        var exerciseIdParsed = ExerciseId.ParseOrEmpty(request.ExerciseId);

        var dto = new AddExerciseDto(
            exerciseIdParsed,
            request.Phase,
            request.RoundNumber,
            request.Metadata.RootElement.GetRawText());

        var result = await _service.AddExerciseAsync(templateIdParsed, dto);

        return result switch
        {
            { IsSuccess: true } => CreatedAtAction(
                nameof(GetTemplateExercises),
                new { templateId },
                new AddExerciseResponseDto(true, result.Data)),
            { Errors: var errors } when errors.Any(e => e.Message.Contains("not found")) => 
                NotFound(new ErrorResponseDto(false, errors.Select(e => e.Message).ToList())),
            { Errors: var errors } => 
                BadRequest(new ErrorResponseDto(false, errors.Select(e => e.Message).ToList()))
        };
    }

    /// <summary>
    /// Remove exercise from workout template with automatic orphan cleanup
    /// </summary>
    [HttpDelete("{exerciseId}")]
    [ProducesResponseType(typeof(RemoveExerciseResponseDto), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 400)]
    [ProducesResponseType(typeof(ErrorResponseDto), 404)]
    public async Task<IActionResult> RemoveExercise(
        [FromRoute] string templateId,
        [FromRoute] string exerciseId)
    {
        var templateIdParsed = WorkoutTemplateId.ParseOrEmpty(templateId);
        var exerciseIdParsed = WorkoutTemplateExerciseId.ParseOrEmpty(exerciseId);

        var result = await _service.RemoveExerciseAsync(templateIdParsed, exerciseIdParsed);

        return result switch
        {
            { IsSuccess: true } => Ok(new RemoveExerciseResponseDto(true, result.Data)),
            { Errors: var errors } when errors.Any(e => e.Message.Contains("not found")) => 
                NotFound(new ErrorResponseDto(false, errors.Select(e => e.Message).ToList())),
            { Errors: var errors } => 
                BadRequest(new ErrorResponseDto(false, errors.Select(e => e.Message).ToList()))
        };
    }

    /// <summary>
    /// Get all exercises for a workout template organized by phase and round
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(WorkoutTemplateExercisesResponseDto), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 404)]
    public async Task<IActionResult> GetTemplateExercises([FromRoute] string templateId)
    {
        var templateIdParsed = WorkoutTemplateId.ParseOrEmpty(templateId);

        var result = await _service.GetTemplateExercisesAsync(templateIdParsed);

        return result switch
        {
            { IsSuccess: true } => Ok(new WorkoutTemplateExercisesResponseDto(true, result.Data)),
            { Errors: var errors } when errors.Any(e => e.Message.Contains("not found")) => 
                NotFound(new ErrorResponseDto(false, errors.Select(e => e.Message).ToList())),
            { Errors: var errors } => 
                BadRequest(new ErrorResponseDto(false, errors.Select(e => e.Message).ToList()))
        };
    }

    /// <summary>
    /// Update exercise metadata
    /// </summary>
    [HttpPut("{exerciseId}/metadata")]
    [ProducesResponseType(typeof(UpdateMetadataResponseDto), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 400)]
    [ProducesResponseType(typeof(ErrorResponseDto), 404)]
    public async Task<IActionResult> UpdateExerciseMetadata(
        [FromRoute] string templateId,
        [FromRoute] string exerciseId,
        [FromBody] UpdateExerciseMetadataRequest request)
    {
        var templateIdParsed = WorkoutTemplateId.ParseOrEmpty(templateId);
        var exerciseIdParsed = WorkoutTemplateExerciseId.ParseOrEmpty(exerciseId);

        var result = await _service.UpdateExerciseMetadataAsync(
            templateIdParsed, 
            exerciseIdParsed, 
            request.Metadata.RootElement.GetRawText());

        return result switch
        {
            { IsSuccess: true } => Ok(new UpdateMetadataResponseDto(true, result.Data)),
            { Errors: var errors } when errors.Any(e => e.Message.Contains("not found")) => 
                NotFound(new ErrorResponseDto(false, errors.Select(e => e.Message).ToList())),
            { Errors: var errors } => 
                BadRequest(new ErrorResponseDto(false, errors.Select(e => e.Message).ToList()))
        };
    }

    /// <summary>
    /// Reorder exercise within its round
    /// </summary>
    [HttpPut("{exerciseId}/order")]
    [ProducesResponseType(typeof(ReorderResponseDto), 200)]
    [ProducesResponseType(typeof(ErrorResponseDto), 400)]
    [ProducesResponseType(typeof(ErrorResponseDto), 404)]
    public async Task<IActionResult> ReorderExercise(
        [FromRoute] string templateId,
        [FromRoute] string exerciseId,
        [FromBody] ReorderExerciseRequest request)
    {
        var templateIdParsed = WorkoutTemplateId.ParseOrEmpty(templateId);
        var exerciseIdParsed = WorkoutTemplateExerciseId.ParseOrEmpty(exerciseId);

        var result = await _service.ReorderExerciseAsync(templateIdParsed, exerciseIdParsed, request.NewOrderInRound);

        return result switch
        {
            { IsSuccess: true } => Ok(new ReorderResponseDto(true, result.Data)),
            { Errors: var errors } when errors.Any(e => e.Message.Contains("not found")) => 
                NotFound(new ErrorResponseDto(false, errors.Select(e => e.Message).ToList())),
            { Errors: var errors } => 
                BadRequest(new ErrorResponseDto(false, errors.Select(e => e.Message).ToList()))
        };
    }
}
```

**Unit Tests:**
- Test all controller actions with various scenarios
- Test proper HTTP status code handling
- Test request/response serialization
- Test authorization requirements
- Test pattern matching for error scenarios

**Critical Patterns:**
- Follow controller patterns from `/GetFitterGetBigger.API/Controllers/ExerciseLinksController.cs`
- Use pattern matching for ServiceResult<T> handling
- Use ParseOrEmpty for ID conversion (don't validate in controller)
- Let service layer handle all business validation
- Proper HTTP status codes (201 for creation, 200 for updates)

### Task 5.3: Create round management endpoints
`[Pending]` (Est: 30m)

**Implementation:**
- Add round management endpoints to the controller:

```csharp
/// <summary>
/// Copy round with all exercises and generate new GUIDs
/// </summary>
[HttpPost("rounds/copy")]
[ProducesResponseType(typeof(CopyRoundResponseDto), 201)]
[ProducesResponseType(typeof(ErrorResponseDto), 400)]
[ProducesResponseType(typeof(ErrorResponseDto), 404)]
public async Task<IActionResult> CopyRound(
    [FromRoute] string templateId,
    [FromBody] CopyRoundRequest request)
{
    var templateIdParsed = WorkoutTemplateId.ParseOrEmpty(templateId);

    var dto = new CopyRoundDto(
        request.SourcePhase,
        request.SourceRoundNumber,
        request.TargetPhase,
        request.TargetRoundNumber);

    var result = await _service.CopyRoundAsync(templateIdParsed, dto);

    return result switch
    {
        { IsSuccess: true } => CreatedAtAction(
            nameof(GetTemplateExercises),
            new { templateId },
            new CopyRoundResponseDto(true, result.Data)),
        { Errors: var errors } when errors.Any(e => e.Message.Contains("not found")) => 
            NotFound(new ErrorResponseDto(false, errors.Select(e => e.Message).ToList())),
        { Errors: var errors } => 
            BadRequest(new ErrorResponseDto(false, errors.Select(e => e.Message).ToList()))
    };
}
```

**Unit Tests:**
- Test round copying functionality
- Test proper response handling
- Test error scenarios

## CHECKPOINT: Phase 5 Complete - API Controllers
`[Pending]`

**Requirements for Completion:**
- Build: ✅ 0 errors, 0 warnings
- Tests: ✅ All controller actions tested + existing tests passing
- OpenAPI: ✅ Swagger documentation complete
- Git Commit Hash: [MANDATORY - To be added after commit]

**Implementation Summary:**
- **DTOs**: Complete request/response model structure
- **WorkoutTemplateExercisesController**: All CRUD endpoints with proper patterns
- **Round Management**: Copy round functionality
- **Error Handling**: Pattern matching for ServiceResult<T>
- **OpenAPI**: Comprehensive Swagger documentation

**Test Requirements:**
- Controller Tests: All actions with comprehensive HTTP scenarios
- DTO Tests: Serialization and validation
- Integration Tests: End-to-end HTTP functionality
- OpenAPI Tests: Documentation generation validation

**Code Review**: Follow `/memory-bank/DevelopmentGuidelines/Templates/FeatureCheckpointTemplate.md`
- **Status**: [To be filled after review]
- **Review Path**: `/memory-bank/features/2-IN_PROGRESS/FEAT-031-workout-template-exercise-management/code-reviews/Phase_5_API_Controllers/`

**Git Commit**: [MANDATORY - Phase cannot be marked complete without this]

## Phase 6: Integration & Testing - Estimated: 4h 0m

### Task 6.1: Create comprehensive BDD integration tests
`[Pending]` (Est: 2h)

**IMPORTANT: Test Builder Pattern**
- ALL test data creation MUST use Test Builder pattern
- Do NOT use `new()` constructors directly in tests
- Create builders for: WorkoutTemplate, WorkoutTemplateExercise, Exercise, ExerciseLink
- Builders provide readable, maintainable test setup
- Example: `new WorkoutTemplateBuilder().WithName("Test").WithProtocol("REPS_AND_SETS").Build()`

**Implementation:**
- Create `/GetFitterGetBigger.API.IntegrationTests/Features/WorkoutTemplate/WorkoutTemplateExerciseManagement.feature`
- Implement all BDD scenarios defined in the planning phase:

```csharp
[Collection("IntegrationTestCollection")]
public class WorkoutTemplateExerciseManagementFeature : IntegrationTestBase
{
    [Fact]
    public async Task Add_Workout_Exercise_Should_Auto_Add_Linked_Exercises()
    {
        // Given
        // NOTE: These helper methods need to be implemented using Test Builders pattern
        // Do NOT use new() constructors directly - use builders for readable tests
        var template = await CreateTestWorkoutTemplateAsync("Leg Burning I", "REPS_AND_SETS");
        var workoutExercise = await CreateTestExerciseAsync("Barbell Squat", ExerciseType.WORKOUT);
        var warmupExercise = await CreateTestExerciseAsync("High Knees", ExerciseType.WARMUP);
        await CreateExerciseLinkAsync(workoutExercise.Id, warmupExercise.Id, ExerciseLinkType.WARMUP);

        // When
        var response = await PostAsync($"/api/workout-templates/{template.Id}/exercises", new
        {
            ExerciseId = workoutExercise.Id,
            Phase = "Workout",
            RoundNumber = 1,
            Metadata = new { reps = 10, weight = new { value = 60, unit = "kg" } }
        });

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadAsAsync<AddExerciseResponseDto>();
        result.Success.Should().BeTrue();
        result.Data.AddedExercises.Should().HaveCount(2);
        result.Data.AddedExercises.Should().Contain(e => e.Phase == "Workout" && e.ExerciseId == workoutExercise.Id);
        result.Data.AddedExercises.Should().Contain(e => e.Phase == "Warmup" && e.ExerciseId == warmupExercise.Id);

        // Verify in database
        var templateExercises = await GetTemplateExercisesFromDbAsync(template.Id);
        templateExercises.Should().HaveCount(2);
    }

    [Fact]
    public async Task Remove_Workout_Exercise_Should_Remove_Orphaned_Exercises()
    {
        // Given
        var template = await CreateTestWorkoutTemplateAsync("Test Template", "REPS_AND_SETS");
        var workoutExercise = await CreateTestExerciseAsync("Barbell Squat", ExerciseType.WORKOUT);
        var warmupExercise = await CreateTestExerciseAsync("High Knees", ExerciseType.WARMUP);
        await CreateExerciseLinkAsync(workoutExercise.Id, warmupExercise.Id, ExerciseLinkType.WARMUP);

        // Add exercises to template
        var addResponse = await PostAsync($"/api/workout-templates/{template.Id}/exercises", new
        {
            ExerciseId = workoutExercise.Id,
            Phase = "Workout",
            RoundNumber = 1,
            Metadata = new { reps = 10 }
        });
        var addResult = await addResponse.Content.ReadAsAsync<AddExerciseResponseDto>();
        var workoutExerciseGuid = addResult.Data.AddedExercises.First(e => e.Phase == "Workout").Id;

        // When
        var response = await DeleteAsync($"/api/workout-templates/{template.Id}/exercises/{workoutExerciseGuid}");

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadAsAsync<RemoveExerciseResponseDto>();
        result.Success.Should().BeTrue();
        result.Data.RemovedExercises.Should().HaveCount(2);

        // Verify in database
        var remainingExercises = await GetTemplateExercisesFromDbAsync(template.Id);
        remainingExercises.Should().BeEmpty();
    }

    [Fact]
    public async Task Copy_Round_Should_Create_New_Exercises_With_Different_GUIDs()
    {
        // Given
        var template = await CreateTestWorkoutTemplateAsync("Test Template", "REPS_AND_SETS");
        await AddExerciseToTemplateAsync(template.Id, "Workout", 1, 1, new { reps = 10 });
        await AddExerciseToTemplateAsync(template.Id, "Workout", 1, 2, new { reps = 15 });
        await AddExerciseToTemplateAsync(template.Id, "Workout", 1, 3, new { reps = 20 });

        // When
        var response = await PostAsync($"/api/workout-templates/{template.Id}/rounds/copy", new
        {
            SourcePhase = "Workout",
            SourceRoundNumber = 1,
            TargetPhase = "Workout", 
            TargetRoundNumber = 2
        });

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadAsAsync<CopyRoundResponseDto>();
        result.Success.Should().BeTrue();
        result.Data.CopiedExercises.Should().HaveCount(3);

        // Verify all have different GUIDs and same metadata
        var originalExercises = await GetRoundExercisesFromDbAsync(template.Id, "Workout", 1);
        var copiedExercises = await GetRoundExercisesFromDbAsync(template.Id, "Workout", 2);
        
        copiedExercises.Should().HaveCount(3);
        copiedExercises.Select(e => e.Id).Should().NotIntersectWith(originalExercises.Select(e => e.Id));
        copiedExercises.Should().AllSatisfy(e => e.RoundNumber.Should().Be(2));
    }

    // Additional test methods for other scenarios...
}
```

**Test Helper Methods:**
- Create comprehensive test data builders
- Add database verification methods
- Include setup and teardown for consistent test state

**Critical Patterns:**
- Follow integration test patterns from `/GetFitterGetBigger.API.IntegrationTests/Features/Exercise/ExerciseLinkEnhancements.feature`
- Use TestContainers for real database testing
- Verify database state, not just API responses
- Test auto-linking behavior end-to-end

### Task 6.2: Create project-specific acceptance tests
`[Pending]` (Est: 1h)

**Implementation:**
- Create `/Tests/Features/WorkoutTemplateExercise/WorkoutTemplateExerciseManagementAcceptanceTests.cs`
- Focus on critical API project paths with BDD format

```csharp
public class WorkoutTemplateExerciseManagementAcceptanceTests
{
    [Fact]
    public void Given_Workout_Template_When_Add_Exercise_Then_Auto_Link_Should_Work()
    {
        // BDD format test focusing on core acceptance criteria
        // Use Given/When/Then structure for clarity
    }
    
    [Fact]
    public void Given_Multiple_Workout_Exercises_When_Remove_One_Then_Shared_Warmup_Should_Remain()
    {
        // Test complex orphan detection logic
    }
    
    // Additional acceptance test methods...
}
```

### Task 6.3: Update existing tests for compatibility
`[Pending]` (Est: 1h)

**Implementation:**
- Review existing WorkoutTemplate tests
- Update any tests that might be affected by new entity structure
- Ensure all existing functionality continues to work

**Testing Areas:**
- Existing WorkoutTemplateService tests
- Controller tests that might use WorkoutTemplateExercise
- Integration tests for WorkoutTemplate CRUD operations

## CHECKPOINT: Phase 6 Complete - Integration & Testing
`[Pending]`

**Requirements for Completion:**
- Build: ✅ 0 errors, 0 warnings
- Tests: ✅ All new integration tests passing + existing tests passing
- Coverage: ✅ Comprehensive scenario coverage achieved
- Git Commit Hash: [MANDATORY - To be added after commit]

**Implementation Summary:**
- **BDD Integration Tests**: Comprehensive scenario coverage with real database
- **Acceptance Tests**: Project-specific critical path validation
- **Test Compatibility**: All existing tests updated and passing
- **Test Data Builders**: Consistent test data generation utilities

**Test Requirements:**
- Integration Tests: Real database scenarios with TestContainers
- Acceptance Tests: BDD format with Given/When/Then structure
- Regression Tests: All existing functionality verified
- Performance Tests: Basic response time validation

**Code Review**: Follow `/memory-bank/DevelopmentGuidelines/Templates/FeatureCheckpointTemplate.md`
- **Status**: [To be filled after review]
- **Review Path**: `/memory-bank/features/2-IN_PROGRESS/FEAT-031-workout-template-exercise-management/code-reviews/Phase_6_Integration_Testing/`

**Git Commit**: [MANDATORY - Phase cannot be marked complete without this]

## Phase 7: Documentation & Deployment - Estimated: 2h 0m

### Task 7.1: Update error messages and constants
`[Pending]` (Est: 30m)

**IMPORTANT: This file already exists!**
- The file `/GetFitterGetBigger.API/Constants/ErrorMessages/WorkoutTemplateExerciseErrorMessages.cs` ALREADY EXISTS
- ADD new constants to the existing file, do NOT create a new one
- Use the EXISTING constant names where they match the intent
- Only add NEW constants for concepts that don't exist yet

**Implementation:**
- Update existing `/GetFitterGetBigger.API/Constants/ErrorMessages/WorkoutTemplateExerciseErrorMessages.cs`
- Add any missing constants that are needed:

```csharp
public static class WorkoutTemplateExerciseErrorMessages
{
    // Core validation messages
    public const string InvalidTemplateId = "Invalid workout template ID format";
    public const string InvalidExerciseId = "Invalid exercise ID format";
    public const string InvalidPhase = "Phase cannot be empty";
    public const string MustBeWarmupWorkoutCooldown = "Phase must be 'Warmup', 'Workout', or 'Cooldown'";
    public const string RoundNumberMustBePositive = "Round number must be greater than 0";
    public const string OrderMustBePositive = "Order in round must be greater than 0";
    public const string MetadataRequired = "Exercise metadata cannot be empty";
    public const string InvalidJsonMetadata = "Metadata must be valid JSON";
    
    // Business logic messages
    public const string TemplateNotInDraftState = "Template must be in Draft state to modify exercises";
    public const string ExerciseNotActiveOrNotFound = "Exercise not found or not active";
    public const string ExerciseNotFoundInTemplate = "Exercise not found in this template";
    public const string DuplicateExerciseInRound = "Exercise already exists in this round";
    
    // Auto-linking messages
    public const string AutoLinkingFailed = "Failed to add linked warmup/cooldown exercises";
    public const string OrphanCleanupFailed = "Failed to clean up orphaned exercises";
    
    // Round management messages
    public const string SourceRoundNotFound = "Source round not found";
    public const string TargetRoundAlreadyExists = "Target round already exists";
    public const string CannotCopyToSameRound = "Cannot copy round to itself";
    
    // Metadata validation messages
    public const string InvalidMetadataForExerciseType = "Metadata is invalid for this exercise type";
    public const string InvalidMetadataForExecutionProtocol = "Metadata is invalid for this execution protocol";
    public const string RestExerciseOnlyAcceptsDuration = "REST exercises only accept duration in metadata";
    public const string WeightExerciseRequiresWeightMetadata = "Weight-based exercises require weight in metadata";
}
```

### Task 7.2: Create comprehensive API documentation
`[Pending]` (Est: 1h)

**Implementation:**
- Update OpenAPI documentation with comprehensive examples
- Document all new endpoints and their behaviors
- Include metadata structure examples for different execution protocols

```yaml
# OpenAPI documentation examples
AddExerciseToTemplateRequest:
  type: object
  properties:
    exerciseId:
      type: string
      format: guid
      example: "123e4567-e89b-12d3-a456-426614174000"
    phase:
      type: string
      enum: ["Warmup", "Workout", "Cooldown"]
      example: "Workout"
    roundNumber:
      type: integer
      minimum: 1
      example: 1
    metadata:
      type: object
      description: "JSON metadata specific to ExecutionProtocol"
      examples:
        reps_and_sets_with_weight:
          summary: "Weight-based exercise"
          value:
            reps: 10
            weight:
              value: 60
              unit: "kg"
        reps_and_sets_bodyweight:
          summary: "Bodyweight exercise"
          value:
            reps: 15
        time_based:
          summary: "Time-based exercise"
          value:
            duration: 30
            unit: "seconds"
        rest_exercise:
          summary: "REST exercise"
          value:
            duration: 90
            unit: "seconds"

AddExerciseResponse:
  type: object
  properties:
    success:
      type: boolean
      example: true
    data:
      $ref: '#/components/schemas/AddExerciseResultDto'
    message:
      type: string
      example: "Successfully added 2 exercise(s)"
    errors:
      type: array
      items:
        type: string
```

### Task 7.3: Create feature documentation
`[Pending]` (Est: 30m)

**Implementation:**
- Create comprehensive feature documentation
- Include API usage examples
- Document auto-linking behavior
- Create migration guide for existing implementations

**Documentation Structure:**
```markdown
# Workout Template Exercise Management

## Overview
Complete redesign of WorkoutTemplate exercise management supporting multiple execution protocols with intelligent auto-linking.

## Key Features
- Multi-phase organization (Warmup, Workout, Cooldown)
- Round-based exercise organization
- Automatic warmup/cooldown linking via ExerciseLinks
- Flexible JSON metadata for any ExecutionProtocol
- Intelligent orphan cleanup
- Round copying with new GUIDs

## API Endpoints
[Comprehensive endpoint documentation]

## Auto-Linking Behavior
[Detailed explanation of auto-linking logic]

## Metadata Structure Examples
[Examples for each ExecutionProtocol]

## Migration from Old System
[Step-by-step migration guide]
```

## CHECKPOINT: Phase 7 Complete - Documentation & Deployment
`[Pending]`

**Requirements for Completion:**
- Build: ✅ 0 errors, 0 warnings
- Documentation: ✅ Complete API documentation with examples
- OpenAPI: ✅ Swagger documentation updated and tested
- Git Commit Hash: [MANDATORY - To be added after commit]

**Implementation Summary:**
- **Error Messages**: Comprehensive error constants with clear messaging
- **API Documentation**: Complete OpenAPI specification with examples
- **Feature Documentation**: User guide and migration documentation
- **Metadata Examples**: Documentation for all ExecutionProtocol types

**Documentation Requirements:**
- OpenAPI: Complete specification with request/response examples
- Error Messages: Clear, actionable error text
- Feature Guide: End-to-end usage documentation
- Migration Guide: Step-by-step conversion from old system

**Code Review**: Follow `/memory-bank/DevelopmentGuidelines/Templates/FeatureCheckpointTemplate.md`
- **Status**: [To be filled after review]
- **Review Path**: `/memory-bank/features/2-IN_PROGRESS/FEAT-031-workout-template-exercise-management/code-reviews/Phase_7_Documentation_Deployment/`

**Git Commit**: [MANDATORY - Phase cannot be marked complete without this]

## CHECKPOINT: Final Implementation Complete
`[Pending]`

**Requirements for Completion:**
- Build: ✅ 0 errors, 0 warnings
- Tests: ✅ All tests passing (baseline + new tests)
- Documentation: ✅ Complete API and feature documentation
- Integration: ✅ ExecutionProtocol and ExerciseLinks working correctly
- Git Commit Hash: [MANDATORY - To be added after commit]

**Feature Verification Summary:**
- **Multi-Protocol Support**: ✅ ExecutionProtocol integration working
- **Auto-Linking**: ✅ ExerciseLinks integration for warmup/cooldown
- **Round Management**: ✅ Copy and reorder functionality complete
- **JSON Metadata**: ✅ Flexible metadata storage implemented
- **Orphan Cleanup**: ✅ Intelligent cleanup of unused exercises
- **Phase Organization**: ✅ Warmup/Workout/Cooldown structure complete

**Test Summary:**
- Total Tests: [To be calculated] (baseline: 1,750)
- New Tests Added: [To be calculated]
- Pass Rate: 100%
- Coverage: [To be determined]

**Code Review**: Final comprehensive review following patterns from FEAT-030
- **Status**: [To be filled after review]
- **Approval Rate**: [Target: >95%]

**Git Commit**: [MANDATORY - Feature cannot be marked complete without this]

## BOY SCOUT RULE - Code Quality Improvements

During implementation, if any code quality issues are discovered, create tasks here to address them:

### Example Improvement Tasks:
- **Task BS.1:** [Issue description and solution]
- **Task BS.2:** [Issue description and solution]

## Implementation Summary Report

**Date/Time**: [To be filled at completion]
**Duration**: [To be calculated]

### Quality Metrics Comparison
| Metric | Baseline | Final | Change |
|--------|----------|-------|--------|
| Build Warnings | 0 | TBD | TBD |
| Test Count | 1,750 | TBD | +TBD |
| Test Pass Rate | 100% | 100% | 0% |

### Enhancement Summary
✅ **Multi-Protocol Support**: ExecutionProtocol integration with flexible metadata
✅ **Auto-Linking System**: Intelligent warmup/cooldown management
✅ **Round-Based Organization**: Unlimited rounds per phase with proper ordering
✅ **GUID Identification**: Unique identification for each exercise instance
✅ **Orphan Cleanup**: Automatic removal of unused auto-linked exercises
✅ **JSON Metadata**: Flexible metadata storage supporting any ExecutionProtocol
✅ **REST Exercise Handling**: Special handling for REST exercises with duration-only metadata

### Technical Debt Addressed
- Replaced old WorkoutZone enum with flexible Phase strings
- Replaced SetConfiguration collection with JSON metadata
- Added ExecutionProtocol integration to WorkoutTemplate
- Implemented proper auto-linking with ExerciseLinks integration

### Quality Improvements Applied
- ✅ ServiceResult<T> pattern for all service methods
- ✅ ServiceValidate pattern for comprehensive validation
- ✅ Proper UnitOfWork usage (ReadOnly vs Writable)
- ✅ Repository pattern with performance optimization
- ✅ Entity Handler patterns with validation
- ✅ Comprehensive BDD test coverage

## Critical Success Criteria
1. ✅ Multi-phase exercise organization (Warmup, Workout, Cooldown)
2. ✅ Round-based organization with unlimited rounds per phase
3. ✅ ExecutionProtocol integration with flexible metadata
4. ✅ Auto-linking with ExerciseLinks for warmup/cooldown
5. ✅ Intelligent orphan cleanup when removing exercises
6. ✅ Exercise reordering within rounds
7. ✅ Round copying with new GUIDs
8. ✅ JSON metadata supporting all ExecutionProtocol types
9. ✅ Complete BDD test coverage
10. ✅ Comprehensive API documentation

## Time Tracking Summary
- **Total Estimated Time:** 24h 0m
- **Total Actual Time:** [To be calculated from task durations]

## Notes
- **CRITICAL**: Follow UnitOfWork patterns from `/memory-bank/PracticalGuides/CommonImplementationPitfalls.md`
- **INTEGRATION**: Leverage existing ExecutionProtocol and ExerciseLink implementations
- **TESTING**: Comprehensive BDD scenarios for complex auto-linking logic
- **PATTERNS**: Follow ServiceResult<T> and ServiceValidate patterns throughout
- **MIGRATION**: Fresh start approach - drop old WorkoutTemplateExercise table
- **JSON**: Use PostgreSQL JSON support for flexible metadata storage
- **PERFORMANCE**: Proper indexing and AsNoTracking() for query optimization

## Final Code Review
**Date**: [To be filled]
**Report**: [Link to final code review report]
**Status**: [To be filled]
**Approval Rate**: [Target: >95%]

---

**Feature Status**: Ready for Implementation
**Next Step**: Begin Phase 1 - Planning & Analysis
**Estimated Completion**: [24h from start]
# FEAT-026: Workout Template Core - Implementation Tasks

## Pre-Implementation Checklist

Before starting implementation, ensure you have reviewed:
- [ ] `/memory-bank/FEATURE_IMPLEMENTATION_PROCESS.md` - For task structure and process
- [ ] `/memory-bank/common-implementation-pitfalls.md` - Critical mistakes to avoid
- [ ] `/memory-bank/service-implementation-checklist.md` - Quick reference during coding
- [ ] `/memory-bank/SERVICE-RESULT-PATTERN.md` - For consistent error handling
- [ ] `/memory-bank/TESTING-QUICK-REFERENCE.md` - For test implementation patterns
- [ ] `/memory-bank/EMPTY-PATTERN-MIGRATION-GUIDE.md` - For null object pattern
- [ ] `/memory-bank/ENTITY-RESULT-PATTERN.md` - For entity validation
- [ ] `/memory-bank/REFERENCE_TABLE_CRUD_PROCESS.md` - For WorkoutState implementation

## Test Structure

### Global Acceptance Tests
Location: `/memory-bank/features/FEAT-026-workout-template-core/acceptance-tests/`

**Scenario 1: Complete Workout Template Creation Flow**
```gherkin
Given a Personal Trainer logs into the Admin portal
When they create a workout template in DRAFT state
And add exercises with warmup/cooldown associations
And transition the template to PRODUCTION
Then the template should be available in the Clients app
And users with appropriate claims can execute the workout
```

**Scenario 2: Template State Management**
```gherkin
Given a workout template in PRODUCTION state with execution logs
When the PT attempts to rollback to DRAFT
Then the operation should fail with appropriate error
And the template should remain in PRODUCTION state
```

### Project-Specific Acceptance Tests
Location: `Tests/Features/WorkoutTemplate/WorkoutTemplateAcceptanceTests.cs`

**Test 1: Draft State Behavior**
```csharp
[Fact]
public async Task Given_WorkoutTemplateInDraft_When_ChangingExecutionProtocol_Then_DeletesAllLogs()
{
    // Given: A workout template in DRAFT state with execution logs
    // When: PT changes the execution protocol
    // Then: All associated logs are deleted
    // And: Template remains in DRAFT state
}
```

**Test 2: Production State Restrictions**
```csharp
[Fact]
public async Task Given_WorkoutTemplateInProduction_When_UsersHaveExecuted_Then_CannotRollbackToDraft()
{
    // Given: A workout template in PRODUCTION with execution logs
    // When: PT attempts to rollback to DRAFT
    // Then: Operation fails with appropriate error
    // And: Template remains in PRODUCTION state
}
```

## Phase 0: Planning and Test Infrastructure

### Task 0.1: Study codebase for similar implementations
`[Pending]` (Est: 2h)
- Search for reference table implementations: Equipment, MuscleGroup
- Study Exercise entity for complex relationships pattern
- Review ExerciseService for state management patterns
- Document findings with specific file references:
  - `Models/Entities/Equipment.cs` - Reference table pattern
  - `Models/Entities/Exercise.cs` - Complex entity with relationships
  - `Services/Implementations/EquipmentService.cs` - Reference service pattern
  - `Services/Implementations/ExerciseService.cs` - Complex service pattern

### Task 0.2: Create test builders for WorkoutState
`[Pending]` (Est: 1h)
- Create `Tests/Builders/WorkoutStateBuilder.cs`
- Add to `Tests/Builders/TestIds.cs`:
  ```csharp
  public static class WorkoutStateIds
  {
      public static readonly WorkoutStateId Draft = WorkoutStateId.ParseOrEmpty("02000001-0000-0000-0000-000000000001");
      public static readonly WorkoutStateId Production = WorkoutStateId.ParseOrEmpty("02000001-0000-0000-0000-000000000002");
      public static readonly WorkoutStateId Archived = WorkoutStateId.ParseOrEmpty("02000001-0000-0000-0000-000000000003");
  }
  ```
- Follow pattern from `Tests/Builders/EquipmentBuilder.cs`

### Task 0.3: Create test builders for WorkoutTemplate entities
`[Pending]` (Est: 2h)
- Create `Tests/Builders/WorkoutTemplateBuilder.cs`
- Create `Tests/Builders/WorkoutTemplateExerciseBuilder.cs`
- Create `Tests/Builders/SetConfigurationBuilder.cs`
- Add appropriate test IDs to `TestIds.cs`
- Follow complex entity pattern from `Tests/Builders/ExerciseBuilder.cs`

## Phase 1: WorkoutState Reference Table Implementation

### Task 1.1: Create WorkoutStateId specialized ID
`[Pending]` (Est: 1h)

**Implementation:**
- Create `Models/ValueObjects/WorkoutStateId.cs`
- Follow pattern from `Models/ValueObjects/EquipmentId.cs`
- Register in `Models/ValueObjects/SpecializedIdJsonConverterFactory.cs`
- Add to `Models/ValueObjects/SpecializedIdSwaggerSchemaFilter.cs`

**Unit Tests:**
- Create `Tests/Models/ValueObjects/WorkoutStateIdTests.cs`
- Test ParseOrEmpty with valid/invalid inputs
- Test JSON serialization/deserialization
- Reference: `Tests/Models/ValueObjects/EquipmentIdTests.cs`

### Task 1.2: Create WorkoutState entity
`[Pending]` (Est: 2h)

**Implementation:**
- Create `Models/Entities/WorkoutState.cs`
- Implement interfaces: `IEnhancedReference<WorkoutStateId>`, `IEmptyEntity<WorkoutState, WorkoutStateId>`
- Properties: Id, Value (DRAFT/PRODUCTION/ARCHIVED), Name, Description, IsActive, CreatedAt, UpdatedAt
- Add Handler pattern for Create/Update
- Follow pattern from `Models/Entities/Equipment.cs`
- **WARNING**: Use unique GUIDs with 02000001 prefix

**Unit Tests:**
- Create `Tests/Models/Entities/WorkoutStateTests.cs`
- Test entity creation and validation
- Test Empty pattern implementation
- Test Handler methods

### Task 1.3: Database configuration for WorkoutState
`[Pending]` (Est: 2h)

**Implementation:**
- Create `Data/EntityConfigurations/WorkoutStateConfiguration.cs`
- Add navigation to `Data/GetFitterGetBiggerContext.cs`
- Create migration with proper seed data:
  ```csharp
  new { Id = "02000001-0000-0000-0000-000000000001", Value = "DRAFT", Name = "Draft", Description = "Template under construction" }
  new { Id = "02000001-0000-0000-0000-000000000002", Value = "PRODUCTION", Name = "Production", Description = "Active template for use" }
  new { Id = "02000001-0000-0000-0000-000000000003", Value = "ARCHIVED", Name = "Archived", Description = "Retired template" }
  ```
- Follow pattern from `Data/EntityConfigurations/EquipmentConfiguration.cs`

**Integration Tests:**
- Update `Tests/Data/GetFitterGetBiggerContextTests.cs`
- Verify WorkoutState table creation
- Test seed data insertion

### Task 1.4: Create WorkoutState repository
`[Pending]` (Est: 2h)

**Implementation:**
- Create `Data/Repositories/Interfaces/IWorkoutStateRepository.cs`
- Create `Data/Repositories/WorkoutStateRepository.cs`
- Extend `EnhancedReferenceRepository<WorkoutState, WorkoutStateId>`
- Override LoadAllAsync to return ordered by Value
- Follow pattern from `Data/Repositories/EquipmentRepository.cs`

**Unit Tests:**
- Create `Tests/Data/Repositories/WorkoutStateRepositoryTests.cs`
- Test GetByValueAsync method
- Test ordering of LoadAllAsync
- Reference: `Tests/Data/Repositories/EquipmentRepositoryTests.cs`

### Task 1.5: Create WorkoutState service
`[Pending]` (Est: 3h)

**Implementation:**
- Create `Services/Interfaces/IWorkoutStateService.cs`
- Create `Services/Implementations/WorkoutStateService.cs`
- Extend `PureReferenceService<WorkoutState, WorkoutStateId>`
- Implement caching with IEternalCacheService
- Override LoadEntityByIdAsync for non-nullable returns
- **CRITICAL**: Use ReadOnlyUnitOfWork for all operations (no mutations)
- Follow pattern from `Services/Implementations/EquipmentService.cs`

**Unit Tests:**
- Create `Tests/Services/WorkoutStateServiceTests.cs`
- Test all service methods
- Test caching behavior
- Test Empty pattern handling
- Reference: `Tests/Services/EquipmentServiceTests.cs`

### Task 1.6: Create WorkoutState controller
`[Pending]` (Est: 3h)

**Implementation:**
- Create `Controllers/WorkoutStateController.cs`
- Route: `/api/ReferenceTables/WorkoutStates`
- Implement GetAll, GetById, GetByValue endpoints
- Use pattern matching for ServiceResult handling
- Follow pattern from `Controllers/EquipmentController.cs`

**Unit Tests:**
- Create `Tests/Controllers/WorkoutStateControllerTests.cs`
- Test all controller actions
- Test error handling scenarios
- Reference: `Tests/Controllers/EquipmentControllerTests.cs`

### Task 1.7: Create WorkoutState integration tests
`[Pending]` (Est: 2h)

**BDD Tests:**
- Create `IntegrationTests/Features/WorkoutState/WorkoutStateOperations.feature`
- Test GetAll returns seeded states
- Test GetById with valid/invalid IDs
- Test GetByValue with DRAFT/PRODUCTION/ARCHIVED
- Follow pattern from `IntegrationTests/Features/Equipment/EquipmentCrudSimple.feature`

**Step Definitions:**
- Create `IntegrationTests/StepDefinitions/WorkoutStateSteps.cs`
- Implement step definitions for BDD scenarios
- Reference: `IntegrationTests/StepDefinitions/EquipmentSteps.cs`

### Task 1.8: Register WorkoutState in dependency injection
`[Pending]` (Est: 1h)

**Implementation:**
- Update `Program.cs` to register:
  - IWorkoutStateRepository
  - IWorkoutStateService
- Follow existing registration patterns
- Ensure proper scoping (Scoped for repositories/services)

**Integration Tests:**
- Run all WorkoutState integration tests
- Verify DI resolution works correctly

## CHECKPOINT: WorkoutState Reference Table
`[Pending]` - Date: 

Build Report: X errors, Y warnings
Test Report: X passed, Y failed (Total: Z)
Code Review: code-review/workoutstate-code-review-YYYY-MM-DD-HHMMSS.md - [STATUS]

Notes: WorkoutState reference table must be fully functional before proceeding to WorkoutTemplate implementation.

## Phase 2: WorkoutTemplate Core Models and Database

### Task 2.1: Create WorkoutTemplate specialized IDs
`[Pending]` (Est: 2h)

**Implementation:**
- Create `Models/ValueObjects/WorkoutTemplateId.cs`
- Create `Models/ValueObjects/WorkoutTemplateExerciseId.cs`
- Create `Models/ValueObjects/SetConfigurationId.cs`
- Register all in `SpecializedIdJsonConverterFactory.cs`
- Add to `SpecializedIdSwaggerSchemaFilter.cs`
- Follow pattern from existing specialized IDs

**Unit Tests:**
- Create tests for each specialized ID
- Test ParseOrEmpty, JSON serialization
- Use unique GUID prefixes (03000001, 04000001, 05000001)

### Task 2.2: Create WorkoutTemplate entity
`[Pending]` (Est: 3h)

**Implementation:**
- Create `Models/Entities/WorkoutTemplate.cs`
- Properties: Id, Name, Description, Category, Objectives, Difficulty, Duration, Tags, IsPublic, CreatedBy, WorkoutStateId
- Navigation properties: WorkoutState, Exercises, Category, Objectives
- Implement Handler pattern with validation
- Follow complex entity pattern from `Models/Entities/Exercise.cs`
- **WARNING**: Name validation (3-100 chars), Duration (5-300 mins)

**Unit Tests:**
- Create `Tests/Models/Entities/WorkoutTemplateTests.cs`
- Test entity creation with valid/invalid data
- Test validation rules
- Test relationships

### Task 2.3: Create WorkoutTemplateExercise entity
`[Pending]` (Est: 2h)

**Implementation:**
- Create `Models/Entities/WorkoutTemplateExercise.cs`
- Properties: Id, WorkoutTemplateId, ExerciseId, Zone (Warmup/Main/Cooldown), SequenceOrder, Notes
- Navigation properties: WorkoutTemplate, Exercise, Configurations
- Implement unique sequence order validation within zones
- Add Handler pattern

**Unit Tests:**
- Create `Tests/Models/Entities/WorkoutTemplateExerciseTests.cs`
- Test zone assignment
- Test sequence order uniqueness
- Test relationships

### Task 2.4: Create SetConfiguration entity
`[Pending]` (Est: 2h)

**Implementation:**
- Create `Models/Entities/SetConfiguration.cs`
- Properties: Id, WorkoutTemplateExerciseId, SetNumber, TargetReps, TargetWeight, TargetTime, RestSeconds
- Support range values for reps (e.g., "8-12")
- Navigation: WorkoutTemplateExercise
- Validation for numeric ranges

**Unit Tests:**
- Create `Tests/Models/Entities/SetConfigurationTests.cs`
- Test range parsing for reps
- Test validation rules
- Test set ordering

### Task 2.5: Database configuration for WorkoutTemplate entities
`[Pending]` (Est: 3h)

**Implementation:**
- Create `Data/EntityConfigurations/WorkoutTemplateConfiguration.cs`
- Create `Data/EntityConfigurations/WorkoutTemplateExerciseConfiguration.cs`
- Create `Data/EntityConfigurations/SetConfigurationConfiguration.cs`
- Add to `GetFitterGetBiggerContext.cs`
- Configure relationships and indexes
- Create migration
- **CRITICAL**: Ensure proper cascade delete rules

**Integration Tests:**
- Update context tests
- Verify table creation
- Test relationship constraints

## Phase 3: WorkoutTemplate Repository Layer

### Task 3.1: Create WorkoutTemplate repository interfaces
`[Pending]` (Est: 1h)

**Implementation:**
- Create `Data/Repositories/Interfaces/IWorkoutTemplateRepository.cs`
- Methods: GetByIdWithDetailsAsync, GetPagedAsync, GetByCreatorAsync
- Create specifications for complex queries
- Follow pattern from `Data/Repositories/Interfaces/IExerciseRepository.cs`

### Task 3.2: Create WorkoutTemplate repository implementation
`[Pending]` (Est: 4h)

**Implementation:**
- Create `Data/Repositories/WorkoutTemplateRepository.cs`
- Implement eager loading for navigation properties
- Implement specification pattern for filtering
- Handle complex queries with includes
- Follow pattern from `Data/Repositories/ExerciseRepository.cs`
- **WARNING**: Optimize queries to prevent N+1 problems

**Unit Tests:**
- Create `Tests/Data/Repositories/WorkoutTemplateRepositoryTests.cs`
- Test eager loading
- Test filtering specifications
- Test paging functionality
- Mock DbContext appropriately

### Task 3.3: Create WorkoutTemplateExercise repository
`[Pending]` (Est: 2h)

**Implementation:**
- Create interfaces and implementation
- Methods for managing exercises within templates
- Ensure sequence order integrity
- Handle zone-specific queries

**Unit Tests:**
- Test CRUD operations
- Test sequence reordering
- Test zone filtering

### Task 3.4: Create SetConfiguration repository
`[Pending]` (Est: 2h)

**Implementation:**
- Create interfaces and implementation
- Methods for bulk operations on sets
- Ensure set number integrity

**Unit Tests:**
- Test bulk create/update
- Test set ordering
- Test cascade operations

## CHECKPOINT: Models and Repository Layer
`[Pending]` - Date: 

Build Report: X errors, Y warnings
Test Report: X passed, Y failed (Total: Z)
Code Review: code-review/models-repository-code-review-YYYY-MM-DD-HHMMSS.md - [STATUS]

Notes: Database and repository layer must be stable before implementing business logic.

## Phase 4: WorkoutTemplate Service Layer

### Task 4.1: Create WorkoutTemplate service interface
`[Pending]` (Est: 1h)

**Implementation:**
- Create `Services/Interfaces/IWorkoutTemplateService.cs`
- Define methods for CRUD, state transitions, exercise management
- Include suggestion and duplication methods
- Follow ServiceResult pattern

### Task 4.2: Create WorkoutTemplate service implementation
`[Pending]` (Est: 6h)

**Implementation:**
- Create `Services/Implementations/WorkoutTemplateService.cs`
- Implement CRUD with proper validation
- State transition logic with business rules
- Equipment aggregation from exercises
- Exercise suggestion algorithm
- Template duplication functionality
- **CRITICAL**: Use ReadOnlyUnitOfWork for validation, WritableUnitOfWork for modifications
- **WARNING**: Never use WritableUnitOfWork for queries
- Follow pattern from `Services/Implementations/ExerciseService.cs`

**Unit Tests:**
- Create `Tests/Services/WorkoutTemplateServiceTests.cs`
- Test all CRUD operations
- Test state transitions with validation
- Test business rule enforcement
- Test suggestion algorithm
- Mock all dependencies properly
- Reference: `Tests/Services/ExerciseServiceTests.cs`

### Task 4.3: Create WorkoutTemplateExercise service
`[Pending]` (Est: 4h)

**Implementation:**
- Service for managing exercises within templates
- Handle sequence ordering logic
- Zone validation
- Associated exercise suggestions
- **WARNING**: Maintain sequence integrity

**Unit Tests:**
- Test exercise addition/removal
- Test sequence reordering
- Test zone changes
- Test validation rules

### Task 4.4: Create SetConfiguration service
`[Pending]` (Est: 3h)

**Implementation:**
- Service for managing set configurations
- Bulk operations support
- Range value parsing
- Set number management

**Unit Tests:**
- Test CRUD operations
- Test bulk updates
- Test range validations
- Test set ordering

### Task 4.5: Implement caching for WorkoutTemplate
`[Pending]` (Est: 2h)

**Implementation:**
- Use ITimedCacheService for lists (5 min TTL)
- Use ITimedCacheService for details (1 hour TTL)
- Implement cache invalidation on mutations
- Follow pattern from Equipment caching
- Reference: `/memory-bank/cache-configuration.md`

**Unit Tests:**
- Test cache hit/miss scenarios
- Test invalidation on updates
- Test concurrent access

## Phase 5: WorkoutTemplate API Controllers

### Task 5.1: Create WorkoutTemplate controller
`[Pending]` (Est: 4h)

**Implementation:**
- Create `Controllers/WorkoutTemplateController.cs`
- Implement all endpoints from API spec
- Use attribute routing
- Implement proper authorization
- Pattern matching for ServiceResult
- Follow RESTful conventions
- Reference: `Controllers/ExerciseController.cs`

**Unit Tests:**
- Create `Tests/Controllers/WorkoutTemplateControllerTests.cs`
- Test all actions
- Test authorization
- Test error responses
- Mock service layer

### Task 5.2: Create WorkoutTemplateExercise endpoints
`[Pending]` (Est: 3h)

**Implementation:**
- Add exercise management endpoints to controller
- Handle nested resource routing
- Validate template ownership

**Unit Tests:**
- Test exercise CRUD
- Test authorization
- Test validation

### Task 5.3: Create SetConfiguration endpoints
`[Pending]` (Est: 2h)

**Implementation:**
- Add set configuration endpoints
- Support bulk operations
- Validate exercise ownership

**Unit Tests:**
- Test set CRUD
- Test bulk operations
- Test validations

### Task 5.4: Create state transition endpoint
`[Pending]` (Est: 2h)

**Implementation:**
- Implement PUT `/api/workout-templates/{id}/state`
- Validate state transitions
- Handle side effects (log deletion)
- Return appropriate errors

**Unit Tests:**
- Test valid transitions
- Test invalid transitions
- Test side effects

## Phase 6: Integration and BDD Tests

### Task 6.1: Create WorkoutTemplate BDD scenarios
`[Pending]` (Est: 3h)

**Implementation:**
- Create `IntegrationTests/Features/WorkoutTemplate/WorkoutTemplateManagement.feature`
- Implement scenarios from `bdd-scenarios.md`
- Cover complete CRUD flow
- Test state transitions
- Test business rules

**Step Definitions:**
- Create `IntegrationTests/StepDefinitions/WorkoutTemplateSteps.cs`
- Implement all step definitions
- Use test builders for data setup

### Task 6.2: Create exercise management BDD tests
`[Pending]` (Est: 2h)

**Implementation:**
- Test adding exercises to zones
- Test sequence ordering
- Test exercise removal
- Test validation scenarios

### Task 6.3: Create performance integration tests
`[Pending]` (Est: 2h)

**Implementation:**
- Test with 10,000+ templates
- Test concurrent user access
- Verify query performance
- Test caching effectiveness

### Task 6.4: Create security integration tests
`[Pending]` (Est: 2h)

**Implementation:**
- Test authorization on all endpoints
- Test ownership validation
- Test public/private visibility
- Verify audit trail creation

## CHECKPOINT: Complete Feature Implementation
`[Pending]` - Date: 

Build Report: X errors, Y warnings
Test Report: X passed, Y failed (Total: Z)
Code Review: code-review/final-code-review-YYYY-MM-DD-HHMMSS.md - [STATUS]

Notes: All code reviews must be in APPROVED state. Full test suite must pass with zero failures.

## Phase 7: Documentation and Propagation

### Task 7.1: Update API documentation
`[Pending]` (Est: 2h)

**Implementation:**
- Document all endpoints in memory-bank
- Update Swagger annotations
- Create example requests/responses
- Document error codes

### Task 7.2: Propagate to Admin and Clients projects
`[Pending]` (Est: 1h)

**Implementation:**
- Update `/GetFitterGetBigger.Admin/memory-bank/`
- Update `/GetFitterGetBigger.Clients/memory-bank/`
- Follow rules in `/api-docs/documentation-propagation-rules.md`
- Do NOT update sub-project memory banks

### Task 7.3: Create feature completion summary
`[Pending]` (Est: 1h)

**Implementation:**
- Create completion summary
- Document lessons learned
- Note any technical debt
- Update feature state to COMPLETED

## BOY SCOUT RULE

Track improvements discovered during implementation:
- [ ] Potential refactoring opportunities
- [ ] Code duplication found
- [ ] Performance optimizations identified
- [ ] Documentation gaps discovered
- [ ] Test coverage improvements needed

## Time Tracking Summary

**Estimated Total**: ~75 hours

**Phase Breakdown**:
- Phase 0 (Planning): 5h
- Phase 1 (WorkoutState): 16h
- Phase 2 (Models): 12h
- Phase 3 (Repositories): 9h
- Phase 4 (Services): 16h
- Phase 5 (Controllers): 11h
- Phase 6 (Integration Tests): 9h
- Phase 7 (Documentation): 4h

**Note**: Estimates include both implementation and testing time, following TDD approach.
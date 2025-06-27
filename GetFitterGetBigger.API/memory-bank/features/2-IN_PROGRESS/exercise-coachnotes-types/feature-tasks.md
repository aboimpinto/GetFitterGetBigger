# Exercise CoachNotes and TypeOfExercise Implementation Tasks

## Feature Branch: `feature/exercise-coachnotes-types`

### Phase 1: Create Reference Table Infrastructure

#### 1.1 Create Strongly-Typed IDs
- **Task 1.1.1:** Create `CoachNoteId` in `SpecializedIds` namespace (format: "coachnote-{guid}") `[Implemented: ac470a0e]`
- **Task 1.1.2:** Create unit tests for `CoachNoteId` `[Implemented: ac470a0e]`
- **Task 1.1.3:** Create `ExerciseTypeId` in `SpecializedIds` namespace (format: "exercisetype-{guid}") `[Implemented: ac470a0e]`
- **Task 1.1.4:** Create unit tests for `ExerciseTypeId` `[Implemented: ac470a0e]`

#### 1.2 Create ExerciseType Reference Table
- **Task 1.2.1:** Create `ExerciseType.cs` entity extending `ReferenceDataBase` `[Implemented: ac470a0e]`
- **Task 1.2.2:** Create `IExerciseTypeRepository` interface `[Implemented: ac470a0e]`
- **Task 1.2.3:** Implement `ExerciseTypeRepository` `[Implemented: ac470a0e]`
- **Task 1.2.4:** Create unit tests for `ExerciseTypeRepository` `[Implemented: ac470a0e]`
- **Task 1.2.5:** Configure ExerciseType in DbContext (DbSet, ID conversion, seed data) `[Implemented: ac470a0e]`
- **Task 1.2.6:** Register repository in dependency injection `[Implemented: ac470a0e]`

### Phase 2: Create CoachNote Entity

#### 2.1 Create CoachNote Entity and Infrastructure
- **Task 2.1.1:** Create `CoachNote.cs` entity with:
  - Id (CoachNoteId)
  - ExerciseId (ExerciseId) 
  - Text (string, max 1000)
  - Order (int)
  - Navigation property to Exercise `[Implemented: cd4c0b01]`
- **Task 2.1.2:** Create unit tests for `CoachNote` entity `[Implemented: cd4c0b01]`
- **Task 2.1.3:** Add CoachNote DbSet to `FitnessDbContext` `[Implemented: cd4c0b01]`
- **Task 2.1.4:** Configure CoachNote in `OnModelCreating`:
  - Configure CoachNoteId value conversion
  - Configure ExerciseId value conversion
  - Set up foreign key to Exercise
  - Configure cascading delete
  - Add composite index on (ExerciseId, Order) `[Implemented: cd4c0b01]`

#### 2.2 Boy Scout Rule Fixes
- **Task 2.2.1:** Fix failing test ExerciseServiceTests.UpdateAsync_WithValidRequest_UpdatesExercise `[Implemented: Boy Scout Rule]`
- **Task 2.2.2:** Fix warning CS8618 in RepositoryBase.cs `[Implemented: Boy Scout Rule]`
- **Task 2.2.3:** Fix warnings in ReferenceTablesBaseController.cs (4 warnings) `[Implemented: Boy Scout Rule]`
- **Task 2.2.4:** Fix warning CS8603 in CacheService.cs `[Implemented: Boy Scout Rule]`
- **Task 2.2.5:** Fix warning CS8634 in CacheServiceTests.cs `[Implemented: Boy Scout Rule]`

### Phase 3: Update Exercise Entity

#### 3.1 Modify Exercise Entity
- **Task 3.1.1:** Remove `Instructions` property from Exercise entity `[ReadyToDevelop]`
- **Task 3.1.2:** Add `CoachNotes` navigation collection `[ReadyToDevelop]`
- **Task 3.1.3:** Add `ExerciseTypes` navigation collection `[ReadyToDevelop]`
- **Task 3.1.4:** Update entity validation attributes `[ReadyToDevelop]`
- **Task 3.1.5:** Update Exercise entity tests `[ReadyToDevelop]`

#### 3.2 Create Junction Table
- **Task 3.2.1:** Create `ExerciseExerciseType` entity for many-to-many relationship `[ReadyToDevelop]`
- **Task 3.2.2:** Configure junction table in `FitnessDbContext` `[ReadyToDevelop]`

### Phase 4: Update DTOs

#### 4.1 Create New DTOs
- **Task 4.1.1:** Create `CoachNoteDto` with Id (string), Text, and Order properties `[ReadyToDevelop]`
- **Task 4.1.2:** Create `ExerciseTypeDto` extending from reference data base DTO `[ReadyToDevelop]`
- **Task 4.1.3:** Create unit tests for new DTOs `[ReadyToDevelop]`

#### 4.2 Update Existing DTOs
- **Task 4.2.1:** Update `ExerciseDto`:
  - Remove `Instructions` property
  - Add `CoachNotes` collection property
  - Add `ExerciseTypes` collection property `[ReadyToDevelop]`
- **Task 4.2.2:** Update `CreateExerciseRequest`:
  - Remove `Instructions` property and validation
  - Add `CoachNotes` collection (array of text and order, no IDs needed)
  - Add `ExerciseTypeIds` collection (string[]) `[ReadyToDevelop]`
- **Task 4.2.3:** Update `UpdateExerciseRequest`:
  - Remove `Instructions` property and validation
  - Add `CoachNotes` collection (with optional IDs for existing notes)
  - Add `ExerciseTypeIds` collection (string[]) `[ReadyToDevelop]`
- **Task 4.2.4:** Update DTO tests `[ReadyToDevelop]`

### Phase 5: Update Repository Layer

#### 5.1 Update Exercise Repository
- **Task 5.1.1:** Update `GetByIdAsync` to include CoachNotes and ExerciseTypes `[ReadyToDevelop]`
- **Task 5.1.2:** Update `GetAllAsync` to include related data `[ReadyToDevelop]`
- **Task 5.1.3:** Update `GetPagedAsync` to include related data `[ReadyToDevelop]`
- **Task 5.1.4:** Ensure CoachNotes are ordered by Order field in all queries `[ReadyToDevelop]`
- **Task 5.1.5:** Update repository tests for new includes `[ReadyToDevelop]`

### Phase 6: Update Service Layer

#### 6.1 Update Exercise Service
- **Task 6.1.1:** Create validation method for Rest exclusivity rule `[ReadyToDevelop]`
- **Task 6.1.2:** Create unit tests for Rest exclusivity validation `[ReadyToDevelop]`
- **Task 6.1.3:** Update `CreateAsync`:
  - Remove Instructions handling
  - Add CoachNotes creation logic
  - Add ExerciseTypes assignment
  - Implement Rest exclusivity validation `[ReadyToDevelop]`
- **Task 6.1.4:** Create unit tests for `CreateAsync` with CoachNotes and ExerciseTypes `[ReadyToDevelop]`
- **Task 6.1.5:** Update `UpdateAsync`:
  - Remove Instructions handling
  - Implement CoachNotes synchronization logic
  - Update ExerciseTypes assignment
  - Implement Rest exclusivity validation `[ReadyToDevelop]`
- **Task 6.1.6:** Create unit tests for `UpdateAsync` with CoachNotes synchronization `[ReadyToDevelop]`
- **Task 6.1.7:** Update `MapToDto` to map CoachNotes and ExerciseTypes `[ReadyToDevelop]`
- **Task 6.1.8:** Create unit tests for `MapToDto` with new properties `[ReadyToDevelop]`

### Phase 7: Update Controller

#### 7.1 Update Exercises Controller
- **Task 7.1.1:** Update API documentation for Create endpoint `[ReadyToDevelop]`
- **Task 7.1.2:** Update API documentation for Update endpoint `[ReadyToDevelop]`
- **Task 7.1.3:** Update example requests in XML comments `[ReadyToDevelop]`
- **Task 7.1.4:** Add validation attributes if needed `[ReadyToDevelop]`

### Phase 8: Integration Tests

#### 8.1 Create Integration Tests
- **Task 8.1.1:** Update Exercise creation integration tests with CoachNotes `[ReadyToDevelop]`
- **Task 8.1.2:** Update Exercise update integration tests with CoachNotes changes `[ReadyToDevelop]`
- **Task 8.1.3:** Test CoachNotes synchronization scenarios `[ReadyToDevelop]`
- **Task 8.1.4:** Add integration tests for ExerciseTypes assignment `[ReadyToDevelop]`
- **Task 8.1.5:** Add integration tests for Rest exclusivity rule `[ReadyToDevelop]`
- **Task 8.1.6:** Test complete workflow and CoachNotes ordering `[ReadyToDevelop]`

### Phase 9: Database Migration

#### 9.1 Create Complete Migration
- **Task 9.1.1:** Generate migration for all changes `[ReadyToDevelop]`
- **Task 9.1.2:** Review migration for correctness `[ReadyToDevelop]`
- **Task 9.1.3:** Create data migration script for existing Instructions (if needed) `[ReadyToDevelop]`

### Phase 10: Documentation and Final Steps

#### 10.1 Update Documentation
- **Task 10.1.1:** Update API documentation `[ReadyToDevelop]`
- **Task 10.1.2:** Update Swagger annotations `[ReadyToDevelop]`
- **Task 10.1.3:** Create/update relevant memory bank documents `[ReadyToDevelop]`

#### 10.2 Quality Assurance
- **Task 10.2.1:** Run all tests `[ReadyToDevelop]`
- **Task 10.2.2:** Run linting `[ReadyToDevelop]`
- **Task 10.2.3:** Run type checking `[ReadyToDevelop]`
- **Task 10.2.4:** Manual testing of all endpoints `[ReadyToDevelop]`
- **Task 10.2.5:** Verify database constraints `[ReadyToDevelop]`

## Notes
- Each implementation task must be immediately followed by its test task
- No task is complete until build passes and all tests are green
- Keep build warnings to minimum
- Migration creation is deferred until all entity changes are complete
# Task List: Exercise CoachNotes and TypeOfExercise

## Phase 1: Create Reference Table Infrastructure

### 1.1 Create Strongly-Typed IDs
- [ ] Create `CoachNoteId` in `SpecializedIds` namespace (format: "coachnote-{guid}")
- [ ] Create unit tests for `CoachNoteId`
- [ ] Create `ExerciseTypeId` in `SpecializedIds` namespace (format: "exercisetype-{guid}")
- [ ] Create unit tests for `ExerciseTypeId`
- [ ] Implement standard methods: `New()`, `From()`, `TryParse()`, `ToString()`
- [ ] Add implicit conversion to `Guid` for EF Core compatibility

### 1.2 Create ExerciseType Reference Table
- [ ] Create `ExerciseType.cs` entity extending `ReferenceDataBase`
- [ ] Use `ExerciseTypeId` as the ID type
- [ ] Add ExerciseType values: Warmup, Workout, Cooldown, Rest
- [ ] Create `IExerciseTypeRepository` interface
- [ ] Implement `ExerciseTypeRepository`
- [ ] Create unit tests for `ExerciseTypeRepository`
- [ ] Add ExerciseType DbSet to `FitnessDbContext`
- [ ] Configure ExerciseType in `OnModelCreating`
- [ ] Create migration for ExerciseType table
- [ ] Add seed data for ExerciseType values

## Phase 2: Create CoachNote Entity

### 2.1 Create CoachNote Entity and Infrastructure
- [ ] Create `CoachNote.cs` entity with:
  - Id (CoachNoteId)
  - ExerciseId (ExerciseId) 
  - Text (string, max 1000)
  - Order (int)
  - Navigation property to Exercise
- [ ] Create unit tests for `CoachNote` entity
- [ ] Add CoachNote DbSet to `FitnessDbContext`
- [ ] Configure CoachNote in `OnModelCreating`:
  - Configure CoachNoteId value conversion
  - Configure ExerciseId value conversion
  - Set up foreign key to Exercise
  - Configure cascading delete
  - Add composite index on (ExerciseId, Order)
- [ ] Create migration for CoachNote table

## Phase 3: Update Exercise Entity

### 3.1 Modify Exercise Entity
- [ ] Remove `Instructions` property from Exercise entity
- [ ] Add `CoachNotes` navigation collection
- [ ] Add `ExerciseTypes` navigation collection
- [ ] Update entity validation attributes

### 3.2 Create Junction Table
- [ ] Create `ExerciseExerciseType` entity for many-to-many relationship
- [ ] Configure junction table in `FitnessDbContext`
- [ ] Create migration for junction table

## Phase 4: Update DTOs

### 4.1 Create New DTOs
- [ ] Create `CoachNoteDto` with Id (string), Text, and Order properties
- [ ] Create `ExerciseTypeDto` extending from reference data base DTO
- [ ] Ensure DTOs use string format for IDs (e.g., "coachnote-{guid}")

### 4.2 Update Existing DTOs
- [ ] Update `ExerciseDto`:
  - Remove `Instructions` property
  - Add `CoachNotes` collection property
  - Add `ExerciseTypes` collection property
- [ ] Update `CreateExerciseRequest`:
  - Remove `Instructions` property and validation
  - Add `CoachNotes` collection (array of text and order, no IDs needed)
  - Add `ExerciseTypeIds` collection (string[])
- [ ] Update `UpdateExerciseRequest`:
  - Remove `Instructions` property and validation
  - Add `CoachNotes` collection (with optional IDs for existing notes)
  - Add `ExerciseTypeIds` collection (string[])
  - CoachNotes without IDs are treated as new additions

## Phase 5: Update Repository Layer

### 5.1 Update Exercise Repository
- [ ] Update `GetByIdAsync` to include CoachNotes and ExerciseTypes
- [ ] Update `GetAllAsync` to include related data
- [ ] Update `GetPagedAsync` to include related data
- [ ] Ensure CoachNotes are ordered by Order field in all queries

## Phase 6: Update Service Layer

### 6.1 Update Exercise Service
- [ ] Create validation method for Rest exclusivity rule
- [ ] Create unit tests for Rest exclusivity validation
- [ ] Update `CreateAsync`:
  - Remove Instructions handling
  - Add CoachNotes creation logic (create new CoachNotes as part of Exercise)
  - Add ExerciseTypes assignment
  - Implement Rest exclusivity validation
- [ ] Create unit tests for `CreateAsync` with CoachNotes and ExerciseTypes
- [ ] Update `UpdateAsync`:
  - Remove Instructions handling
  - Implement CoachNotes synchronization logic:
    - Delete removed CoachNotes
    - Update existing CoachNotes (match by ID)
    - Add new CoachNotes
    - Preserve ordering
  - Update ExerciseTypes assignment
  - Implement Rest exclusivity validation
- [ ] Create unit tests for `UpdateAsync` with CoachNotes synchronization
- [ ] Update `MapToDto` to map CoachNotes and ExerciseTypes
- [ ] Create unit tests for `MapToDto` with new properties
- [ ] Test CoachNotes ordering is preserved

## Phase 7: Update Controller

### 7.1 Update Exercises Controller
- [ ] Update API documentation for Create endpoint
- [ ] Update API documentation for Update endpoint
- [ ] Update example requests in XML comments
- [ ] Add validation attributes if needed

## Phase 8: Integration Tests

### 8.1 Create Integration Tests
- [ ] Update Exercise creation integration tests with CoachNotes
- [ ] Update Exercise update integration tests with CoachNotes changes
- [ ] Test CoachNotes synchronization scenarios:
  - Adding new CoachNotes to existing exercise
  - Removing CoachNotes from exercise
  - Updating CoachNote text and order
  - Reordering CoachNotes
- [ ] Add integration tests for ExerciseTypes assignment
- [ ] Add integration tests for Rest exclusivity rule
- [ ] Test complete workflow: Create exercise with CoachNotes and Types
- [ ] Test CoachNotes ordering in API responses

## Phase 9: Database Migration

### 9.1 Create Complete Migration
- [ ] Generate migration for all changes
- [ ] Review migration for correctness
- [ ] Create data migration script for existing Instructions (if needed)

## Phase 10: Documentation and Final Steps

### 10.1 Update Documentation
- [ ] Update API documentation
- [ ] Update Swagger annotations
- [ ] Create/update relevant memory bank documents

### 10.2 Quality Assurance
- [ ] Run all tests
- [ ] Run linting
- [ ] Run type checking
- [ ] Manual testing of all endpoints
- [ ] Verify database constraints

## Notes
- Each phase should be completed and tested before moving to the next
- The Rest exclusivity rule must be enforced at the service layer
- Consider performance implications of loading related data
- Ensure backward compatibility considerations are documented
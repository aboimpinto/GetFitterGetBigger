# MuscleGroup CRUD Implementation Tasks

## Feature Branch: `feature/muscle-group-crud`
## Estimated Total Time: 20-26 hours (2.5-3.5 days)
## Actual Total Time: [To be calculated at completion]

## Prerequisites
- [ ] Review REFERENCE_TABLE_CRUD_PROCESS.md
- [ ] Ensure all existing MuscleGroup tests are passing
- [ ] Create feature branch: `feature/muscle-group-crud`

## 1. Entity & Database Updates (Estimated: 3-4 hours)

### 1.1 Update MuscleGroup Entity
- [ ] Add `IsActive` property with default value `true` `[ReadyToDevelop]` (Est: 15m)
- [ ] Add `CreatedAt` property with UTC timestamp `[ReadyToDevelop]` (Est: 15m)
- [ ] Add `UpdatedAt` nullable property `[ReadyToDevelop]` (Est: 15m)
- [ ] Update Handler class with Create method `[ReadyToDevelop]` (Est: 30m)
- [ ] Update Handler class with Update method `[ReadyToDevelop]` (Est: 30m)
- [ ] Update Handler class with Deactivate method `[ReadyToDevelop]` (Est: 20m)
- [ ] Add unit tests for Handler methods `[ReadyToDevelop]` (Est: 45m)

### 1.2 Database Migration
- [ ] Create migration: `Add-Migration AddCrudFieldsToMuscleGroup` `[ReadyToDevelop]` (Est: 15m)
- [ ] Set default IsActive = true for existing records `[ReadyToDevelop]` (Est: 10m)
- [ ] Set default CreatedAt = current UTC for existing records `[ReadyToDevelop]` (Est: 10m)
- [ ] Add index on (Name, IsActive) for uniqueness checks `[ReadyToDevelop]` (Est: 15m)
- [ ] Review and optimize migration script `[ReadyToDevelop]` (Est: 15m)
- [ ] Test migration up and down `[ReadyToDevelop]` (Est: 15m)

### 🛑 Checkpoint 1: Entity & Database
- [ ] Run `dotnet build` - BUILD MUST BE SUCCESSFUL
- [ ] Run `dotnet test` - ALL TESTS MUST BE GREEN
- [ ] Verify entity changes compile without errors
- [ ] Verify migration applies cleanly

## 2. DTOs & Validation (Estimated: 2-3 hours)

### 2.1 Create DTOs
- [ ] Create `CreateMuscleGroupDto` with Name and BodyPartId `[ReadyToDevelop]` (Est: 20m)
- [ ] Create `UpdateMuscleGroupDto` with Name and BodyPartId `[ReadyToDevelop]` (Est: 20m)
- [ ] Update `MuscleGroupDto` to include IsActive, CreatedAt, UpdatedAt `[ReadyToDevelop]` (Est: 15m)
- [ ] Add XML documentation to all DTO properties `[ReadyToDevelop]` (Est: 20m)

### 2.2 Validation Rules
- [ ] Name: Required, 1-100 characters, trimmed `[ReadyToDevelop]` (Est: 15m)
- [ ] BodyPartId: Required, valid format, must exist and be active `[ReadyToDevelop]` (Est: 20m)
- [ ] Add custom validation attribute for unique name check `[ReadyToDevelop]` (Est: 30m)
- [ ] Unit test all validation scenarios `[ReadyToDevelop]` (Est: 30m)

### 🛑 Checkpoint 2: DTOs & Validation
- [ ] Run `dotnet build` - BUILD MUST BE SUCCESSFUL
- [ ] Run `dotnet test` - ALL TESTS MUST BE GREEN
- [ ] Verify all DTOs have proper validation attributes
- [ ] Verify validation tests cover all scenarios

## 3. Repository Layer (Estimated: 4-5 hours)

### 3.1 Update Repository Interface
- [ ] Add `CreateAsync(MuscleGroup entity)` method `[ReadyToDevelop]` (Est: 10m)
- [ ] Add `UpdateAsync(MuscleGroup entity)` method `[ReadyToDevelop]` (Est: 10m)
- [ ] Add `DeactivateAsync(MuscleGroupId id)` method `[ReadyToDevelop]` (Est: 10m)
- [ ] Add `ExistsByNameAsync(string name, MuscleGroupId? excludeId)` method `[ReadyToDevelop]` (Est: 10m)
- [ ] Add `GetByNameAsync(string name)` method `[ReadyToDevelop]` (Est: 10m)
- [ ] Add `CanDeactivateAsync(MuscleGroupId id)` method (check exercise dependencies) `[ReadyToDevelop]` (Est: 10m)

### 3.2 Implement Repository Methods
- [ ] Implement CreateAsync with proper ID generation `[ReadyToDevelop]` (Est: 20m)
- [ ] Implement UpdateAsync with concurrency handling `[ReadyToDevelop]` (Est: 30m)
- [ ] Implement DeactivateAsync with soft delete `[ReadyToDevelop]` (Est: 20m)
- [ ] Implement ExistsByNameAsync with case-insensitive comparison `[ReadyToDevelop]` (Est: 20m)
- [ ] Implement GetByNameAsync with case-insensitive search `[ReadyToDevelop]` (Est: 20m)
- [ ] Implement CanDeactivateAsync checking active exercise usage `[ReadyToDevelop]` (Est: 30m)

### 3.3 Repository Tests
- [ ] Test CreateAsync with valid data `[ReadyToDevelop]` (Est: 15m)
- [ ] Test CreateAsync with duplicate name `[ReadyToDevelop]` (Est: 15m)
- [ ] Test UpdateAsync with valid changes `[ReadyToDevelop]` (Est: 15m)
- [ ] Test UpdateAsync with concurrency conflict `[ReadyToDevelop]` (Est: 20m)
- [ ] Test DeactivateAsync for existing item `[ReadyToDevelop]` (Est: 15m)
- [ ] Test DeactivateAsync for non-existent item `[ReadyToDevelop]` (Est: 15m)
- [ ] Test ExistsByNameAsync with various cases `[ReadyToDevelop]` (Est: 20m)
- [ ] Test CanDeactivateAsync with and without dependencies `[ReadyToDevelop]` (Est: 25m)

### 🛑 Checkpoint 3: Repository Layer
- [ ] Run `dotnet build` - BUILD MUST BE SUCCESSFUL
- [ ] Run `dotnet test` - ALL TESTS MUST BE GREEN
- [ ] Verify all repository methods are implemented
- [ ] Verify repository tests achieve good coverage

## 4. Controller Updates (Estimated: 4-5 hours)

### 4.1 Add CRUD Endpoints
- [ ] Add POST endpoint with CreateMuscleGroupDto `[ReadyToDevelop]` (Est: 20m)
- [ ] Add PUT endpoint with id parameter and UpdateMuscleGroupDto `[ReadyToDevelop]` (Est: 20m)
- [ ] Add DELETE endpoint with id parameter `[ReadyToDevelop]` (Est: 20m)
- [ ] Add authorization attributes (commented out for now) `[ReadyToDevelop]` (Est: 10m)
- [ ] Add proper ProducesResponseType attributes `[ReadyToDevelop]` (Est: 15m)
- [ ] Add Swagger documentation `[ReadyToDevelop]` (Est: 15m)

### 4.2 Implement Controller Methods
- [ ] POST: Validate BodyPart exists and is active `[ReadyToDevelop]` (Est: 10m)
- [ ] POST: Check for duplicate names before creating `[ReadyToDevelop]` (Est: 10m)
- [ ] POST: Create entity and save through UoW `[ReadyToDevelop]` (Est: 15m)
- [ ] POST: Invalidate cache after successful save `[ReadyToDevelop]` (Est: 10m)
- [ ] POST: Return 201 Created with location header `[ReadyToDevelop]` (Est: 10m)
- [ ] PUT: Validate muscle group exists `[ReadyToDevelop]` (Est: 10m)
- [ ] PUT: Validate BodyPart exists and is active `[ReadyToDevelop]` (Est: 10m)
- [ ] PUT: Check for duplicate names (excluding current) `[ReadyToDevelop]` (Est: 15m)
- [ ] PUT: Update entity and save through UoW `[ReadyToDevelop]` (Est: 15m)
- [ ] PUT: Invalidate cache after successful save `[ReadyToDevelop]` (Est: 10m)
- [ ] PUT: Return 200 OK with updated DTO `[ReadyToDevelop]` (Est: 10m)
- [ ] DELETE: Validate muscle group exists `[ReadyToDevelop]` (Est: 10m)
- [ ] DELETE: Check if can be deactivated (no active dependencies) `[ReadyToDevelop]` (Est: 15m)
- [ ] DELETE: Deactivate entity and save through UoW `[ReadyToDevelop]` (Est: 15m)
- [ ] DELETE: Invalidate cache after successful save `[ReadyToDevelop]` (Est: 10m)
- [ ] DELETE: Return 204 No Content `[ReadyToDevelop]` (Est: 10m)

### 4.3 Controller Tests
- [ ] Test POST with valid data `[ReadyToDevelop]` (Est: 15m)
- [ ] Test POST with missing/invalid fields `[ReadyToDevelop]` (Est: 15m)
- [ ] Test POST with duplicate name `[ReadyToDevelop]` (Est: 15m)
- [ ] Test POST with inactive BodyPart `[ReadyToDevelop]` (Est: 15m)
- [ ] Test POST with non-existent BodyPart `[ReadyToDevelop]` (Est: 15m)
- [ ] Test PUT with valid updates `[ReadyToDevelop]` (Est: 15m)
- [ ] Test PUT with non-existent ID `[ReadyToDevelop]` (Est: 15m)
- [ ] Test PUT with duplicate name `[ReadyToDevelop]` (Est: 15m)
- [ ] Test PUT with invalid BodyPart `[ReadyToDevelop]` (Est: 15m)
- [ ] Test DELETE with valid ID `[ReadyToDevelop]` (Est: 15m)
- [ ] Test DELETE with non-existent ID `[ReadyToDevelop]` (Est: 15m)
- [ ] Test DELETE with active exercise dependencies `[ReadyToDevelop]` (Est: 20m)
- [ ] Test cache invalidation for all mutations `[ReadyToDevelop]` (Est: 25m)

### 🛑 Checkpoint 4: Controller Implementation
- [ ] Run `dotnet build` - BUILD MUST BE SUCCESSFUL
- [ ] Run `dotnet test` - ALL TESTS MUST BE GREEN
- [ ] Verify all CRUD endpoints are implemented
- [ ] Test endpoints manually with Swagger UI

## 5. Integration Tests (Estimated: 3-4 hours)

### 5.1 Create Integration Test Class
- [ ] Create `MuscleGroupIntegrationTests.cs` `[ReadyToDevelop]` (Est: 20m)
- [ ] Set up test fixtures with seeded data `[ReadyToDevelop]` (Est: 30m)
- [ ] Configure test authentication/authorization `[ReadyToDevelop]` (Est: 20m)

### 5.2 Backward Compatibility Tests
- [ ] Test GET all muscle groups returns same format `[ReadyToDevelop]` (Est: 15m)
- [ ] Test GET by ID returns same format `[ReadyToDevelop]` (Est: 15m)
- [ ] Test GET by value returns same format `[ReadyToDevelop]` (Est: 15m)
- [ ] Test GET by BodyPart returns same format `[ReadyToDevelop]` (Est: 15m)
- [ ] Test deactivated muscle groups excluded from GET all `[ReadyToDevelop]` (Est: 20m)

### 5.3 CRUD Flow Tests
- [ ] Test complete create flow with cache verification `[ReadyToDevelop]` (Est: 20m)
- [ ] Test create with duplicate name returns 400 `[ReadyToDevelop]` (Est: 15m)
- [ ] Test create with invalid BodyPart returns 400 `[ReadyToDevelop]` (Est: 15m)
- [ ] Test complete update flow with cache verification `[ReadyToDevelop]` (Est: 20m)
- [ ] Test update to duplicate name returns 400 `[ReadyToDevelop]` (Est: 15m)
- [ ] Test complete delete flow with cache verification `[ReadyToDevelop]` (Est: 20m)
- [ ] Test delete with dependencies returns 400 `[ReadyToDevelop]` (Est: 15m)
- [ ] Test rapid CRUD operations for race conditions `[ReadyToDevelop]` (Est: 25m)

### 5.4 Cache Behavior Tests
- [ ] Test cache cleared after POST `[ReadyToDevelop]` (Est: 15m)
- [ ] Test cache cleared after PUT `[ReadyToDevelop]` (Est: 15m)
- [ ] Test cache cleared after DELETE `[ReadyToDevelop]` (Est: 15m)
- [ ] Test GET uses cache when available `[ReadyToDevelop]` (Est: 15m)
- [ ] Test cache expiration (1 hour for dynamic tables) `[ReadyToDevelop]` (Est: 20m)

### 🛑 Checkpoint 5: Integration Tests
- [ ] Run `dotnet build` - BUILD MUST BE SUCCESSFUL
- [ ] Run `dotnet test` - ALL TESTS MUST BE GREEN
- [ ] Verify all integration tests pass
- [ ] Verify cache behavior is correct

## 6. Documentation (Estimated: 2-3 hours)

### 6.1 API Documentation
- [ ] Create `/api-docs/reference-tables/muscle-groups.md` `[ReadyToDevelop]` (Est: 30m)
- [ ] Document all endpoints with examples `[ReadyToDevelop]` (Est: 30m)
- [ ] Document request/response formats `[ReadyToDevelop]` (Est: 20m)
- [ ] Document error responses and codes `[ReadyToDevelop]` (Est: 20m)
- [ ] Document authorization requirements (future) `[ReadyToDevelop]` (Est: 10m)
- [ ] Add curl examples for each operation `[ReadyToDevelop]` (Est: 20m)

### 6.2 Update Existing Documentation
- [ ] Update `systemPatterns.md` with reference table CRUD pattern `[ReadyToDevelop]` (Est: 20m)
- [ ] Update `cache-invalidation-strategy.md` with MuscleGroup specifics `[ReadyToDevelop]` (Est: 15m)
- [ ] Update reference table overview documentation `[ReadyToDevelop]` (Est: 15m)
- [ ] Add notes about soft delete behavior `[ReadyToDevelop]` (Est: 10m)

### 6.3 Code Documentation
- [ ] Add XML comments to all public methods `[ReadyToDevelop]` (Est: 20m)
- [ ] Document business rules in code comments `[ReadyToDevelop]` (Est: 15m)
- [ ] Add TODO comments for future authorization `[ReadyToDevelop]` (Est: 10m)

### 🛑 Checkpoint 6: Documentation
- [ ] Verify all documentation is complete and accurate
- [ ] Verify API documentation includes examples
- [ ] Verify code has proper XML comments

## 7. Final Verification (Estimated: 2 hours)

### 7.1 Code Quality
- [ ] Run code analysis and fix any warnings `[ReadyToDevelop]` (Est: 20m)
- [ ] Ensure consistent code style `[ReadyToDevelop]` (Est: 15m)
- [ ] Remove any debug code or console writes `[ReadyToDevelop]` (Est: 10m)
- [ ] Verify no hard-coded values `[ReadyToDevelop]` (Est: 10m)

### 7.2 Testing
- [ ] All unit tests pass `[ReadyToDevelop]` (Est: 10m)
- [ ] All integration tests pass `[ReadyToDevelop]` (Est: 10m)
- [ ] Code coverage > 90% for new code `[ReadyToDevelop]` (Est: 15m)
- [ ] Manual testing of all endpoints `[ReadyToDevelop]` (Est: 20m)
- [ ] Test with Swagger UI `[ReadyToDevelop]` (Est: 15m)

### 7.3 Performance
- [ ] Verify query performance with profiler `[ReadyToDevelop]` (Est: 15m)
- [ ] Check for N+1 query issues `[ReadyToDevelop]` (Est: 15m)
- [ ] Verify cache hit rates `[ReadyToDevelop]` (Est: 10m)
- [ ] Load test CRUD operations `[ReadyToDevelop]` (Est: 20m)

### 🛑 Checkpoint 7: Final Verification
- [ ] Run `dotnet build` - BUILD MUST BE SUCCESSFUL (no warnings)
- [ ] Run `dotnet test` - ALL TESTS MUST BE GREEN
- [ ] Verify code coverage meets requirements
- [ ] Verify performance is acceptable

## 8. Deployment Preparation

### 8.1 Migration Strategy
- [ ] Document migration rollback plan `[ReadyToDevelop]` (Est: 20m)
- [ ] Test migration on copy of production data `[ReadyToDevelop]` (Est: 30m)
- [ ] Prepare SQL scripts for manual verification `[ReadyToDevelop]` (Est: 20m)

### 8.2 Release Notes
- [ ] Document new endpoints `[ReadyToDevelop]` (Est: 15m)
- [ ] Document any behavior changes `[ReadyToDevelop]` (Est: 15m)
- [ ] Note future authorization requirements `[ReadyToDevelop]` (Est: 10m)
- [ ] Include examples for API consumers `[ReadyToDevelop]` (Est: 20m)

### ✅ Final Checkpoint: Ready for Release
- [ ] All previous checkpoints passed
- [ ] Feature branch ready to merge
- [ ] Documentation complete
- [ ] Release notes prepared

## Estimated Timeline
- Entity & Database Updates: 3-4 hours
- DTOs & Validation: 2-3 hours
- Repository Layer: 4-5 hours
- Controller Updates: 4-5 hours
- Integration Tests: 3-4 hours
- Documentation: 2-3 hours
- Final Verification: 2 hours
- **Total: 20-26 hours (2.5-3.5 days)**

## Time Tracking Summary (To be completed during implementation)
- **Total Estimated Time:** 23 hours (midpoint of 20-26)
- **Total Actual Time:** [Sum of all task durations]
- **AI Assistance Impact:** [((23 - Actual) / 23) × 100%]
- **Implementation Started:** [First task start timestamp]
- **Implementation Completed:** [Last task finish timestamp]
- **Work Sessions:** [List if implementation was interrupted]

### Example of completed task with time tracking:
```
- [x] Add `IsActive` property with default value `true` 
  `[Implemented: abc123 | Started: 2025-01-15 09:00 | Finished: 2025-01-15 09:10 | Duration: 0h 10m]` (Est: 15m)
  Note: Faster than estimated due to AI code generation
```

## Notes
- Keep existing GET endpoints unchanged for backward compatibility
- Use soft delete to preserve referential integrity
- Authorization will be added when ReferenceData-Management claim is defined
- Consider batch operations for future enhancement
- Time estimates are based on manual implementation without AI assistance
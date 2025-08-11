# Migrate Reference Table Command

Migrates a reference table service from PureReferenceService dependency to standalone implementation with direct cache integration, following the BodyPartService pattern.

## Usage

```
/migrate-reference-table <ServiceName>
```

## Examples

```
/migrate-reference-table DifficultyLevel
/migrate-reference-table ExecutionProtocol
/migrate-reference-table WorkoutCategory
```

**Note**: Provide the service name WITHOUT the "Service" suffix.

## Migration Process Overview

This command triggers the reference-table-migrator agent to systematically migrate a reference service through these phases:

1. **Integration Test Creation** - Writes cache verification tests
2. **Service Refactoring** - Converts to standalone with direct cache
3. **Unit Test Updates** - Adds cache mocking and fixes tests
4. **Progressive Testing** - Verifies changes incrementally
5. **Cleanup** - Removes obsolete reference service files
6. **Final Verification** - Ensures entire app builds and tests pass

## Detailed Process Flow

### Phase 1: Setup and Analysis
- Creates TodoWrite task list for tracking
- Identifies target service and reference wrapper files
- Analyzes current implementation vs target pattern (BodyPartService)
- Locates all related test files

### Phase 2: Integration Test Creation
Creates a new feature file for cache verification:
- Tests that repeated API calls only hit database once
- Verifies cache keys are properly isolated
- Ensures different IDs have separate cache entries
- Tests GetByValue caching (if applicable)

Location: `/GetFitterGetBigger.API.IntegrationTests/Features/ReferenceData/[ServiceName]Caching.feature`

### Phase 3: Service Refactoring
Transforms the service to standalone implementation:
- Removes inheritance from PureReferenceService
- Adds direct IEternalCacheService dependency
- Implements CacheLoad pattern for all methods
- Preserves all existing functionality
- Follows BodyPartService as reference implementation

Key changes:
```csharp
// BEFORE
public class ServiceName : PureReferenceService<Entity, Dto>

// AFTER
public class ServiceName(
    IUnitOfWorkProvider<FitnessDbContext> unitOfWorkProvider,
    IEternalCacheService cacheService,
    ILogger<ServiceName> logger) : IServiceName
```

### Phase 4: Unit Test Updates
Updates existing unit tests:
- Adds Mock<IEternalCacheService> setup
- Updates constructor calls with new dependencies
- Fixes cache-related test expectations
- Ensures all assertions still pass

### Phase 5: Progressive Testing
Runs tests incrementally to catch issues early:
1. New integration tests for caching
2. Updated unit tests for the service
3. All tests to ensure no regressions

### Phase 6: Cleanup
Removes obsolete components:
- Deletes registration from Program.cs
- Removes [ServiceName]ReferenceService.cs file
- Cleans up any unused imports

### Phase 7: Final Verification
- Runs `dotnet clean && dotnet build`
- Executes full test suite
- Reports final status

## Pattern References

The migration follows these established patterns:

### Target Implementation
- **Reference**: `/Services/Implementations/BodyPartService.cs`
- **Test Pattern**: `/IntegrationTests/Features/ReferenceData/BodyPartsCaching.feature`

### Key Patterns Applied
- **CacheLoad Pattern** - Fluent cache operations with auto-caching
- **ServiceValidate Pattern** - Fluent validation with single exit
- **Empty Pattern** - IEmptyDto<T> for null object handling
- **Eternal Cache** - For reference data that never changes

## Services Available for Migration

Current reference services using PureReferenceService:
- DifficultyLevel
- ExecutionProtocol
- ExerciseType
- ExerciseWeightType
- KineticChainType
- MetricType
- MovementPattern
- MuscleRole
- WorkoutCategory
- WorkoutObjective
- WorkoutState

## Output Format

### Initial Report
```
STARTING REFERENCE TABLE MIGRATION
Target Service: [ServiceName]Service
Current Status: Uses PureReferenceService
Target Pattern: Standalone with direct cache (like BodyPartService)
Tasks Created: 8
```

### Progress Updates
```
✅ Created integration tests: [ServiceName]Caching.feature
✅ Refactored service to standalone implementation
✅ Updated unit tests with cache mocks
⏳ Running integration tests...
✅ Integration tests: PASSED (3 scenarios)
⏳ Running all tests...
✅ All tests: PASSED (1172 total)
✅ Removed obsolete registration from Program.cs
✅ Deleted [ServiceName]ReferenceService.cs
```

### Final Summary
```
MIGRATION COMPLETE
Service: [ServiceName]Service
Status: ✅ Successfully migrated
Pattern: Standalone with IEternalCacheService
Tests: All passing (X unit, Y integration)
Files removed: [ServiceName]ReferenceService.cs
Build: ✅ Success
```

## Common Issues and Solutions

### Integration Tests Failing Due to Fixture Sharing
**Problem**: Tests pass individually but fail when run together in the same feature file

**Symptoms**: 
- Cache tests showing unexpected query counts
- "Expected 1 query but found 0" or vice versa
- Tests interfering with each other's cache state

**Solution**: Split tests into separate feature files for proper isolation:
1. Create `{ServiceName}Caching.feature` for basic scenarios (GetAll and GetById twice)
2. Create `{ServiceName}AdvancedCaching.feature` for complex scenarios (different IDs, GetByValue, GetByCode)
3. Each feature file gets its own test fixture and PostgreSQL container
4. This ensures tests don't share cache state

### Cache Key Mismatch Between GetAll and GetById
**Problem**: GetById hits database even after GetAll cached everything

**Reason**: GetAll and GetById use different cache keys:
- GetAll uses: `CacheKeyGenerator.GetAllKey("{Entities}")`
- GetById uses: `CacheKeyGenerator.GetByIdKey("{Entities}", id.ToString())`

**Solution**: Expect 1 DB query on first GetById after GetAll (this is correct behavior)

### Cache Mock Not Returning Expected Value
**Solution**: Ensure mock setup matches exact cache key format used by CacheKeyGenerator

### Repository Method Not Found
**Solution**: Verify repository has required methods (GetAllActiveAsync, GetByValueAsync)

### Error Messages Not Defined
**Solution**: Add constants to [ServiceName]ErrorMessages class

### DTO Missing Empty Implementation
**Solution**: Implement IEmptyDto<T> interface with Empty property and IsEmpty check

### Tests Failing After Migration
**Solution**: Update mock setups and verify cache behavior expectations

## Prerequisites

Before running this command:
1. Ensure the project builds successfully
2. All existing tests should be passing
3. The service must currently use PureReferenceService pattern
4. Related DTOs should implement IEmptyDto<T>

## Standards Applied

The migration strictly follows:
- `/memory-bank/CODE_QUALITY_STANDARDS.md` - Core quality standards
- `/memory-bank/CodeQualityGuidelines/CacheIntegrationPattern.md` - Cache patterns
- `/memory-bank/CodeQualityGuidelines/ServiceValidatePattern.md` - Validation patterns
- `/memory-bank/CodeQualityGuidelines/NullObjectPattern.md` - Empty pattern

## Best Practices

1. **One at a time**: Migrate services individually to isolate issues
2. **Test incrementally**: Run tests after each major change
3. **Preserve behavior**: Service must work exactly as before
4. **Document issues**: Report any problems for future reference
5. **Commit between migrations**: Save progress after each successful migration

## Related Commands

- `/analyze-code-quality` - Check service quality before migration
- `/fix-failing-tests` - Fix any test issues after migration
- `/git-commit-push` - Commit successful migration

## Notes

- The agent uses the Task tool with "reference-table-migrator" subagent type
- References `.claude/agents/reference-table-migrator.md` for detailed logic
- Follows BodyPartService as the reference implementation
- Uses TodoWrite to track progress through migration phases
- All changes are verified with build and test runs
- Migration is atomic - either fully succeeds or can be rolled back

## Success Criteria

✅ Integration tests verify cache works (DB hit only once)  
✅ Service no longer inherits from PureReferenceService  
✅ Service uses IEternalCacheService directly with CacheLoad  
✅ All unit tests updated and passing  
✅ All integration tests passing  
✅ Reference service wrapper removed  
✅ Application builds successfully  
✅ ALL tests green (no regressions)

---

## Command Processing

When invoked with a service name (e.g., "DifficultyLevel"):

1. Parse the service name from $ARGUMENTS
2. Remove "Service" suffix if provided
3. Pass the clean service name to the reference-table-migrator agent

The agent will then:
- Look for `/Services/Implementations/{ServiceName}Service.cs` to refactor
- Look for `/Services/ReferenceTables/{ServiceName}ReferenceService.cs` to remove
- Follow the migration process described above

Use the reference-table-migrator agent to migrate the specified service from $ARGUMENTS to a standalone implementation without PureReferenceService dependency.
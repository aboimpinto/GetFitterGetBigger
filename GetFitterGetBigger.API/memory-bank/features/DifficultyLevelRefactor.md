  Please read these documents before starting (they contain critical patterns and lessons learned):

  Core Architecture Patterns:
  - @memory-bank/THREE-TIER-ENTITY-ARCHITECTURE.md - Overall architecture principles
  - @memory-bank/EMPTY-GUID-HANDLING-PATTERN.md - Empty pattern implementation
  - @memory-bank/SERVICE-RESULT-PATTERN.md - Error handling structure
  - @memory-bank/CONTROLLER-PATTERN-REFERENCE-TABLES.md - Controller patterns

  Implementation Guides:
  - @memory-bank/INVALID-ID-HANDLING-PATTERN.md - How to handle invalid IDs consistently
  - @memory-bank/REFACTORING-REFERENCE-TABLES-TO-EMPTY-PATTERN.md - Step-by-step refactoring process

  Critical Process Guidelines:
  - @memory-bank/REFACTORING-AND-TEST-EVOLUTION.md - MUST READ - When to change tests vs code
  - @memory-bank/CODE_QUALITY_STANDARDS.md - Quality standards to maintain
  - @memory-bank/TESTING-QUICK-REFERENCE.md - Common test patterns and solutions

  Reference Implementations

  Use these as the exact templates to follow:
  - @GetFitterGetBigger.API/Services/Implementations/BodyPartService.cs - Service pattern
  - @GetFitterGetBigger.API/Services/Implementations/MovementPatternService.cs - Service pattern
  - @GetFitterGetBigger.API/Controllers/BodyPartsController.cs - Controller pattern
  - @GetFitterGetBigger.API/Models/SpecializedIds/BodyPartId.cs - ID type pattern

  Expected Deliverables

  1. Create these new files (following exact patterns from references):
    - DifficultyLevelService.cs implementing EmptyEnabledPureReferenceService
    - IDifficultyLevelService.cs interface
    - DifficultyLevelsController.cs with proper ProducesResponseType attributes
    - Unit tests: DifficultyLevelServiceTests.cs and DifficultyLevelsControllerTests.cs
    - Integration tests: Update existing or create new BDD scenarios
  2. Update existing files:
    - Migrate DifficultyLevel.cs entity to implement IEmptyEntity<DifficultyLevel>
    - Update DifficultyLevelId.cs to follow BodyPartId pattern exactly
    - Update repository to extend EmptyEnabledReferenceDataRepository
    - Register new service in DI container
  3. Apply the Invalid ID Handling Pattern:
    - Service pattern matching: id.IsEmpty ? ValidationFailed : GetByIdAsync(id.ToString())
    - Controller pattern matching for 400/404/200 responses
    - Integration tests expecting 400 for invalid formats, 404 for valid but not found

  Critical Requirements

  🚨 Test Evolution Guidelines:
  - Before changing ANY code to fix a failing test, ask: "Is this test still relevant?"
  - If a test expects null and we've eliminated nulls → Delete/rewrite the test
  - If a test expects wrong error codes → Update the test, not the code
  - Integration tests should expect 400 for invalid IDs, 404 for valid but not found
  - Unit tests should expect ValidationFailed for empty IDs

  🔧 Technical Requirements:
  - Must use EmptyEnabledPureReferenceService<DifficultyLevel, ReferenceDataDto> as base
  - Must implement pattern matching in GetByIdAsync(DifficultyLevelId id)
  - Must return appropriate DTOs (probably ReferenceDataDto)
  - Must handle caching with IEmptyEnabledCacheService
  - Must follow the exact error handling patterns from BodyParts/MovementPatterns

  ⚠️ What NOT to do:
  - Don't break architecture patterns to satisfy tests
  - Don't add special cases or null handling
  - Don't create new error types - use existing ServiceError patterns
  - Don't change ProducesResponseType to match impossible scenarios
  - Don't attempt to "fix" tests by breaking established patterns

  Success Criteria

  - All new code follows established patterns exactly
  - Integration tests pass with correct status codes (400/404/200)
  - Unit tests validate the Empty pattern correctly
  - No null handling anywhere in the implementation
  - Service performance optimized (no DB calls for invalid IDs)
  - Documentation updated if new patterns emerge

  Questions to Address

  If you encounter failing tests, apply this decision tree:
  1. Does this test validate current business rules? → Keep and adapt
  2. Does this test expect eliminated patterns (nulls, wrong errors)? → Delete/rewrite
  3. Does this test conflict with architecture principles? → Fix the test
  4. Is this an integration test that should remain stable? → Investigate if contract actually changed

  🛑 IMPORTANT: Test Failure Reporting Process

  If you encounter failing tests after implementing the pattern correctly:
  1. STOP - Do not attempt to "fix" the code to make tests pass
  2. Verify your implementation matches the reference patterns exactly
  3. Create a detailed report for the user including:
     - Test name and location
     - Expected vs Actual behavior
     - How the reference implementation (BodyPart/MovementPattern) handles this scenario
     - Specific questions about whether to update tests or maintain current behavior

  Example Report Format:
  ```
  ## Test Failure Report - [Entity Name]
  
  The refactor is complete and follows the [Reference] pattern exactly.
  
  ### Failing Test 1: [Test Name]
  - **Location**: [File path]
  - **Scenario**: [What the test is testing]
  - **Expected**: [What the test expects]
  - **Actual**: [What actually happens]
  - **Reference Pattern**: [How BodyPart/MovementPattern handles this]
  - **Question**: Should we [specific question about test evolution]?
  
  ### Architecture Verification
  - Controller: ✅ Matches [Reference]Controller pattern
  - Service: ✅ Matches [Reference]Service pattern
  - Entity: ✅ Implements IEmptyEntity correctly
  - Repository: ✅ Uses EmptyEnabledReferenceDataRepository
  ```

  This prevents circular fixes and maintains architectural integrity.

  Common Test Conflicts After Refactoring

  Based on experience with BodyPart and MovementPattern refactors:
  1. **Error Message Specificity**: Tests may expect exact error messages that differ from reference patterns
     - Example: "Invalid ID format" vs "Invalid [entity] ID format"
     - Resolution: Usually update test to match reference pattern
  
  2. **Empty GUID Handling**: Tests may expect 404 for empty GUIDs, but Empty pattern returns 400
     - Example: "difficultylevel-00000000-0000-0000-0000-000000000000"
     - Resolution: Confirm if this is a contract change that needs user decision
  
  3. **Null vs Empty**: Tests expecting null responses need complete rewrite
     - Example: GetByIdAsync returning null vs returning Empty with error
     - Resolution: Always rewrite to expect Empty pattern

  Time Investment

  Based on previous experience: ~2-3 hours for implementation, potentially longer for test evolution discussions if significant conflicts arise.

  ## Code Review Results - APPROVED FOR MERGE

  **Review Date:** 2025-07-15  
  **Status:** ✅ APPROVED - Ready for Production

  ### Review Summary
  The DifficultyLevel refactor has been completed and underwent comprehensive code review. The implementation perfectly follows the established Empty/Null Object Pattern with zero critical issues.

  **Key Findings:**
  - ✅ **No null handling present** - All implementations use Empty pattern
  - ✅ **No exceptions thrown** - All error handling via ServiceResult pattern
  - ✅ **Perfect pattern compliance** - Matches BodyPart/MovementPattern exactly
  - ✅ **All tests updated appropriately** - Following established patterns

  ### Magic String Issue Resolution

  **Issue Identified:** Tests contain magic strings for IDs (e.g., "difficultylevel-123", "bodypart-456")

  **Proposed Solution:**
  
  1. **Create Test Constants Classes**
     ```csharp
     public static class TestConstants
     {
         public static class DifficultyLevels
         {
             public const string ValidId = "difficultylevel-11111111-1111-1111-1111-111111111111";
             public const string InvalidFormatId = "invalid-format";
             public const string NonExistentId = "difficultylevel-99999999-9999-9999-9999-999999999999";
         }
     }
     ```

  2. **Test Builder Pattern Enhancement**
     ```csharp
     public class DifficultyLevelTestBuilder
     {
         public static DifficultyLevelId ValidId() => 
             DifficultyLevelId.From(new Guid("11111111-1111-1111-1111-111111111111"));
         
         public static DifficultyLevelId NonExistentId() => 
             DifficultyLevelId.From(new Guid("99999999-9999-9999-9999-999999999999"));
     }
     ```

  3. **Integration Test Data Management**
     - Use consistent test data across all reference data tests
     - Create shared test data seeders
     - Maintain test data in dedicated configuration files

  **Implementation Priority:**
  - **Phase 1**: Complete current DifficultyLevel implementation (✅ DONE)
  - **Phase 2**: Create test constants and builders for all reference data
  - **Phase 3**: Refactor existing tests to eliminate magic strings

  **Benefits:**
  - Eliminates magic strings throughout test suite
  - Provides consistent test data across all reference data entities
  - Makes tests more maintainable and less brittle
  - Follows established testing patterns

  ### Architecture Verification Complete
  - **Entity Layer**: ✅ Perfect IEmptyEntity implementation
  - **Service Layer**: ✅ Exemplary EmptyEnabledPureReferenceService usage
  - **Controller Layer**: ✅ Clean ServiceResult pattern matching
  - **Repository Layer**: ✅ Proper EmptyEnabledReferenceDataRepository inheritance
  - **Tests**: ✅ Well-structured with proper mocking

  **Full Code Review Report:** See `/memory-bank/features/DIFFICULTY-LEVEL-CODE-REVIEW-REPORT.md`
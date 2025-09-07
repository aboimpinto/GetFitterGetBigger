# FEAT-031: Workout Template Exercise Management System - Implementation Tasks

## Feature Branch: `feature/feat-031-workout-template-exercise-management`
## Estimated Total Time: 24h 0m
## Actual Total Time: 3h 15m+ (in progress)

## 📚 Pre-Implementation Checklist
- [x] Read `/memory-bank/Overview/SystemPatterns.md` - Architecture rules
- [x] Read `/memory-bank/CodeQualityGuidelines/ServiceResultPattern.md` - Error handling patterns
- [x] Read `/memory-bank/PracticalGuides/CommonImplementationPitfalls.md` - Critical mistakes to avoid
- [x] Read `/memory-bank/CodeQualityGuidelines/ServiceValidatePattern.md` - Validation patterns
- [x] Read `/memory-bank/Overview/UnitVsIntegrationTests.md` - Test separation rules
- [x] Read `/memory-bank/DevelopmentGuidelines/Templates/FeatureCheckpointTemplate.md` - Checkpoint format requirements
- [x] Run baseline health check (`dotnet build` and `dotnet test`)
- [x] Study existing WorkoutTemplate implementation patterns
- [x] Review ExecutionProtocol and ExerciseLink integrations

## 📊 Phase Overview

| Phase | Name | Status | Tasks | Estimated | Actual | File |
|-------|------|--------|-------|-----------|--------|------|
| 1 | Planning & Analysis | ✅ Complete | 1/1 | 2h 0m | 1h 30m | [Phase 1: Planning & Analysis.md](./Phases/Phase%201:%20Planning%20&%20Analysis.md) |
| 2 | Models & Database | ⏳ In Progress | 2/4 | 4h 0m | 1h 45m+ | [Phase 2: Models & Database.md](./Phases/Phase%202:%20Models%20&%20Database.md) |
| 3 | Repository Layer | 🔄 Pending | 0/2 | 3h 0m | - | [Phase 3: Repository Layer.md](./Phases/Phase%203:%20Repository%20Layer.md) |
| 4 | Service Layer | 🔄 Pending | 0/4 | 6h 0m | - | [Phase 4: Service Layer.md](./Phases/Phase%204:%20Service%20Layer.md) |
| 5 | API Controllers | 🔄 Pending | 0/3 | 3h 0m | - | [Phase 5: API Controllers.md](./Phases/Phase%205:%20API%20Controllers.md) |
| 6 | Integration & Testing | 🔄 Pending | 0/3 | 4h 0m | - | [Phase 6: Integration & Testing.md](./Phases/Phase%206:%20Integration%20&%20Testing.md) |
| 7 | Documentation & Deployment | 🔄 Pending | 0/3 | 2h 0m | - | [Phase 7: Documentation & Deployment.md](./Phases/Phase%207:%20Documentation%20&%20Deployment.md) |

**Total Progress**: 3/20 tasks complete (15%)

## 📝 Current Status

### Active Phase: Phase 2 - Models & Database
- **Current Task**: BLOCKED - Critical code review violations
- **Blockers**: ❌ CRITICAL - Build failing with 15 errors, ServiceValidate pattern violations
- **Next Steps**: Fix all critical violations before proceeding (see Code Review section)

### Recent Achievements
✅ Phase 1 completed with comprehensive codebase analysis  
✅ ExecutionProtocol renamed from "Standard" to "Reps and Sets"  
✅ WorkoutTemplate entity enhanced with ExecutionProtocolId  
⏳ WorkoutTemplateExercise entity redesign in progress  

### Code Review Status
- **Phase 1**: APPROVED (97% quality score)
- **Phase 2**: ❌ BLOCKED (62% quality score - Build failing, 5 critical violations)

## 🎯 Phase Details

Detailed implementation tasks for each phase are now in separate files in the `./Phases/` directory. Each phase file contains:
- Complete task descriptions with implementation details
- Code examples and patterns to follow
- Testing requirements
- Checkpoint requirements and validation criteria
- Code review integration points

Navigate to the appropriate phase file for detailed implementation instructions.

---

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

## CHECKPOINT: Phase 0 - Baseline Health Check
`[Completed]` - 2025-09-07 13:52

**Requirements for Transition to IN_PROGRESS:**
- Build: ✅ 0 errors, 0 warnings
- Tests: ✅ All existing tests passing (1758 total)
- Feature Branch: ✅ Created `feature/FEAT-031-workout-template-exercise-management`
- Folder Status: ✅ Moved to `/memory-bank/features/2-IN_PROGRESS/`

**Baseline Metrics:**
- **Date/Time**: 2025-09-07 13:52
- **Branch**: feature/FEAT-031-workout-template-exercise-management
- **Build Status**: SUCCESS - 0 errors, 0 warnings
- **Test Results**: 
  - Unit Tests: 1403 passed, 0 failed
  - Integration Tests: 355 passed, 0 failed
  - Total: 1758 passed (100% success rate)
- **Code Coverage**: 59.19% line coverage
- **Pre-Validation**: APPROVED (see pre-validation-report-APPROVED-2025-09-07-13-50.md)

**Transition Summary:**
- Feature successfully transitioned from READY_TO_DEVELOP to IN_PROGRESS
- All architectural patterns verified and documented
- Implementation can proceed with Phase 1

**Git Commit Hash**: 220b22c3 (fix: address pre-validation issues in feature-tasks.md)

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

## Notes
- **CRITICAL**: Follow UnitOfWork patterns from `/memory-bank/PracticalGuides/CommonImplementationPitfalls.md`
- **INTEGRATION**: Leverage existing ExecutionProtocol and ExerciseLink implementations
- **TESTING**: Comprehensive BDD scenarios for complex auto-linking logic
- **PATTERNS**: Follow ServiceResult<T> and ServiceValidate patterns throughout
- **MIGRATION**: Fresh start approach - drop old WorkoutTemplateExercise table
- **JSON**: Use PostgreSQL JSON support for flexible metadata storage
- **PERFORMANCE**: Proper indexing and AsNoTracking() for query optimization

## Code Review
_All code review violations and fixes are tracked here_

### Review: 2025-09-07 - phase-2-review-2025-09-07 ✅ COMPLETED
- [x] **CRITICAL**: Fix WhenValidAsync Empty pattern violation in ServiceValidationBuilderExtensions.cs:376
- [x] **CRITICAL**: Replace WhenValidAsync() with MatchAsync() in WorkoutTemplateService.cs (10 instances)  
- [x] **CRITICAL**: Replace WhenValidAsync() with MatchAsync() in DuplicationHandler.cs:36-55
- [x] **CRITICAL**: Move complex nested validation logic from MatchAsync to main ServiceValidate chain (DuplicationHandler.cs:44-55)
- [x] **CRITICAL**: Remove entity conversion anti-pattern from DuplicationHandler.cs:142-182 (ConvertToEntityAsync method)
- [x] **CRITICAL**: Replace reflection-based ToReferenceDataDto with strongly-typed extension methods (WorkoutTemplateExtensions.cs:88-160)
- [x] **HIGH**: Fix hardcoded DateTime.UtcNow in DTO mappings (WorkoutTemplateExtensions.cs:58-59, 81-82) - User noted entities don't have those properties (acknowledged)
- [ ] **MEDIUM**: Refactor complex search logic into SearchHandler class (WorkoutTemplateService.cs:63-160) - Future improvement
- [ ] **MEDIUM**: Improve documentation style in ExecutionProtocolConstants.cs (minor issue) - Future improvement  
- [x] **CRITICAL FIXES VERIFIED**: All violations resolved in follow-up review
- [x] **Re-run code review after fixes**: APPROVED (91% approval rate, 0 critical violations)

### Review: 2025-09-07 - Follow-up Post-Fix Review ✅ APPROVED
**Report**: [code-review-report-2025-09-07-001.md](./code-reviews/Phase_2_Models_Database/code-review-report-2025-09-07-001.md)
**Status**: ✅ APPROVED (91% approval rate, 0 critical violations)
**Quality Improvement**: +19% approval rate improvement (72% → 91%)
**Critical Issues**: 8 → 0 (All resolved 🎉)
**Phase 2 Status**: ✅ CLEARED FOR PHASE 3 PROGRESSION

### Review: 2025-09-07 - Current State Review ❌ BLOCKED
**Report**: [code-review-report-2025-09-07-002.md](./code-reviews/Phase_2_Models_Database/code-review-report-2025-09-07-002.md)
**Status**: ❌ BLOCKED (62% approval rate, 5 critical violations, 15 build errors)
**Build Status**: FAILING - ServiceValidate pattern violations
**Critical Issues**: 5 critical violations causing build failures
**Commits Reviewed**: 81c3d9ba, bf5865fe, d991daee, ebe620b8
**Files Reviewed**: 4 files (3 modified, 1 new)

**CRITICAL VIOLATIONS - MUST FIX**:
- [ ] Fix incorrect MatchAsync signatures in WorkoutTemplateService.cs (11 instances)
- [ ] Fix PagedResponse<WorkoutTemplateDto>.Empty reference (line 67)
- [ ] Remove all WhenValidAsync calls from other services (15 build errors)
- [ ] Replace magic strings with constants (multiple files)
- [ ] Implement proper Empty pattern for DTOs

**Phase 2 Status**: ❌ BLOCKED FOR PHASE 3 PROGRESSION

**Git Commit Hash**: a76e50b9 (docs(feat-031): add comprehensive Phase 2 code review and block progression)

---

**Feature Status**: BLOCKED - Phase 2 Critical Issues
**Current Focus**: URGENT - Fix 5 critical violations causing 15 build errors before proceeding
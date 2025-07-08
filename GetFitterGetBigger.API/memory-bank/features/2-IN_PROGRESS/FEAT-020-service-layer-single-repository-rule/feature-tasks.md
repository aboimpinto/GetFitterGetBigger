# Service Layer Single Repository Rule Implementation Tasks

## Feature Branch: `feature/service-layer-single-repository-rule`
## Estimated Total Time: 16 hours 15 minutes
## Actual Total Time: [To be calculated at completion]

## 📚 Pre-Implementation Checklist
- [x] Read `/memory-bank/systemPatterns.md` - Architecture rules
- [x] Read `/memory-bank/unitOfWorkPattern.md` - Critical ReadOnly vs Writable patterns
- [x] Read `/memory-bank/common-implementation-pitfalls.md` - Common mistakes to avoid
- [x] Run baseline health check (`dotnet build` and `dotnet test`)

## Baseline Health Check Report
**Date/Time**: 2025-01-08 18:45
**Branch**: feature/service-layer-single-repository-rule

### Build Status
- **Build Result**: ✅ Success
- **Warning Count**: 0 warnings
- **Warning Details**: None

### Test Status
- **Total Tests**: 553
- **Passed**: 553
- **Failed**: 0 (MUST be 0 to proceed)
- **Skipped/Ignored**: 0
- **Test Execution Time**: 6.0 seconds

### Decision to Proceed
- [x] All tests passing
- [x] Build successful
- [x] Warnings documented and approved

**Approval to Proceed**: Yes

## Category 1: New Service Implementations - Estimated: 4h 15m

### BodyPart Service
- **Task 1.1:** Create IBodyPartService interface `[Implemented: 9bb71f0f | Started: 2025-01-08 18:47 | Finished: 2025-01-08 18:50 | Duration: 0h 3m]` (Est: 15m)
- **Task 1.2:** Implement BodyPartService with ExistsAsync method `[Implemented: ff14f8cb | Started: 2025-01-08 18:50 | Finished: 2025-01-08 18:53 | Duration: 0h 3m]` (Est: 30m)
- **Task 1.3:** Write unit tests for BodyPartService `[Implemented: bc95d6a6 | Started: 2025-01-08 18:53 | Finished: 2025-01-08 18:56 | Duration: 0h 3m]` (Est: 30m)
- **Task 1.3a:** Register BodyPartService in DI container (Program.cs) `[Implemented: 8240f700 | Started: 2025-01-08 19:02 | Finished: 2025-01-08 19:03 | Duration: 0h 1m]` (Est: 5m)

### ExerciseType Service
- **Task 1.4:** Create IExerciseTypeService interface `[Implemented: 58c59879 | Started: 2025-01-08 18:56 | Finished: 2025-01-08 18:57 | Duration: 0h 1m]` (Est: 15m)
- **Task 1.5:** Implement ExerciseTypeService with ExistsAsync and AllExistAsync methods `[Implemented: 03316071 | Started: 2025-01-08 18:57 | Finished: 2025-01-08 18:59 | Duration: 0h 2m]` (Est: 45m)
- **Task 1.6:** Write unit tests for ExerciseTypeService `[Implemented: 5c150273 | Started: 2025-01-08 19:04 | Finished: 2025-01-08 19:06 | Duration: 0h 2m]` (Est: 45m)
- **Task 1.6a:** Register ExerciseTypeService in DI container (Program.cs) `[Implemented: dbfa5f8d | Started: 2025-01-08 19:06 | Finished: 2025-01-08 19:07 | Duration: 0h 1m]` (Est: 5m)

### Claim Service
- **Task 1.7:** Create IClaimService interface `[InProgress: Started: 2025-01-08 19:07]` (Est: 15m)
- **Task 1.8:** Implement ClaimService with CreateUserClaimsAsync supporting UoW parameter `[ReadyToDevelop]` (Est: 30m)
- **Task 1.9:** Write unit tests for ClaimService `[ReadyToDevelop]` (Est: 30m)
- **Task 1.9a:** Register ClaimService in DI container (Program.cs) `[ReadyToDevelop]` (Est: 5m)

## 🔄 Checkpoint 1
- Status: 🛑
- [ ] All new services implemented with tests
- [ ] Build successful (`dotnet build`)
- [ ] All tests passing (`dotnet test`)

## Category 2: MuscleGroupService Refactoring - Estimated: 2h

#### ⚠️ CRITICAL Before Starting:
- [ ] Re-read `/memory-bank/common-implementation-pitfalls.md` Section 1
- [ ] Remember: Service-to-service calls for validation, not direct repository access

- **Task 2.1:** Replace IBodyPartRepository with IBodyPartService in constructor `[ReadyToDevelop]` (Est: 15m)
- **Task 2.2:** Update CreateAsync method to use BodyPartService.ExistsAsync `[ReadyToDevelop]` (Est: 30m)
- **Task 2.3:** Update UpdateAsync method to use BodyPartService.ExistsAsync `[ReadyToDevelop]` (Est: 30m)
- **Task 2.4:** Update unit tests to mock IBodyPartService instead of repository `[ReadyToDevelop]` (Est: 45m)

## 🔄 Checkpoint 2
- Status: 🛑
- [ ] MuscleGroupService refactored and working
- [ ] All MuscleGroupService tests passing
- [ ] No build warnings

## Category 3: ExerciseService Refactoring - Estimated: 3h

#### ⚠️ CRITICAL Before Starting:
- [ ] Review all 4 locations where ExerciseTypeRepository is used
- [ ] Ensure service maintains same validation behavior

- **Task 3.1:** Replace IExerciseTypeRepository with IExerciseTypeService in constructor `[ReadyToDevelop]` (Est: 15m)
- **Task 3.2:** Update CreateAsync method to use ExerciseTypeService.AllExistAsync `[ReadyToDevelop]` (Est: 30m)
- **Task 3.3:** Update UpdateAsync method to use ExerciseTypeService.AllExistAsync `[ReadyToDevelop]` (Est: 30m)
- **Task 3.4:** Update two internal validation methods to use ExerciseTypeService `[ReadyToDevelop]` (Est: 30m)
- **Task 3.5:** Update unit tests to mock IExerciseTypeService instead of repository `[ReadyToDevelop]` (Est: 1h 15m)

## 🔄 Checkpoint 3
- Status: 🛑
- [ ] ExerciseService refactored and working
- [ ] All ExerciseService tests passing
- [ ] Integration tests still working

## Category 4: AuthService Refactoring - Estimated: 2.5h

#### ⚠️ CRITICAL Before Starting:
- [ ] This involves transactional pattern - UoW must be passed correctly
- [ ] Ensure atomic transaction behavior is preserved

- **Task 4.1:** Replace IClaimRepository with IClaimService in constructor `[ReadyToDevelop]` (Est: 15m)
- **Task 4.2:** Update RegisterAsync to use ClaimService.CreateUserClaimsAsync with UoW `[ReadyToDevelop]` (Est: 45m)
- **Task 4.3:** Update unit tests to mock IClaimService instead of repository `[ReadyToDevelop]` (Est: 45m)
- **Task 4.4:** Write integration test to verify transactional behavior `[ReadyToDevelop]` (Est: 45m)

## 🔄 Checkpoint 4
- Status: 🛑
- [ ] AuthService refactored with transactional pattern
- [ ] All AuthService tests passing
- [ ] Transaction atomicity verified

## Category 5: Dependency Injection Configuration - Estimated: 1h

- **Task 5.1:** Register IBodyPartService and BodyPartService in Program.cs `[ReadyToDevelop]` (Est: 10m)
- **Task 5.2:** Register IExerciseTypeService and ExerciseTypeService in Program.cs `[ReadyToDevelop]` (Est: 10m)
- **Task 5.3:** Register IClaimService and ClaimService in Program.cs `[ReadyToDevelop]` (Est: 10m)
- **Task 5.4:** Run full application to verify DI configuration `[ReadyToDevelop]` (Est: 30m)

## Category 6: Documentation and Architecture Update - Estimated: 1.5h

- **Task 6.1:** Update ARCHITECTURE-REFACTORING-INITIATIVE.md with service layer rules `[ReadyToDevelop]` (Est: 30m)
- **Task 6.2:** Create SERVICE-LAYER-PATTERNS.md documenting the patterns `[ReadyToDevelop]` (Est: 45m)
- **Task 6.3:** Update systemPatterns.md with service-to-service communication patterns `[ReadyToDevelop]` (Est: 15m)

## Category 7: Final Validation - Estimated: 1h

- **Task 7.1:** Run complete test suite to ensure no regressions `[ReadyToDevelop]` (Est: 15m)
- **Task 7.2:** Manual API testing of affected endpoints `[ReadyToDevelop]` (Est: 30m)
- **Task 7.3:** Performance validation to ensure no degradation `[ReadyToDevelop]` (Est: 15m)

## 🔄 Final Checkpoint
- Status: 🛑
- [ ] All tests passing (100% green)
- [ ] No build warnings
- [ ] API endpoints working correctly
- [ ] Documentation updated

## Time Tracking Summary
- **Total Estimated Time:** 16 hours
- **Total Actual Time:** [To be calculated from task durations]
- **AI Assistance Impact:** [% reduction in time]
- **Implementation Started:** [First task start time]
- **Implementation Completed:** [Last task finish time]

## Implementation Summary Report
[To be completed at feature completion]

## Notes
- Each service refactoring must maintain existing behavior
- Focus on one service at a time to minimize risk
- Ensure all tests pass after each refactoring step
- Document any unexpected issues or pattern variations discovered
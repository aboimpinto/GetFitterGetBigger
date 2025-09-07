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
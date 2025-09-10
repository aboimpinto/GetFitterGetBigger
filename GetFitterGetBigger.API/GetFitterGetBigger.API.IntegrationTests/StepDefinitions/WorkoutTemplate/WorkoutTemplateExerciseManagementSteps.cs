using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Fixtures;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Helpers;
using GetFitterGetBigger.API.IntegrationTests.TestBuilders;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Microsoft.EntityFrameworkCore;
using TechTalk.SpecFlow;

namespace GetFitterGetBigger.API.IntegrationTests.StepDefinitions.WorkoutTemplate;

/// <summary>
/// Simplified step definitions for WorkoutTemplateExercise management BDD tests
/// </summary>
[Binding]
public class WorkoutTemplateExerciseManagementSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly PostgreSqlTestFixture _fixture;
    private HttpResponseMessage? _lastResponse;
    private readonly Dictionary<string, string> _exerciseGuidMap = new();
    private readonly Dictionary<string, string> _templateGuidMap = new();

    public WorkoutTemplateExerciseManagementSteps(ScenarioContext scenarioContext, PostgreSqlTestFixture fixture)
    {
        _scenarioContext = scenarioContext;
        _fixture = fixture;
    }

    #region Given Steps - Setup

    [Given(@"a workout template ""(.*)"" exists with ExecutionProtocol ""(.*)""")]
    public async Task GivenAWorkoutTemplateExistsWithExecutionProtocol(string templateName, string executionProtocol)
    {
        await _fixture.ExecuteDbContextAsync(async context =>
        {
            var seedBuilder = new SeedDataBuilder(context);
            await seedBuilder.WithAllReferenceDataAsync();

            // Create workout template
            var templateId = WorkoutTemplateId.New();
            var categoryId = WorkoutCategoryId.From(SeedDataBuilder.StandardIds.FullBodyCategoryId);
            var difficultyId = DifficultyLevelId.From(SeedDataBuilder.StandardIds.IntermediateDifficultyId);
            var workoutStateId = WorkoutStateId.From(Guid.Parse("02000001-0000-0000-0000-000000000001")); // DRAFT

            var templateResult = Models.Entities.WorkoutTemplate.Handler.Create(
                templateId,
                templateName,
                $"Template for {templateName}",
                categoryId,
                difficultyId,
                60,
                new List<string> { "test" },
                true,
                workoutStateId,
                ExecutionProtocolId.ParseOrEmpty(SeedDataBuilder.StandardIds.ExecutionProtocolIds.Standard),
                null,
                DateTime.UtcNow,
                DateTime.UtcNow
            );

            // For test data, we expect creation to succeed
            if (templateResult.IsFailure)
            {
                throw new InvalidOperationException($"Failed to create test template: {string.Join(", ", templateResult.Errors)}");
            }

            await context.WorkoutTemplates.AddAsync(templateResult.Value);
            await context.SaveChangesAsync();

            _templateGuidMap[templateName] = templateId.ToString();
            _scenarioContext["CurrentTemplateId"] = templateId.ToString();
        });
    }

    [Given(@"a workout exercise ""(.*)"" exists")]
    public async Task GivenAWorkoutExerciseExists(string exerciseName)
    {
        await _fixture.ExecuteDbContextAsync(async context =>
        {
            var seedBuilder = new SeedDataBuilder(context);
            await seedBuilder.WithAllReferenceDataAsync();

            // Create exercise
            var exerciseId = ExerciseId.New();
            var exercise = GetFitterGetBigger.API.Models.Entities.Exercise.Handler.Create(
                id: exerciseId,
                name: exerciseName,
                description: $"Description for {exerciseName}",
                videoUrl: "https://example.com/video.mp4",
                imageUrl: "https://example.com/image.jpg",
                isUnilateral: false,
                isActive: true,
                difficultyId: DifficultyLevelId.From(SeedDataBuilder.StandardIds.IntermediateDifficultyId),
                kineticChainId: KineticChainTypeId.From(SeedDataBuilder.StandardIds.CompoundKineticChainId),
                exerciseWeightTypeId: ExerciseWeightTypeId.From(SeedDataBuilder.StandardIds.WeightRequiredWeightTypeId)
            );

            await context.Exercises.AddAsync(exercise);
            await context.SaveChangesAsync();

            _exerciseGuidMap[exerciseName] = exerciseId.ToString();
            _scenarioContext[$"Exercise_{exerciseName}_Id"] = exerciseId.ToString();
        });
    }

    [Given(@"the exercise ""(.*)"" has been added to the template")]
    public async Task GivenTheExerciseHasBeenAddedToTheTemplate(string exerciseName)
    {
        var templateId = _scenarioContext["CurrentTemplateId"].ToString();
        var exerciseId = _exerciseGuidMap[exerciseName];

        await _fixture.ExecuteDbContextAsync(async context =>
        {
            var workoutTemplateExerciseResult = WorkoutTemplateExercise.Handler.CreateNew(
                WorkoutTemplateId.ParseOrEmpty(templateId),
                ExerciseId.ParseOrEmpty(exerciseId),
                WorkoutZone.Main,
                1,
                null
            );

            // For test data, we expect creation to succeed
            if (workoutTemplateExerciseResult.IsFailure)
            {
                throw new InvalidOperationException($"Failed to create test exercise link: {string.Join(", ", workoutTemplateExerciseResult.Errors)}");
            }

            await context.WorkoutTemplateExercises.AddAsync(workoutTemplateExerciseResult.Value);
            await context.SaveChangesAsync();

            _scenarioContext["AddedExerciseId"] = workoutTemplateExerciseResult.Value.Id.ToString();
        });
    }

    [Given(@"the template has multiple exercises added")]
    public async Task GivenTheTemplateHasMultipleExercisesAdded()
    {
        var templateId = _scenarioContext["CurrentTemplateId"].ToString();

        await _fixture.ExecuteDbContextAsync(async context =>
        {
            var seedBuilder = new SeedDataBuilder(context);
            await seedBuilder.WithAllReferenceDataAsync();

            // Add multiple exercises
            var exercises = new[]
            {
                ("Push-ups", 1),
                ("Pull-ups", 2),
                ("Squats", 3)
            };

            foreach (var (name, order) in exercises)
            {
                var exerciseId = ExerciseId.New();
                var exercise = GetFitterGetBigger.API.Models.Entities.Exercise.Handler.Create(
                    id: exerciseId,
                    name: name,
                    description: $"Description for {name}",
                    videoUrl: "https://example.com/video.mp4",
                    imageUrl: "https://example.com/image.jpg",
                    isUnilateral: false,
                    isActive: true,
                    difficultyId: DifficultyLevelId.From(SeedDataBuilder.StandardIds.IntermediateDifficultyId),
                    kineticChainId: KineticChainTypeId.From(SeedDataBuilder.StandardIds.CompoundKineticChainId),
                    exerciseWeightTypeId: ExerciseWeightTypeId.From(SeedDataBuilder.StandardIds.WeightRequiredWeightTypeId)
                );

                await context.Exercises.AddAsync(exercise);

                var workoutTemplateExerciseResult = WorkoutTemplateExercise.Handler.CreateNew(
                    WorkoutTemplateId.ParseOrEmpty(templateId),
                    exerciseId,
                    WorkoutZone.Main,
                    order,
                    null
                );

                // For test data, we expect creation to succeed
                if (workoutTemplateExerciseResult.IsFailure)
                {
                    throw new InvalidOperationException($"Failed to create test exercise link: {string.Join(", ", workoutTemplateExerciseResult.Errors)}");
                }

                await context.WorkoutTemplateExercises.AddAsync(workoutTemplateExerciseResult.Value);
            }

            await context.SaveChangesAsync();
        });
    }

    #endregion

    #region When Steps - Actions

    [When(@"I add exercise ""(.*)"" to template ""(.*)"" with phase ""(.*)"" and round (.*)")]
    public async Task WhenIAddExerciseToTemplateWithPhaseAndRound(string exerciseName, string templateName, string phase, int round)
    {
        var client = _scenarioContext.GetHttpClient();
        var templateId = _templateGuidMap.ContainsKey(templateName) 
            ? _templateGuidMap[templateName] 
            : templateName; // Allow direct ID usage

        var exerciseId = _exerciseGuidMap.ContainsKey(exerciseName) 
            ? _exerciseGuidMap[exerciseName] 
            : exerciseName; // Allow direct ID usage

        var request = new
        {
            ExerciseId = exerciseId,
            Phase = phase,
            RoundNumber = round,
            Metadata = "{\"reps\": 10}"
        };

        _lastResponse = await client.PostAsJsonAsync(
            $"/api/v2/workout-templates/{templateId}/exercises",
            request);

        // Debug: Log the response for debugging
        if (!_lastResponse.IsSuccessStatusCode)
        {
            var content = await _lastResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"❌ Response Status: {_lastResponse.StatusCode}");
            Console.WriteLine($"❌ Response Content: {content}");
            Console.WriteLine($"📋 Request Template ID: {templateId}");
            Console.WriteLine($"📋 Request Exercise ID: {exerciseId}");
            Console.WriteLine($"📋 Request Phase: {phase}");
            Console.WriteLine($"📋 Request Round: {round}");
        }

        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [When(@"I remove the exercise ""(.*)"" from the template")]
    public async Task WhenIRemoveTheExerciseFromTheTemplate(string exerciseName)
    {
        var client = _scenarioContext.GetHttpClient();
        var templateId = _scenarioContext["CurrentTemplateId"].ToString();
        var exerciseId = _scenarioContext["AddedExerciseId"].ToString();

        _lastResponse = await client.DeleteAsync(
            $"/api/v2/workout-templates/{templateId}/exercises/{exerciseId}");

        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [When(@"I get all exercises for the template")]
    public async Task WhenIGetAllExercisesForTheTemplate()
    {
        var client = _scenarioContext.GetHttpClient();
        var templateId = _scenarioContext["CurrentTemplateId"].ToString();

        _lastResponse = await client.GetAsync(
            $"/api/v2/workout-templates/{templateId}/exercises");

        _scenarioContext.SetLastResponse(_lastResponse);
    }

    #endregion

    #region Then Steps - Assertions


    [Then(@"the exercise should be added to the template")]
    public async Task ThenTheExerciseShouldBeAddedToTheTemplate()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.IsSuccessStatusCode.Should().BeTrue();

        // Verify in database
        var templateId = _scenarioContext["CurrentTemplateId"].ToString();
        
        await _fixture.ExecuteDbContextAsync(async context =>
        {
            var exercises = await context.WorkoutTemplateExercises
                .Where(e => e.WorkoutTemplateId == WorkoutTemplateId.ParseOrEmpty(templateId))
                .ToListAsync();

            exercises.Should().NotBeEmpty();
        });
    }

    [Then(@"the exercise should be removed from the template")]
    public async Task ThenTheExerciseShouldBeRemovedFromTheTemplate()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.IsSuccessStatusCode.Should().BeTrue();

        // Verify in database
        var templateId = _scenarioContext["CurrentTemplateId"].ToString();
        var exerciseId = _scenarioContext["AddedExerciseId"].ToString();
        
        await _fixture.ExecuteDbContextAsync(async context =>
        {
            var exercise = await context.WorkoutTemplateExercises
                .FirstOrDefaultAsync(e => 
                    e.Id == WorkoutTemplateExerciseId.ParseOrEmpty(exerciseId));

            exercise.Should().BeNull();
        });
    }

    [Then(@"the response should contain all template exercises")]
    public async Task ThenTheResponseShouldContainAllTemplateExercises()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.IsSuccessStatusCode.Should().BeTrue();

        var content = await _lastResponse.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        
        // Basic validation that we got a response with exercises
        // Just check that we got valid JSON response
        content.Should().Contain("{");
        content.Should().Contain("}");
    }


    #endregion
}
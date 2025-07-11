using System.Net;
using System.Net.Http.Json;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Tests.TestBuilders;
using Xunit;

namespace GetFitterGetBigger.API.Tests.IntegrationTests;

/// <summary>
/// Integration tests for complete exercise workflow including CoachNotes and ExerciseTypes
/// </summary>
[Collection("SharedDatabase")]
public class ExerciseCompleteWorkflowTests : IClassFixture<SharedDatabaseTestFixture>
{
    private readonly SharedDatabaseTestFixture _fixture;
    private readonly HttpClient _client;
    
    public ExerciseCompleteWorkflowTests(SharedDatabaseTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
    }
    
    [Fact]
    public async Task CompleteExerciseWorkflow_CreateWithAllFeatures_Success()
    {
        // Arrange
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Complete Feature Test Exercise")
            .WithDescription("A comprehensive exercise demonstrating all features")
            .WithCoachNotes(
                ("Setup: Position yourself at the squat rack", 1),
                ("Execution: Lower slowly for 3 seconds", 2),
                ("Hold: Pause at the bottom for 1 second", 3),
                ("Return: Push through heels to stand", 4),
                ("Breathing: Inhale down, exhale up", 5)
            )
            .WithExerciseTypes(
                TestConstants.ExerciseTypeIds.Warmup,
                TestConstants.ExerciseTypeIds.Workout
            )
            .WithVideoUrl("https://example.com/squat-tutorial.mp4")
            .WithImageUrl("https://example.com/squat-form.jpg")
            .WithIsUnilateral(false)
            .WithDifficultyId(TestConstants.DifficultyLevelIds.Intermediate)
            .WithMuscleGroups(
                (TestConstants.MuscleGroupIds.Legs, TestConstants.MuscleRoleIds.Primary), // Quadriceps - Primary
                (TestConstants.MuscleGroupIds.Chest, TestConstants.MuscleRoleIds.Stabilizer) // Pectoralis - Stabilizer
            )
            .WithBodyPartIds(
                TestConstants.BodyPartIds.Legs,
                TestConstants.BodyPartIds.Chest
            )
            .Build();
        
        // Act - Create Exercise
        var createResponse = await _client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert - Creation
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        
        var createdExercise = await createResponse.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        Assert.Equal("Complete Feature Test Exercise", createdExercise.Name);
        
        // Verify CoachNotes are ordered correctly
        Assert.Equal(5, createdExercise.CoachNotes.Count);
        for (int i = 0; i < createdExercise.CoachNotes.Count; i++)
        {
            Assert.Equal(i + 1, createdExercise.CoachNotes[i].Order);
        }
        Assert.Equal("Setup: Position yourself at the squat rack", createdExercise.CoachNotes[0].Text);
        Assert.Equal("Breathing: Inhale down, exhale up", createdExercise.CoachNotes[4].Text);
        
        // Verify ExerciseTypes
        Assert.Equal(2, createdExercise.ExerciseTypes.Count);
        var typeIds = createdExercise.ExerciseTypes.Select(et => et.Id).ToList();
        Assert.Contains("exercisetype-a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d", typeIds);
        Assert.Contains("exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e", typeIds);
        
        // Verify other properties
        Assert.Equal("https://example.com/squat-tutorial.mp4", createdExercise.VideoUrl);
        Assert.Equal("https://example.com/squat-form.jpg", createdExercise.ImageUrl);
        Assert.False(createdExercise.IsUnilateral);
        Assert.Equal(2, createdExercise.MuscleGroups.Count);
        Assert.Single(createdExercise.Equipment);
        Assert.Equal(2, createdExercise.BodyParts.Count);
        Assert.Single(createdExercise.MovementPatterns);
    }
    
    [Fact]
    public async Task CompleteExerciseWorkflow_CreateThenRetrieve_MaintainsCoachNotesOrder()
    {
        // Arrange
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Retrieve Order Test Exercise")
            .WithDescription("Testing coach notes order after retrieval")
            .WithCoachNotes(
                ("Step E", 5),
                ("Step A", 1),
                ("Step C", 3),
                ("Step B", 2),
                ("Step D", 4)
            )
            .Build();
        
        // Act - Create
        var createResponse = await _client.PostAsJsonAsync("/api/exercises", request);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        
        var createdExercise = await createResponse.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        
        // Act - Retrieve by ID
        var getResponse = await _client.GetAsync($"/api/exercises/{createdExercise.Id}");
        
        // Assert - Retrieval maintains order
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var retrievedExercise = await getResponse.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(retrievedExercise);
        Assert.Equal(5, retrievedExercise.CoachNotes.Count);
        
        // Verify alphabetical order by step letter
        Assert.Equal("Step A", retrievedExercise.CoachNotes[0].Text);
        Assert.Equal("Step B", retrievedExercise.CoachNotes[1].Text);
        Assert.Equal("Step C", retrievedExercise.CoachNotes[2].Text);
        Assert.Equal("Step D", retrievedExercise.CoachNotes[3].Text);
        Assert.Equal("Step E", retrievedExercise.CoachNotes[4].Text);
    }
    
    [Fact]
    public async Task CompleteExerciseWorkflow_CreateMinimalExercise_Success()
    {
        // Arrange - Minimal valid exercise
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Minimal Exercise Test")
            .WithDescription("The bare minimum required exercise")
            .WithCoachNotes() // Empty is allowed
            .WithExerciseTypes() // Empty is allowed
            .WithEquipmentIds() // Empty equipment
            .WithMovementPatternIds() // Empty movement patterns
            .Build();
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdExercise = await response.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        Assert.Empty(createdExercise.CoachNotes);
        Assert.Empty(createdExercise.ExerciseTypes);
        Assert.Empty(createdExercise.Equipment);
        Assert.Empty(createdExercise.MovementPatterns);
        Assert.Single(createdExercise.MuscleGroups);
        Assert.Single(createdExercise.BodyParts);
    }
}
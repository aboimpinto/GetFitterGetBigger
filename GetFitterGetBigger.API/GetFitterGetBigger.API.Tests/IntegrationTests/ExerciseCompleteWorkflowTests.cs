using System.Net;
using System.Net.Http.Json;
using GetFitterGetBigger.API.DTOs;
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
        var request = new CreateExerciseRequest
        {
            Name = "Complete Feature Test Exercise",
            Description = "A comprehensive exercise demonstrating all features",
            CoachNotes = new List<CoachNoteRequest>
            {
                new() { Text = "Setup: Position yourself at the squat rack", Order = 1 },
                new() { Text = "Execution: Lower slowly for 3 seconds", Order = 2 },
                new() { Text = "Hold: Pause at the bottom for 1 second", Order = 3 },
                new() { Text = "Return: Push through heels to stand", Order = 4 },
                new() { Text = "Breathing: Inhale down, exhale up", Order = 5 }
            },
            ExerciseTypeIds = new List<string>
            {
                "exercisetype-a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d", // Warmup
                "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e"  // Workout
            },
            VideoUrl = "https://example.com/squat-tutorial.mp4",
            ImageUrl = "https://example.com/squat-form.jpg",
            IsUnilateral = false,
            DifficultyId = "difficultylevel-9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a", // Intermediate
            MuscleGroups = new List<MuscleGroupWithRoleRequest>
            {
                new()
                {
                    MuscleGroupId = "musclegroup-eeff0011-2233-4455-6677-889900112233", // Quadriceps
                    MuscleRoleId = "musclerole-abcdef12-3456-7890-abcd-ef1234567890" // Primary
                },
                new()
                {
                    MuscleGroupId = "musclegroup-ccddeeff-0011-2233-4455-667788990011", // Pectoralis (as stabilizer)
                    MuscleRoleId = "musclerole-22334455-6677-8899-aabb-ccddeeff0011" // Stabilizer
                }
            },
            EquipmentIds = new List<string> 
            { 
                "equipment-33445566-7788-99aa-bbcc-ddeeff001122" // Barbell
            },
            BodyPartIds = new List<string> 
            { 
                "bodypart-4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5", // Legs
                "bodypart-7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c"  // Chest (stabilizer)
            },
            MovementPatternIds = new List<string> 
            { 
                "movementpattern-bbccddee-ff00-1122-3344-556677889900" // Squat
            }
        };
        
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
        var request = new CreateExerciseRequest
        {
            Name = "Retrieve Order Test Exercise",
            Description = "Testing coach notes order after retrieval",
            CoachNotes = new List<CoachNoteRequest>
            {
                new() { Text = "Step E", Order = 5 },
                new() { Text = "Step A", Order = 1 },
                new() { Text = "Step C", Order = 3 },
                new() { Text = "Step B", Order = 2 },
                new() { Text = "Step D", Order = 4 }
            },
            ExerciseTypeIds = new List<string> { "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e" },
            DifficultyId = "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
            MuscleGroups = new List<MuscleGroupWithRoleRequest>
            {
                new()
                {
                    MuscleGroupId = "musclegroup-ccddeeff-0011-2233-4455-667788990011",
                    MuscleRoleId = "musclerole-abcdef12-3456-7890-abcd-ef1234567890"
                }
            },
            EquipmentIds = new List<string>(),
            BodyPartIds = new List<string> { "bodypart-7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c" },
            MovementPatternIds = new List<string>()
        };
        
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
        var request = new CreateExerciseRequest
        {
            Name = "Minimal Exercise Test",
            Description = "The bare minimum required exercise",
            CoachNotes = new List<CoachNoteRequest>(), // Empty is allowed
            ExerciseTypeIds = new List<string>(), // Empty is allowed
            DifficultyId = "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
            MuscleGroups = new List<MuscleGroupWithRoleRequest>
            {
                new()
                {
                    MuscleGroupId = "musclegroup-ccddeeff-0011-2233-4455-667788990011",
                    MuscleRoleId = "musclerole-abcdef12-3456-7890-abcd-ef1234567890"
                }
            },
            EquipmentIds = new List<string>(), // Empty is allowed
            BodyPartIds = new List<string> { "bodypart-7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c" },
            MovementPatternIds = new List<string>() // Empty is allowed
        };
        
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
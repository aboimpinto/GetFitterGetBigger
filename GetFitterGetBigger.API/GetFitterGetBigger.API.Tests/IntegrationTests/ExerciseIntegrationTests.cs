using System.Net;
using System.Net.Http.Json;
using GetFitterGetBigger.API.DTOs;
using Xunit;

namespace GetFitterGetBigger.API.Tests.IntegrationTests;

[Collection("SharedDatabase")]
public class ExerciseIntegrationTests : IClassFixture<SharedDatabaseTestFixture>
{
    private readonly SharedDatabaseTestFixture _fixture;
    private readonly HttpClient _client;
    
    public ExerciseIntegrationTests(SharedDatabaseTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
    }
    
    [Fact]
    public async Task CreateExercise_WithCoachNotes_ReturnsCreatedExerciseWithOrderedNotes()
    {
        // Arrange
        var request = new CreateExerciseRequest
        {
            Name = "Integration Test Squat",
            Description = "Test squat exercise with coach notes",
            CoachNotes = new List<CoachNoteRequest>
            {
                new() { Text = "Keep your back straight", Order = 2 },
                new() { Text = "Warm up properly first", Order = 1 },
                new() { Text = "Control the descent", Order = 3 }
            },
            ExerciseTypeIds = new List<string>
            {
                "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e" // Workout
            },
            VideoUrl = "https://example.com/squat.mp4",
            ImageUrl = "https://example.com/squat.jpg",
            IsUnilateral = false,
            DifficultyId = "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b", // Beginner
            MuscleGroups = new List<MuscleGroupWithRoleRequest>
            {
                new()
                {
                    MuscleGroupId = "musclegroup-eeff0011-2233-4455-6677-889900112233", // Quadriceps
                    MuscleRoleId = "musclerole-abcdef12-3456-7890-abcd-ef1234567890" // Primary
                }
            },
            EquipmentIds = new List<string> { "equipment-33445566-7788-99aa-bbcc-ddeeff001122" }, // Barbell
            BodyPartIds = new List<string> { "bodypart-4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5" }, // Legs
            MovementPatternIds = new List<string> { "movementpattern-bbccddee-ff00-1122-3344-556677889900" }, // Squat
            KineticChainId = "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4" // Compound
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdExercise = await response.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        Assert.Equal("Integration Test Squat", createdExercise.Name);
        
        // Check coach notes are ordered correctly
        Assert.Equal(3, createdExercise.CoachNotes.Count);
        Assert.Equal("Warm up properly first", createdExercise.CoachNotes[0].Text);
        Assert.Equal(1, createdExercise.CoachNotes[0].Order);
        Assert.Equal("Keep your back straight", createdExercise.CoachNotes[1].Text);
        Assert.Equal(2, createdExercise.CoachNotes[1].Order);
        Assert.Equal("Control the descent", createdExercise.CoachNotes[2].Text);
        Assert.Equal(3, createdExercise.CoachNotes[2].Order);
        
        // Check exercise types
        Assert.Single(createdExercise.ExerciseTypes);
        Assert.Equal("exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e", createdExercise.ExerciseTypes[0].Id);
    }
    
    [Fact]
    public async Task CreateExercise_WithMultipleExerciseTypes_ReturnsCreatedExerciseWithAllTypes()
    {
        // Arrange
        var request = new CreateExerciseRequest
        {
            Name = "Integration Test Complex Exercise",
            Description = "Exercise with multiple types",
            CoachNotes = new List<CoachNoteRequest>(),
            ExerciseTypeIds = new List<string>
            {
                "exercisetype-a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d", // Warmup
                "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e", // Workout
                "exercisetype-c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f"  // Cooldown
            },
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
            MovementPatternIds = new List<string>(),
            KineticChainId = "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4" // Compound
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdExercise = await response.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        Assert.Equal(3, createdExercise.ExerciseTypes.Count);
        
        var typeIds = createdExercise.ExerciseTypes.Select(et => et.Id).OrderBy(id => id).ToList();
        Assert.Contains("exercisetype-a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d", typeIds);
        Assert.Contains("exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e", typeIds);
        Assert.Contains("exercisetype-c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f", typeIds);
    }
    
    [Fact]
    public async Task CreateExercise_WithRestTypeAndOtherTypes_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateExerciseRequest
        {
            Name = "Integration Test Rest Exercise",
            Description = "Invalid exercise with Rest and other types",
            CoachNotes = new List<CoachNoteRequest>(),
            ExerciseTypeIds = new List<string>
            {
                "exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a", // Rest (with "rest" in ID)
                "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e"  // Workout
            },
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
            MovementPatternIds = new List<string>(),
            KineticChainId = null // REST exercises should have null KineticChainId
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Rest", content);
    }
    
    [Fact]
    public async Task CreateExercise_WithOnlyRestType_ReturnsCreatedExercise()
    {
        // Arrange
        var request = new CreateExerciseRequest
        {
            Name = "Integration Test Rest Period",
            Description = "Valid rest exercise",
            CoachNotes = new List<CoachNoteRequest>
            {
                new() { Text = "Take a 60 second break", Order = 1 }
            },
            ExerciseTypeIds = new List<string>
            {
                "exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a" // Rest only
            },
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
            MovementPatternIds = new List<string>(),
            KineticChainId = null // REST exercises should have null KineticChainId
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdExercise = await response.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        Assert.Single(createdExercise.ExerciseTypes);
        Assert.Equal("exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a", createdExercise.ExerciseTypes[0].Id);
    }
    
    [Fact]
    public async Task CreateExercise_WithEmptyCoachNotes_ReturnsCreatedExerciseWithNoNotes()
    {
        // Arrange
        var request = new CreateExerciseRequest
        {
            Name = "Integration Test No Notes Exercise",
            Description = "Exercise without coach notes",
            CoachNotes = new List<CoachNoteRequest>(), // Empty
            ExerciseTypeIds = new List<string>
            {
                "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e"
            },
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
            MovementPatternIds = new List<string>(),
            KineticChainId = "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4" // Compound
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdExercise = await response.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        Assert.Empty(createdExercise.CoachNotes);
    }
    
    [Fact]
    public async Task UpdateExercise_AddCoachNotes_UpdatesExerciseWithNewNotes()
    {
        // Arrange - Create an exercise without coach notes
        var createRequest = new CreateExerciseRequest
        {
            Name = "Update Test Exercise",
            Description = "Exercise to test updates",
            CoachNotes = new List<CoachNoteRequest>(),
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
            MovementPatternIds = new List<string>(),
            KineticChainId = "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4" // Compound
        };
        
        var createResponse = await _client.PostAsJsonAsync("/api/exercises", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        
        var createdExercise = await createResponse.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        Assert.NotNull(createdExercise.Id);
        
        // Update with coach notes
        var updateRequest = new UpdateExerciseRequest
        {
            Name = "Updated Test Exercise",
            Description = "Updated description",
            CoachNotes = new List<CoachNoteRequest>
            {
                new() { Text = "First step", Order = 1 },
                new() { Text = "Second step", Order = 2 },
                new() { Text = "Third step", Order = 3 }
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
            MovementPatternIds = new List<string>(),
            KineticChainId = "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4" // Compound
        };
        
        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/api/exercises/{createdExercise.Id}", updateRequest);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        
        var updatedExercise = await updateResponse.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(updatedExercise);
        Assert.Equal("Updated Test Exercise", updatedExercise.Name);
        Assert.Equal(3, updatedExercise.CoachNotes.Count);
        Assert.Equal("First step", updatedExercise.CoachNotes[0].Text);
        Assert.Equal(1, updatedExercise.CoachNotes[0].Order);
        Assert.Equal("Second step", updatedExercise.CoachNotes[1].Text);
        Assert.Equal(2, updatedExercise.CoachNotes[1].Order);
        Assert.Equal("Third step", updatedExercise.CoachNotes[2].Text);
        Assert.Equal(3, updatedExercise.CoachNotes[2].Order);
    }
    
    [Fact]
    public async Task UpdateExercise_ModifyExistingCoachNotes_UpdatesNotesCorrectly()
    {
        // Arrange - Create an exercise with coach notes
        var createRequest = new CreateExerciseRequest
        {
            Name = "Exercise With Notes",
            Description = "Exercise with existing notes",
            CoachNotes = new List<CoachNoteRequest>
            {
                new() { Text = "Original first step", Order = 1 },
                new() { Text = "Original second step", Order = 2 }
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
            MovementPatternIds = new List<string>(),
            KineticChainId = "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4" // Compound
        };
        
        var createResponse = await _client.PostAsJsonAsync("/api/exercises", createRequest);
        var createdExercise = await createResponse.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        
        // Update with modified coach notes
        var updateRequest = new UpdateExerciseRequest
        {
            Name = "Exercise With Notes",
            Description = "Exercise with existing notes",
            CoachNotes = new List<CoachNoteRequest>
            {
                new() { Id = createdExercise.CoachNotes[0].Id, Text = "Modified first step", Order = 1 },
                new() { Text = "New second step", Order = 2 }, // New note without ID
                new() { Text = "New third step", Order = 3 }   // Another new note
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
            MovementPatternIds = new List<string>(),
            KineticChainId = "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4" // Compound
        };
        
        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/api/exercises/{createdExercise.Id}", updateRequest);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        
        var updatedExercise = await updateResponse.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(updatedExercise);
        Assert.Equal(3, updatedExercise.CoachNotes.Count); // Should have 3 notes now
        Assert.Equal("Modified first step", updatedExercise.CoachNotes[0].Text);
        Assert.Equal("New second step", updatedExercise.CoachNotes[1].Text);
        Assert.Equal("New third step", updatedExercise.CoachNotes[2].Text);
    }
    
    [Fact]
    public async Task UpdateExercise_ChangeExerciseTypes_UpdatesTypesCorrectly()
    {
        // Arrange - Create an exercise with multiple types
        var createRequest = new CreateExerciseRequest
        {
            Name = "Multi-Type Exercise",
            Description = "Exercise with multiple types",
            CoachNotes = new List<CoachNoteRequest>(),
            ExerciseTypeIds = new List<string>
            {
                "exercisetype-a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d", // Warmup
                "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e"  // Workout
            },
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
            MovementPatternIds = new List<string>(),
            KineticChainId = "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4" // Compound
        };
        
        var createResponse = await _client.PostAsJsonAsync("/api/exercises", createRequest);
        var createdExercise = await createResponse.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        
        // Update with different exercise types
        var updateRequest = new UpdateExerciseRequest
        {
            Name = "Multi-Type Exercise",
            Description = "Exercise with updated types",
            CoachNotes = new List<CoachNoteRequest>(),
            ExerciseTypeIds = new List<string>
            {
                "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e", // Workout
                "exercisetype-c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f"  // Cooldown
            },
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
            MovementPatternIds = new List<string>(),
            KineticChainId = "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4" // Compound
        };
        
        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/api/exercises/{createdExercise.Id}", updateRequest);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        
        var updatedExercise = await updateResponse.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(updatedExercise);
        Assert.Equal(2, updatedExercise.ExerciseTypes.Count);
        
        var typeIds = updatedExercise.ExerciseTypes.Select(et => et.Id).OrderBy(id => id).ToList();
        Assert.Contains("exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e", typeIds); // Workout
        Assert.Contains("exercisetype-c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f", typeIds); // Cooldown
        Assert.DoesNotContain("exercisetype-a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d", typeIds); // Warmup should be removed
    }
    
    [Fact]
    public async Task UpdateExercise_WithRestTypeAndOtherTypes_ReturnsBadRequest()
    {
        // Arrange - Create a normal exercise
        var createRequest = new CreateExerciseRequest
        {
            Name = "Normal Exercise",
            Description = "Exercise to test Rest validation",
            CoachNotes = new List<CoachNoteRequest>(),
            ExerciseTypeIds = new List<string> { "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e" }, // Workout
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
            MovementPatternIds = new List<string>(),
            KineticChainId = "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4" // Compound
        };
        
        var createResponse = await _client.PostAsJsonAsync("/api/exercises", createRequest);
        var createdExercise = await createResponse.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        
        // Try to update with Rest and other types
        var updateRequest = new UpdateExerciseRequest
        {
            Name = "Normal Exercise",
            Description = "Exercise to test Rest validation",
            CoachNotes = new List<CoachNoteRequest>(),
            ExerciseTypeIds = new List<string>
            {
                "exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a", // Rest (with "rest" in ID)
                "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e"  // Workout
            },
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
            MovementPatternIds = new List<string>(),
            KineticChainId = null // REST exercises should have null KineticChainId
        };
        
        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/api/exercises/{createdExercise.Id}", updateRequest);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);
        
        var content = await updateResponse.Content.ReadAsStringAsync();
        Assert.Contains("Rest", content);
    }
}
using System.Net;
using System.Net.Http.Json;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Tests.TestBuilders;
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
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Integration Test Squat")
            .WithDescription("Test squat exercise with coach notes")
            .WithCoachNotes(
                ("Keep your back straight", 2),
                ("Warm up properly first", 1),
                ("Control the descent", 3)
            )
            .WithVideoUrl("https://example.com/squat.mp4")
            .WithImageUrl("https://example.com/squat.jpg")
            .WithIsUnilateral(false)
            .WithMuscleGroups(
                ("musclegroup-eeff0011-2233-4455-6677-889900112233", "musclerole-abcdef12-3456-7890-abcd-ef1234567890") // Quadriceps - Primary
            )
            .Build();
        
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
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Integration Test Complex Exercise")
            .WithDescription("Exercise with multiple types")
            .WithExerciseTypes(
                "exercisetype-a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d", // Warmup
                "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e", // Workout
                "exercisetype-c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f"  // Cooldown
            )
            .Build();
        
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
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Integration Test Rest Exercise")
            .WithDescription("Invalid exercise with Rest and other types")
            .WithExerciseTypes(
                "exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a", // Rest (with "rest" in ID)
                "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e"  // Workout
            )
            .WithKineticChainId(null) // REST exercises should have null KineticChainId
            .Build();
        
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
        var request = CreateExerciseRequestBuilder.ForRestExercise()
            .WithName("Integration Test Rest Period")
            .WithDescription("Valid rest exercise")
            .WithCoachNotes(
                ("Take a 60 second break", 1)
            )
            .Build();
        
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
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Integration Test No Notes Exercise")
            .WithDescription("Exercise without coach notes")
            .WithCoachNotes() // Empty
            .Build();
        
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
        var createRequest = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Update Test Exercise")
            .WithDescription("Exercise to test updates")
            .WithCoachNotes()
            .Build();
        
        var createResponse = await _client.PostAsJsonAsync("/api/exercises", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        
        var createdExercise = await createResponse.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        Assert.NotNull(createdExercise.Id);
        
        // Update with coach notes
        var updateRequest = UpdateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Updated Test Exercise")
            .WithDescription("Updated description")
            .WithCoachNotes(
                ("First step", 1),
                ("Second step", 2),
                ("Third step", 3)
            )
            .Build();
        
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
        var createRequest = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Exercise With Notes")
            .WithDescription("Exercise with existing notes")
            .WithCoachNotes(
                ("Original first step", 1),
                ("Original second step", 2)
            )
            .Build();
        
        var createResponse = await _client.PostAsJsonAsync("/api/exercises", createRequest);
        var createdExercise = await createResponse.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        
        // Update with modified coach notes
        var updateRequest = UpdateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Exercise With Notes")
            .WithDescription("Exercise with existing notes")
            .Build();
        
        // Manually set coach notes since we need a mix of existing and new notes
        updateRequest.CoachNotes = new List<CoachNoteRequest>
        {
            new() { Id = createdExercise.CoachNotes[0].Id, Text = "Modified first step", Order = 1 },
            new() { Text = "New second step", Order = 2 }, // New note without ID
            new() { Text = "New third step", Order = 3 }   // Another new note
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
        var createRequest = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Multi-Type Exercise")
            .WithDescription("Exercise with multiple types")
            .WithExerciseTypes(
                "exercisetype-a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d", // Warmup
                "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e"  // Workout
            )
            .Build();
        
        var createResponse = await _client.PostAsJsonAsync("/api/exercises", createRequest);
        var createdExercise = await createResponse.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        
        // Update with different exercise types
        var updateRequest = UpdateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Multi-Type Exercise")
            .WithDescription("Exercise with updated types")
            .WithExerciseTypes(
                "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e", // Workout
                "exercisetype-c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f"  // Cooldown
            )
            .Build();
        
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
        var createRequest = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Normal Exercise")
            .WithDescription("Exercise to test Rest validation")
            .Build();
        
        var createResponse = await _client.PostAsJsonAsync("/api/exercises", createRequest);
        var createdExercise = await createResponse.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        
        // Try to update with Rest and other types
        var updateRequest = UpdateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Normal Exercise")
            .WithDescription("Exercise to test Rest validation")
            .WithExerciseTypes(
                "exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a", // Rest (with "rest" in ID)
                "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e"  // Workout
            )
            .WithKineticChainId(null) // REST exercises should have null KineticChainId
            .Build();
        
        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/api/exercises/{createdExercise.Id}", updateRequest);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);
        
        var content = await updateResponse.Content.ReadAsStringAsync();
        Assert.Contains("Rest", content);
    }
}
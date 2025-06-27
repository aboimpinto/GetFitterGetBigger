using System.Net;
using System.Net.Http.Json;
using GetFitterGetBigger.API.DTOs;
using Xunit;

namespace GetFitterGetBigger.API.Tests.IntegrationTests;

/// <summary>
/// Integration tests for CoachNotes synchronization scenarios
/// </summary>
public class ExerciseCoachNotesSyncTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;
    private readonly HttpClient _client;
    
    public ExerciseCoachNotesSyncTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
    }
    
    [Fact]
    public async Task CreateExercise_WithOrderedCoachNotes_ReturnsNotesInCorrectOrder()
    {
        // Arrange
        var request = new CreateExerciseRequest
        {
            Name = "Coach Notes Order Test",
            Description = "Testing coach notes ordering",
            CoachNotes = new List<CoachNoteRequest>
            {
                new() { Text = "Step 3", Order = 3 },
                new() { Text = "Step 1", Order = 1 },
                new() { Text = "Step 2", Order = 2 }
            },
            ExerciseTypeIds = new List<string> { "exercisetype-22334455-6677-8899-aabb-ccddeeff0011" },
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
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdExercise = await response.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        Assert.Equal(3, createdExercise.CoachNotes.Count);
        
        // Verify ordering
        for (int i = 0; i < createdExercise.CoachNotes.Count; i++)
        {
            Assert.Equal($"Step {i + 1}", createdExercise.CoachNotes[i].Text);
            Assert.Equal(i + 1, createdExercise.CoachNotes[i].Order);
        }
    }
    
    [Fact]
    public async Task CreateExercise_WithDuplicateCoachNoteOrders_HandlesGracefully()
    {
        // Arrange
        var request = new CreateExerciseRequest
        {
            Name = "Duplicate Order Test",
            Description = "Testing duplicate coach note orders",
            CoachNotes = new List<CoachNoteRequest>
            {
                new() { Text = "First with order 1", Order = 1 },
                new() { Text = "Second with order 1", Order = 1 },
                new() { Text = "Third with order 2", Order = 2 }
            },
            ExerciseTypeIds = new List<string> { "exercisetype-22334455-6677-8899-aabb-ccddeeff0011" },
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
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdExercise = await response.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        Assert.Equal(3, createdExercise.CoachNotes.Count);
    }
    
    [Fact]
    public async Task GetExerciseById_ReturnsCoachNotesInOrder()
    {
        // Arrange - Create an exercise with coach notes
        var createRequest = new CreateExerciseRequest
        {
            Name = "Get By Id Test",
            Description = "Testing get by id with coach notes",
            CoachNotes = new List<CoachNoteRequest>
            {
                new() { Text = "Last step", Order = 99 },
                new() { Text = "First step", Order = 1 },
                new() { Text = "Middle step", Order = 50 }
            },
            ExerciseTypeIds = new List<string> { "exercisetype-22334455-6677-8899-aabb-ccddeeff0011" },
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
        
        var createResponse = await _client.PostAsJsonAsync("/api/exercises", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        
        var createdExercise = await createResponse.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        
        // Act - Get the exercise by ID
        var getResponse = await _client.GetAsync($"/api/exercises/{createdExercise.Id}");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var retrievedExercise = await getResponse.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(retrievedExercise);
        Assert.Equal(3, retrievedExercise.CoachNotes.Count);
        
        // Verify ordering
        Assert.Equal("First step", retrievedExercise.CoachNotes[0].Text);
        Assert.Equal(1, retrievedExercise.CoachNotes[0].Order);
        Assert.Equal("Middle step", retrievedExercise.CoachNotes[1].Text);
        Assert.Equal(50, retrievedExercise.CoachNotes[1].Order);
        Assert.Equal("Last step", retrievedExercise.CoachNotes[2].Text);
        Assert.Equal(99, retrievedExercise.CoachNotes[2].Order);
    }
    
    [Fact]
    public async Task GetExercisesList_ReturnsExercisesWithCoachNotes()
    {
        // Arrange - Create two exercises with coach notes
        var exercise1 = new CreateExerciseRequest
        {
            Name = "List Test Exercise 1",
            Description = "First exercise for list test",
            CoachNotes = new List<CoachNoteRequest>
            {
                new() { Text = "Exercise 1 Note 1", Order = 1 },
                new() { Text = "Exercise 1 Note 2", Order = 2 }
            },
            ExerciseTypeIds = new List<string> { "exercisetype-22334455-6677-8899-aabb-ccddeeff0011" },
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
        
        var exercise2 = new CreateExerciseRequest
        {
            Name = "List Test Exercise 2",
            Description = "Second exercise for list test",
            CoachNotes = new List<CoachNoteRequest>
            {
                new() { Text = "Exercise 2 Note 1", Order = 1 }
            },
            ExerciseTypeIds = new List<string> { "exercisetype-22334455-6677-8899-aabb-ccddeeff0011" },
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
        
        var response1 = await _client.PostAsJsonAsync("/api/exercises", exercise1);
        var response2 = await _client.PostAsJsonAsync("/api/exercises", exercise2);
        
        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
        Assert.Equal(HttpStatusCode.Created, response2.StatusCode);
        
        // Act - Get the list of exercises
        var listResponse = await _client.GetAsync("/api/exercises?pageSize=50");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        
        var pagedResponse = await listResponse.Content.ReadFromJsonAsync<PagedResponse<ExerciseDto>>();
        Assert.NotNull(pagedResponse);
        Assert.NotEmpty(pagedResponse.Items);
        
        // Find our test exercises
        var testExercise1 = pagedResponse.Items.FirstOrDefault(e => e.Name == "List Test Exercise 1");
        var testExercise2 = pagedResponse.Items.FirstOrDefault(e => e.Name == "List Test Exercise 2");
        
        Assert.NotNull(testExercise1);
        Assert.NotNull(testExercise2);
        
        // Verify coach notes are included and ordered
        Assert.Equal(2, testExercise1.CoachNotes.Count);
        Assert.Equal("Exercise 1 Note 1", testExercise1.CoachNotes[0].Text);
        Assert.Equal("Exercise 1 Note 2", testExercise1.CoachNotes[1].Text);
        
        Assert.Single(testExercise2.CoachNotes);
        Assert.Equal("Exercise 2 Note 1", testExercise2.CoachNotes[0].Text);
    }
}
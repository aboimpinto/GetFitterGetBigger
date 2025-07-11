using System.Net;
using System.Net.Http.Json;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Tests.TestBuilders;
using Xunit;

namespace GetFitterGetBigger.API.Tests.IntegrationTests;

/// <summary>
/// Integration tests for CoachNotes synchronization scenarios
/// </summary>
[Collection("SharedDatabase")]
public class ExerciseCoachNotesSyncTests : IClassFixture<SharedDatabaseTestFixture>
{
    private readonly SharedDatabaseTestFixture _fixture;
    private readonly HttpClient _client;
    
    public ExerciseCoachNotesSyncTests(SharedDatabaseTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
    }
    
    [Fact]
    public async Task CreateExercise_WithOrderedCoachNotes_ReturnsNotesInCorrectOrder()
    {
        // Arrange
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Coach Notes Order Test")
            .WithDescription("Testing coach notes ordering")
            .WithMuscleGroups(("musclegroup-ccddeeff-0011-2233-4455-667788990011", "musclerole-abcdef12-3456-7890-abcd-ef1234567890"))
            .WithCoachNotes(
                ("Step 3", 3),
                ("Step 1", 1),
                ("Step 2", 2)
            )
            .Build();
        
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
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Duplicate Order Test")
            .WithDescription("Testing duplicate coach note orders")
            .WithMuscleGroups(("musclegroup-ccddeeff-0011-2233-4455-667788990011", "musclerole-abcdef12-3456-7890-abcd-ef1234567890"))
            .WithCoachNotes(
                ("First with order 1", 1),
                ("Second with order 1", 1),
                ("Third with order 2", 2)
            )
            .Build();
        
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
        var createRequest = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Get By Id Test")
            .WithDescription("Testing get by id with coach notes")
            .WithMuscleGroups(("musclegroup-ccddeeff-0011-2233-4455-667788990011", "musclerole-abcdef12-3456-7890-abcd-ef1234567890"))
            .WithCoachNotes(
                ("Last step", 99),
                ("First step", 1),
                ("Middle step", 50)
            )
            .Build();
        
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
        var exercise1 = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("List Test Exercise 1")
            .WithDescription("First exercise for list test")
            .WithMuscleGroups(("musclegroup-ccddeeff-0011-2233-4455-667788990011", "musclerole-abcdef12-3456-7890-abcd-ef1234567890"))
            .WithCoachNotes(
                ("Exercise 1 Note 1", 1),
                ("Exercise 1 Note 2", 2)
            )
            .Build();
        
        var exercise2 = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("List Test Exercise 2")
            .WithDescription("Second exercise for list test")
            .WithMuscleGroups(("musclegroup-ccddeeff-0011-2233-4455-667788990011", "musclerole-abcdef12-3456-7890-abcd-ef1234567890"))
            .WithCoachNotes(
                ("Exercise 2 Note 1", 1)
            )
            .Build();
        
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
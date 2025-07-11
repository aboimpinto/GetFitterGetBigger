using System.Net;
using System.Net.Http.Json;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Tests.TestBuilders;
using Xunit;

namespace GetFitterGetBigger.API.Tests.IntegrationTests;

/// <summary>
/// Integration tests for the Rest type exclusivity business rule
/// </summary>
[Collection("SharedDatabase")]
public class ExerciseRestExclusivityTests : IClassFixture<SharedDatabaseTestFixture>
{
    private readonly SharedDatabaseTestFixture _fixture;
    private readonly HttpClient _client;
    
    public ExerciseRestExclusivityTests(SharedDatabaseTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
    }
    
    [Fact]
    public async Task CreateExercise_WithOnlyRestType_CreatesSuccessfully()
    {
        // Arrange
        var request = CreateExerciseRequestBuilder.ForRestExercise()
            .WithName("Valid Rest Period")
            .WithDescription("A rest period between exercises")
            .WithCoachNotes(
                ("Take a 90 second break", 1),
                ("Hydrate during this time", 2)
            )
            .Build();
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdExercise = await response.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        Assert.Single(createdExercise.ExerciseTypes);
    }
    
    [Fact]
    public async Task CreateExercise_WithRestAndWarmupTypes_ReturnsBadRequest()
    {
        // Arrange
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Invalid Rest with Warmup")
            .WithDescription("Trying to combine Rest with Warmup")
            .WithExerciseTypes(
                "exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a", // ID containing "rest"
                "exercisetype-a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d" // Warmup
            )
            .WithKineticChainId(null) // REST exercises should have null KineticChainId
            .Build();
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Rest", content);
        Assert.Contains("cannot be combined", content);
    }
    
    [Fact]
    public async Task CreateExercise_WithRestAndWorkoutTypes_ReturnsBadRequest()
    {
        // Arrange
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Invalid Rest with Workout")
            .WithDescription("Trying to combine Rest with Workout")
            .WithExerciseTypes(
                "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e", // Workout
                "exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a" // ID containing "rest"
            )
            .WithKineticChainId(null) // REST exercises should have null KineticChainId
            .Build();
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task CreateExercise_WithRestAndAllOtherTypes_ReturnsBadRequest()
    {
        // Arrange
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Invalid Rest with All Types")
            .WithDescription("Trying to combine Rest with all other types")
            .WithExerciseTypes(
                "exercisetype-a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d", // Warmup
                "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e", // Workout
                "exercisetype-c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f", // Cooldown
                "exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a" // Rest (with "rest" in ID)
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
    public async Task CreateExercise_WithoutRestType_AllowsMultipleTypes()
    {
        // Arrange
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Multiple Types Without Rest")
            .WithDescription("Exercise with multiple non-Rest types")
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
    }
}
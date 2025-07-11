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
            .WithMuscleGroups((SeedDataBuilder.StandardIds.MuscleGroupIds.Chest, SeedDataBuilder.StandardIds.MuscleRoleIds.Primary))
            .WithExerciseTypes(
                SeedDataBuilder.StandardIds.ExerciseTypeIds.Rest,
                SeedDataBuilder.StandardIds.ExerciseTypeIds.Warmup
            )
            .WithKineticChainId(null) // REST exercises should have null KineticChainId
            .Build();
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("REST", content);
        Assert.Contains("cannot be combined", content);
    }
    
    [Fact]
    public async Task CreateExercise_WithRestAndWorkoutTypes_ReturnsBadRequest()
    {
        // Arrange
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Invalid Rest with Workout")
            .WithDescription("Trying to combine Rest with Workout")
            .WithMuscleGroups((SeedDataBuilder.StandardIds.MuscleGroupIds.Chest, SeedDataBuilder.StandardIds.MuscleRoleIds.Primary))
            .WithExerciseTypes(
                SeedDataBuilder.StandardIds.ExerciseTypeIds.Workout,
                SeedDataBuilder.StandardIds.ExerciseTypeIds.Rest
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
            .WithMuscleGroups((SeedDataBuilder.StandardIds.MuscleGroupIds.Chest, SeedDataBuilder.StandardIds.MuscleRoleIds.Primary))
            .WithExerciseTypes(
                SeedDataBuilder.StandardIds.ExerciseTypeIds.Warmup,
                SeedDataBuilder.StandardIds.ExerciseTypeIds.Workout,
                SeedDataBuilder.StandardIds.ExerciseTypeIds.Cooldown,
                SeedDataBuilder.StandardIds.ExerciseTypeIds.Rest
            )
            .WithKineticChainId(null) // REST exercises should have null KineticChainId
            .Build();
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("REST", content);
    }
    
    [Fact]
    public async Task CreateExercise_WithoutRestType_AllowsMultipleTypes()
    {
        // Arrange
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Multiple Types Without Rest")
            .WithDescription("Exercise with multiple non-Rest types")
            .WithMuscleGroups((SeedDataBuilder.StandardIds.MuscleGroupIds.Chest, SeedDataBuilder.StandardIds.MuscleRoleIds.Primary))
            .WithExerciseTypes(
                SeedDataBuilder.StandardIds.ExerciseTypeIds.Warmup,
                SeedDataBuilder.StandardIds.ExerciseTypeIds.Workout,
                SeedDataBuilder.StandardIds.ExerciseTypeIds.Cooldown
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
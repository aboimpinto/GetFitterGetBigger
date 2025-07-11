using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Tests.TestBuilders;
using Xunit;

namespace GetFitterGetBigger.API.Tests.IntegrationTests;

[Collection("SharedDatabase")]
public class ExerciseRestMuscleGroupValidationTests : IClassFixture<SharedDatabaseTestFixture>
{
    private readonly SharedDatabaseTestFixture _fixture;
    private readonly HttpClient _client;
    
    public ExerciseRestMuscleGroupValidationTests(SharedDatabaseTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
    }
    
    [Fact]
    public async Task CreateExercise_RestExerciseWithoutMuscleGroups_ReturnsCreated()
    {
        // Arrange
        var request = CreateExerciseRequestBuilder.ForRestExercise()
            .WithName($"Rest Period {Guid.NewGuid()}")
            .WithDescription("Recovery time between sets")
            .WithMuscleGroups() // Empty muscle groups
            .Build();
        
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Expected Created but got {response.StatusCode}. Error: {errorContent}");
        }
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdExercise = await response.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        Assert.Contains("Rest Period", createdExercise.Name);
        Assert.Empty(createdExercise.MuscleGroups);
    }
    
    [Fact]
    public async Task CreateExercise_NonRestExerciseWithoutMuscleGroups_ReturnsBadRequest()
    {
        // Arrange
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName($"Push Up {Guid.NewGuid()}")
            .WithDescription("Upper body exercise")
            .WithMuscleGroups() // Empty muscle groups - should fail
            .Build();
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var problemDetails = await response.Content.ReadAsStringAsync();
        Assert.Contains(ExerciseErrorMessages.NonRestExerciseMustHaveMuscleGroups, problemDetails);
    }
    
    [Fact]
    public async Task CreateExercise_RestExerciseWithMuscleGroups_ReturnsCreated()
    {
        // Arrange - REST exercises CAN have muscle groups, they're just not required
        var request = CreateExerciseRequestBuilder.ForRestExercise()
            .WithName($"Active Rest {Guid.NewGuid()}")
            .WithDescription("Light movement during rest")
            .WithMuscleGroups(
                (SeedDataBuilder.StandardIds.MuscleGroupIds.Quadriceps, SeedDataBuilder.StandardIds.MuscleRoleIds.Primary)
            )
            .Build();
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdExercise = await response.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        Assert.Contains("Active Rest", createdExercise.Name);
        Assert.Single(createdExercise.MuscleGroups);
    }
    
    [Fact]
    public async Task CreateExercise_NonRestExerciseWithMuscleGroups_ReturnsCreated()
    {
        // Arrange - Non-REST exercises with muscle groups should work
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName($"Bench Press {Guid.NewGuid()}")
            .WithDescription("Chest exercise")
            .WithMuscleGroups(
                (SeedDataBuilder.StandardIds.MuscleGroupIds.Chest, SeedDataBuilder.StandardIds.MuscleRoleIds.Primary)
            )
            .Build();
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdExercise = await response.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        Assert.Contains("Bench Press", createdExercise.Name);
        Assert.Single(createdExercise.MuscleGroups);
    }
}
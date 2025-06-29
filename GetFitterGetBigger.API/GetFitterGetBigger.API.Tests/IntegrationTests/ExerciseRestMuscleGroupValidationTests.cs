using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
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
        var request = new CreateExerciseRequest
        {
            Name = $"Rest Period {Guid.NewGuid()}",
            Description = "Recovery time between sets",
            DifficultyId = "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b", // Beginner
            ExerciseTypeIds = new List<string> 
            { 
                "exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a" // REST
            },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>(), // Empty muscle groups
            EquipmentIds = new List<string>(),
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>()
        };
        
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
        var request = new CreateExerciseRequest
        {
            Name = $"Push Up {Guid.NewGuid()}",
            Description = "Upper body exercise",
            DifficultyId = "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b", // Beginner
            ExerciseTypeIds = new List<string> 
            { 
                "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e" // Workout
            },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>(), // Empty muscle groups - should fail
            EquipmentIds = new List<string>(),
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>()
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var problemDetails = await response.Content.ReadAsStringAsync();
        Assert.Contains("At least one muscle group must be specified for non-REST exercises", problemDetails);
    }
    
    [Fact]
    public async Task CreateExercise_RestExerciseWithMuscleGroups_ReturnsCreated()
    {
        // Arrange - REST exercises CAN have muscle groups, they're just not required
        var request = new CreateExerciseRequest
        {
            Name = $"Active Rest {Guid.NewGuid()}",
            Description = "Light movement during rest",
            DifficultyId = "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b", // Beginner
            ExerciseTypeIds = new List<string> 
            { 
                "exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a" // REST
            },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>
            {
                new() 
                { 
                    MuscleGroupId = "musclegroup-eeff0011-2233-4455-6677-889900112233", // Quadriceps  
                    MuscleRoleId = "musclerole-abcdef12-3456-7890-abcd-ef1234567890" // Primary
                }
            },
            EquipmentIds = new List<string>(),
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>()
        };
        
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
        var request = new CreateExerciseRequest
        {
            Name = $"Bench Press {Guid.NewGuid()}",
            Description = "Chest exercise",
            DifficultyId = "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b", // Beginner
            ExerciseTypeIds = new List<string> 
            { 
                "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e" // Workout
            },
            MuscleGroups = new List<MuscleGroupWithRoleRequest>
            {
                new() 
                { 
                    MuscleGroupId = "musclegroup-eeff0011-2233-4455-6677-889900112233", // Quadriceps  
                    MuscleRoleId = "musclerole-abcdef12-3456-7890-abcd-ef1234567890" // Primary
                }
            },
            EquipmentIds = new List<string>(),
            MovementPatternIds = new List<string>(),
            BodyPartIds = new List<string>()
        };
        
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
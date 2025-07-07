using System.Net;
using System.Net.Http.Json;
using GetFitterGetBigger.API.DTOs;
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
        var request = new CreateExerciseRequest
        {
            Name = "Valid Rest Period",
            Description = "A rest period between exercises",
            CoachNotes = new List<CoachNoteRequest>
            {
                new() { Text = "Take a 90 second break", Order = 1 },
                new() { Text = "Hydrate during this time", Order = 2 }
            },
            ExerciseTypeIds = new List<string>
            {
                "exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a" // ID containing "rest"
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
    }
    
    [Fact]
    public async Task CreateExercise_WithRestAndWarmupTypes_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateExerciseRequest
        {
            Name = "Invalid Rest with Warmup",
            Description = "Trying to combine Rest with Warmup",
            CoachNotes = new List<CoachNoteRequest>(),
            ExerciseTypeIds = new List<string>
            {
                "exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a", // ID containing "rest"
                "exercisetype-a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d" // Warmup
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
        Assert.Contains("cannot be combined", content);
    }
    
    [Fact]
    public async Task CreateExercise_WithRestAndWorkoutTypes_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateExerciseRequest
        {
            Name = "Invalid Rest with Workout",
            Description = "Trying to combine Rest with Workout",
            CoachNotes = new List<CoachNoteRequest>(),
            ExerciseTypeIds = new List<string>
            {
                "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e", // Workout
                "exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a" // ID containing "rest"
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
    }
    
    [Fact]
    public async Task CreateExercise_WithRestAndAllOtherTypes_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateExerciseRequest
        {
            Name = "Invalid Rest with All Types",
            Description = "Trying to combine Rest with all other types",
            CoachNotes = new List<CoachNoteRequest>(),
            ExerciseTypeIds = new List<string>
            {
                "exercisetype-a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d", // Warmup
                "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e", // Workout
                "exercisetype-c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f", // Cooldown
                "exercisetype-d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a" // Rest (with "rest" in ID)
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
    public async Task CreateExercise_WithoutRestType_AllowsMultipleTypes()
    {
        // Arrange
        var request = new CreateExerciseRequest
        {
            Name = "Multiple Types Without Rest",
            Description = "Exercise with multiple non-Rest types",
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
    }
}
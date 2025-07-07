using System.Net;
using System.Net.Http.Json;
using GetFitterGetBigger.API.DTOs;
using Xunit;

namespace GetFitterGetBigger.API.Tests.IntegrationTests;

/// <summary>
/// Integration tests for ExerciseTypes assignment functionality
/// </summary>
[Collection("SharedDatabase")]
public class ExerciseTypesAssignmentTests : IClassFixture<SharedDatabaseTestFixture>
{
    private readonly SharedDatabaseTestFixture _fixture;
    private readonly HttpClient _client;
    
    public ExerciseTypesAssignmentTests(SharedDatabaseTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
    }
    
    [Fact]
    public async Task CreateExercise_WithSingleExerciseType_AssignsTypeCorrectly()
    {
        // Arrange
        var request = new CreateExerciseRequest
        {
            Name = "Single Type Exercise",
            Description = "Exercise with single type",
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
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdExercise = await response.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        Assert.Single(createdExercise.ExerciseTypes);
        Assert.Equal("exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e", createdExercise.ExerciseTypes[0].Id);
        Assert.Equal("Workout", createdExercise.ExerciseTypes[0].Value);
    }
    
    [Fact]
    public async Task CreateExercise_WithMultipleExerciseTypes_AssignsAllTypesCorrectly()
    {
        // Arrange
        var request = new CreateExerciseRequest
        {
            Name = "Multi Type Exercise",
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
        
        var typeValues = createdExercise.ExerciseTypes.Select(et => et.Value).OrderBy(v => v).ToList();
        Assert.Contains("Warmup", typeValues);
        Assert.Contains("Workout", typeValues);
        Assert.Contains("Cooldown", typeValues);
    }
    
    [Fact]
    public async Task CreateExercise_WithNoExerciseTypes_CreatesExerciseWithEmptyTypes()
    {
        // Arrange
        var request = new CreateExerciseRequest
        {
            Name = "No Type Exercise",
            Description = "Exercise without types",
            CoachNotes = new List<CoachNoteRequest>(),
            ExerciseTypeIds = new List<string>(), // Empty
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
        Assert.Empty(createdExercise.ExerciseTypes);
    }
    
    [Fact]
    public async Task CreateExercise_WithInvalidExerciseTypeId_IgnoresInvalidId()
    {
        // Arrange
        var request = new CreateExerciseRequest
        {
            Name = "Invalid Type Exercise",
            Description = "Exercise with invalid type ID",
            CoachNotes = new List<CoachNoteRequest>(),
            ExerciseTypeIds = new List<string>
            {
                "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e", // Valid
                "invalid-type-id-format", // Invalid format
                "exercisetype-99999999-9999-9999-9999-999999999999" // Valid format but non-existent
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
        // Should only have the valid exercise type
        Assert.Single(createdExercise.ExerciseTypes);
        Assert.Equal("exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e", createdExercise.ExerciseTypes[0].Id);
    }
    
    [Fact]
    public async Task CreateExercise_WithDuplicateExerciseTypeIds_DeduplicatesTypes()
    {
        // Arrange
        var request = new CreateExerciseRequest
        {
            Name = "Duplicate Type Exercise",
            Description = "Exercise with duplicate type IDs",
            CoachNotes = new List<CoachNoteRequest>(),
            ExerciseTypeIds = new List<string>
            {
                "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e", // Workout
                "exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e", // Duplicate
                "exercisetype-a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d"  // Warmup
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
        // Should only have 2 unique types
        Assert.Equal(2, createdExercise.ExerciseTypes.Count);
        
        var typeIds = createdExercise.ExerciseTypes.Select(et => et.Id).OrderBy(id => id).ToList();
        Assert.Equal(typeIds.Count, typeIds.Distinct().Count()); // All IDs should be unique
    }
}
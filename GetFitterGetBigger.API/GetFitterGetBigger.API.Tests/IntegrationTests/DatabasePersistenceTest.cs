using System.Net;
using System.Net.Http.Json;
using GetFitterGetBigger.API.DTOs;
using Xunit;

namespace GetFitterGetBigger.API.Tests.IntegrationTests;

[Collection("SharedDatabase")]
public class DatabasePersistenceTest : IClassFixture<SharedDatabaseTestFixture>
{
    private readonly SharedDatabaseTestFixture _fixture;
    private readonly HttpClient _client;
    
    public DatabasePersistenceTest(SharedDatabaseTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
    }
    
    [Fact]
    public async Task CreateAndRetrieveExercise_WithSharedDatabase_PersistsAcrossRequests()
    {
        // Arrange - Create an exercise
        var createRequest = new CreateExerciseRequest
        {
            Name = $"Persistence Test Exercise {Guid.NewGuid()}",
            Description = "Testing database persistence",
            CoachNotes = new List<CoachNoteRequest>
            {
                new() { Text = "Test Note", Order = 1 }
            },
            ExerciseTypeIds = new List<string> { "exercisetype-22334455-6677-8899-aabb-ccddeeff0011" },
            DifficultyId = "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
            KineticChainId = "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4", // Compound
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
        
        // Act 1 - Create the exercise
        var createResponse = await _client.PostAsJsonAsync("/api/exercises", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        
        var createdExercise = await createResponse.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        Assert.NotNull(createdExercise.Id);
        
        // Act 2 - Retrieve the exercise in a separate request
        var getResponse = await _client.GetAsync($"/api/exercises/{createdExercise.Id}");
        
        // Assert - The exercise should be found
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var retrievedExercise = await getResponse.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(retrievedExercise);
        Assert.Equal(createdExercise.Id, retrievedExercise.Id);
        Assert.Equal(createdExercise.Name, retrievedExercise.Name);
    }
}
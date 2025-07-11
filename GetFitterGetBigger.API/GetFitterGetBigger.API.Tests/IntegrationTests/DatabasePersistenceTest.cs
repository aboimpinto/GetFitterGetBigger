using System.Net;
using System.Net.Http.Json;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Tests.TestBuilders;
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
        var createRequest = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName($"Persistence Test Exercise {Guid.NewGuid()}")
            .WithDescription("Testing database persistence")
            .WithMuscleGroups(("musclegroup-ccddeeff-0011-2233-4455-667788990011", "musclerole-abcdef12-3456-7890-abcd-ef1234567890"))
            .WithCoachNotes(("Test Note", 1))
            .Build();
        
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
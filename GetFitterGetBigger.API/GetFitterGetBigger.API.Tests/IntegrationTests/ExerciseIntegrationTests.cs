using System.Net;
using System.Net.Http.Json;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;
using Xunit;

namespace GetFitterGetBigger.API.Tests.IntegrationTests;

[Collection("SharedDatabase")]
public class ExerciseIntegrationTests : IClassFixture<SharedDatabaseTestFixture>
{
    private readonly SharedDatabaseTestFixture _fixture;
    private readonly HttpClient _client;
    
    public ExerciseIntegrationTests(SharedDatabaseTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
    }
    
    [Fact]
    public async Task CreateExercise_WithCoachNotes_ReturnsCreatedExerciseWithOrderedNotes()
    {
        // Arrange
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Integration Test Squat")
            .WithDescription("Test squat exercise with coach notes")
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
            .AddMuscleGroup(MuscleGroupTestBuilder.Quadriceps(), MuscleRoleTestBuilder.Primary())
            .WithCoachNotes(
                ("Warm up properly first", 1),
                ("Keep your back straight", 2),
                ("Control the descent", 3)
            )
            .WithVideoUrl("https://example.com/squat.mp4")
            .WithImageUrl("https://example.com/squat.jpg")
            .WithIsUnilateral(false)
            .Build();
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdExercise = await response.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        Assert.Equal("Integration Test Squat", createdExercise.Name);
        
        // Check coach notes are ordered correctly
        Assert.Equal(3, createdExercise.CoachNotes.Count);
        Assert.Equal("Warm up properly first", createdExercise.CoachNotes[0].Text);
        Assert.Equal(1, createdExercise.CoachNotes[0].Order);
        Assert.Equal("Keep your back straight", createdExercise.CoachNotes[1].Text);
        Assert.Equal(2, createdExercise.CoachNotes[1].Order);
        Assert.Equal("Control the descent", createdExercise.CoachNotes[2].Text);
        Assert.Equal(3, createdExercise.CoachNotes[2].Order);
        
        // Check exercise types
        Assert.Single(createdExercise.ExerciseTypes);
        Assert.Equal(SeedDataBuilder.StandardIds.ExerciseTypeIds.Workout, createdExercise.ExerciseTypes[0].Id);
    }
    
    [Fact]
    public async Task CreateExercise_WithMultipleExerciseTypes_ReturnsCreatedExerciseWithAllTypes()
    {
        // Arrange
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Integration Test Complex Exercise")
            .WithDescription("Exercise with multiple types")
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
            .WithExerciseTypes(new[]
            {
                ExerciseTypeTestBuilder.Warmup().Build(),
                ExerciseTypeTestBuilder.Workout().Build(),
                ExerciseTypeTestBuilder.Cooldown().Build()
            })
            .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
            .Build();
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdExercise = await response.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        Assert.Equal(3, createdExercise.ExerciseTypes.Count);
        
        var typeIds = createdExercise.ExerciseTypes.Select(et => et.Id).OrderBy(id => id).ToList();
        Assert.Contains(SeedDataBuilder.StandardIds.ExerciseTypeIds.Warmup, typeIds);
        Assert.Contains(SeedDataBuilder.StandardIds.ExerciseTypeIds.Workout, typeIds);
        Assert.Contains(SeedDataBuilder.StandardIds.ExerciseTypeIds.Cooldown, typeIds);
    }
    
    [Fact]
    public async Task CreateExercise_WithRestTypeAndOtherTypes_ReturnsBadRequest()
    {
        // Arrange
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Integration Test Rest Exercise")
            .WithDescription("Invalid exercise with Rest and other types")
            .WithKineticChainId(null) // REST exercises shouldn't have kinetic chain
            .WithExerciseWeightTypeId(null) // REST exercises shouldn't have weight type
            .WithExerciseTypes(new[]
            {
                ExerciseTypeTestBuilder.Rest().Build(),
                ExerciseTypeTestBuilder.Workout().Build()
            })
            .Build();
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("REST", content);
    }
    
    [Fact]
    public async Task CreateExercise_WithOnlyRestType_ReturnsCreatedExercise()
    {
        // Arrange
        var request = CreateExerciseRequestBuilder.ForRestExercise()
            .WithName("Integration Test Rest Period")
            .WithDescription("Valid rest exercise")
            .Build();
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdExercise = await response.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        Assert.Single(createdExercise.ExerciseTypes);
        Assert.Equal(SeedDataBuilder.StandardIds.ExerciseTypeIds.Rest, createdExercise.ExerciseTypes[0].Id);
    }
    
    [Fact]
    public async Task CreateExercise_WithEmptyCoachNotes_ReturnsCreatedExerciseWithNoNotes()
    {
        // Arrange
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Integration Test No Notes Exercise")
            .WithDescription("Exercise without coach notes")
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
            .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
            .Build();
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var createdExercise = await response.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        Assert.Empty(createdExercise.CoachNotes);
    }
    
    [Fact]
    public async Task UpdateExercise_AddCoachNotes_UpdatesExerciseWithNewNotes()
    {
        // Arrange - Create an exercise without coach notes
        var createRequest = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Update Test Exercise")
            .WithDescription("Exercise to test updates")
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
            .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
            .Build();
        
        var createResponse = await _client.PostAsJsonAsync("/api/exercises", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        
        var createdExercise = await createResponse.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        Assert.NotNull(createdExercise.Id);
        
        // Update with coach notes
        var updateRequest = UpdateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Updated Test Exercise")
            .WithDescription("Updated description")
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
            .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
            .WithCoachNotes(
                ("First step", 1),
                ("Second step", 2),
                ("Third step", 3)
            )
            .Build();
        
        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/api/exercises/{createdExercise.Id}", updateRequest);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        
        var updatedExercise = await updateResponse.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(updatedExercise);
        Assert.Equal("Updated Test Exercise", updatedExercise.Name);
        Assert.Equal(3, updatedExercise.CoachNotes.Count);
        Assert.Equal("First step", updatedExercise.CoachNotes[0].Text);
        Assert.Equal(1, updatedExercise.CoachNotes[0].Order);
        Assert.Equal("Second step", updatedExercise.CoachNotes[1].Text);
        Assert.Equal(2, updatedExercise.CoachNotes[1].Order);
        Assert.Equal("Third step", updatedExercise.CoachNotes[2].Text);
        Assert.Equal(3, updatedExercise.CoachNotes[2].Order);
    }
    
    [Fact]
    public async Task UpdateExercise_ModifyExistingCoachNotes_UpdatesNotesCorrectly()
    {
        // Arrange - Create an exercise with coach notes
        var createRequest = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Exercise With Notes")
            .WithDescription("Exercise with existing notes")
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
            .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
            .AddCoachNote("Original first step", 1)
            .AddCoachNote("Original second step", 2)
            .Build();
        
        var createResponse = await _client.PostAsJsonAsync("/api/exercises", createRequest);
        var createdExercise = await createResponse.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        
        // Update with modified coach notes
        var updateRequest = UpdateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Exercise With Notes")
            .WithDescription("Exercise with existing notes")
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
            .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
            .AddCoachNote(createdExercise.CoachNotes[0].Id, "Modified first step", 1)
            .AddCoachNote("New second step", 2)
            .AddCoachNote("New third step", 3)
            .Build();
        
        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/api/exercises/{createdExercise.Id}", updateRequest);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        
        var updatedExercise = await updateResponse.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(updatedExercise);
        Assert.Equal(3, updatedExercise.CoachNotes.Count); // Should have 3 notes now
        Assert.Equal("Modified first step", updatedExercise.CoachNotes[0].Text);
        Assert.Equal("New second step", updatedExercise.CoachNotes[1].Text);
        Assert.Equal("New third step", updatedExercise.CoachNotes[2].Text);
    }
    
    [Fact]
    public async Task UpdateExercise_ChangeExerciseTypes_UpdatesTypesCorrectly()
    {
        // Arrange - Create an exercise with multiple types
        var createRequest = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Multi-Type Exercise")
            .WithDescription("Exercise with multiple types")
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
            .WithExerciseTypes(new[]
            {
                ExerciseTypeTestBuilder.Warmup().Build(),
                ExerciseTypeTestBuilder.Workout().Build()
            })
            .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
            .Build();
        
        var createResponse = await _client.PostAsJsonAsync("/api/exercises", createRequest);
        var createdExercise = await createResponse.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        
        // Update with different exercise types
        var updateRequest = UpdateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Multi-Type Exercise")
            .WithDescription("Exercise with updated types")
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
            .WithExerciseTypes(new[]
            {
                ExerciseTypeTestBuilder.Workout().Build(),
                ExerciseTypeTestBuilder.Cooldown().Build()
            })
            .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
            .Build();
        
        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/api/exercises/{createdExercise.Id}", updateRequest);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        
        var updatedExercise = await updateResponse.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(updatedExercise);
        Assert.Equal(2, updatedExercise.ExerciseTypes.Count);
        
        var typeIds = updatedExercise.ExerciseTypes.Select(et => et.Id).OrderBy(id => id).ToList();
        Assert.Contains(SeedDataBuilder.StandardIds.ExerciseTypeIds.Workout, typeIds);
        Assert.Contains(SeedDataBuilder.StandardIds.ExerciseTypeIds.Cooldown, typeIds);
        Assert.DoesNotContain(SeedDataBuilder.StandardIds.ExerciseTypeIds.Warmup, typeIds); // Warmup should be removed
    }
    
    [Fact]
    public async Task UpdateExercise_WithRestTypeAndOtherTypes_ReturnsBadRequest()
    {
        // Arrange - Create a normal exercise
        var createRequest = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Normal Exercise")
            .WithDescription("Exercise to test Rest validation")
            .WithKineticChain(KineticChainTypeTestBuilder.Compound())
            .WithWeightType(ExerciseWeightTypeTestBuilder.Barbell())
            .AddMuscleGroup(MuscleGroupTestBuilder.Chest(), MuscleRoleTestBuilder.Primary())
            .Build();
        
        var createResponse = await _client.PostAsJsonAsync("/api/exercises", createRequest);
        var createdExercise = await createResponse.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(createdExercise);
        
        // Try to update with Rest and other types
        var updateRequest = UpdateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Normal Exercise")
            .WithDescription("Exercise to test Rest validation")
            .WithKineticChainId(null) // REST exercises shouldn't have kinetic chain
            .WithExerciseWeightTypeId(null) // REST exercises shouldn't have weight type
            .WithExerciseTypes(new[]
            {
                ExerciseTypeTestBuilder.Rest().Build(),
                ExerciseTypeTestBuilder.Workout().Build()
            })
            .Build();
        
        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/api/exercises/{createdExercise.Id}", updateRequest);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);
        
        var content = await updateResponse.Content.ReadAsStringAsync();
        Assert.Contains("REST", content);
    }
}
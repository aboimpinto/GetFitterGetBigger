using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GetFitterGetBigger.API.Tests.IntegrationTests;

/// <summary>
/// Integration tests for circular reference prevention in exercise links
/// </summary>
public class ExerciseLinkCircularReferenceTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;
    private readonly HttpClient _client;

    public ExerciseLinkCircularReferenceTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
    }

    [Fact]
    public async Task CreateLink_DirectCircularReference_ShouldFail()
    {
        // Arrange
        await using var scope = _fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        var linkRepo = scope.ServiceProvider.GetRequiredService<IExerciseLinkRepository>();
        
        var workoutType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Workout");
        var warmupType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Warmup");
        
        // Create two exercises that can be both workout and warmup
        var exerciseA = await CreateExerciseWithTypes(context, "Exercise A", workoutType.Id, warmupType.Id);
        var exerciseB = await CreateExerciseWithTypes(context, "Exercise B", workoutType.Id, warmupType.Id);
        
        // Create first link: A -> B
        var firstLink = ExerciseLink.Handler.CreateNew(exerciseA.Id, exerciseB.Id, "Warmup", 1);
        await linkRepo.AddAsync(firstLink);
        await context.SaveChangesAsync();
        
        // Try to create reverse link: B -> A
        var dto = new CreateExerciseLinkDto
        {
            TargetExerciseId = exerciseA.Id.ToString(),
            LinkType = "Warmup",
            DisplayOrder = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/exercises/{exerciseB.Id}/links", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("circular reference", errorContent);
    }

    [Fact]
    public async Task CreateLink_IndirectCircularReference_ShouldFail()
    {
        // Arrange
        await using var scope = _fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        var linkRepo = scope.ServiceProvider.GetRequiredService<IExerciseLinkRepository>();
        
        var workoutType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Workout");
        var warmupType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Warmup");
        
        // Create three exercises that can be both workout and warmup
        var exerciseA = await CreateExerciseWithTypes(context, "Exercise A Chain", workoutType.Id, warmupType.Id);
        var exerciseB = await CreateExerciseWithTypes(context, "Exercise B Chain", workoutType.Id, warmupType.Id);
        var exerciseC = await CreateExerciseWithTypes(context, "Exercise C Chain", workoutType.Id, warmupType.Id);
        
        // Create chain: A -> B -> C
        var linkAB = ExerciseLink.Handler.CreateNew(exerciseA.Id, exerciseB.Id, "Warmup", 1);
        var linkBC = ExerciseLink.Handler.CreateNew(exerciseB.Id, exerciseC.Id, "Warmup", 1);
        await linkRepo.AddAsync(linkAB);
        await linkRepo.AddAsync(linkBC);
        await context.SaveChangesAsync();
        
        // Try to create link that closes the loop: C -> A
        var dto = new CreateExerciseLinkDto
        {
            TargetExerciseId = exerciseA.Id.ToString(),
            LinkType = "Warmup",
            DisplayOrder = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/exercises/{exerciseC.Id}/links", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("circular reference", errorContent);
    }

    [Fact]
    public async Task CreateLink_ComplexCircularReference_ShouldFail()
    {
        // Arrange
        await using var scope = _fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        var linkRepo = scope.ServiceProvider.GetRequiredService<IExerciseLinkRepository>();
        
        var workoutType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Workout");
        var warmupType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Warmup");
        var cooldownType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Cooldown");
        
        // Create exercises with mixed types
        var exerciseA = await CreateExerciseWithTypes(context, "Complex A", workoutType.Id, warmupType.Id);
        var exerciseB = await CreateExerciseWithTypes(context, "Complex B", workoutType.Id, warmupType.Id, cooldownType.Id);
        var exerciseC = await CreateExerciseWithTypes(context, "Complex C", workoutType.Id, cooldownType.Id);
        var exerciseD = await CreateExerciseWithTypes(context, "Complex D", workoutType.Id, warmupType.Id);
        
        // Create complex chain: A -> B (warmup), B -> C (cooldown), C -> D (cooldown)
        var linkAB = ExerciseLink.Handler.CreateNew(exerciseA.Id, exerciseB.Id, "Warmup", 1);
        var linkBC = ExerciseLink.Handler.CreateNew(exerciseB.Id, exerciseC.Id, "Cooldown", 1);
        var linkCD = ExerciseLink.Handler.CreateNew(exerciseC.Id, exerciseD.Id, "Cooldown", 1);
        
        await linkRepo.AddAsync(linkAB);
        await linkRepo.AddAsync(linkBC);
        await linkRepo.AddAsync(linkCD);
        await context.SaveChangesAsync();
        
        // Try to create link that creates a cycle: D -> A
        var dto = new CreateExerciseLinkDto
        {
            TargetExerciseId = exerciseA.Id.ToString(),
            LinkType = "Warmup",
            DisplayOrder = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/exercises/{exerciseD.Id}/links", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("circular reference", errorContent);
    }

    [Fact]
    public async Task CreateLink_NonCircularComplexStructure_ShouldSucceed()
    {
        // Arrange
        await using var scope = _fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        var linkRepo = scope.ServiceProvider.GetRequiredService<IExerciseLinkRepository>();
        
        var workoutType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Workout");
        var warmupType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Warmup");
        var cooldownType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Cooldown");
        
        // Create exercises - tree structure, not circular
        var rootExercise = await CreateExerciseWithTypes(context, "Root Exercise", workoutType.Id);
        var warmup1 = await CreateExerciseWithTypes(context, "Warmup 1", workoutType.Id, warmupType.Id);
        var warmup2 = await CreateExerciseWithTypes(context, "Warmup 2", workoutType.Id, warmupType.Id);
        var cooldown1 = await CreateExerciseWithTypes(context, "Cooldown 1", workoutType.Id, cooldownType.Id);
        
        // Create existing links forming a tree
        var link1 = ExerciseLink.Handler.CreateNew(rootExercise.Id, warmup1.Id, "Warmup", 1);
        var link2 = ExerciseLink.Handler.CreateNew(rootExercise.Id, warmup2.Id, "Warmup", 2);
        await linkRepo.AddAsync(link1);
        await linkRepo.AddAsync(link2);
        await context.SaveChangesAsync();
        
        // Add a cooldown link - this should succeed as it doesn't create a cycle
        var dto = new CreateExerciseLinkDto
        {
            TargetExerciseId = cooldown1.Id.ToString(),
            LinkType = "Cooldown",
            DisplayOrder = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/exercises/{rootExercise.Id}/links", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ExerciseLinkDto>();
        Assert.NotNull(result);
        Assert.Equal(cooldown1.Id.ToString(), result.TargetExerciseId);
    }

    private async Task<Exercise> CreateExerciseWithTypes(FitnessDbContext context, string name, params ExerciseTypeId[] typeIds)
    {
        var exercise = Exercise.Handler.CreateNew(
            name,
            $"Description for {name}",
            null,
            null,
            false,
            DifficultyLevelId.From(Guid.Parse("8a8adb1d-24d2-4979-a5a6-0d760e6da24b")),
            KineticChainTypeId.From(Guid.Parse("12345678-9abc-def0-1234-567890abcdef"))
        );

        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        // Add exercise types
        foreach (var typeId in typeIds)
        {
            var exerciseType = ExerciseExerciseType.Handler.Create(exercise.Id, typeId);
            context.ExerciseExerciseTypes.Add(exerciseType);
        }

        await context.SaveChangesAsync();
        
        // Reload with navigation properties
        return await context.Exercises
            .Include(e => e.ExerciseExerciseTypes)
            .ThenInclude(eet => eet.ExerciseType)
            .FirstAsync(e => e.Id == exercise.Id);
    }
}
using System;
using System.Linq;
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
/// Integration tests for exercise link functionality
/// </summary>
public class ExerciseLinkIntegrationTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;
    private readonly HttpClient _client;

    public ExerciseLinkIntegrationTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
    }

    [Fact]
    public async Task CreateLink_WithValidData_ShouldSucceed()
    {
        // Arrange
        await using var scope = _fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        // Create exercises with proper types
        var workoutType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Workout");
        var warmupType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Warmup");
        
        var sourceExercise = await CreateExerciseWithType(context, "Barbell Squat", workoutType.Id);
        var targetExercise = await CreateExerciseWithType(context, "Air Squat", workoutType.Id, warmupType.Id);
        
        var dto = new CreateExerciseLinkDto
        {
            TargetExerciseId = targetExercise.Id.ToString(),
            LinkType = "Warmup",
            DisplayOrder = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/exercises/{sourceExercise.Id}/links", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<ExerciseLinkDto>();
        Assert.NotNull(result);
        Assert.Equal(dto.TargetExerciseId, result.TargetExerciseId);
        Assert.Equal(dto.LinkType, result.LinkType);
        Assert.Equal(dto.DisplayOrder, result.DisplayOrder);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreateLink_WithNonWorkoutSource_ShouldFail()
    {
        // Arrange
        await using var scope = _fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        // Create exercises - source without Workout type
        var warmupType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Warmup");
        
        var sourceExercise = await CreateExerciseWithType(context, "Warmup Only Exercise", warmupType.Id);
        var targetExercise = await CreateExerciseWithType(context, "Target Warmup", warmupType.Id);
        
        var dto = new CreateExerciseLinkDto
        {
            TargetExerciseId = targetExercise.Id.ToString(),
            LinkType = "Warmup",
            DisplayOrder = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/exercises/{sourceExercise.Id}/links", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("Source exercise must be of type 'Workout'", errorContent);
    }

    [Fact]
    public async Task CreateLink_WithMismatchedTargetType_ShouldFail()
    {
        // Arrange
        await using var scope = _fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        var workoutType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Workout");
        var cooldownType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Cooldown");
        
        var sourceExercise = await CreateExerciseWithType(context, "Source Workout", workoutType.Id);
        var targetExercise = await CreateExerciseWithType(context, "Cooldown Exercise", cooldownType.Id);
        
        var dto = new CreateExerciseLinkDto
        {
            TargetExerciseId = targetExercise.Id.ToString(),
            LinkType = "Warmup", // Mismatch - target is Cooldown
            DisplayOrder = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/exercises/{sourceExercise.Id}/links", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("Target exercise must be of type 'Warmup'", errorContent);
    }

    [Fact]
    public async Task CreateLink_WithRestExercise_ShouldFail()
    {
        // Arrange
        await using var scope = _fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        var workoutType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Workout");
        var restType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Rest");
        
        var sourceExercise = await CreateExerciseWithType(context, "Source Workout", workoutType.Id);
        var targetExercise = await CreateExerciseWithType(context, "Rest Period", restType.Id);
        
        var dto = new CreateExerciseLinkDto
        {
            TargetExerciseId = targetExercise.Id.ToString(),
            LinkType = "Warmup",
            DisplayOrder = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/exercises/{sourceExercise.Id}/links", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("Target exercise must be of type 'Warmup'", errorContent);
    }

    [Fact]
    public async Task GetLinks_WithFilters_ShouldReturnCorrectResults()
    {
        // Arrange
        await using var scope = _fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        var workoutType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Workout");
        var warmupType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Warmup");
        var cooldownType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Cooldown");
        
        var sourceExercise = await CreateExerciseWithType(context, "Get Links Source", workoutType.Id);
        var warmupExercise = await CreateExerciseWithType(context, "Warmup Target", workoutType.Id, warmupType.Id);
        var cooldownExercise = await CreateExerciseWithType(context, "Cooldown Target", workoutType.Id, cooldownType.Id);
        
        // Create links
        var warmupLink = ExerciseLink.Handler.CreateNew(sourceExercise.Id, warmupExercise.Id, "Warmup", 1);
        var cooldownLink = ExerciseLink.Handler.CreateNew(sourceExercise.Id, cooldownExercise.Id, "Cooldown", 1);
        
        context.ExerciseLinks.Add(warmupLink);
        context.ExerciseLinks.Add(cooldownLink);
        await context.SaveChangesAsync();

        // Act - Get all links
        var allResponse = await _client.GetAsync($"/api/exercises/{sourceExercise.Id}/links");
        Assert.Equal(HttpStatusCode.OK, allResponse.StatusCode);
        
        var allResult = await allResponse.Content.ReadFromJsonAsync<ExerciseLinksResponseDto>();
        Assert.NotNull(allResult);
        Assert.Equal(2, allResult.Links.Count);

        // Act - Get warmup links only
        var warmupResponse = await _client.GetAsync($"/api/exercises/{sourceExercise.Id}/links?linkType=Warmup");
        Assert.Equal(HttpStatusCode.OK, warmupResponse.StatusCode);
        
        var warmupResult = await warmupResponse.Content.ReadFromJsonAsync<ExerciseLinksResponseDto>();
        Assert.NotNull(warmupResult);
        Assert.Single(warmupResult.Links);
        Assert.Equal("Warmup", warmupResult.Links.First().LinkType);
    }

    [Fact]
    public async Task UpdateLink_WithValidData_ShouldSucceed()
    {
        // Arrange
        await using var scope = _fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        var workoutType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Workout");
        var warmupType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Warmup");
        
        var sourceExercise = await CreateExerciseWithType(context, "Update Link Source", workoutType.Id);
        var targetExercise = await CreateExerciseWithType(context, "Update Link Target", workoutType.Id, warmupType.Id);
        
        var link = ExerciseLink.Handler.CreateNew(sourceExercise.Id, targetExercise.Id, "Warmup", 1);
        context.ExerciseLinks.Add(link);
        await context.SaveChangesAsync();
        
        var updateDto = new UpdateExerciseLinkDto
        {
            DisplayOrder = 5,
            IsActive = false
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/exercises/{sourceExercise.Id}/links/{link.Id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<ExerciseLinkDto>();
        Assert.NotNull(result);
        Assert.Equal(5, result.DisplayOrder);
        Assert.False(result.IsActive);
    }

    [Fact]
    public async Task DeleteLink_WithValidData_ShouldSucceed()
    {
        // Arrange
        await using var scope = _fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        var workoutType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Workout");
        var warmupType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Warmup");
        
        var sourceExercise = await CreateExerciseWithType(context, "Delete Link Source", workoutType.Id);
        var targetExercise = await CreateExerciseWithType(context, "Delete Link Target", workoutType.Id, warmupType.Id);
        
        var link = ExerciseLink.Handler.CreateNew(sourceExercise.Id, targetExercise.Id, "Warmup", 1);
        context.ExerciseLinks.Add(link);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/exercises/{sourceExercise.Id}/links/{link.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        // Verify soft delete - need to get a fresh context since the API uses its own
        await using var verifyScope = _fixture.Services.CreateAsyncScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        var deletedLink = await verifyContext.ExerciseLinks.FindAsync(link.Id);
        Assert.NotNull(deletedLink);
        Assert.False(deletedLink.IsActive);
    }

    private async Task<Exercise> CreateExerciseWithType(FitnessDbContext context, string name, params ExerciseTypeId[] typeIds)
    {
        var exercise = Exercise.Handler.CreateNew(
            name,
            $"Description for {name}",
            null,
            null,
            false,
            DifficultyLevelId.From(Guid.Parse("8a8adb1d-24d2-4979-a5a6-0d760e6da24b")),
            typeIds.Contains(ExerciseTypeId.From(Guid.Parse("91fc6a9f-70d1-4ae3-906f-3e4e3a5dce30"))) // Rest type check
                ? null
                : KineticChainTypeId.From(Guid.Parse("12345678-9abc-def0-1234-567890abcdef"))
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
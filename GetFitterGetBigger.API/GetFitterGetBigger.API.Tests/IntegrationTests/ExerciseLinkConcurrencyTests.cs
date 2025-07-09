using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GetFitterGetBigger.API.Tests.IntegrationTests;

/// <summary>
/// Integration tests for concurrent operations on exercise links
/// </summary>
public class ExerciseLinkConcurrencyTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;

    public ExerciseLinkConcurrencyTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateLink_ConcurrentDuplicateCreation_OnlyOneSucceeds()
    {
        // Arrange
        await using var scope = _fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        var workoutType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Workout");
        var warmupType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Warmup");
        
        var sourceExercise = await CreateExerciseWithTypes(context, "Concurrent Source", workoutType.Id);
        var targetExercise = await CreateExerciseWithTypes(context, "Concurrent Target", workoutType.Id, warmupType.Id);
        
        var dto = new CreateExerciseLinkDto
        {
            TargetExerciseId = targetExercise.Id.ToString(),
            LinkType = "Warmup",
            DisplayOrder = 1
        };

        // Act - Create multiple clients to simulate concurrent requests
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 5; i++)
        {
            var client = _fixture.CreateClient();
            tasks.Add(client.PostAsJsonAsync($"/api/exercises/{sourceExercise.Id}/links", dto));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert - Only one should succeed with 201, others should fail
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        var failureCount = responses.Count(r => r.StatusCode == HttpStatusCode.BadRequest);
        
        Assert.Equal(1, successCount);
        Assert.Equal(4, failureCount);
        
        // Verify only one link exists in database
        var links = await context.ExerciseLinks
            .Where(el => el.SourceExerciseId == sourceExercise.Id && el.TargetExerciseId == targetExercise.Id)
            .ToListAsync();
        Assert.Single(links);
    }

    [Fact]
    public async Task CreateLink_ConcurrentMaxLinksReached_OnlyTenSucceed()
    {
        // Arrange
        await using var scope = _fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        var workoutType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Workout");
        var warmupType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Warmup");
        
        var sourceExercise = await CreateExerciseWithTypes(context, "Max Links Source", workoutType.Id);
        
        // Create 15 different target exercises
        var targetExercises = new List<Exercise>();
        for (int i = 0; i < 15; i++)
        {
            var target = await CreateExerciseWithTypes(context, $"Target Exercise {i}", workoutType.Id, warmupType.Id);
            targetExercises.Add(target);
        }

        // Act - Try to create 15 concurrent links (max is 10)
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 15; i++)
        {
            var client = _fixture.CreateClient();
            var dto = new CreateExerciseLinkDto
            {
                TargetExerciseId = targetExercises[i].Id.ToString(),
                LinkType = "Warmup",
                DisplayOrder = i + 1
            };
            tasks.Add(client.PostAsJsonAsync($"/api/exercises/{sourceExercise.Id}/links", dto));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert - Only 10 should succeed
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        var failureCount = responses.Count(r => r.StatusCode == HttpStatusCode.BadRequest);
        
        Assert.Equal(10, successCount);
        Assert.Equal(5, failureCount);
        
        // Verify exactly 10 links in database
        var links = await context.ExerciseLinks
            .Where(el => el.SourceExerciseId == sourceExercise.Id && el.LinkType == "Warmup")
            .ToListAsync();
        Assert.Equal(10, links.Count);
    }

    [Fact]
    public async Task UpdateLink_ConcurrentUpdates_LastWriteWins()
    {
        // Arrange
        await using var scope = _fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        var workoutType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Workout");
        var cooldownType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Cooldown");
        
        var sourceExercise = await CreateExerciseWithTypes(context, "Update Race Source", workoutType.Id);
        var targetExercise = await CreateExerciseWithTypes(context, "Update Race Target", workoutType.Id, cooldownType.Id);
        
        // Create initial link
        var link = ExerciseLink.Handler.CreateNew(sourceExercise.Id, targetExercise.Id, "Cooldown", 1);
        context.ExerciseLinks.Add(link);
        await context.SaveChangesAsync();

        // Act - Multiple concurrent updates with different display orders
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 1; i <= 5; i++)
        {
            var client = _fixture.CreateClient();
            var updateDto = new UpdateExerciseLinkDto
            {
                DisplayOrder = i * 10,
                IsActive = i % 2 == 0
            };
            tasks.Add(client.PutAsJsonAsync($"/api/exercises/{sourceExercise.Id}/links/{link.Id}", updateDto));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert - All should succeed
        Assert.All(responses, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));
        
        // Verify final state - should have one of the display orders
        var finalLink = await context.ExerciseLinks.FindAsync(link.Id);
        Assert.NotNull(finalLink);
        Assert.Contains(finalLink.DisplayOrder, new[] { 10, 20, 30, 40, 50 });
    }

    [Fact]
    public async Task DeleteLink_ConcurrentDeletes_OnlyFirstSucceeds()
    {
        // Arrange
        await using var scope = _fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        var workoutType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Workout");
        var cooldownType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Cooldown");
        
        var sourceExercise = await CreateExerciseWithTypes(context, "Delete Race Source", workoutType.Id);
        var targetExercise = await CreateExerciseWithTypes(context, "Delete Race Target", workoutType.Id, cooldownType.Id);
        
        // Create link to delete
        var link = ExerciseLink.Handler.CreateNew(sourceExercise.Id, targetExercise.Id, "Cooldown", 1);
        context.ExerciseLinks.Add(link);
        await context.SaveChangesAsync();

        // Act - Multiple concurrent deletes
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 3; i++)
        {
            var client = _fixture.CreateClient();
            tasks.Add(client.DeleteAsync($"/api/exercises/{sourceExercise.Id}/links/{link.Id}"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert - At least one should succeed, others might get 404
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.NoContent);
        Assert.True(successCount >= 1);
        
        // Verify link is soft deleted
        var deletedLink = await context.ExerciseLinks.FindAsync(link.Id);
        Assert.NotNull(deletedLink);
        Assert.False(deletedLink.IsActive);
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
        
        return exercise;
    }
}
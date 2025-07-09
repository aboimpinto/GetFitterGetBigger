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
/// Integration tests for sequential operations on exercise links
/// Simulates real Admin UI behavior where operations happen one after another
/// </summary>
public class ExerciseLinkSequentialOperationsTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;
    private readonly HttpClient _client;

    public ExerciseLinkSequentialOperationsTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
    }

    [Fact]
    public async Task CreateLink_DuplicateCreationAttempt_SecondFailsWithBadRequest()
    {
        // Arrange
        await using var scope = _fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        var workoutType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Workout");
        var warmupType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Warmup");
        
        var sourceExercise = await CreateExerciseWithTypes(context, "Source Exercise", workoutType.Id);
        var targetExercise = await CreateExerciseWithTypes(context, "Target Exercise", workoutType.Id, warmupType.Id);
        
        var dto = new CreateExerciseLinkDto
        {
            TargetExerciseId = targetExercise.Id.ToString(),
            LinkType = "Warmup",
            DisplayOrder = 1
        };

        // Act - Create first link (should succeed)
        var firstResponse = await _client.PostAsJsonAsync($"/api/exercises/{sourceExercise.Id}/links", dto);
        
        // Act - Try to create duplicate (should fail)
        var secondResponse = await _client.PostAsJsonAsync($"/api/exercises/{sourceExercise.Id}/links", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);
        
        var errorContent = await secondResponse.Content.ReadAsStringAsync();
        Assert.Contains("already exists", errorContent);
        
        // Verify only one link exists in database
        var links = await context.ExerciseLinks
            .Where(el => el.SourceExerciseId == sourceExercise.Id && el.TargetExerciseId == targetExercise.Id)
            .ToListAsync();
        Assert.Single(links);
    }

    [Fact]
    public async Task CreateLink_MaxLinksReached_SubsequentCreationFails()
    {
        // Arrange
        await using var scope = _fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        var workoutType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Workout");
        var warmupType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Warmup");
        
        var sourceExercise = await CreateExerciseWithTypes(context, "Max Links Source", workoutType.Id);
        
        // Create 10 different target exercises
        var targetExercises = new List<Exercise>();
        for (int i = 0; i < 11; i++)
        {
            var target = await CreateExerciseWithTypes(context, $"Target Exercise {i}", workoutType.Id, warmupType.Id);
            targetExercises.Add(target);
        }

        // Act - Create 10 links (should all succeed)
        for (int i = 0; i < 10; i++)
        {
            var dto = new CreateExerciseLinkDto
            {
                TargetExerciseId = targetExercises[i].Id.ToString(),
                LinkType = "Warmup",
                DisplayOrder = i + 1
            };
            
            var response = await _client.PostAsJsonAsync($"/api/exercises/{sourceExercise.Id}/links", dto);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
        
        // Act - Try to create 11th link (should fail)
        var eleventhDto = new CreateExerciseLinkDto
        {
            TargetExerciseId = targetExercises[10].Id.ToString(),
            LinkType = "Warmup",
            DisplayOrder = 11
        };
        
        var eleventhResponse = await _client.PostAsJsonAsync($"/api/exercises/{sourceExercise.Id}/links", eleventhDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, eleventhResponse.StatusCode);
        
        var errorContent = await eleventhResponse.Content.ReadAsStringAsync();
        Assert.Contains("Maximum number", errorContent);
        
        // Verify exactly 10 links in database
        var links = await context.ExerciseLinks
            .Where(el => el.SourceExerciseId == sourceExercise.Id && el.LinkType == "Warmup")
            .ToListAsync();
        Assert.Equal(10, links.Count);
    }

    [Fact]
    public async Task UpdateLink_SequentialUpdates_AllSucceed()
    {
        // Arrange
        await using var scope = _fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        var workoutType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Workout");
        var cooldownType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Cooldown");
        
        var sourceExercise = await CreateExerciseWithTypes(context, "Update Source", workoutType.Id);
        var targetExercise = await CreateExerciseWithTypes(context, "Update Target", workoutType.Id, cooldownType.Id);
        
        // Create initial link
        var createDto = new CreateExerciseLinkDto
        {
            TargetExerciseId = targetExercise.Id.ToString(),
            LinkType = "Cooldown",
            DisplayOrder = 1
        };
        
        var createResponse = await _client.PostAsJsonAsync($"/api/exercises/{sourceExercise.Id}/links", createDto);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        
        var createdLink = await createResponse.Content.ReadFromJsonAsync<ExerciseLinkDto>();
        Assert.NotNull(createdLink);

        // Act - Perform sequential updates (simulating UI interactions)
        var displayOrders = new[] { 5, 10, 3, 7 };
        
        foreach (var order in displayOrders)
        {
            var updateDto = new UpdateExerciseLinkDto
            {
                DisplayOrder = order,
                IsActive = true
            };
            
            var response = await _client.PutAsJsonAsync($"/api/exercises/{sourceExercise.Id}/links/{createdLink.Id}", updateDto);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var updatedLink = await response.Content.ReadFromJsonAsync<ExerciseLinkDto>();
            Assert.NotNull(updatedLink);
            Assert.Equal(order, updatedLink.DisplayOrder);
        }

        // Verify final state
        var finalLink = await context.ExerciseLinks.FindAsync(ExerciseLinkId.From(Guid.Parse(createdLink.Id.Replace("exerciselink-", ""))));
        Assert.NotNull(finalLink);
        Assert.Equal(7, finalLink.DisplayOrder); // Last update value
    }

    [Fact]
    public async Task DeleteLink_FollowedBySecondDelete_SecondReturnsNotFound()
    {
        // Arrange
        await using var scope = _fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        var workoutType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Workout");
        var cooldownType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Cooldown");
        
        var sourceExercise = await CreateExerciseWithTypes(context, "Delete Source", workoutType.Id);
        var targetExercise = await CreateExerciseWithTypes(context, "Delete Target", workoutType.Id, cooldownType.Id);
        
        // Create link to delete
        var createDto = new CreateExerciseLinkDto
        {
            TargetExerciseId = targetExercise.Id.ToString(),
            LinkType = "Cooldown",
            DisplayOrder = 1
        };
        
        var createResponse = await _client.PostAsJsonAsync($"/api/exercises/{sourceExercise.Id}/links", createDto);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        
        var createdLink = await createResponse.Content.ReadFromJsonAsync<ExerciseLinkDto>();
        Assert.NotNull(createdLink);

        // Act - First delete (should succeed)
        var firstDeleteResponse = await _client.DeleteAsync($"/api/exercises/{sourceExercise.Id}/links/{createdLink.Id}");
        Assert.Equal(HttpStatusCode.NoContent, firstDeleteResponse.StatusCode);

        // Act - Second delete (should fail with 404 as link no longer exists)
        var secondDeleteResponse = await _client.DeleteAsync($"/api/exercises/{sourceExercise.Id}/links/{createdLink.Id}");
        Assert.Equal(HttpStatusCode.NotFound, secondDeleteResponse.StatusCode);

        // Verify link is hard deleted (completely removed from database)
        var deletedLink = await context.ExerciseLinks.FindAsync(ExerciseLinkId.From(Guid.Parse(createdLink.Id.Replace("exerciselink-", ""))));
        Assert.Null(deletedLink);
    }

    [Fact]
    public async Task CreateUpdateDeleteWorkflow_SimulatesRealUIInteraction()
    {
        // This test simulates a complete workflow as it would happen in the Admin UI
        // 1. Create a link
        // 2. Update its display order
        // 3. Deactivate it
        // 4. Delete it
        // 5. Verify it's gone from GET results
        
        // Arrange
        await using var scope = _fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        var workoutType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Workout");
        var warmupType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Warmup");
        
        var sourceExercise = await CreateExerciseWithTypes(context, "Workflow Source", workoutType.Id);
        var targetExercise = await CreateExerciseWithTypes(context, "Workflow Target", workoutType.Id, warmupType.Id);
        
        // Step 1: Create link
        var createDto = new CreateExerciseLinkDto
        {
            TargetExerciseId = targetExercise.Id.ToString(),
            LinkType = "Warmup",
            DisplayOrder = 1
        };
        
        var createResponse = await _client.PostAsJsonAsync($"/api/exercises/{sourceExercise.Id}/links", createDto);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        
        var createdLink = await createResponse.Content.ReadFromJsonAsync<ExerciseLinkDto>();
        Assert.NotNull(createdLink);
        Assert.Equal(1, createdLink.DisplayOrder);
        Assert.True(createdLink.IsActive);
        
        // Step 2: Update display order
        var updateOrderDto = new UpdateExerciseLinkDto
        {
            DisplayOrder = 5,
            IsActive = true
        };
        
        var updateOrderResponse = await _client.PutAsJsonAsync($"/api/exercises/{sourceExercise.Id}/links/{createdLink.Id}", updateOrderDto);
        Assert.Equal(HttpStatusCode.OK, updateOrderResponse.StatusCode);
        
        // Step 3: Delete it (hard delete)
        var deleteResponse = await _client.DeleteAsync($"/api/exercises/{sourceExercise.Id}/links/{createdLink.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        
        // Step 4: Verify it's completely removed from database and not in results
        var finalLinksResponse = await _client.GetAsync($"/api/exercises/{sourceExercise.Id}/links");
        Assert.Equal(HttpStatusCode.OK, finalLinksResponse.StatusCode);
        
        var finalLinks = await finalLinksResponse.Content.ReadFromJsonAsync<ExerciseLinksResponseDto>();
        Assert.NotNull(finalLinks);
        Assert.Empty(finalLinks.Links);
        
        // Step 5: Verify link is completely removed from database
        var deletedLink = await context.ExerciseLinks.FindAsync(ExerciseLinkId.From(Guid.Parse(createdLink.Id.Replace("exerciselink-", ""))));
        Assert.Null(deletedLink);
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
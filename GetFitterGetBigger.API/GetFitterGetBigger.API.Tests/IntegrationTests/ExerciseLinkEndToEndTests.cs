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
/// End-to-end workflow tests for exercise linking feature
/// </summary>
public class ExerciseLinkEndToEndTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;
    private readonly HttpClient _client;

    public ExerciseLinkEndToEndTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
    }

    [Fact]
    public async Task CompleteWorkflow_CreateUpdateAndDeleteLinks_ShouldWorkCorrectly()
    {
        // Arrange - Create a complete workout setup
        await using var scope = _fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        var workoutType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Workout");
        var warmupType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Warmup");
        var cooldownType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Cooldown");
        
        // Create main workout exercise
        var mainWorkout = await CreateExerciseWithTypes(context, "Barbell Back Squat", workoutType.Id);
        
        // Create warmup exercises
        var warmup1 = await CreateExerciseWithTypes(context, "Air Squat", workoutType.Id, warmupType.Id);
        var warmup2 = await CreateExerciseWithTypes(context, "Leg Swings", warmupType.Id);
        var warmup3 = await CreateExerciseWithTypes(context, "Goblet Squat", workoutType.Id, warmupType.Id);
        
        // Create cooldown exercises
        var cooldown1 = await CreateExerciseWithTypes(context, "Quad Stretch", cooldownType.Id);
        var cooldown2 = await CreateExerciseWithTypes(context, "Pigeon Pose", cooldownType.Id);

        // Step 1: Create warmup links
        var warmupLink1 = await CreateLink(mainWorkout.Id.ToString(), warmup1.Id.ToString(), "Warmup", 1);
        Assert.NotNull(warmupLink1);
        
        var warmupLink3 = await CreateLink(mainWorkout.Id.ToString(), warmup3.Id.ToString(), "Warmup", 2);
        Assert.NotNull(warmupLink3);

        // Step 2: Try to create invalid link (warmup-only exercise)
        var invalidResponse = await _client.PostAsJsonAsync($"/api/exercises/{warmup2.Id}/links", new CreateExerciseLinkDto
        {
            TargetExerciseId = warmup1.Id.ToString(),
            LinkType = "Warmup",
            DisplayOrder = 1
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidResponse.StatusCode);

        // Step 3: Create cooldown links
        var cooldownLink1 = await CreateLink(mainWorkout.Id.ToString(), cooldown1.Id.ToString(), "Cooldown", 1);
        var cooldownLink2 = await CreateLink(mainWorkout.Id.ToString(), cooldown2.Id.ToString(), "Cooldown", 2);

        // Step 4: Get all links with details
        var allLinksResponse = await _client.GetAsync($"/api/exercises/{mainWorkout.Id}/links?includeExerciseDetails=true");
        Assert.Equal(HttpStatusCode.OK, allLinksResponse.StatusCode);
        
        var allLinks = await allLinksResponse.Content.ReadFromJsonAsync<ExerciseLinksResponseDto>();
        Assert.NotNull(allLinks);
        Assert.Equal(4, allLinks.Links.Count);
        Assert.Equal(2, allLinks.Links.Count(l => l.LinkType == "Warmup"));
        Assert.Equal(2, allLinks.Links.Count(l => l.LinkType == "Cooldown"));

        // Verify exercise details are included
        Assert.All(allLinks.Links, link =>
        {
            Assert.NotNull(link.TargetExercise);
            Assert.NotEmpty(link.TargetExercise!.Name);
        });

        // Step 5: Update display order
        var updateDto = new UpdateExerciseLinkDto
        {
            DisplayOrder = 3,
            IsActive = true
        };
        var updateResponse = await _client.PutAsJsonAsync($"/api/exercises/{mainWorkout.Id}/links/{warmupLink1.Id}", updateDto);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        
        var updatedLink = await updateResponse.Content.ReadFromJsonAsync<ExerciseLinkDto>();
        Assert.NotNull(updatedLink);
        Assert.Equal(3, updatedLink!.DisplayOrder);

        // Step 6: Get suggested links
        var suggestedResponse = await _client.GetAsync($"/api/exercises/{mainWorkout.Id}/links/suggested?count=3");
        Assert.Equal(HttpStatusCode.OK, suggestedResponse.StatusCode);
        
        var suggested = await suggestedResponse.Content.ReadFromJsonAsync<ExerciseLinkDto[]>();
        Assert.NotNull(suggested);

        // Step 7: Delete a link
        var deleteResponse = await _client.DeleteAsync($"/api/exercises/{mainWorkout.Id}/links/{cooldownLink2.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Step 8: Verify final state
        var finalLinksResponse = await _client.GetAsync($"/api/exercises/{mainWorkout.Id}/links");
        var finalLinks = await finalLinksResponse.Content.ReadFromJsonAsync<ExerciseLinksResponseDto>();
        Assert.NotNull(finalLinks);
        Assert.Equal(3, finalLinks!.Links.Count(l => l.IsActive)); // One was deleted
    }

    [Fact]
    public async Task CompleteWorkflow_WithComplexLinkingScenarios_ShouldHandleCorrectly()
    {
        // Arrange - Create a more complex scenario
        await using var scope = _fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        var workoutType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Workout");
        var warmupType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Warmup");
        var cooldownType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Cooldown");
        
        // Create exercises that serve multiple purposes
        var deadlift = await CreateExerciseWithTypes(context, "Deadlift", workoutType.Id);
        var rdl = await CreateExerciseWithTypes(context, "Romanian Deadlift", workoutType.Id, warmupType.Id);
        var goodMorning = await CreateExerciseWithTypes(context, "Good Morning", workoutType.Id, warmupType.Id);
        var hamstringStretch = await CreateExerciseWithTypes(context, "Hamstring Stretch", cooldownType.Id);

        // Scenario 1: RDL as warmup for Deadlift
        var rdlWarmupLink = await CreateLink(deadlift.Id.ToString(), rdl.Id.ToString(), "Warmup", 1);
        Assert.NotNull(rdlWarmupLink);

        // Scenario 2: Good Morning as warmup for both Deadlift and RDL
        var gmForDeadlift = await CreateLink(deadlift.Id.ToString(), goodMorning.Id.ToString(), "Warmup", 2);
        var gmForRdl = await CreateLink(rdl.Id.ToString(), goodMorning.Id.ToString(), "Warmup", 1);
        Assert.NotNull(gmForDeadlift);
        Assert.NotNull(gmForRdl);

        // Scenario 3: Add cooldown
        var stretchLink = await CreateLink(deadlift.Id.ToString(), hamstringStretch.Id.ToString(), "Cooldown", 1);
        Assert.NotNull(stretchLink);

        // Verify the link structure
        var deadliftLinks = await GetLinks(deadlift.Id.ToString());
        Assert.Equal(3, deadliftLinks.Links.Count); // 2 warmups, 1 cooldown

        var rdlLinks = await GetLinks(rdl.Id.ToString());
        Assert.Equal(1, rdlLinks.Links.Count); // 1 warmup

        // Test filtering
        var warmupOnlyLinks = await GetLinks(deadlift.Id.ToString(), "Warmup");
        Assert.Equal(2, warmupOnlyLinks.Links.Count);
        Assert.All(warmupOnlyLinks.Links, link => Assert.Equal("Warmup", link.LinkType));

        // Test display order sorting
        Assert.Single(warmupOnlyLinks.Links.Where(l => l.DisplayOrder == 1));
        Assert.Single(warmupOnlyLinks.Links.Where(l => l.DisplayOrder == 2));
    }

    [Fact]
    public async Task CompleteWorkflow_MaxLinksAndEdgeCases_ShouldHandleCorrectly()
    {
        // Arrange
        await using var scope = _fixture.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
        
        var workoutType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Workout");
        var warmupType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Warmup");
        var cooldownType = await context.ExerciseTypes.FirstAsync(et => et.Value == "Cooldown");
        
        var mainExercise = await CreateExerciseWithTypes(context, "Bench Press", workoutType.Id);
        
        // Create exactly 10 warmup exercises (max limit)
        var warmupExercises = new List<Exercise>();
        for (int i = 1; i <= 10; i++)
        {
            var warmup = await CreateExerciseWithTypes(context, $"Warmup Exercise {i}", workoutType.Id, warmupType.Id);
            warmupExercises.Add(warmup);
            await CreateLink(mainExercise.Id.ToString(), warmup.Id.ToString(), "Warmup", i);
        }

        // Try to add 11th warmup - should fail
        var extraWarmup = await CreateExerciseWithTypes(context, "Extra Warmup", workoutType.Id, warmupType.Id);
        var failedResponse = await _client.PostAsJsonAsync($"/api/exercises/{mainExercise.Id}/links", new CreateExerciseLinkDto
        {
            TargetExerciseId = extraWarmup.Id.ToString(),
            LinkType = "Warmup",
            DisplayOrder = 11
        });
        Assert.Equal(HttpStatusCode.BadRequest, failedResponse.StatusCode);
        var error = await failedResponse.Content.ReadAsStringAsync();
        Assert.Contains("Maximum number", error);

        // But we can still add cooldowns (different type)
        var cooldown = await CreateExerciseWithTypes(context, "Chest Stretch", cooldownType.Id);
        var cooldownLink = await CreateLink(mainExercise.Id.ToString(), cooldown.Id.ToString(), "Cooldown", 1);
        Assert.NotNull(cooldownLink);

        // Verify counts
        var allLinks = await GetLinks(mainExercise.Id.ToString());
        Assert.Equal(11, allLinks.Links.Count); // 10 warmups + 1 cooldown

        // Delete one warmup to make room
        var firstWarmupLink = allLinks.Links.First(l => l.LinkType == "Warmup");
        var deleteResponse = await _client.DeleteAsync($"/api/exercises/{mainExercise.Id}/links/{firstWarmupLink.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Now we can add the extra warmup
        var successResponse = await _client.PostAsJsonAsync($"/api/exercises/{mainExercise.Id}/links", new CreateExerciseLinkDto
        {
            TargetExerciseId = extraWarmup.Id.ToString(),
            LinkType = "Warmup",
            DisplayOrder = 11
        });
        Assert.Equal(HttpStatusCode.Created, successResponse.StatusCode);
    }

    private async Task<ExerciseLinkDto?> CreateLink(string sourceId, string targetId, string linkType, int displayOrder)
    {
        var dto = new CreateExerciseLinkDto
        {
            TargetExerciseId = targetId,
            LinkType = linkType,
            DisplayOrder = displayOrder
        };

        var response = await _client.PostAsJsonAsync($"/api/exercises/{sourceId}/links", dto);
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to create link: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<ExerciseLinkDto>();
        return result ?? throw new InvalidOperationException("Failed to deserialize link response");
    }

    private async Task<ExerciseLinksResponseDto> GetLinks(string exerciseId, string? linkType = null)
    {
        var url = $"/api/exercises/{exerciseId}/links";
        if (!string.IsNullOrEmpty(linkType))
        {
            url += $"?linkType={linkType}";
        }

        var response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ExerciseLinksResponseDto>();
        return result ?? throw new InvalidOperationException("Failed to deserialize response");
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
            typeIds.Any(id => id == ExerciseTypeId.From(Guid.Parse("91fc6a9f-70d1-4ae3-906f-3e4e3a5dce30"))) // Rest type
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
        
        return exercise;
    }
}
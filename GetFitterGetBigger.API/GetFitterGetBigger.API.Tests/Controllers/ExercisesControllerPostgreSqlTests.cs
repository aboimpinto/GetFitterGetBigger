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
using GetFitterGetBigger.API.Tests.TestBuilders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Controllers;

[Collection("PostgreSQL Integration Tests")]
public class ExercisesControllerPostgreSqlTests : PostgreSqlTestBase
{
    public ExercisesControllerPostgreSqlTests(PostgreSqlApiTestFixture factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetExercises_ReturnsPagedListOfExercises()
    {
        // Arrange - Seed test data first
        await SeedTestDataAsync();
        
        // Create some exercises through the API
        var exercise1 = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Test Exercise 1 " + Guid.NewGuid())
            .WithDescription("Test Description 1")
            .WithMuscleGroups((SeedDataBuilder.StandardIds.MuscleGroupIds.Chest, SeedDataBuilder.StandardIds.MuscleRoleIds.Primary))
            .WithCoachNotes(("Step 1", 0))
            .WithDifficultyId(SeedDataBuilder.StandardIds.DifficultyLevelIds.Beginner)
            .WithKineticChainId(SeedDataBuilder.StandardIds.KineticChainTypeIds.Compound)
            .WithExerciseWeightTypeId(SeedDataBuilder.StandardIds.ExerciseWeightTypeIds.WeightRequired)
            .Build();
        
        var createResponse = await Client.PostAsJsonAsync("/api/exercises", exercise1);
        
        // Ensure the exercise was created successfully
        if (!createResponse.IsSuccessStatusCode)
        {
            var errorContent = await createResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to create exercise: {createResponse.StatusCode} - {errorContent}");
        }

        // Act
        var response = await Client.GetAsync("/api/exercises?page=1&pageSize=10");
        
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<ExerciseDto>>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task GetExercises_WithNameFilter_ReturnsFilteredResults()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await Client.GetAsync("/api/exercises?name=squat&page=1&pageSize=10");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<ExerciseDto>>();
        
        Assert.NotNull(result);
        Assert.All(result.Items, item => Assert.Contains("squat", item.Name, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetExercises_WithMuscleGroupFilter_ReturnsFilteredResults()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var response = await Client.GetAsync($"/api/exercises?MuscleGroupIds={SeedDataBuilder.StandardIds.MuscleGroupIds.Chest}&page=1&pageSize=10");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<ExerciseDto>>();
        
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
        Assert.All(result.Items, item => 
            Assert.Contains(item.MuscleGroups, mg => mg.MuscleGroup.Id == SeedDataBuilder.StandardIds.MuscleGroupIds.Chest));
    }

    [Fact]
    public async Task GetExercise_WithValidId_ReturnsExercise()
    {
        // Arrange
        var exercises = await SeedTestDataAsync();
        var existingExercise = exercises.First();

        // Act
        var response = await Client.GetAsync($"/api/exercises/{existingExercise.Id}");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ExerciseDto>();
        
        Assert.NotNull(result);
        Assert.Equal(existingExercise.Id.ToString(), result.Id);
        Assert.Equal(existingExercise.Name, result.Name);
    }

    [Fact]
    public async Task CreateExercise_WithDuplicateName_ReturnsConflict()
    {
        // Arrange
        var exercises = await SeedTestDataAsync();
        var existingExercise = exercises.First();
        
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName(existingExercise.Name)
            .WithDescription("Test Description")
            .WithMuscleGroups((SeedDataBuilder.StandardIds.MuscleGroupIds.Chest, SeedDataBuilder.StandardIds.MuscleRoleIds.Primary))
            .WithCoachNotes(("Step 1", 0))
            .WithDifficultyId(SeedDataBuilder.StandardIds.DifficultyLevelIds.Beginner)
            .WithKineticChainId(SeedDataBuilder.StandardIds.KineticChainTypeIds.Compound)
            .WithExerciseWeightTypeId(SeedDataBuilder.StandardIds.ExerciseWeightTypeIds.WeightRequired)
            .Build();

        // Act
        var response = await Client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateExercise_WithValidData_ReturnsUpdatedExercise()
    {
        // Arrange
        var exercises = await SeedTestDataAsync();
        var existingExercise = exercises.First();
        
        var request = UpdateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Updated Exercise Name")
            .WithDescription("Updated Description")
            .WithMuscleGroups((SeedDataBuilder.StandardIds.MuscleGroupIds.Chest, SeedDataBuilder.StandardIds.MuscleRoleIds.Primary))
            .WithCoachNotes(
                ("Updated Step 1", 0),
                ("Updated Step 2", 1)
            )
            .WithVideoUrl("https://example.com/updated-video.mp4")
            .WithImageUrl("https://example.com/updated-image.jpg")
            .WithDifficultyId(SeedDataBuilder.StandardIds.DifficultyLevelIds.Intermediate)
            .Build();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/exercises/{existingExercise.Id}", request);
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ExerciseDto>();
        
        Assert.NotNull(result);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Description, result.Description);
        Assert.Equal(request.VideoUrl, result.VideoUrl);
    }

    [Fact]
    public async Task UpdateExercise_WithDuplicateName_ReturnsConflict()
    {
        // Arrange
        var exercises = await SeedTestDataAsync();
        var exercise1 = exercises.First();
        var exercise2 = exercises.Skip(1).First();
        
        var request = UpdateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName(exercise2.Name) // Try to update exercise1 with exercise2's name
            .WithDescription("Updated Description")
            .WithMuscleGroups((SeedDataBuilder.StandardIds.MuscleGroupIds.Chest, SeedDataBuilder.StandardIds.MuscleRoleIds.Primary))
            .WithCoachNotes(("Step 1", 0))
            .WithDifficultyId(exercise1.DifficultyId.ToString())
            .WithKineticChainId(exercise1.KineticChainId?.ToString())
            .WithExerciseWeightTypeId(exercise1.ExerciseWeightTypeId?.ToString() ?? SeedDataBuilder.StandardIds.ExerciseWeightTypeIds.WeightRequired)
            .Build();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/exercises/{exercise1.Id}", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteExercise_WithValidId_ReturnsNoContent()
    {
        // Arrange - Create an exercise through the API
        var exercises = await SeedTestDataAsync();
        var exercise = exercises.First();

        // Act
        var response = await Client.DeleteAsync($"/api/exercises/{exercise.Id}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify exercise is deleted
        var getResponse = await Client.GetAsync($"/api/exercises/{exercise.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    private async Task<List<Exercise>> SeedTestDataAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();

        // Use the centralized SeedDataBuilder for consistent data
        var seedBuilder = new SeedDataBuilder(context);
        
        // Clear any existing test data first
        await seedBuilder.ClearAllDataAsync();
        
        // Create all reference data and some sample exercises
        await seedBuilder.WithAllReferenceDataAsync();
        
        await seedBuilder
            .WithWorkoutExercise(
                name: "Barbell Back Squat",
                description: "A compound lower body exercise",
                videoUrl: "https://example.com/squat-video.mp4",
                imageUrl: "https://example.com/squat-image.jpg",
                difficultyId: SeedDataBuilder.StandardIds.IntermediateDifficultyId)
            .WithWorkoutExercise(
                name: "Dumbbell Bicep Curl",
                description: "An isolation exercise for biceps",
                videoUrl: null,
                imageUrl: null,
                isUnilateral: true,
                difficultyId: SeedDataBuilder.StandardIds.BeginnerDifficultyId,
                kineticChainId: SeedDataBuilder.StandardIds.IsolationKineticChainId,
                exerciseWeightTypeId: SeedDataBuilder.StandardIds.WeightRequiredWeightTypeId,
                configure: exercise =>
                {
                    // Clear default muscle groups and add biceps-specific ones
                    exercise.ExerciseMuscleGroups.Clear();
                    exercise.ExerciseMuscleGroups.Add(ExerciseMuscleGroup.Handler.Create(
                        exercise.Id,
                        MuscleGroupId.From(SeedDataBuilder.StandardIds.BicepsMuscleGroupId),
                        MuscleRoleId.From(SeedDataBuilder.StandardIds.PrimaryMuscleRoleId)
                    ));
                    
                    // Clear default equipment and add dumbbells
                    exercise.ExerciseEquipment.Clear();
                    exercise.ExerciseEquipment.Add(ExerciseEquipment.Handler.Create(
                        exercise.Id,
                        EquipmentId.From(SeedDataBuilder.StandardIds.DumbbellEquipmentId)
                    ));
                })
            .WithWorkoutExercise(
                name: "Push-up",
                description: "A bodyweight upper body exercise",
                videoUrl: "https://example.com/pushup-video.mp4",
                imageUrl: null,
                difficultyId: SeedDataBuilder.StandardIds.BeginnerDifficultyId,
                exerciseWeightTypeId: SeedDataBuilder.StandardIds.BodyweightOnlyWeightTypeId,
                configure: exercise =>
                {
                    // Clear default muscle groups and add chest-specific ones
                    exercise.ExerciseMuscleGroups.Clear();
                    exercise.ExerciseMuscleGroups.Add(ExerciseMuscleGroup.Handler.Create(
                        exercise.Id,
                        MuscleGroupId.From(SeedDataBuilder.StandardIds.ChestMuscleGroupId),
                        MuscleRoleId.From(SeedDataBuilder.StandardIds.PrimaryMuscleRoleId)
                    ));
                    
                    // No equipment needed for bodyweight exercise
                    exercise.ExerciseEquipment.Clear();
                })
            .CommitAsync();

        // Load and return the exercises with full relationships
        return await context.Exercises
            .Include(e => e.Difficulty)
            .Include(e => e.KineticChain)
            .Include(e => e.ExerciseWeightType)
            .Include(e => e.ExerciseExerciseTypes)
                .ThenInclude(eet => eet.ExerciseType)
            .Include(e => e.ExerciseMuscleGroups)
                .ThenInclude(emg => emg.MuscleGroup)
            .Include(e => e.ExerciseMuscleGroups)
                .ThenInclude(emg => emg.MuscleRole)
            .Include(e => e.ExerciseEquipment)
                .ThenInclude(ee => ee.Equipment)
            .Include(e => e.ExerciseBodyParts)
                .ThenInclude(ebp => ebp.BodyPart)
            .Include(e => e.ExerciseMovementPatterns)
                .ThenInclude(emp => emp.MovementPattern)
            .ToListAsync();
    }
}
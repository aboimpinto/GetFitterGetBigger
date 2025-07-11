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

[Collection("ApiTestCollection")]
public class ExercisesControllerTests : IClassFixture<ApiTestFixture>
{
    private readonly ApiTestFixture _fixture;
    private readonly HttpClient _client;

    public ExercisesControllerTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
    }

    [Fact]
    public async Task GetExercise_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = ExerciseId.New().ToString();

        // Act
        var response = await _client.GetAsync($"/api/exercises/{invalidId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateExercise_WithValidData_ReturnsCreatedExercise()
    {
        // Arrange
        await SeedTestDataAsync();
        var request = CreateExerciseRequestBuilder.ForWorkoutExercise()
            .WithName("Test Exercise")
            .WithDescription("Test Description")
            .WithCoachNotes(
                ("Step 1", 0),
                ("Step 2", 1)
            )
            .WithVideoUrl("https://example.com/video.mp4")
            .WithImageUrl("https://example.com/image.jpg")
            .Build();

        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<ExerciseDto>();
        Assert.NotNull(result);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Description, result.Description);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task CreateExercise_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateExerciseRequest
        {
            Name = "", // Invalid - empty name
            Description = "Test Description",
            CoachNotes = new List<CoachNoteRequest>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/exercises", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateExercise_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = ExerciseId.New().ToString();
        var request = new UpdateExerciseRequest
        {
            Name = "Updated Name",
            Description = "Updated Description",
            CoachNotes = new List<CoachNoteRequest> { new() { Text = "Step 1", Order = 0 } },
            DifficultyId = "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
            MuscleGroups = new List<MuscleGroupWithRoleRequest> { new() { MuscleGroupId = "musclegroup-ccddeeff-0011-2233-4455-667788990011", MuscleRoleId = "musclerole-abcdef12-3456-7890-abcd-ef1234567890" } }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/exercises/{nonExistentId}", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteExercise_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = ExerciseId.New().ToString();

        // Act
        var response = await _client.DeleteAsync($"/api/exercises/{nonExistentId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<List<Exercise>> SeedTestDataAsync()
    {
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();

        // Clear existing exercises (EF Core In-Memory doesn't support raw SQL)
        var existingExercises = await context.Exercises.ToListAsync();
        context.Exercises.RemoveRange(existingExercises);
        await context.SaveChangesAsync();

        // Create test exercises
        var exercises = new List<Exercise>
        {
            Exercise.Handler.CreateNew(
                "Barbell Back Squat",
                "A compound lower body exercise",
                "https://example.com/squat-video.mp4",
                "https://example.com/squat-image.jpg",
                false,
                DifficultyLevelId.From(Guid.Parse("9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a")),
                KineticChainTypeId.From(Guid.Parse("f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4")) // Compound
            ),
            Exercise.Handler.CreateNew(
                "Dumbbell Bicep Curl",
                "An isolation exercise for biceps",
                null,
                null,
                true,
                DifficultyLevelId.From(Guid.Parse("8a8adb1d-24d2-4979-a5a6-0d760e6da24b")),
                KineticChainTypeId.From(Guid.Parse("2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b")) // Isolation
            ),
            Exercise.Handler.CreateNew(
                "Push-up",
                "A bodyweight upper body exercise",
                "https://example.com/pushup-video.mp4",
                null,
                false,
                DifficultyLevelId.From(Guid.Parse("8a8adb1d-24d2-4979-a5a6-0d760e6da24b")),
                KineticChainTypeId.From(Guid.Parse("f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4")) // Compound
            )
        };

        context.Exercises.AddRange(exercises);
        await context.SaveChangesAsync();

        // Add muscle groups
        exercises[0].ExerciseMuscleGroups.Add(ExerciseMuscleGroup.Handler.Create(
            exercises[0].Id,
            MuscleGroupId.From(Guid.Parse("ccddeeff-0011-2233-4455-667788990011")),
            MuscleRoleId.From(Guid.Parse("abcdef12-3456-7890-abcd-ef1234567890"))
        ));
        
        exercises[1].ExerciseMuscleGroups.Add(ExerciseMuscleGroup.Handler.Create(
            exercises[1].Id,
            MuscleGroupId.From(Guid.Parse("ddeeff00-1122-3344-5566-778899001122")),
            MuscleRoleId.From(Guid.Parse("abcdef12-3456-7890-abcd-ef1234567890"))
        ));

        // Add equipment
        exercises[0].ExerciseEquipment.Add(ExerciseEquipment.Handler.Create(
            exercises[0].Id,
            EquipmentId.From(Guid.Parse("33445566-7788-99aa-bbcc-ddeeff001122"))
        ));
        
        exercises[1].ExerciseEquipment.Add(ExerciseEquipment.Handler.Create(
            exercises[1].Id,
            EquipmentId.From(Guid.Parse("44556677-8899-aabb-ccdd-eeff00112233"))
        ));

        // Add body parts
        exercises[0].ExerciseBodyParts.Add(ExerciseBodyPart.Handler.Create(
            exercises[0].Id,
            BodyPartId.From(Guid.Parse("7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c"))
        ));

        // Add movement patterns
        exercises[0].ExerciseMovementPatterns.Add(ExerciseMovementPattern.Handler.Create(
            exercises[0].Id,
            MovementPatternId.From(Guid.Parse("99aabbcc-ddee-ff00-1122-334455667788"))
        ));

        await context.SaveChangesAsync();

        // Load full entities with relationships
        return await context.Exercises
            .Include(e => e.Difficulty)
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
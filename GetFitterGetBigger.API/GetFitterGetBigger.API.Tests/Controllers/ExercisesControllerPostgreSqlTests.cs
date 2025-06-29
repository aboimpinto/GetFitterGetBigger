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
        // Arrange
        await SeedTestDataAsync();

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
        var response = await Client.GetAsync("/api/exercises?MuscleGroupIds=musclegroup-ccddeeff-0011-2233-4455-667788990011&page=1&pageSize=10");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<ExerciseDto>>();
        
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
        Assert.All(result.Items, item => 
            Assert.Contains(item.MuscleGroups, mg => mg.MuscleGroup.Id == "musclegroup-ccddeeff-0011-2233-4455-667788990011"));
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
        
        var request = new CreateExerciseRequest
        {
            Name = existingExercise.Name,
            Description = "Test Description",
            CoachNotes = new List<CoachNoteRequest> { new() { Text = "Step 1", Order = 0 } },
            DifficultyId = "difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b",
            MuscleGroups = new List<MuscleGroupWithRoleRequest> { new() { MuscleGroupId = "musclegroup-ccddeeff-0011-2233-4455-667788990011", MuscleRoleId = "musclerole-abcdef12-3456-7890-abcd-ef1234567890" } }
        };

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
        
        var request = new UpdateExerciseRequest
        {
            Name = "Updated Exercise Name",
            Description = "Updated Description",
            CoachNotes = new List<CoachNoteRequest> 
            {
                new() { Text = "Updated Step 1", Order = 0 },
                new() { Text = "Updated Step 2", Order = 1 }
            },
            VideoUrl = "https://example.com/updated-video.mp4",
            ImageUrl = "https://example.com/updated-image.jpg",
            DifficultyId = "difficultylevel-9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a",
            MuscleGroups = new List<MuscleGroupWithRoleRequest> 
            { 
                new() 
                { 
                    MuscleGroupId = "musclegroup-ddeeff00-1122-3344-5566-778899001122",
                    MuscleRoleId = "musclerole-abcdef12-3456-7890-abcd-ef1234567890"
                }
            },
            EquipmentIds = new List<string> { "equipment-44556677-8899-aabb-ccdd-eeff00112233" },
            BodyPartIds = new List<string> { "bodypart-b2d89d5c-cb8a-4f5d-8a9e-2c3b76612c5a" },
            MovementPatternIds = new List<string> { "movementpattern-aabbccdd-eeff-0011-2233-445566778899" }
        };

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
        
        var request = new UpdateExerciseRequest
        {
            Name = exercise2.Name, // Try to update exercise1 with exercise2's name
            Description = "Updated Description",
            CoachNotes = new List<CoachNoteRequest> { new() { Text = "Step 1", Order = 0 } },
            DifficultyId = exercise1.DifficultyId.ToString(),
            MuscleGroups = new List<MuscleGroupWithRoleRequest> { new() { MuscleGroupId = "musclegroup-ccddeeff-0011-2233-4455-667788990011", MuscleRoleId = "musclerole-abcdef12-3456-7890-abcd-ef1234567890" } }
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/exercises/{exercise1.Id}", request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteExercise_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var exercises = await SeedTestDataAsync();
        var exerciseToDelete = exercises.Last();

        // Act
        var response = await Client.DeleteAsync($"/api/exercises/{exerciseToDelete.Id}");
        
        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify exercise is deleted
        var getResponse = await Client.GetAsync($"/api/exercises/{exerciseToDelete.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    private async Task<List<Exercise>> SeedTestDataAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();

        // Create test exercises
        var exercises = new List<Exercise>
        {
            Exercise.Handler.CreateNew(
                "Barbell Back Squat",
                "A compound lower body exercise",
                "https://example.com/squat-video.mp4",
                "https://example.com/squat-image.jpg",
                false,
                DifficultyLevelId.From(Guid.Parse("9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a"))
            ),
            Exercise.Handler.CreateNew(
                "Dumbbell Bicep Curl",
                "An isolation exercise for biceps",
                null,
                null,
                true,
                DifficultyLevelId.From(Guid.Parse("8a8adb1d-24d2-4979-a5a6-0d760e6da24b"))
            ),
            Exercise.Handler.CreateNew(
                "Push-up",
                "A bodyweight upper body exercise",
                "https://example.com/pushup-video.mp4",
                null,
                false,
                DifficultyLevelId.From(Guid.Parse("8a8adb1d-24d2-4979-a5a6-0d760e6da24b"))
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
        
        // Add muscle group to push-up (chest exercise)
        exercises[2].ExerciseMuscleGroups.Add(ExerciseMuscleGroup.Handler.Create(
            exercises[2].Id,
            MuscleGroupId.From(Guid.Parse("ccddeeff-0011-2233-4455-667788990011")), // Pectoralis
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
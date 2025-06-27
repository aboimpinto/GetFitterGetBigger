using System;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Data;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Repositories;

/// <summary>
/// Integration tests for ExerciseRepository using actual PostgreSQL database
/// These tests are marked with Skip attribute by default to avoid CI/CD issues
/// Remove Skip attribute to run locally with a PostgreSQL instance
/// </summary>
public class ExerciseRepositoryPostgresTests : IDisposable
{
    private readonly FitnessDbContext _context;
    private readonly ExerciseRepository _repository;
    private readonly bool _skipTests = true; // Set to false to run tests locally

    public ExerciseRepositoryPostgresTests()
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Testing.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("FitnessDb") 
            ?? "Host=localhost;Database=fitness_test;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<FitnessDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        _context = new FitnessDbContext(options);
        _repository = new ExerciseRepository(_context);
        
        // Ensure database is created and migrations are applied
        _context.Database.EnsureCreated();
        
        SeedTestData();
    }

    [Fact(Skip = "Requires PostgreSQL database")]
    public async Task GetPagedAsync_WithMultipleMuscleGroups_PostgreSQL_ReturnsExercise()
    {
        if (_skipTests) return;

        // Arrange
        var difficultyId = _context.DifficultyLevels.First().Id;
        var chestMuscleId = _context.MuscleGroups.First(mg => mg.Name == "Chest").Id;
        var tricepsMuscleId = _context.MuscleGroups.First(mg => mg.Name == "Triceps").Id;
        var primaryRoleId = _context.MuscleRoles.First(mr => mr.Name == "Primary").Id;
        var secondaryRoleId = _context.MuscleRoles.First(mr => mr.Name == "Secondary").Id;
        
        // Create exercise
        var exercise = Exercise.Handler.CreateNew(
            "PostgreSQL Test Exercise", 
            "Exercise with multiple muscle groups", 
            "Test instructions", 
            null, null, false, difficultyId);
        
        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync();
        
        // Add multiple muscle groups
        var chestAssociation = ExerciseMuscleGroup.Handler.Create(
            exercise.Id, chestMuscleId, primaryRoleId);
        var tricepsAssociation = ExerciseMuscleGroup.Handler.Create(
            exercise.Id, tricepsMuscleId, secondaryRoleId);
        
        _context.ExerciseMuscleGroups.AddRange(chestAssociation, tricepsAssociation);
        await _context.SaveChangesAsync();

        // Clear context to ensure fresh query
        _context.ChangeTracker.Clear();

        // Act
        var (exercises, totalCount) = await _repository.GetPagedAsync(1, 10);

        // Assert
        Assert.True(totalCount >= 1, $"Expected at least 1 exercise but got {totalCount}");
        var testExercise = exercises.FirstOrDefault(e => e.Name == "PostgreSQL Test Exercise");
        Assert.NotNull(testExercise);
        Assert.Equal(2, testExercise.ExerciseMuscleGroups.Count);
    }

    private void SeedTestData()
    {
        // Clear existing data
        _context.Database.ExecuteSqlRaw("TRUNCATE TABLE \"ExerciseMuscleGroups\", \"Exercises\", \"MuscleGroups\", \"MuscleRoles\", \"DifficultyLevels\", \"BodyParts\" CASCADE");
        
        // Add body parts
        var upperBody = BodyPart.Handler.Create(
            BodyPartId.New(), "Upper Body", null, 1, true);
        _context.BodyParts.Add(upperBody);
        
        // Add muscle groups
        var chest = MuscleGroup.Handler.Create(
            MuscleGroupId.New(), "Chest", upperBody.Id);
        var triceps = MuscleGroup.Handler.Create(
            MuscleGroupId.New(), "Triceps", upperBody.Id);
        _context.MuscleGroups.AddRange(chest, triceps);
        
        // Add muscle roles
        var primaryRole = MuscleRole.Handler.Create(
            MuscleRoleId.New(), "Primary", "Primary muscle", 1, true);
        var secondaryRole = MuscleRole.Handler.Create(
            MuscleRoleId.New(), "Secondary", "Secondary muscle", 2, true);
        _context.MuscleRoles.AddRange(primaryRole, secondaryRole);
        
        // Add difficulty
        var beginnerDifficulty = DifficultyLevel.Handler.Create(
            DifficultyLevelId.New(), "Beginner", "For beginners", 1, true);
        _context.DifficultyLevels.Add(beginnerDifficulty);
        
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
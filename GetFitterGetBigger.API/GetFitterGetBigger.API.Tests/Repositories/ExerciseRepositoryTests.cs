using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Implementations;
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Repositories
{
    public class ExerciseRepositoryTests : IDisposable
    {
        private readonly FitnessDbContext _context;
        private readonly ExerciseRepository _repository;

        public ExerciseRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<FitnessDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new FitnessDbContext(options);
            _repository = new ExerciseRepository();
            _repository.SetContext(_context);
            
            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Add difficulty levels
            var beginnerDifficulty = DifficultyLevel.Handler.Create(
                DifficultyLevelId.New(), "Beginner", "For beginners", 1, true).Value;
            _context.DifficultyLevels.Add(beginnerDifficulty);
            
            // Add body parts
            var bodyPart = BodyPart.Handler.Create(
                BodyPartId.New(), "Upper Body", null, 1, true).Value;
            _context.BodyParts.Add(bodyPart);
            
            // Add muscle groups
            var chestMuscle = MuscleGroup.Handler.Create(
                MuscleGroupId.New(), "Chest", bodyPart.BodyPartId);
            _context.MuscleGroups.Add(chestMuscle);
            
            // Add muscle roles
            var primaryRoleResult = MuscleRole.Handler.Create(
                MuscleRoleId.New(), "Primary", "Primary muscle", 1, true);
            if (primaryRoleResult.IsSuccess)
                _context.MuscleRoles.Add(primaryRoleResult.Value);
            
            // Add equipment
            var barbell = Equipment.Handler.Create(
                EquipmentId.New(), "Barbell");
            _context.Equipment.Add(barbell);
            
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetPagedAsync_WithNoFilters_ReturnsAllActiveExercises()
        {
            // Arrange
            var difficultyId = _context.DifficultyLevels.First().DifficultyLevelId;
            var exercise1 = ExerciseBuilder.AWorkoutExercise()
                .WithName("Push-up")
                .WithDescription("Basic push-up")
                .WithDifficultyId(difficultyId)
                .Build();
            var exercise2 = ExerciseBuilder.AWorkoutExercise()
                .WithName("Pull-up")
                .WithDescription("Basic pull-up")
                .WithDifficultyId(difficultyId)
                .Build();
            var inactiveExercise = ExerciseBuilder.AnInactiveExercise()
                .WithName("Inactive")
                .WithDescription("Inactive exercise")
                .WithDifficultyId(difficultyId)
                .Build();
            
            _context.Exercises.AddRange(exercise1, exercise2, inactiveExercise);
            await _context.SaveChangesAsync();

            // Act
            var (exercises, totalCount) = await _repository.GetPagedAsync(
                1, 
                10, 
                string.Empty,
                DifficultyLevelId.Empty,
                new List<MuscleGroupId>(),
                new List<EquipmentId>(),
                new List<MovementPatternId>(),
                new List<BodyPartId>());

            // Assert
            Assert.Equal(2, totalCount);
            Assert.Equal(2, exercises.Count());
            Assert.All(exercises, e => Assert.True(e.IsActive));
        }

        [Fact]
        public async Task GetPagedAsync_WithIncludeInactive_ReturnsAllExercises()
        {
            // Arrange
            var difficultyId = _context.DifficultyLevels.First().DifficultyLevelId;
            var activeExercise = ExerciseBuilder.AWorkoutExercise()
                .WithName("Active Exercise")
                .WithDescription("Active description")
                .WithDifficultyId(difficultyId)
                .Build();
            var inactiveExercise = ExerciseBuilder.AnInactiveExercise()
                .WithName("Inactive Exercise")
                .WithDescription("Inactive description")
                .WithDifficultyId(difficultyId)
                .Build();
            
            _context.Exercises.AddRange(activeExercise, inactiveExercise);
            await _context.SaveChangesAsync();

            // Act
            var (exercises, totalCount) = await _repository.GetPagedAsync(
                1, 
                10, 
                string.Empty,
                DifficultyLevelId.Empty,
                new List<MuscleGroupId>(),
                new List<EquipmentId>(),
                new List<MovementPatternId>(),
                new List<BodyPartId>(),
                includeInactive: true);

            // Assert
            Assert.Equal(2, totalCount);
            Assert.Equal(2, exercises.Count());
            Assert.Contains(exercises, e => e.IsActive);
            Assert.Contains(exercises, e => !e.IsActive);
        }

        [Fact]
        public async Task GetPagedAsync_WithNameFilter_ReturnsMatchingExercises()
        {
            // Arrange
            var difficultyId = _context.DifficultyLevels.First().DifficultyLevelId;
            var exercise1 = Exercise.Handler.CreateNew(
                "Bench Press", "Chest exercise", null, null, false, difficultyId);
            var exercise2 = Exercise.Handler.CreateNew(
                "Overhead Press", "Shoulder exercise", null, null, false, difficultyId);
            var exercise3 = Exercise.Handler.CreateNew(
                "Squat", "Leg exercise", null, null, false, difficultyId);
            
            _context.Exercises.AddRange(exercise1, exercise2, exercise3);
            await _context.SaveChangesAsync();

            // Act
            var (exercises, totalCount) = await _repository.GetPagedAsync(
                1, 
                10, 
                "press",
                DifficultyLevelId.Empty,
                new List<MuscleGroupId>(),
                new List<EquipmentId>(),
                new List<MovementPatternId>(),
                new List<BodyPartId>());

            // Assert
            Assert.Equal(2, totalCount);
            Assert.All(exercises, e => Assert.Contains("Press", e.Name));
        }

        [Fact]
        public async Task GetPagedAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var difficultyId = _context.DifficultyLevels.First().DifficultyLevelId;
            for (int i = 1; i <= 15; i++)
            {
                var exercise = Exercise.Handler.CreateNew(
                    $"Exercise {i:D2}", $"Description {i}", null, null, false, difficultyId);
                _context.Exercises.Add(exercise);
            }
            await _context.SaveChangesAsync();

            // Act
            var (firstPage, totalCount) = await _repository.GetPagedAsync(
                1, 
                10, 
                string.Empty,
                DifficultyLevelId.Empty,
                new List<MuscleGroupId>(),
                new List<EquipmentId>(),
                new List<MovementPatternId>(),
                new List<BodyPartId>());
            var (secondPage, _) = await _repository.GetPagedAsync(
                2, 
                10, 
                string.Empty,
                DifficultyLevelId.Empty,
                new List<MuscleGroupId>(),
                new List<EquipmentId>(),
                new List<MovementPatternId>(),
                new List<BodyPartId>());

            // Assert
            Assert.Equal(15, totalCount);
            Assert.Equal(10, firstPage.Count());
            Assert.Equal(5, secondPage.Count());
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsExerciseWithRelatedData()
        {
            // Arrange
            var difficultyId = _context.DifficultyLevels.First().DifficultyLevelId;
            var muscleGroupId = _context.MuscleGroups.First().Id;
            var muscleRoleId = _context.MuscleRoles.First().MuscleRoleId;
            var equipmentId = _context.Equipment.First().EquipmentId;
            
            var exercise = Exercise.Handler.CreateNew(
                "Bench Press", "Chest exercise", null, null, false, difficultyId);
            
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();
            
            // Add relationships after the exercise is saved
            var exerciseMuscleGroup = ExerciseMuscleGroup.Handler.Create(exercise.Id, muscleGroupId, muscleRoleId);
            var exerciseEquipment = ExerciseEquipment.Handler.Create(exercise.Id, equipmentId);
            
            _context.ExerciseMuscleGroups.Add(exerciseMuscleGroup);
            _context.ExerciseEquipment.Add(exerciseEquipment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(exercise.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Bench Press", result.Name);
            Assert.NotNull(result.Difficulty);
            Assert.Single(result.ExerciseMuscleGroups);
            Assert.Single(result.ExerciseEquipment);
        }

        [Fact]
        public async Task GetByNameAsync_WithExistingName_ReturnsExercise()
        {
            // Arrange
            var difficultyId = _context.DifficultyLevels.First().DifficultyLevelId;
            var exercise = Exercise.Handler.CreateNew(
                "Unique Exercise Name", "Description", null, null, false, difficultyId);
            
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByNameAsync("UNIQUE EXERCISE NAME");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Unique Exercise Name", result.Name);
        }

        [Fact]
        public async Task ExistsAsync_WithExistingName_ReturnsTrue()
        {
            // Arrange
            var difficultyId = _context.DifficultyLevels.First().DifficultyLevelId;
            var exercise = Exercise.Handler.CreateNew(
                "Existing Exercise", "Description", null, null, false, difficultyId);
            
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();

            // Act
            var exists = await _repository.ExistsAsync("existing exercise");

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_WithExcludeId_ExcludesSpecifiedExercise()
        {
            // Arrange
            var difficultyId = _context.DifficultyLevels.First().DifficultyLevelId;
            var exercise = Exercise.Handler.CreateNew(
                "My Exercise", "Description", null, null, false, difficultyId);
            
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();

            // Act
            var existsWithoutExclude = await _repository.ExistsAsync("my exercise");
            var existsWithExclude = await _repository.ExistsAsync("my exercise", exercise.Id);

            // Assert
            Assert.True(existsWithoutExclude);
            Assert.False(existsWithExclude);
        }

        [Fact]
        public async Task HasReferencesAsync_WithNoReferences_ReturnsFalse()
        {
            // Arrange
            var difficultyId = _context.DifficultyLevels.First().DifficultyLevelId;
            var exercise = Exercise.Handler.CreateNew(
                "Exercise", "Description", null, null, false, difficultyId);
            
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();

            // Act
            var hasReferences = await _repository.HasReferencesAsync(exercise.Id);

            // Assert
            Assert.False(hasReferences);
        }

        [Fact]
        public async Task AddAsync_CreatesNewExercise()
        {
            // Arrange
            var difficultyId = _context.DifficultyLevels.First().DifficultyLevelId;
            var exercise = Exercise.Handler.CreateNew(
                "New Exercise", "Description", null, null, false, difficultyId);

            // Act
            var result = await _repository.AddAsync(exercise);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(default(ExerciseId), result.Id);
            var savedExercise = await _context.Exercises.FindAsync(result.Id);
            Assert.NotNull(savedExercise);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesExistingExercise()
        {
            // Arrange
            var difficultyId = _context.DifficultyLevels.First().DifficultyLevelId;
            var exercise = Exercise.Handler.CreateNew(
                "Original Name", "Description", null, null, false, difficultyId);
            
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();

            var updatedExercise = Exercise.Handler.Create(
                exercise.Id, "Updated Name", "Updated Description", 
                null, null, false, true, difficultyId);

            // Act
            var result = await _repository.UpdateAsync(updatedExercise);

            // Assert
            Assert.Equal("Updated Name", result.Name);
            Assert.Equal("Updated Description", result.Description);
        }

        [Fact]
        public async Task DeleteAsync_RemovesExercise()
        {
            // Arrange
            var difficultyId = _context.DifficultyLevels.First().DifficultyLevelId;
            var exercise = Exercise.Handler.CreateNew(
                "To Delete", "Description", null, null, false, difficultyId);
            
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteAsync(exercise.Id);

            // Assert
            Assert.True(result);
            var deletedExercise = await _context.Exercises.FindAsync(exercise.Id);
            Assert.Null(deletedExercise);
        }

        [Fact]
        public async Task GetPagedAsync_WithMultipleMuscleGroups_ReturnsExercise()
        {
            // Arrange
            var difficultyId = _context.DifficultyLevels.First().DifficultyLevelId;
            var chestMuscleId = _context.MuscleGroups.First().Id;
            var muscleRoleId = _context.MuscleRoles.First().MuscleRoleId;
            
            // Create a second muscle group (Triceps)
            var tricepsMuscle = MuscleGroup.Handler.Create(
                MuscleGroupId.New(), "Triceps", _context.BodyParts.First().BodyPartId);
            _context.MuscleGroups.Add(tricepsMuscle);
            await _context.SaveChangesAsync();
            
            // Create exercise
            var exercise = Exercise.Handler.CreateNew(
                "Bench Press Multi", "Compound chest exercise", 
                null, null, false, difficultyId);
            
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();
            
            // Add multiple muscle groups to the exercise
            var chestAssociation = ExerciseMuscleGroup.Handler.Create(
                exercise.Id, chestMuscleId, muscleRoleId);
            var tricepsAssociation = ExerciseMuscleGroup.Handler.Create(
                exercise.Id, tricepsMuscle.Id, muscleRoleId);
            
            _context.ExerciseMuscleGroups.AddRange(chestAssociation, tricepsAssociation);
            await _context.SaveChangesAsync();

            // Act
            var (exercises, totalCount) = await _repository.GetPagedAsync(
                1, 
                10, 
                string.Empty,
                DifficultyLevelId.Empty,
                new List<MuscleGroupId>(),
                new List<EquipmentId>(),
                new List<MovementPatternId>(),
                new List<BodyPartId>());

            // Assert
            Assert.Equal(1, totalCount);
            Assert.Single(exercises);
            Assert.Equal("Bench Press Multi", exercises.First().Name);
            Assert.Equal(2, exercises.First().ExerciseMuscleGroups.Count);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
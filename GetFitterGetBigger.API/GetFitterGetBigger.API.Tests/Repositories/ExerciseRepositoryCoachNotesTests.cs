using System;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Repositories
{
    public class ExerciseRepositoryCoachNotesTests : IDisposable
    {
        private readonly FitnessDbContext _context;
        private readonly ExerciseRepository _repository;

        public ExerciseRepositoryCoachNotesTests()
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
                DifficultyLevelId.New(), "Beginner", "For beginners", 1, true);
            _context.DifficultyLevels.Add(beginnerDifficulty);
            
            // Add exercise types from seed data
            var warmupType = ExerciseType.Handler.Create(
                ExerciseTypeId.New(), "Warmup", "Exercises for warming up", 1, true);
            var workoutType = ExerciseType.Handler.Create(
                ExerciseTypeId.New(), "Workout", "Main workout exercises", 2, true);
            var cooldownType = ExerciseType.Handler.Create(
                ExerciseTypeId.New(), "Cooldown", "Cool down exercises", 3, true);
            
            _context.ExerciseTypes.AddRange(warmupType, workoutType, cooldownType);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetByIdAsync_WithCoachNotes_ReturnsExerciseWithOrderedCoachNotes()
        {
            // Arrange
            var difficultyId = _context.DifficultyLevels.First().Id;
            var exercise = Exercise.Handler.CreateNew(
                "Squat", "Lower body exercise", null, null, false, difficultyId);
            
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();
            
            // Add coach notes in non-sequential order
            var note1 = CoachNote.Handler.Create(
                CoachNoteId.New(), exercise.Id, "Stand with feet shoulder-width apart", 2);
            var note2 = CoachNote.Handler.Create(
                CoachNoteId.New(), exercise.Id, "Keep your back straight", 0);
            var note3 = CoachNote.Handler.Create(
                CoachNoteId.New(), exercise.Id, "Lower until thighs are parallel", 1);
            
            _context.CoachNotes.AddRange(note1, note2, note3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(exercise.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.CoachNotes.Count);
            
            // Verify notes are present with correct content and order values
            Assert.Contains(result.CoachNotes, n => n.Text == "Keep your back straight" && n.Order == 0);
            Assert.Contains(result.CoachNotes, n => n.Text == "Lower until thighs are parallel" && n.Order == 1);
            Assert.Contains(result.CoachNotes, n => n.Text == "Stand with feet shoulder-width apart" && n.Order == 2);
        }

        [Fact]
        public async Task GetByIdAsync_WithExerciseTypes_ReturnsExerciseWithTypes()
        {
            // Arrange
            var difficultyId = _context.DifficultyLevels.First().Id;
            var warmupTypeId = _context.ExerciseTypes.First(et => et.Value == "Warmup").Id;
            var workoutTypeId = _context.ExerciseTypes.First(et => et.Value == "Workout").Id;
            
            var exercise = Exercise.Handler.CreateNew(
                "Jumping Jacks", "Full body warmup", null, null, false, difficultyId);
            
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();
            
            // Add exercise types
            var warmupAssociation = ExerciseExerciseType.Handler.Create(exercise.Id, warmupTypeId);
            var workoutAssociation = ExerciseExerciseType.Handler.Create(exercise.Id, workoutTypeId);
            
            _context.ExerciseExerciseTypes.AddRange(warmupAssociation, workoutAssociation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(exercise.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.ExerciseExerciseTypes.Count);
            Assert.Contains(result.ExerciseExerciseTypes, eet => eet.ExerciseType?.Value == "Warmup");
            Assert.Contains(result.ExerciseExerciseTypes, eet => eet.ExerciseType?.Value == "Workout");
        }

        [Fact]
        public async Task GetPagedAsync_WithCoachNotes_ReturnsExercisesWithCoachNotes()
        {
            // Arrange
            var difficultyId = _context.DifficultyLevels.First().Id;
            
            // Create exercises
            var exercise1 = Exercise.Handler.CreateNew(
                "Push-up", "Upper body exercise", null, null, false, difficultyId);
            var exercise2 = Exercise.Handler.CreateNew(
                "Pull-up", "Back exercise", null, null, false, difficultyId);
            
            _context.Exercises.AddRange(exercise1, exercise2);
            await _context.SaveChangesAsync();
            
            // Add coach notes
            var note1 = CoachNote.Handler.Create(
                CoachNoteId.New(), exercise1.Id, "Keep core tight", 0);
            var note2 = CoachNote.Handler.Create(
                CoachNoteId.New(), exercise2.Id, "Use full range of motion", 0);
            
            _context.CoachNotes.AddRange(note1, note2);
            await _context.SaveChangesAsync();

            // Act
            var (exercises, totalCount) = await _repository.GetPagedAsync(1, 10);

            // Assert
            Assert.Equal(2, totalCount);
            var exerciseList = exercises.ToList();
            
            var pushUp = exerciseList.First(e => e.Name == "Push-up");
            Assert.Single(pushUp.CoachNotes);
            Assert.Equal("Keep core tight", pushUp.CoachNotes.First().Text);
            
            var pullUp = exerciseList.First(e => e.Name == "Pull-up");
            Assert.Single(pullUp.CoachNotes);
            Assert.Equal("Use full range of motion", pullUp.CoachNotes.First().Text);
        }

        [Fact]
        public async Task GetPagedAsync_WithExerciseTypes_ReturnsExercisesWithTypes()
        {
            // Arrange
            var difficultyId = _context.DifficultyLevels.First().Id;
            var cooldownTypeId = _context.ExerciseTypes.First(et => et.Value == "Cooldown").Id;
            
            var exercise = Exercise.Handler.CreateNew(
                "Stretching", "Cool down exercise", null, null, false, difficultyId);
            
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();
            
            // Add exercise type
            var cooldownAssociation = ExerciseExerciseType.Handler.Create(exercise.Id, cooldownTypeId);
            _context.ExerciseExerciseTypes.Add(cooldownAssociation);
            await _context.SaveChangesAsync();

            // Act
            var (exercises, totalCount) = await _repository.GetPagedAsync(1, 10);

            // Assert
            Assert.Equal(1, totalCount);
            var stretchingExercise = exercises.First();
            Assert.Single(stretchingExercise.ExerciseExerciseTypes);
            Assert.Equal("Cooldown", stretchingExercise.ExerciseExerciseTypes.First().ExerciseType?.Value);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
using System;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Repositories
{
    public class ExerciseKineticChainTests : IDisposable
    {
        private readonly FitnessDbContext _context;
        private readonly ExerciseRepository _repository;

        public ExerciseKineticChainTests()
        {
            var options = new DbContextOptionsBuilder<FitnessDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new FitnessDbContext(options);
            _repository = new ExerciseRepository();
            _repository.SetContext(_context);
        }

        [Fact]
        public async Task Exercise_WithKineticChain_SavesAndLoadsCorrectly()
        {
            // Arrange
            var difficultyId = DifficultyLevelId.New();
            var difficulty = DifficultyLevel.Handler.Create(
                difficultyId, "Intermediate", "For intermediate users", 2, true).Value;
            _context.DifficultyLevels.Add(difficulty);

            var kineticChainId = KineticChainTypeId.New();
            var kineticChain = KineticChainType.Handler.Create(
                kineticChainId, "Closed Chain", "Movement where the distal segment is fixed", 1, true);
            _context.KineticChainTypes.Add(kineticChain);
            
            await _context.SaveChangesAsync();

            var exercise = Exercise.Handler.CreateNew(
                "Squat",
                "Lower body compound exercise",
                null,
                null,
                false,
                difficultyId,
                kineticChainId);

            // Act
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();
            
            // Load the exercise with navigation property
            var savedExercise = await _context.Exercises
                .Include(e => e.KineticChain)
                .FirstOrDefaultAsync(e => e.Id == exercise.Id);

            // Assert
            Assert.NotNull(savedExercise);
            Assert.NotNull(savedExercise.KineticChainId);
            Assert.Equal(kineticChainId, savedExercise.KineticChainId);
            Assert.NotNull(savedExercise.KineticChain);
            Assert.Equal("Closed Chain", savedExercise.KineticChain.Value);
        }

        [Fact]
        public async Task Exercise_WithoutKineticChain_SavesWithNullKineticChain()
        {
            // Arrange
            var difficultyId = DifficultyLevelId.New();
            var difficulty = DifficultyLevel.Handler.Create(
                difficultyId, "Beginner", "For beginners", 1, true).Value;
            _context.DifficultyLevels.Add(difficulty);
            await _context.SaveChangesAsync();

            var exercise = Exercise.Handler.CreateNew(
                "Rest",
                "Rest period between exercises",
                null,
                null,
                false,
                difficultyId,
                null); // No kinetic chain for rest

            // Act
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();
            
            var savedExercise = await _context.Exercises
                .Include(e => e.KineticChain)
                .FirstOrDefaultAsync(e => e.Id == exercise.Id);

            // Assert
            Assert.NotNull(savedExercise);
            Assert.Null(savedExercise.KineticChainId);
            Assert.Null(savedExercise.KineticChain);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
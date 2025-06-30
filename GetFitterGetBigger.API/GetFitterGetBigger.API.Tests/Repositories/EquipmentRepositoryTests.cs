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
    public class EquipmentRepositoryTests : IDisposable
    {
        private readonly FitnessDbContext _context;
        private readonly EquipmentRepository _repository;

        public EquipmentRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<FitnessDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new FitnessDbContext(options);
            _repository = new EquipmentRepository();
            _repository.SetContext(_context);
            
            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Add equipment
            var barbell = Equipment.Handler.Create(
                EquipmentId.New(), "Barbell", true, DateTime.UtcNow.AddDays(-10));
            var dumbbell = Equipment.Handler.Create(
                EquipmentId.New(), "Dumbbell", true, DateTime.UtcNow.AddDays(-10));
            var inactiveEquipment = Equipment.Handler.Create(
                EquipmentId.New(), "Old Machine", false, DateTime.UtcNow.AddDays(-10));
            
            _context.Equipment.AddRange(barbell, dumbbell, inactiveEquipment);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_ReturnsOnlyActiveEquipment()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, e => Assert.True(e.IsActive));
            Assert.DoesNotContain(result, e => e.Name == "Old Machine");
        }

        [Fact]
        public async Task GetByIdAsync_WhenExists_ReturnsEquipment()
        {
            // Arrange
            var equipment = await _context.Equipment.FirstAsync(e => e.Name == "Barbell");

            // Act
            var result = await _repository.GetByIdAsync(equipment.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Barbell", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotExists_ReturnsNull()
        {
            // Arrange
            var nonExistentId = EquipmentId.New();

            // Act
            var result = await _repository.GetByIdAsync(nonExistentId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByNameAsync_WhenActiveExists_ReturnsEquipment()
        {
            // Act
            var result = await _repository.GetByNameAsync("Barbell");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Barbell", result.Name);
            Assert.True(result.IsActive);
        }

        [Fact]
        public async Task GetByNameAsync_WhenInactive_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByNameAsync("Old Machine");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByNameAsync_IsCaseInsensitive()
        {
            // Act
            var result = await _repository.GetByNameAsync("BARBELL");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Barbell", result.Name);
        }

        [Fact]
        public async Task CreateAsync_CreatesNewEquipment()
        {
            // Arrange
            var newEquipment = Equipment.Handler.CreateNew("Kettlebell");

            // Act
            var result = await _repository.CreateAsync(newEquipment);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Kettlebell", result.Name);
            Assert.True(result.IsActive);
            
            // Verify it was saved
            var saved = await _context.Equipment.FindAsync(result.Id);
            Assert.NotNull(saved);
            Assert.Equal("Kettlebell", saved.Name);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesExistingEquipment()
        {
            // Arrange
            var equipment = await _context.Equipment.FirstAsync(e => e.Name == "Barbell");
            var updated = Equipment.Handler.Update(equipment, "Olympic Barbell");

            // Act
            var result = await _repository.UpdateAsync(updated);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Olympic Barbell", result.Name);
            Assert.NotNull(result.UpdatedAt);
            
            // Verify it was saved
            var saved = await _context.Equipment.FindAsync(result.Id);
            Assert.NotNull(saved);
            Assert.Equal("Olympic Barbell", saved.Name);
        }

        [Fact]
        public async Task DeactivateAsync_WhenExists_DeactivatesEquipment()
        {
            // Arrange
            var equipment = await _context.Equipment.FirstAsync(e => e.Name == "Barbell");

            // Act
            var result = await _repository.DeactivateAsync(equipment.Id);

            // Assert
            Assert.True(result);
            
            // Verify it was deactivated
            var deactivated = await _context.Equipment.FindAsync(equipment.Id);
            Assert.NotNull(deactivated);
            Assert.False(deactivated.IsActive);
            Assert.NotNull(deactivated.UpdatedAt);
        }

        [Fact]
        public async Task DeactivateAsync_WhenNotExists_ReturnsFalse()
        {
            // Arrange
            var nonExistentId = EquipmentId.New();

            // Act
            var result = await _repository.DeactivateAsync(nonExistentId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_WhenActiveExists_ReturnsTrue()
        {
            // Act
            var result = await _repository.ExistsAsync("Barbell");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WhenInactive_ReturnsFalse()
        {
            // Act
            var result = await _repository.ExistsAsync("Old Machine");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_WhenNotExists_ReturnsFalse()
        {
            // Act
            var result = await _repository.ExistsAsync("NonExistent");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_WithExcludeId_ExcludesSpecifiedId()
        {
            // Arrange
            var equipment = await _context.Equipment.FirstAsync(e => e.Name == "Barbell");

            // Act
            var result = await _repository.ExistsAsync("Barbell", equipment.Id);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_IsCaseInsensitive()
        {
            // Act
            var result = await _repository.ExistsAsync("BARBELL");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsInUseAsync_WhenNotUsed_ReturnsFalse()
        {
            // Arrange
            var equipment = await _context.Equipment.FirstAsync(e => e.Name == "Barbell");

            // Act
            var result = await _repository.IsInUseAsync(equipment.Id);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsInUseAsync_WhenUsedByExercise_ReturnsTrue()
        {
            // Arrange
            var equipment = await _context.Equipment.FirstAsync(e => e.Name == "Barbell");
            
            // Create an exercise that uses this equipment
            var difficulty = DifficultyLevel.Handler.Create(
                DifficultyLevelId.New(), "Beginner", "For beginners", 1, true);
            _context.DifficultyLevels.Add(difficulty);
            
            var exercise = Exercise.Handler.CreateNew(
                "Barbell Squat", "Leg exercise", null, null, false, difficulty.Id);
            _context.Exercises.Add(exercise);
            
            var exerciseEquipment = ExerciseEquipment.Handler.Create(
                exercise.Id, equipment.Id);
            _context.ExerciseEquipment.Add(exerciseEquipment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.IsInUseAsync(equipment.Id);

            // Assert
            Assert.True(result);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
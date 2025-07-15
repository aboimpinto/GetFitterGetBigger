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
    public class MuscleGroupRepositoryTests : IDisposable
    {
        private readonly FitnessDbContext _context;
        private readonly MuscleGroupRepository _repository;
        private readonly BodyPartId _upperBodyId;
        private readonly BodyPartId _lowerBodyId;

        public MuscleGroupRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<FitnessDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new FitnessDbContext(options);
            _repository = new MuscleGroupRepository();
            _repository.SetContext(_context);
            
            // Initialize IDs
            _upperBodyId = BodyPartId.New();
            _lowerBodyId = BodyPartId.New();
            
            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Add body parts
            var upperBody = BodyPart.Handler.Create(
                _upperBodyId, "Upper Body", null, 1, true).Value;
            var lowerBody = BodyPart.Handler.Create(
                _lowerBodyId, "Lower Body", null, 2, true).Value;
            _context.BodyParts.AddRange(upperBody, lowerBody);
            
            // Add muscle groups
            var chest = MuscleGroup.Handler.Create(
                MuscleGroupId.New(), "Chest", _upperBodyId, true, DateTime.UtcNow.AddDays(-10));
            var biceps = MuscleGroup.Handler.Create(
                MuscleGroupId.New(), "Biceps", _upperBodyId, true, DateTime.UtcNow.AddDays(-10));
            var quadriceps = MuscleGroup.Handler.Create(
                MuscleGroupId.New(), "Quadriceps", _lowerBodyId, true, DateTime.UtcNow.AddDays(-10));
            var inactiveGroup = MuscleGroup.Handler.Create(
                MuscleGroupId.New(), "Inactive Group", _upperBodyId, false, DateTime.UtcNow.AddDays(-10));
            
            _context.MuscleGroups.AddRange(chest, biceps, quadriceps, inactiveGroup);
            
            // Add difficulty level for exercises
            var beginnerDifficulty = DifficultyLevel.Handler.Create(
                DifficultyLevelId.New(), "Beginner", "For beginners", 1, true).Value;
            _context.DifficultyLevels.Add(beginnerDifficulty);
            
            // Add an exercise that uses the chest muscle group
            var benchPress = Exercise.Handler.CreateNew(
                "Bench Press", 
                "Chest exercise", 
                null, // videoUrl
                null, // imageUrl
                false, // isUnilateral
                beginnerDifficulty.DifficultyLevelId);
            _context.Exercises.Add(benchPress);
            
            // Add muscle role for the relationship
            var primaryRoleResult = MuscleRole.Handler.Create(
                MuscleRoleId.New(), "Primary", "Primary muscle", 1, true);
            if (!primaryRoleResult.IsSuccess)
                throw new InvalidOperationException("Failed to create primary role");
            _context.MuscleRoles.Add(primaryRoleResult.Value);
            
            // Add exercise-muscle group relationship
            var exerciseMuscleGroup = ExerciseMuscleGroup.Handler.Create(
                benchPress.Id,
                chest.Id,
                primaryRoleResult.Value.MuscleRoleId);
            _context.ExerciseMuscleGroups.Add(exerciseMuscleGroup);
            
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetAllAsync_ReturnsOnlyActiveMuscleGroups()
        {
            // Act
            var result = await _repository.GetAllAsync();
            
            // Assert
            Assert.Equal(3, result.Count());
            Assert.All(result, mg => Assert.True(mg.IsActive));
            Assert.All(result, mg => Assert.NotNull(mg.BodyPart));
        }
        
        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsMuscleGroup()
        {
            // Arrange
            var chest = await _context.MuscleGroups.FirstAsync(mg => mg.Name == "Chest");
            
            // Act
            var result = await _repository.GetByIdAsync(chest.Id);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("Chest", result.Name);
            Assert.NotNull(result.BodyPart);
        }
        
        [Fact]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(MuscleGroupId.New());
            
            // Assert
            Assert.Null(result);
        }
        
        [Fact]
        public async Task GetByNameAsync_ExistingActiveName_ReturnsMuscleGroup()
        {
            // Act
            var result = await _repository.GetByNameAsync("chest");
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("Chest", result.Name);
            Assert.True(result.IsActive);
        }
        
        [Fact]
        public async Task GetByNameAsync_InactiveName_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByNameAsync("Inactive Group");
            
            // Assert
            Assert.Null(result);
        }
        
        [Fact]
        public async Task GetByBodyPartAsync_ReturnsOnlyActiveMuscleGroupsForBodyPart()
        {
            // Act
            var result = await _repository.GetByBodyPartAsync(_upperBodyId);
            
            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, mg => Assert.Equal(_upperBodyId, mg.BodyPartId));
            Assert.All(result, mg => Assert.True(mg.IsActive));
        }
        
        [Fact]
        public async Task CreateAsync_ValidData_CreatesMuscleGroup()
        {
            // Arrange
            var newMuscleGroup = MuscleGroup.Handler.CreateNew("Triceps", _upperBodyId);
            
            // Act
            var result = await _repository.CreateAsync(newMuscleGroup);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("Triceps", result.Name);
            Assert.NotNull(result.BodyPart);
            Assert.Equal("Upper Body", result.BodyPart.Value);
            
            // Verify it was saved
            var saved = await _context.MuscleGroups.FindAsync(result.Id);
            Assert.NotNull(saved);
        }
        
        [Fact]
        public async Task UpdateAsync_ExistingMuscleGroup_UpdatesSuccessfully()
        {
            // Arrange
            var chest = await _context.MuscleGroups.FirstAsync(mg => mg.Name == "Chest");
            var updated = MuscleGroup.Handler.Update(chest, "Pectorals", _upperBodyId);
            
            // Act
            var result = await _repository.UpdateAsync(updated);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("Pectorals", result.Name);
            Assert.NotNull(result.UpdatedAt);
            Assert.NotNull(result.BodyPart);
            
            // Verify it was saved
            var saved = await _context.MuscleGroups.FindAsync(result.Id);
            Assert.Equal("Pectorals", saved!.Name);
        }
        
        [Fact]
        public async Task DeactivateAsync_ExistingMuscleGroup_DeactivatesSuccessfully()
        {
            // Arrange
            var biceps = await _context.MuscleGroups.FirstAsync(mg => mg.Name == "Biceps");
            
            // Act
            var result = await _repository.DeactivateAsync(biceps.Id);
            
            // Assert
            Assert.True(result);
            
            // Verify it was deactivated
            var saved = await _context.MuscleGroups.FindAsync(biceps.Id);
            Assert.False(saved!.IsActive);
            Assert.NotNull(saved.UpdatedAt);
        }
        
        [Fact]
        public async Task DeactivateAsync_NonExistingMuscleGroup_ReturnsFalse()
        {
            // Act
            var result = await _repository.DeactivateAsync(MuscleGroupId.New());
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public async Task ExistsByNameAsync_ExistingActiveName_ReturnsTrue()
        {
            // Act
            var result = await _repository.ExistsByNameAsync("chest");
            
            // Assert
            Assert.True(result);
        }
        
        [Fact]
        public async Task ExistsByNameAsync_NonExistingName_ReturnsFalse()
        {
            // Act
            var result = await _repository.ExistsByNameAsync("NonExistent");
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public async Task ExistsByNameAsync_WithExcludeId_ExcludesSpecifiedId()
        {
            // Arrange
            var chest = await _context.MuscleGroups.FirstAsync(mg => mg.Name == "Chest");
            
            // Act
            var result = await _repository.ExistsByNameAsync("chest", chest.Id);
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public async Task CanDeactivateAsync_NoActiveExercises_ReturnsTrue()
        {
            // Arrange
            var biceps = await _context.MuscleGroups.FirstAsync(mg => mg.Name == "Biceps");
            
            // Act
            var result = await _repository.CanDeactivateAsync(biceps.Id);
            
            // Assert
            Assert.True(result);
        }
        
        [Fact]
        public async Task CanDeactivateAsync_WithActiveExercises_ReturnsFalse()
        {
            // Arrange
            var chest = await _context.MuscleGroups.FirstAsync(mg => mg.Name == "Chest");
            
            // Act
            var result = await _repository.CanDeactivateAsync(chest.Id);
            
            // Assert
            Assert.False(result);
        }
        
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
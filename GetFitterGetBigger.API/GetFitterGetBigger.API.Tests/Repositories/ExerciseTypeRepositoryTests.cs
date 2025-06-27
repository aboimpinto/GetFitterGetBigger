using System;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Repositories;

public class ExerciseTypeRepositoryTests : IDisposable
{
    private readonly FitnessDbContext _context;
    private readonly ExerciseTypeRepository _repository;
    
    public ExerciseTypeRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<FitnessDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _context = new FitnessDbContext(options);
        _repository = new ExerciseTypeRepository();
        _repository.SetContext(_context);
        
        SeedTestData();
    }
    
    private void SeedTestData()
    {
        var exerciseTypes = new[]
        {
            ExerciseType.Handler.Create(
                ExerciseTypeId.From(Guid.Parse("11111111-1111-1111-1111-111111111111")),
                "Warmup", 
                "Exercises to prepare the body for workout", 
                1, 
                true),
            ExerciseType.Handler.Create(
                ExerciseTypeId.From(Guid.Parse("22222222-2222-2222-2222-222222222222")),
                "Workout", 
                "Main workout exercises", 
                2, 
                true),
            ExerciseType.Handler.Create(
                ExerciseTypeId.From(Guid.Parse("33333333-3333-3333-3333-333333333333")),
                "Cooldown", 
                "Exercises to cool down after workout", 
                3, 
                true),
            ExerciseType.Handler.Create(
                ExerciseTypeId.From(Guid.Parse("44444444-4444-4444-4444-444444444444")),
                "Rest", 
                "Rest period between exercises", 
                4, 
                true),
            ExerciseType.Handler.Create(
                ExerciseTypeId.From(Guid.Parse("55555555-5555-5555-5555-555555555555")),
                "Inactive Type", 
                "This type is inactive", 
                5, 
                false) // Inactive
        };
        
        _context.Set<ExerciseType>().AddRange(exerciseTypes);
        _context.SaveChanges();
    }
    
    [Fact]
    public async Task GetAllActiveAsync_ReturnsOnlyActiveItemsOrderedByDisplayOrder()
    {
        // Act
        var result = await _repository.GetAllActiveAsync();
        var exerciseTypes = result.ToList();
        
        // Assert
        Assert.Equal(4, exerciseTypes.Count); // Only 4 active types
        Assert.DoesNotContain(exerciseTypes, et => et.Value == "Inactive Type");
        
        // Verify ordering
        Assert.Equal("Warmup", exerciseTypes[0].Value);
        Assert.Equal("Workout", exerciseTypes[1].Value);
        Assert.Equal("Cooldown", exerciseTypes[2].Value);
        Assert.Equal("Rest", exerciseTypes[3].Value);
    }
    
    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCorrectItem()
    {
        // Arrange
        var id = ExerciseTypeId.From(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        
        // Act
        var result = await _repository.GetByIdAsync(id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Warmup", result.Value);
        Assert.Equal("Exercises to prepare the body for workout", result.Description);
        Assert.Equal(1, result.DisplayOrder);
        Assert.True(result.IsActive);
    }
    
    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var id = ExerciseTypeId.New();
        
        // Act
        var result = await _repository.GetByIdAsync(id);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task GetByIdAsync_ReturnsDetachedEntity()
    {
        // Arrange
        var id = ExerciseTypeId.From(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        
        // Act
        var result = await _repository.GetByIdAsync(id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(EntityState.Detached, _context.Entry(result).State);
    }
    
    [Fact]
    public async Task GetByValueAsync_ExistingValue_ReturnsCorrectItem()
    {
        // Act
        var result = await _repository.GetByValueAsync("Workout");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Workout", result.Value);
        Assert.Equal("Main workout exercises", result.Description);
    }
    
    [Fact]
    public async Task GetByValueAsync_CaseInsensitive_ReturnsCorrectItem()
    {
        // Act - test with different case
        var result = await _repository.GetByValueAsync("COOLDOWN");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Cooldown", result.Value); // Original casing preserved
    }
    
    [Fact]
    public async Task GetByValueAsync_InactiveItem_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByValueAsync("Inactive Type");
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task GetByValueAsync_NonExistingValue_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByValueAsync("NonExistent");
        
        // Assert
        Assert.Null(result);
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
}
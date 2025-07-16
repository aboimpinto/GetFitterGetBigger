using System;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Implementations;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Repositories;

public class WorkoutCategoryRepositoryTests : IDisposable
{
    private readonly FitnessDbContext _context;
    private readonly WorkoutCategoryRepository _repository;
    
    public WorkoutCategoryRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<FitnessDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _context = new FitnessDbContext(options);
        _repository = new WorkoutCategoryRepository();
        _repository.SetContext(_context);
        
        SeedTestData();
    }
    
    private void SeedTestData()
    {
        var workoutCategories = new[]
        {
            WorkoutCategoryTestBuilder.UpperBodyPush().Build(),
            WorkoutCategoryTestBuilder.UpperBodyPull().Build(),
            WorkoutCategoryTestBuilder.LowerBody().Build(),
            WorkoutCategoryTestBuilder.Core().Build(),
            WorkoutCategoryTestBuilder.InactiveCategory().Build()
        };
        
        _context.Set<WorkoutCategory>().AddRange(workoutCategories);
        _context.SaveChanges();
    }
    
    [Fact]
    public async Task GetAllActiveAsync_ReturnsOnlyActiveItemsOrderedByDisplayOrder()
    {
        // Act
        var result = await _repository.GetAllActiveAsync();
        var workoutCategories = result.ToList();
        
        // Assert
        Assert.Equal(4, workoutCategories.Count); // Only 4 active categories
        Assert.DoesNotContain(workoutCategories, wc => wc.Value == "Inactive Category");
        
        // Verify ordering
        Assert.Equal("Upper Body Push", workoutCategories[0].Value);
        Assert.Equal("Upper Body Pull", workoutCategories[1].Value);
        Assert.Equal("Lower Body", workoutCategories[2].Value);
        Assert.Equal("Core", workoutCategories[3].Value);
    }
    
    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCorrectItem()
    {
        // Arrange
        var id = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.UpperBodyPush);
        
        // Act
        var result = await _repository.GetByIdAsync(id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Upper Body Push", result.Value);
        Assert.Equal("Pushing exercises for chest, shoulders, and triceps", result.Description);
        Assert.Equal("push-icon", result.Icon);
        Assert.Equal("#FF5733", result.Color);
        Assert.Equal("Chest, Shoulders, Triceps", result.PrimaryMuscleGroups);
        Assert.Equal(1, result.DisplayOrder);
        Assert.True(result.IsActive);
    }
    
    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsEmpty()
    {
        // Arrange
        var id = WorkoutCategoryId.From(Guid.Parse("99999999-9999-9999-9999-999999999999"));
        
        // Act
        var result = await _repository.GetByIdAsync(id);
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsEmpty);
        Assert.Equal(WorkoutCategory.Empty, result);
    }
    
    [Fact]
    public async Task GetByIdAsync_InactiveId_ReturnsItem()
    {
        // Arrange
        var id = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.InactiveCategory);
        
        // Act
        var result = await _repository.GetByIdAsync(id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Inactive Category", result.Value);
        Assert.False(result.IsActive);
    }
    
    [Fact]
    public async Task GetByValueAsync_ExistingActiveValue_ReturnsCorrectItem()
    {
        // Act
        var result = await _repository.GetByValueAsync("Upper Body Push");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Upper Body Push", result.Value);
        Assert.True(result.IsActive);
    }
    
    [Fact]
    public async Task GetByValueAsync_CaseInsensitive_ReturnsCorrectItem()
    {
        // Act
        var result = await _repository.GetByValueAsync("UPPER BODY PUSH");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Upper Body Push", result.Value);
    }
    
    [Fact]
    public async Task GetByValueAsync_InactiveValue_ReturnsEmpty()
    {
        // Act
        var result = await _repository.GetByValueAsync("Inactive Category");
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsEmpty); // Should return Empty for inactive items
        Assert.Equal(WorkoutCategory.Empty, result);
    }
    
    [Fact]
    public async Task GetByValueAsync_NonExistingValue_ReturnsEmpty()
    {
        // Act
        var result = await _repository.GetByValueAsync("Non-Existing Category");
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsEmpty);
        Assert.Equal(WorkoutCategory.Empty, result);
    }
    
    public void Dispose()
    {
        _context?.Dispose();
    }
}
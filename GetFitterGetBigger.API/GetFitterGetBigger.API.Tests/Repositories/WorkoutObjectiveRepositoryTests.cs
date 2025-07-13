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

public class WorkoutObjectiveRepositoryTests : IDisposable
{
    private readonly FitnessDbContext _context;
    private readonly WorkoutObjectiveRepository _repository;
    
    public WorkoutObjectiveRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<FitnessDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _context = new FitnessDbContext(options);
        _repository = new WorkoutObjectiveRepository();
        _repository.SetContext(_context);
        
        SeedTestData();
    }
    
    private void SeedTestData()
    {
        var workoutObjectives = new[]
        {
            WorkoutObjectiveTestBuilder.MuscularStrength().Build(),
            WorkoutObjectiveTestBuilder.MuscularHypertrophy().Build(),
            WorkoutObjectiveTestBuilder.MuscularEndurance().Build(),
            WorkoutObjectiveTestBuilder.PowerDevelopment().Build(),
            WorkoutObjectiveTestBuilder.InactiveObjective().Build()
        };
        
        _context.Set<WorkoutObjective>().AddRange(workoutObjectives);
        _context.SaveChanges();
    }
    
    [Fact]
    public async Task GetAllActiveAsync_ReturnsOnlyActiveItemsOrderedByDisplayOrder()
    {
        // Act
        var result = await _repository.GetAllActiveAsync();
        var workoutObjectives = result.ToList();
        
        // Assert
        Assert.Equal(4, workoutObjectives.Count); // Only 4 active objectives
        Assert.DoesNotContain(workoutObjectives, wo => wo.Value == "Inactive Objective");
        
        // Verify ordering
        Assert.Equal("Muscular Strength", workoutObjectives[0].Value);
        Assert.Equal("Muscular Hypertrophy", workoutObjectives[1].Value);
        Assert.Equal("Muscular Endurance", workoutObjectives[2].Value);
        Assert.Equal("Power Development", workoutObjectives[3].Value);
    }
    
    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCorrectItem()
    {
        // Arrange
        var id = WorkoutObjectiveId.From(TestIds.WorkoutObjectiveIds.MuscularStrength);
        
        // Act
        var result = await _repository.GetByIdAsync(id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Muscular Strength", result.Value);
        Assert.Equal("Build maximum strength through heavy loads and low repetitions", result.Description);
        Assert.Equal(1, result.DisplayOrder);
        Assert.True(result.IsActive);
    }
    
    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var id = WorkoutObjectiveId.From(Guid.Parse("99999999-9999-9999-9999-999999999999"));
        
        // Act
        var result = await _repository.GetByIdAsync(id);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task GetByIdAsync_InactiveId_ReturnsItem()
    {
        // Arrange
        var id = WorkoutObjectiveId.From(TestIds.WorkoutObjectiveIds.InactiveObjective);
        
        // Act
        var result = await _repository.GetByIdAsync(id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Inactive Objective", result.Value);
        Assert.False(result.IsActive);
    }
    
    [Fact]
    public async Task GetByValueAsync_ExistingActiveValue_ReturnsCorrectItem()
    {
        // Act
        var result = await _repository.GetByValueAsync("Muscular Strength");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Muscular Strength", result.Value);
        Assert.True(result.IsActive);
    }
    
    [Fact]
    public async Task GetByValueAsync_CaseInsensitive_ReturnsCorrectItem()
    {
        // Act
        var result = await _repository.GetByValueAsync("MUSCULAR STRENGTH");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Muscular Strength", result.Value);
    }
    
    [Fact]
    public async Task GetByValueAsync_InactiveValue_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByValueAsync("Inactive Objective");
        
        // Assert
        Assert.Null(result); // Should not return inactive items
    }
    
    [Fact]
    public async Task GetByValueAsync_NonExistingValue_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByValueAsync("Non-Existing Objective");
        
        // Assert
        Assert.Null(result);
    }
    
    public void Dispose()
    {
        _context?.Dispose();
    }
}
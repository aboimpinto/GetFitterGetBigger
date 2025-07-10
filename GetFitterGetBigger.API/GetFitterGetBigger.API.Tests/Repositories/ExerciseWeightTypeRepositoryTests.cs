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

public class ExerciseWeightTypeRepositoryTests : IDisposable
{
    private readonly FitnessDbContext _context;
    private readonly ExerciseWeightTypeRepository _repository;
    
    public ExerciseWeightTypeRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<FitnessDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _context = new FitnessDbContext(options);
        _repository = new ExerciseWeightTypeRepository();
        _repository.SetContext(_context);
        
        SeedTestData();
    }
    
    private void SeedTestData()
    {
        var exerciseWeightTypes = new[]
        {
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeId.From(Guid.Parse("a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a")),
                "BODYWEIGHT_ONLY",
                "Bodyweight Only", 
                "Exercises that cannot have external weight added", 
                1, 
                true),
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeId.From(Guid.Parse("b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f")),
                "BODYWEIGHT_OPTIONAL",
                "Bodyweight Optional", 
                "Exercises that can be performed with or without additional weight", 
                2, 
                true),
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeId.From(Guid.Parse("c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a")),
                "WEIGHT_REQUIRED",
                "Weight Required", 
                "Exercises that must have external weight specified", 
                3, 
                true),
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeId.From(Guid.Parse("d4c6b5a7-8c9d-7e0f-1a2b-3c4d5e6f7a8b")),
                "MACHINE_WEIGHT",
                "Machine Weight", 
                "Exercises performed on machines with weight stacks", 
                4, 
                true),
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeId.From(Guid.Parse("e5b7a698-9d0e-8f1a-2b3c-4d5e6f7a8b9c")),
                "NO_WEIGHT",
                "No Weight", 
                "Exercises that do not use weight as a metric", 
                5, 
                true),
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeId.From(Guid.Parse("f6c8b7a9-0e1d-9f2a-3b4c-5d6e7f8a9b0c")),
                "INACTIVE_TYPE",
                "Inactive Type", 
                "This type is inactive", 
                6, 
                false) // Inactive
        };
        
        _context.Set<ExerciseWeightType>().AddRange(exerciseWeightTypes);
        _context.SaveChanges();
    }
    
    [Fact]
    public async Task GetAllActiveAsync_ReturnsOnlyActiveItemsOrderedByDisplayOrder()
    {
        // Act
        var result = await _repository.GetAllActiveAsync();
        var weightTypes = result.ToList();
        
        // Assert
        Assert.Equal(5, weightTypes.Count); // Only 5 active types
        Assert.DoesNotContain(weightTypes, wt => wt.Code == "INACTIVE_TYPE");
        
        // Verify ordering
        Assert.Equal("BODYWEIGHT_ONLY", weightTypes[0].Code);
        Assert.Equal("BODYWEIGHT_OPTIONAL", weightTypes[1].Code);
        Assert.Equal("WEIGHT_REQUIRED", weightTypes[2].Code);
        Assert.Equal("MACHINE_WEIGHT", weightTypes[3].Code);
        Assert.Equal("NO_WEIGHT", weightTypes[4].Code);
    }
    
    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCorrectItem()
    {
        // Arrange
        var id = ExerciseWeightTypeId.From(Guid.Parse("a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a"));
        
        // Act
        var result = await _repository.GetByIdAsync(id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("BODYWEIGHT_ONLY", result.Code);
        Assert.Equal("Bodyweight Only", result.Value);
        Assert.Equal("Exercises that cannot have external weight added", result.Description);
        Assert.Equal(1, result.DisplayOrder);
        Assert.True(result.IsActive);
    }
    
    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var id = ExerciseWeightTypeId.New();
        
        // Act
        var result = await _repository.GetByIdAsync(id);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task GetByIdAsync_ReturnsDetachedEntity()
    {
        // Arrange
        var id = ExerciseWeightTypeId.From(Guid.Parse("c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a"));
        
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
        var result = await _repository.GetByValueAsync("Weight Required");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("WEIGHT_REQUIRED", result.Code);
        Assert.Equal("Weight Required", result.Value);
        Assert.Equal("Exercises that must have external weight specified", result.Description);
    }
    
    [Fact]
    public async Task GetByValueAsync_CaseInsensitive_ReturnsCorrectItem()
    {
        // Act - test with different case
        var result = await _repository.GetByValueAsync("MACHINE weight");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Machine Weight", result.Value); // Original casing preserved
        Assert.Equal("MACHINE_WEIGHT", result.Code);
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
    
    [Fact]
    public async Task GetByCodeAsync_ExistingCode_ReturnsCorrectItem()
    {
        // Act
        var result = await _repository.GetByCodeAsync("BODYWEIGHT_OPTIONAL");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("BODYWEIGHT_OPTIONAL", result.Code);
        Assert.Equal("Bodyweight Optional", result.Value);
        Assert.Equal("Exercises that can be performed with or without additional weight", result.Description);
    }
    
    [Fact]
    public async Task GetByCodeAsync_CaseSensitive_ReturnsNull()
    {
        // Act - test with different case (should be case-sensitive)
        var result = await _repository.GetByCodeAsync("bodyweight_only");
        
        // Assert
        Assert.Null(result); // Code comparison is case-sensitive
    }
    
    [Fact]
    public async Task GetByCodeAsync_InactiveItem_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByCodeAsync("INACTIVE_TYPE");
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task GetByCodeAsync_NonExistingCode_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByCodeAsync("NON_EXISTENT_CODE");
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task GetByCodeAsync_NullCode_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByCodeAsync(null!);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task GetByCodeAsync_EmptyCode_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByCodeAsync(string.Empty);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task GetByCodeAsync_ReturnsDetachedEntity()
    {
        // Act
        var result = await _repository.GetByCodeAsync("NO_WEIGHT");
        
        // Assert
        Assert.NotNull(result);
        // The entity should not be tracked since we use AsNoTracking
        Assert.Equal(EntityState.Detached, _context.Entry(result).State);
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
}
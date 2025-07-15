using System;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Implementations;
using GetFitterGetBigger.API.Tests.Constants;
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
                ExerciseWeightTypeTestConstants.BodyweightOnlyId,
                ExerciseWeightTypeTestConstants.BodyweightOnlyCode,
                ExerciseWeightTypeTestConstants.BodyweightOnlyValue, 
                ExerciseWeightTypeTestConstants.BodyweightOnlyDescription, 
                1, 
                true).Value,
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeTestConstants.BodyweightOptionalId,
                ExerciseWeightTypeTestConstants.BodyweightOptionalCode,
                ExerciseWeightTypeTestConstants.BodyweightOptionalValue, 
                ExerciseWeightTypeTestConstants.BodyweightOptionalDescription, 
                2, 
                true).Value,
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeTestConstants.WeightRequiredId,
                ExerciseWeightTypeTestConstants.WeightRequiredCode,
                ExerciseWeightTypeTestConstants.WeightRequiredValue, 
                ExerciseWeightTypeTestConstants.WeightRequiredDescription, 
                3, 
                true).Value,
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeTestConstants.MachineWeightId,
                ExerciseWeightTypeTestConstants.MachineWeightCode,
                ExerciseWeightTypeTestConstants.MachineWeightValue, 
                ExerciseWeightTypeTestConstants.MachineWeightDescription, 
                4, 
                true).Value,
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeTestConstants.NoWeightId,
                ExerciseWeightTypeTestConstants.NoWeightCode,
                ExerciseWeightTypeTestConstants.NoWeightValue, 
                ExerciseWeightTypeTestConstants.NoWeightDescription, 
                5, 
                true).Value,
            ExerciseWeightType.Handler.Create(
                ExerciseWeightTypeTestConstants.InactiveTypeId,
                ExerciseWeightTypeTestConstants.InactiveTypeCode,
                ExerciseWeightTypeTestConstants.InactiveTypeValue, 
                ExerciseWeightTypeTestConstants.InactiveTypeDescription, 
                6, 
                false).Value // Inactive
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
        Assert.DoesNotContain(weightTypes, wt => wt.Code == ExerciseWeightTypeTestConstants.InactiveTypeCode);
        
        // Verify ordering
        Assert.Equal(ExerciseWeightTypeTestConstants.BodyweightOnlyCode, weightTypes[0].Code);
        Assert.Equal(ExerciseWeightTypeTestConstants.BodyweightOptionalCode, weightTypes[1].Code);
        Assert.Equal(ExerciseWeightTypeTestConstants.WeightRequiredCode, weightTypes[2].Code);
        Assert.Equal(ExerciseWeightTypeTestConstants.MachineWeightCode, weightTypes[3].Code);
        Assert.Equal(ExerciseWeightTypeTestConstants.NoWeightCode, weightTypes[4].Code);
    }
    
    [Fact]
    public async Task GetAllAsync_ReturnsAllItemsIncludingInactive()
    {
        // Act
        var result = await _repository.GetAllAsync();
        var weightTypes = result.ToList();
        
        // Assert
        Assert.Equal(6, weightTypes.Count); // All 6 types including inactive
        Assert.Contains(weightTypes, wt => wt.Code == ExerciseWeightTypeTestConstants.InactiveTypeCode);
    }
    
    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCorrectItem()
    {
        // Arrange
        var id = ExerciseWeightTypeTestConstants.BodyweightOnlyId;
        
        // Act
        var result = await _repository.GetByIdAsync(id);
        
        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsEmpty);
        Assert.Equal(ExerciseWeightTypeTestConstants.BodyweightOnlyCode, result.Code);
        Assert.Equal(ExerciseWeightTypeTestConstants.BodyweightOnlyValue, result.Value);
        Assert.Equal(ExerciseWeightTypeTestConstants.BodyweightOnlyDescription, result.Description);
    }
    
    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsEmpty()
    {
        // Arrange
        var id = ExerciseWeightTypeId.New();
        
        // Act
        var result = await _repository.GetByIdAsync(id);
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsEmpty);
        Assert.Equal(ExerciseWeightType.Empty, result);
    }
    
    [Fact]
    public async Task GetByIdAsync_ReturnsDetachedEntity()
    {
        // Arrange
        var id = ExerciseWeightTypeTestConstants.WeightRequiredId;
        
        // Act
        var result = await _repository.GetByIdAsync(id);
        
        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsEmpty);
        Assert.Equal(EntityState.Detached, _context.Entry(result).State);
    }
    
    [Fact]
    public async Task GetByValueAsync_ExistingValue_ReturnsCorrectItem()
    {
        // Act
        var result = await _repository.GetByValueAsync(ExerciseWeightTypeTestConstants.WeightRequiredValue);
        
        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsEmpty);
        Assert.Equal(ExerciseWeightTypeTestConstants.WeightRequiredCode, result.Code);
        Assert.Equal(ExerciseWeightTypeTestConstants.WeightRequiredValue, result.Value);
        Assert.Equal(ExerciseWeightTypeTestConstants.WeightRequiredDescription, result.Description);
    }
    
    [Fact]
    public async Task GetByValueAsync_CaseInsensitive_ReturnsCorrectItem()
    {
        // Act - test with different case
        var result = await _repository.GetByValueAsync(ExerciseWeightTypeTestConstants.MixedCaseMachineWeightValue);
        
        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsEmpty);
        Assert.Equal(ExerciseWeightTypeTestConstants.MachineWeightValue, result.Value); // Original casing preserved
        Assert.Equal(ExerciseWeightTypeTestConstants.MachineWeightCode, result.Code);
    }
    
    [Fact]
    public async Task GetByValueAsync_InactiveItem_ReturnsEmpty()
    {
        // Act
        var result = await _repository.GetByValueAsync(ExerciseWeightTypeTestConstants.InactiveTypeValue);
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsEmpty);
        Assert.Equal(ExerciseWeightType.Empty, result);
    }
    
    [Fact]
    public async Task GetByValueAsync_NonExistingValue_ReturnsEmpty()
    {
        // Act
        var result = await _repository.GetByValueAsync(ExerciseWeightTypeTestConstants.NonExistentValue);
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsEmpty);
        Assert.Equal(ExerciseWeightType.Empty, result);
    }
    
    [Fact]
    public async Task GetByCodeAsync_ExistingCode_ReturnsCorrectItem()
    {
        // Act
        var result = await _repository.GetByCodeAsync(ExerciseWeightTypeTestConstants.BodyweightOptionalCode);
        
        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsEmpty);
        Assert.Equal(ExerciseWeightTypeTestConstants.BodyweightOptionalCode, result.Code);
        Assert.Equal(ExerciseWeightTypeTestConstants.BodyweightOptionalValue, result.Value);
        Assert.Equal(ExerciseWeightTypeTestConstants.BodyweightOptionalDescription, result.Description);
    }
    
    [Fact]
    public async Task GetByCodeAsync_CaseSensitive_ReturnsEmpty()
    {
        // Act - test with different case (should be case-sensitive)
        var result = await _repository.GetByCodeAsync(ExerciseWeightTypeTestConstants.LowercaseBodyweightOnlyCode);
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsEmpty); // Code comparison is case-sensitive
        Assert.Equal(ExerciseWeightType.Empty, result);
    }
    
    [Fact]
    public async Task GetByCodeAsync_InactiveItem_ReturnsEmpty()
    {
        // Act
        var result = await _repository.GetByCodeAsync(ExerciseWeightTypeTestConstants.InactiveTypeCode);
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsEmpty);
        Assert.Equal(ExerciseWeightType.Empty, result);
    }
    
    [Fact]
    public async Task GetByCodeAsync_NonExistingCode_ReturnsEmpty()
    {
        // Act
        var result = await _repository.GetByCodeAsync(ExerciseWeightTypeTestConstants.NonExistentCode);
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsEmpty);
        Assert.Equal(ExerciseWeightType.Empty, result);
    }
    
    [Fact]
    public async Task GetByCodeAsync_NullCode_ReturnsEmpty()
    {
        // Act
        var result = await _repository.GetByCodeAsync(null!);
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsEmpty);
        Assert.Equal(ExerciseWeightType.Empty, result);
    }
    
    [Fact]
    public async Task GetByCodeAsync_EmptyCode_ReturnsEmpty()
    {
        // Act
        var result = await _repository.GetByCodeAsync(string.Empty);
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsEmpty);
        Assert.Equal(ExerciseWeightType.Empty, result);
    }
    
    [Fact]
    public async Task GetByCodeAsync_ReturnsDetachedEntity()
    {
        // Act
        var result = await _repository.GetByCodeAsync(ExerciseWeightTypeTestConstants.NoWeightCode);
        
        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsEmpty);
        Assert.Equal(EntityState.Detached, _context.Entry(result).State);
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
}
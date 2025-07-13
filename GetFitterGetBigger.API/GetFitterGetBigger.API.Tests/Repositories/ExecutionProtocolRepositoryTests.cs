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

public class ExecutionProtocolRepositoryTests : IDisposable
{
    private readonly FitnessDbContext _context;
    private readonly ExecutionProtocolRepository _repository;
    
    public ExecutionProtocolRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<FitnessDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _context = new FitnessDbContext(options);
        _repository = new ExecutionProtocolRepository();
        _repository.SetContext(_context);
        
        SeedTestData();
    }
    
    private void SeedTestData()
    {
        var executionProtocols = new[]
        {
            ExecutionProtocolTestBuilder.Standard().Build(),
            ExecutionProtocolTestBuilder.Superset().Build(),
            ExecutionProtocolTestBuilder.DropSet().Build(),
            ExecutionProtocolTestBuilder.AMRAP().Build(),
            ExecutionProtocolTestBuilder.InactiveProtocol().Build()
        };
        
        _context.Set<ExecutionProtocol>().AddRange(executionProtocols);
        _context.SaveChanges();
    }
    
    [Fact]
    public async Task GetAllActiveAsync_ReturnsOnlyActiveItemsOrderedByDisplayOrder()
    {
        // Act
        var result = await _repository.GetAllActiveAsync();
        var executionProtocols = result.ToList();
        
        // Assert
        Assert.Equal(4, executionProtocols.Count); // Only 4 active protocols
        Assert.DoesNotContain(executionProtocols, ep => ep.Value == "Inactive Protocol");
        
        // Verify ordering
        Assert.Equal("Standard", executionProtocols[0].Value);
        Assert.Equal("Superset", executionProtocols[1].Value);
        Assert.Equal("Drop Set", executionProtocols[2].Value);
        Assert.Equal("AMRAP", executionProtocols[3].Value);
    }
    
    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCorrectItem()
    {
        // Arrange
        var id = ExecutionProtocolId.From(TestIds.ExecutionProtocolIds.Standard);
        
        // Act
        var result = await _repository.GetByIdAsync(id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("STANDARD", result.Code);
        Assert.Equal("Standard", result.Value);
        Assert.Equal("Traditional set and rep scheme", result.Description);
        Assert.False(result.TimeBase);
        Assert.True(result.RepBase);
        Assert.Equal("Fixed Rest", result.RestPattern);
        Assert.Equal("Moderate", result.IntensityLevel);
        Assert.Equal(1, result.DisplayOrder);
        Assert.True(result.IsActive);
    }
    
    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var id = ExecutionProtocolId.From(Guid.Parse("99999999-9999-9999-9999-999999999999"));
        
        // Act
        var result = await _repository.GetByIdAsync(id);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task GetByIdAsync_InactiveId_ReturnsItem()
    {
        // Arrange
        var id = ExecutionProtocolId.From(TestIds.ExecutionProtocolIds.InactiveProtocol);
        
        // Act
        var result = await _repository.GetByIdAsync(id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Inactive Protocol", result.Value);
        Assert.False(result.IsActive);
    }
    
    [Fact]
    public async Task GetByValueAsync_ExistingActiveValue_ReturnsCorrectItem()
    {
        // Act
        var result = await _repository.GetByValueAsync("Standard");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Standard", result.Value);
        Assert.Equal("STANDARD", result.Code);
        Assert.True(result.IsActive);
    }
    
    [Fact]
    public async Task GetByValueAsync_CaseInsensitive_ReturnsCorrectItem()
    {
        // Act
        var result = await _repository.GetByValueAsync("STANDARD");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("Standard", result.Value);
    }
    
    [Fact]
    public async Task GetByValueAsync_InactiveValue_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByValueAsync("Inactive Protocol");
        
        // Assert
        Assert.Null(result); // Should not return inactive items
    }
    
    [Fact]
    public async Task GetByValueAsync_NonExistingValue_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByValueAsync("Non-Existing Protocol");
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task GetByCodeAsync_ExistingActiveCode_ReturnsCorrectItem()
    {
        // Act
        var result = await _repository.GetByCodeAsync("STANDARD");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("STANDARD", result.Code);
        Assert.Equal("Standard", result.Value);
        Assert.True(result.IsActive);
    }
    
    [Fact]
    public async Task GetByCodeAsync_CaseInsensitive_ReturnsCorrectItem()
    {
        // Act
        var result = await _repository.GetByCodeAsync("standard");
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("STANDARD", result.Code);
        Assert.Equal("Standard", result.Value);
    }
    
    [Fact]
    public async Task GetByCodeAsync_InactiveCode_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByCodeAsync("INACTIVE");
        
        // Assert
        Assert.Null(result); // Should not return inactive items
    }
    
    [Fact]
    public async Task GetByCodeAsync_NonExistingCode_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByCodeAsync("NON_EXISTING");
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task GetByCodeAsync_VariousCodes_ReturnsCorrectItems()
    {
        // Act & Assert
        var superset = await _repository.GetByCodeAsync("SUPERSET");
        Assert.NotNull(superset);
        Assert.Equal("Superset", superset.Value);
        
        var dropset = await _repository.GetByCodeAsync("DROPSET");
        Assert.NotNull(dropset);
        Assert.Equal("Drop Set", dropset.Value);
        
        var amrap = await _repository.GetByCodeAsync("AMRAP");
        Assert.NotNull(amrap);
        Assert.Equal("AMRAP", amrap.Value);
    }
    
    public void Dispose()
    {
        _context?.Dispose();
    }
}
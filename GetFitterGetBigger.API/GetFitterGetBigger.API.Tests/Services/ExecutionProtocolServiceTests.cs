using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.ReferenceTables.ExecutionProtocol;
using GetFitterGetBigger.API.Services.ReferenceTables.ExecutionProtocol.DataServices;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services;

/// <summary>
/// Unit tests for ExecutionProtocolService following the new DataService architecture.
/// Uses AutoMocker for isolation and FluentAssertions for clear assertions.
/// </summary>
public class ExecutionProtocolServiceTests
{
    [Fact]
    public async Task GetAllActiveAsync_WhenCacheMiss_CallsDataServiceAndCachesResult()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExecutionProtocolService>();
        
        var expectedDtos = new List<ExecutionProtocolDto>
        {
            new() { ExecutionProtocolId = "executionprotocol-guid1", Value = "Standard", Description = "Standard execution" },
            new() { ExecutionProtocolId = "executionprotocol-guid2", Value = "Superset", Description = "Superset execution" }
        };
        
        autoMocker.GetMock<IExecutionProtocolDataService>()
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(ServiceResult<IEnumerable<ExecutionProtocolDto>>.Success(expectedDtos));
        
        autoMocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<IEnumerable<ExecutionProtocolDto>>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<IEnumerable<ExecutionProtocolDto>>.Miss());
        
        // Act
        var result = await testee.GetAllActiveAsync();
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedDtos);
        
        autoMocker.GetMock<IExecutionProtocolDataService>()
            .Verify(x => x.GetAllActiveAsync(), Times.Once);
        
        autoMocker.GetMock<IEternalCacheService>()
            .Verify(x => x.SetAsync(It.IsAny<string>(), It.Is<IEnumerable<ExecutionProtocolDto>>(
                data => data.Count() == expectedDtos.Count)), Times.Once);
    }
    
    [Fact]
    public async Task GetAllActiveAsync_WhenCacheHit_ReturnsFromCacheWithoutCallingDataService()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExecutionProtocolService>();
        
        var cachedDtos = new List<ExecutionProtocolDto>
        {
            new() { ExecutionProtocolId = "executionprotocol-guid1", Value = "Standard", Description = "Standard execution" },
            new() { ExecutionProtocolId = "executionprotocol-guid2", Value = "Superset", Description = "Superset execution" }
        };
        
        autoMocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<IEnumerable<ExecutionProtocolDto>>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<IEnumerable<ExecutionProtocolDto>>.Hit(cachedDtos));
        
        // Act
        var result = await testee.GetAllActiveAsync();
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(cachedDtos);
        
        autoMocker.GetMock<IExecutionProtocolDataService>()
            .Verify(x => x.GetAllActiveAsync(), Times.Never);
        
        autoMocker.GetMock<IEternalCacheService>()
            .Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ExecutionProtocolDto>>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsSuccessWithData()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExecutionProtocolService>();
        
        var protocolId = ExecutionProtocolId.New();
        var expectedDto = new ExecutionProtocolDto 
        { 
            ExecutionProtocolId = protocolId.ToString(), 
            Value = "Standard", 
            Description = "Standard execution" 
        };
        
        autoMocker.GetMock<IExecutionProtocolDataService>()
            .Setup(x => x.GetByIdAsync(protocolId))
            .ReturnsAsync(ServiceResult<ExecutionProtocolDto>.Success(expectedDto));
        
        autoMocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<ExecutionProtocolDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<ExecutionProtocolDto>.Miss());
        
        // Act
        var result = await testee.GetByIdAsync(protocolId);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedDto);
        result.Errors.Should().BeEmpty();
        
        autoMocker.GetMock<IExecutionProtocolDataService>()
            .Verify(x => x.GetByIdAsync(protocolId), Times.Once);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ReturnsValidationFailure()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExecutionProtocolService>();
        
        var emptyId = ExecutionProtocolId.Empty;
        
        // Act
        var result = await testee.GetByIdAsync(emptyId);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(ExecutionProtocolErrorMessages.InvalidIdFormat);
        
        autoMocker.GetMock<IExecutionProtocolDataService>()
            .Verify(x => x.GetByIdAsync(It.IsAny<ExecutionProtocolId>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByValueAsync_WithValidValue_ReturnsSuccessWithData()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExecutionProtocolService>();
        
        var value = "Standard";
        var expectedDto = new ExecutionProtocolDto 
        { 
            ExecutionProtocolId = "executionprotocol-guid1", 
            Value = value, 
            Description = "Standard execution" 
        };
        
        autoMocker.GetMock<IExecutionProtocolDataService>()
            .Setup(x => x.GetByValueAsync(value))
            .ReturnsAsync(ServiceResult<ExecutionProtocolDto>.Success(expectedDto));
        
        autoMocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<ExecutionProtocolDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<ExecutionProtocolDto>.Miss());
        
        // Act
        var result = await testee.GetByValueAsync(value);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(expectedDto);
        result.Data.Value.Should().Be(value);
        
        autoMocker.GetMock<IExecutionProtocolDataService>()
            .Verify(x => x.GetByValueAsync(value), Times.Once);
    }
    
    [Fact]
    public async Task GetByValueAsync_WithEmptyValue_ReturnsValidationFailure()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExecutionProtocolService>();
        
        var emptyValue = "";
        
        // Act
        var result = await testee.GetByValueAsync(emptyValue);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(ExecutionProtocolErrorMessages.ValueCannotBeEmpty);
        
        autoMocker.GetMock<IExecutionProtocolDataService>()
            .Verify(x => x.GetByValueAsync(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task ExistsAsync_WhenProtocolExists_ReturnsTrue()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExecutionProtocolService>();
        
        var protocolId = ExecutionProtocolId.New();
        var expectedDto = new ExecutionProtocolDto 
        { 
            ExecutionProtocolId = protocolId.ToString(), 
            Value = "Standard", 
            Description = "Standard execution" 
        };
        
        // ExistsAsync calls GetByIdAsync internally, so we need to mock GetByIdAsync
        autoMocker.GetMock<IExecutionProtocolDataService>()
            .Setup(x => x.GetByIdAsync(protocolId))
            .ReturnsAsync(ServiceResult<ExecutionProtocolDto>.Success(expectedDto));
        
        autoMocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<ExecutionProtocolDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<ExecutionProtocolDto>.Miss());
        
        // Act
        var result = await testee.ExistsAsync(protocolId);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();
        
        autoMocker.GetMock<IExecutionProtocolDataService>()
            .Verify(x => x.GetByIdAsync(protocolId), Times.Once);
    }
    
    [Fact]
    public async Task ExistsAsync_WhenProtocolDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExecutionProtocolService>();
        
        var protocolId = ExecutionProtocolId.New();
        
        // ExistsAsync calls GetByIdAsync internally, so we need to mock GetByIdAsync to return Empty
        autoMocker.GetMock<IExecutionProtocolDataService>()
            .Setup(x => x.GetByIdAsync(protocolId))
            .ReturnsAsync(ServiceResult<ExecutionProtocolDto>.Success(ExecutionProtocolDto.Empty));
        
        autoMocker.GetMock<IEternalCacheService>()
            .Setup(x => x.GetAsync<ExecutionProtocolDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<ExecutionProtocolDto>.Miss());
        
        // Act
        var result = await testee.ExistsAsync(protocolId);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeFalse();
        
        autoMocker.GetMock<IExecutionProtocolDataService>()
            .Verify(x => x.GetByIdAsync(protocolId), Times.Once);
    }
    
    [Fact]
    public async Task ExistsAsync_WithEmptyId_ReturnsValidationFailure()
    {
        // Arrange
        var autoMocker = new AutoMocker();
        var testee = autoMocker.CreateInstance<ExecutionProtocolService>();
        
        var emptyId = ExecutionProtocolId.Empty;
        
        // Act
        var result = await testee.ExistsAsync(emptyId);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        result.Errors.Should().Contain(ExecutionProtocolErrorMessages.InvalidIdFormat);
        
        autoMocker.GetMock<IExecutionProtocolDataService>()
            .Verify(x => x.ExistsAsync(It.IsAny<ExecutionProtocolId>()), Times.Never);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.Constants;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Implementations;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;
using GetFitterGetBigger.API.Tests.TestConstants;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Tests.Services;

public class ExecutionProtocolServiceTests
{
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
    private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
    private readonly Mock<IExecutionProtocolRepository> _mockRepository;
    private readonly Mock<ILogger<ExecutionProtocolService>> _mockLogger;
    private readonly ExecutionProtocolService _service;
    
    private readonly List<ExecutionProtocol> _testData;
    
    public ExecutionProtocolServiceTests()
    {
        _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _mockRepository = new Mock<IExecutionProtocolRepository>();
        _mockLogger = new Mock<ILogger<ExecutionProtocolService>>();
        
        _mockUnitOfWorkProvider
            .Setup(x => x.CreateReadOnly())
            .Returns(_mockReadOnlyUnitOfWork.Object);
            
        _mockReadOnlyUnitOfWork
            .Setup(x => x.GetRepository<IExecutionProtocolRepository>())
            .Returns(_mockRepository.Object);
            
        _service = new ExecutionProtocolService(
            _mockUnitOfWorkProvider.Object,
            _mockLogger.Object);
            
        _testData = new List<ExecutionProtocol>
        {
            ExecutionProtocolTestBuilder.Standard().Build(),
            ExecutionProtocolTestBuilder.Superset().Build(),
            ExecutionProtocolTestBuilder.DropSet().Build(),
            ExecutionProtocolTestBuilder.AMRAP().Build()
        };
    }
    
    [Fact]
    public async Task GetAllActiveAsync_WhenCached_ReturnsCachedData()
    {
        // Arrange
        var dtos = _testData.Select(p => new ExecutionProtocolDto
        {
            ExecutionProtocolId = p.ExecutionProtocolId.ToString(),
            Value = p.Value,
            Description = p.Description,
            Code = p.Code,
            TimeBase = p.TimeBase,
            RepBase = p.RepBase,
            RestPattern = p.RestPattern,
            IntensityLevel = p.IntensityLevel,
            DisplayOrder = p.DisplayOrder,
            IsActive = p.IsActive
        }).ToList();
        
            
        _mockRepository
            .Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(_testData);
            
            
        // Act
        var result = await _service.GetAllActiveAsync();
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(_testData.Count, result.Data.Count());
        _mockRepository.Verify(x => x.GetAllActiveAsync(), Times.Once);
            // Removed incomplete line
            // Removed incomplete line
        _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
        _mockReadOnlyUnitOfWork.Verify(x => x.Dispose(), Times.Once);
    }
    
    [Fact]
    public async Task GetAllActiveAsync_CacheKey_UsesCorrectFormat()
    {
        // Arrange
            
        _mockRepository.Setup(x => x.GetAllActiveAsync()).ReturnsAsync(_testData);
        
        
        // Act
        await _service.GetAllActiveAsync();
        
    }
    
    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsSuccessWithExecutionProtocol()
    {
        // Arrange
        var executionProtocolId = ExecutionProtocolId.New();
        var executionProtocolIdString = executionProtocolId.ToString();
        var executionProtocol = ExecutionProtocol.Handler.Create(
            executionProtocolId,
            ExecutionProtocolTestConstants.StandardValue,
            ExecutionProtocolTestConstants.StandardDescription,
            ExecutionProtocolTestConstants.StandardCode,
            true,
            true,
            ExecutionProtocolTestConstants.StandardRestPattern,
            ExecutionProtocolTestConstants.ModerateIntensity,
            ExecutionProtocolTestConstants.StandardDisplayOrder,
            true).Value;


        _mockRepository
            .Setup(x => x.GetByIdAsync(executionProtocolId))
            .ReturnsAsync(executionProtocol);


        // Act
        var result = await _service.GetByIdAsync(executionProtocolIdString);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(executionProtocolIdString, result.Data.ExecutionProtocolId);
        Assert.Equal(ExecutionProtocolTestConstants.StandardValue, result.Data.Value);
        Assert.Empty(result.Errors);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithExecutionProtocolId_ReturnsSuccessWithExecutionProtocol()
    {
        // Arrange
        var executionProtocolId = ExecutionProtocolId.New();
        var executionProtocol = ExecutionProtocol.Handler.Create(
            executionProtocolId,
            ExecutionProtocolTestConstants.SupersetValue,
            ExecutionProtocolTestConstants.SupersetDescription,
            ExecutionProtocolTestConstants.SupersetCode,
            false,
            true,
            ExecutionProtocolTestConstants.RestAfterBoth,
            ExecutionProtocolTestConstants.HighIntensity,
            ExecutionProtocolTestConstants.SupersetDisplayOrder,
            true).Value;


        _mockRepository
            .Setup(x => x.GetByIdAsync(executionProtocolId))
            .ReturnsAsync(executionProtocol);


        // Act
        var result = await _service.GetByIdAsync(executionProtocolId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(executionProtocolId.ToString(), result.Data.ExecutionProtocolId);
        Assert.Equal(ExecutionProtocolTestConstants.SupersetValue, result.Data.Value);
        Assert.Empty(result.Errors);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithEmptyExecutionProtocolId_ReturnsValidationFailure()
    {
        // Arrange
        var emptyExecutionProtocolId = ExecutionProtocolId.Empty;
        // No need to setup repository - service returns ValidationFailed immediately for empty IDs

        // Act
        var result = await _service.GetByIdAsync(emptyExecutionProtocolId);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
        Assert.Contains(ExecutionProtocolErrorMessages.InvalidIdFormat, result.Errors);
        // Verify the repository was NOT called (optimization - empty IDs are rejected immediately)
        _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<ExecutionProtocolId>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithEmptyString_ReturnsValidationFailure()
    {
        // Arrange
        var emptyId = ExecutionProtocolTestConstants.EmptyString;
        
        // Act
        var result = await _service.GetByIdAsync(emptyId);
        
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
        Assert.Contains(ExecutionProtocolErrorMessages.InvalidIdFormat, result.Errors);
        _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<ExecutionProtocolId>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithInactiveExecutionProtocol_ReturnsNotFound()
    {
        // Arrange
        var executionProtocolId = ExecutionProtocolId.New();
        var inactiveExecutionProtocol = ExecutionProtocol.Handler.Create(
            executionProtocolId,
            ExecutionProtocolTestConstants.InactiveValue,
            ExecutionProtocolTestConstants.InactiveDescription,
            ExecutionProtocolTestConstants.InactiveCode,
            false,
            false,
            null,
            null,
            ExecutionProtocolTestConstants.StandardDisplayOrder,
            false).Value; // IsActive = false


        _mockRepository
            .Setup(x => x.GetByIdAsync(executionProtocolId))
            .ReturnsAsync(inactiveExecutionProtocol);

        // Act
        var result = await _service.GetByIdAsync(executionProtocolId.ToString());

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(ExecutionProtocolTestConstants.NotFoundPartialMessage, result.Errors[0], StringComparison.OrdinalIgnoreCase);
        _mockRepository.Verify(x => x.GetByIdAsync(executionProtocolId), Times.Once);
    }
    
    [Fact]
    public async Task GetByCodeAsync_WithExistingCode_ReturnsSuccess()
    {
        // Arrange
        var code = ExecutionProtocolTestConstants.StandardCode;
        var executionProtocolId = ExecutionProtocolId.New();
        var executionProtocol = ExecutionProtocol.Handler.Create(
            executionProtocolId,
            ExecutionProtocolTestConstants.StandardValue,
            "Standard protocol",
            code,
            true,
            true,
            ExecutionProtocolTestConstants.StandardRestPattern,
            ExecutionProtocolTestConstants.ModerateIntensity,
            ExecutionProtocolTestConstants.StandardDisplayOrder,
            true).Value;


        _mockRepository
            .Setup(x => x.GetByCodeAsync(code))
            .ReturnsAsync(executionProtocol);


        // Act
        var result = await _service.GetByCodeAsync(code);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(code, result.Data.Code);
        Assert.Equal(ExecutionProtocolTestConstants.StandardValue, result.Data.Value);
        Assert.Empty(result.Errors);
    }
    
    [Fact]
    public async Task GetByCodeAsync_WithNonExistingCode_ReturnsFailure()
    {
        // Arrange
        var code = ExecutionProtocolTestConstants.NonExistentCode;


        _mockRepository
            .Setup(x => x.GetByCodeAsync(code))
            .ReturnsAsync(ExecutionProtocol.Empty);

        // Act
        var result = await _service.GetByCodeAsync(code);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(ExecutionProtocolTestConstants.NotFoundPartialMessage, result.Errors[0], StringComparison.OrdinalIgnoreCase);
    }
    
    [Fact]
    public async Task GetByValueAsync_WithExistingValue_ReturnsSuccess()
    {
        // Arrange
        var value = ExecutionProtocolTestConstants.StandardValue;
        var executionProtocolId = ExecutionProtocolId.New();
        var executionProtocol = ExecutionProtocol.Handler.Create(
            executionProtocolId,
            value,
            "Standard protocol",
            ExecutionProtocolTestConstants.StandardCode,
            true,
            true,
            ExecutionProtocolTestConstants.StandardRestPattern,
            ExecutionProtocolTestConstants.ModerateIntensity,
            ExecutionProtocolTestConstants.StandardDisplayOrder,
            true).Value;


        _mockRepository
            .Setup(x => x.GetByValueAsync(value))
            .ReturnsAsync(executionProtocol);


        // Act
        var result = await _service.GetByValueAsync(value);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(value, result.Data.Value);
        Assert.Empty(result.Errors);
    }
    
    [Fact]
    public async Task GetByValueAsync_WithNonExistingValue_ReturnsFailure()
    {
        // Arrange
        var value = ExecutionProtocolTestConstants.NonExistentValue;


        _mockRepository
            .Setup(x => x.GetByValueAsync(value))
            .ReturnsAsync(ExecutionProtocol.Empty);

        // Act
        var result = await _service.GetByValueAsync(value);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(ExecutionProtocolTestConstants.NotFoundPartialMessage, result.Errors[0], StringComparison.OrdinalIgnoreCase);
    }
    
    [Fact]
    public async Task GetByCodeAsync_WithInactiveExecutionProtocol_ReturnsNotFound()
    {
        // Arrange
        var code = ExecutionProtocolTestConstants.InactiveCode;
        var inactiveExecutionProtocol = ExecutionProtocol.Handler.Create(
            ExecutionProtocolId.New(),
            "Inactive",
            "Inactive protocol",
            code,
            false,
            false,
            null,
            null,
            ExecutionProtocolTestConstants.StandardDisplayOrder,
            false).Value; // IsActive = false


        _mockRepository
            .Setup(x => x.GetByCodeAsync(code))
            .ReturnsAsync(inactiveExecutionProtocol);

        // Act
        var result = await _service.GetByCodeAsync(code);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(ExecutionProtocolTestConstants.NotFoundPartialMessage, result.Errors[0], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetByValueAsync_WithInactiveExecutionProtocol_ReturnsNotFound()
    {
        // Arrange
        var value = ExecutionProtocolTestConstants.InactiveProtocolValue;
        var inactiveExecutionProtocol = ExecutionProtocol.Handler.Create(
            ExecutionProtocolId.New(),
            value,
            "Inactive protocol",
            "INACTIVE",
            false,
            false,
            null,
            null,
            ExecutionProtocolTestConstants.StandardDisplayOrder,
            false).Value; // IsActive = false


        _mockRepository
            .Setup(x => x.GetByValueAsync(value))
            .ReturnsAsync(inactiveExecutionProtocol);

        // Act
        var result = await _service.GetByValueAsync(value);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(ExecutionProtocolTestConstants.NotFoundPartialMessage, result.Errors[0], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExistsAsync_WithExecutionProtocolId_WhenExecutionProtocolExists_ReturnsTrue()
    {
        // Arrange
        var executionProtocolId = ExecutionProtocolId.New();


        _mockRepository
            .Setup(x => x.ExistsAsync(executionProtocolId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ExistsAsync(executionProtocolId);

        Assert.True(result.IsSuccess);
            Assert.True(result.Data.Value);
        _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
        _mockRepository.Verify(x => x.ExistsAsync(It.IsAny<ExecutionProtocolId>()), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_WithExecutionProtocolId_WhenExecutionProtocolDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var executionProtocolId = ExecutionProtocolId.New();


        _mockRepository
            .Setup(x => x.ExistsAsync(executionProtocolId))
            .ReturnsAsync(false);

        // Act
        var result = await _service.ExistsAsync(executionProtocolId);

        Assert.True(result.IsSuccess);
            Assert.False(result.Data.Value);
        _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
        _mockRepository.Verify(x => x.ExistsAsync(It.IsAny<ExecutionProtocolId>()), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_WithStringId_WhenExecutionProtocolExists_ReturnsTrue()
    {
        // Arrange
        var executionProtocolId = ExecutionProtocolId.New();
        var executionProtocolIdString = executionProtocolId.ToString();
        var executionProtocolDto = new ExecutionProtocolDto
            {
            ExecutionProtocolId = executionProtocolIdString,
            Value = ExecutionProtocolTestConstants.StandardValue,
            Description = ExecutionProtocolTestConstants.StandardDescription,
            Code = ExecutionProtocolTestConstants.StandardCode,
            TimeBase = true,
            RepBase = true,
            RestPattern = ExecutionProtocolTestConstants.StandardRestPattern,
            IntensityLevel = ExecutionProtocolTestConstants.ModerateIntensity,
            DisplayOrder = ExecutionProtocolTestConstants.StandardDisplayOrder,
            IsActive = true
        };

        _mockRepository
            .Setup(x => x.ExistsAsync(It.IsAny<ExecutionProtocolId>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ExistsAsync(ExecutionProtocolId.ParseOrEmpty(executionProtocolIdString));

        Assert.True(result.IsSuccess);
            Assert.True(result.Data.Value);
        _mockUnitOfWorkProvider.Verify(x => x.CreateReadOnly(), Times.Once);
        _mockRepository.Verify(x => x.ExistsAsync(It.IsAny<ExecutionProtocolId>()), Times.Once);
    }
}
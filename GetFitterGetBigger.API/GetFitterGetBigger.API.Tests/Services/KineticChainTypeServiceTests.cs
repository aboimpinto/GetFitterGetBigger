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
using Microsoft.Extensions.Logging;
using Moq;
using Olimpo.EntityFramework.Persistency;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services;

public class KineticChainTypeServiceTests
{
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _unitOfWorkProviderMock;
    private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _unitOfWorkMock;
    private readonly Mock<IKineticChainTypeRepository> _repositoryMock;
    private readonly Mock<IEmptyEnabledCacheService> _cacheServiceMock;
    private readonly Mock<ILogger<KineticChainTypeService>> _loggerMock;
    private readonly KineticChainTypeService _service;

    public KineticChainTypeServiceTests()
    {
        _unitOfWorkProviderMock = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _unitOfWorkMock = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _repositoryMock = new Mock<IKineticChainTypeRepository>();
        _cacheServiceMock = new Mock<IEmptyEnabledCacheService>();
        _loggerMock = new Mock<ILogger<KineticChainTypeService>>();

        _unitOfWorkProviderMock
            .Setup(p => p.CreateReadOnly())
            .Returns(_unitOfWorkMock.Object);

        _unitOfWorkMock
            .Setup(u => u.GetRepository<IKineticChainTypeRepository>())
            .Returns(_repositoryMock.Object);

        _service = new KineticChainTypeService(
            _unitOfWorkProviderMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllActiveAsync_ReturnsAllKineticChainTypes()
    {
        // Arrange
        var kineticChainTypes = new List<KineticChainType>
        {
            KineticChainTypeTestBuilder.Compound().Build(),
            KineticChainTypeTestBuilder.Isolation().Build()
        };

        _cacheServiceMock
            .Setup(c => c.GetAsync<IEnumerable<ReferenceDataDto>>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<IEnumerable<ReferenceDataDto>>.Miss());

        _repositoryMock
            .Setup(r => r.GetAllActiveAsync())
            .ReturnsAsync(kineticChainTypes);

        // Act
        var result = await _service.GetAllActiveAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count());
        
        var dtos = result.Data.ToList();
        Assert.Equal("COMPOUND", dtos[0].Value);
        Assert.Equal("ISOLATION", dtos[1].Value);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsKineticChainType()
    {
        // Arrange
        var kineticChainType = KineticChainTypeTestBuilder.Compound().Build();
        var id = KineticChainTypeId.ParseOrEmpty(TestIds.KineticChainTypeIds.Compound);

        _cacheServiceMock
            .Setup(c => c.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<KineticChainTypeId>()))
            .ReturnsAsync(kineticChainType);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("COMPOUND", result.Data.Value);
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ReturnsValidationError()
    {
        // Arrange
        var emptyId = KineticChainTypeId.Empty;

        // Act
        var result = await _service.GetByIdAsync(emptyId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Errors);
        Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
        Assert.Contains(KineticChainTypeErrorMessages.InvalidIdFormat, result.Errors.First());
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var id = KineticChainTypeId.ParseOrEmpty(TestIds.KineticChainTypeIds.NonExistent);

        _cacheServiceMock
            .Setup(c => c.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<KineticChainTypeId>()))
            .ReturnsAsync(KineticChainType.Empty);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Errors);
        Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
        Assert.Contains("KineticChainType not found", result.Errors.First());
    }

    [Fact]
    public async Task GetByValueAsync_WithValidValue_ReturnsKineticChainType()
    {
        // Arrange
        var kineticChainType = KineticChainTypeTestBuilder.Compound().Build();

        _cacheServiceMock
            .Setup(c => c.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

        _repositoryMock
            .Setup(r => r.GetByValueAsync("COMPOUND"))
            .ReturnsAsync(kineticChainType);

        // Act
        var result = await _service.GetByValueAsync("COMPOUND");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("COMPOUND", result.Data.Value);
    }

    [Fact]
    public async Task GetByValueAsync_WithEmptyValue_ReturnsValidationError()
    {
        // Act
        var result = await _service.GetByValueAsync("");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Errors);
        Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
    }

    [Fact]
    public async Task GetByValueAsync_WithNonExistentValue_ReturnsNotFound()
    {
        // Arrange
        _cacheServiceMock
            .Setup(c => c.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

        _repositoryMock
            .Setup(r => r.GetByValueAsync("NONEXISTENT"))
            .ReturnsAsync(KineticChainType.Empty);

        // Act
        var result = await _service.GetByValueAsync("NONEXISTENT");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Errors);
        Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
        Assert.Contains("Kinetic chain type not found with value 'NONEXISTENT'", result.Errors.First());
    }

    [Fact]
    public async Task GetByIdAsync_WithCacheHit_ReturnsCachedValue()
    {
        // Arrange
        var cachedDto = new ReferenceDataDto
        {
            Id = TestIds.KineticChainTypeIds.Compound,
            Value = "COMPOUND",
            Description = "Multi-joint movement engaging multiple muscle groups"
        };

        var id = KineticChainTypeId.ParseOrEmpty(TestIds.KineticChainTypeIds.Compound);

        _cacheServiceMock
            .Setup(c => c.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<ReferenceDataDto>.Hit(cachedDto));

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("COMPOUND", result.Data.Value);

        // Verify repository was not called
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<KineticChainTypeId>()), Times.Never);
    }

    [Fact]
    public async Task ExistsAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        var kineticChainType = KineticChainTypeTestBuilder.Compound().Build();
        var id = KineticChainTypeId.ParseOrEmpty(TestIds.KineticChainTypeIds.Compound);

        _cacheServiceMock
            .Setup(c => c.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<KineticChainTypeId>()))
            .ReturnsAsync(kineticChainType);

        // Act
        var result = await _service.ExistsAsync(id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_WithEmptyId_ReturnsFalse()
    {
        // Arrange
        var emptyId = KineticChainTypeId.Empty;

        // Act
        var result = await _service.ExistsAsync(emptyId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExistsAsync_WithInactiveKineticChainType_ReturnsFalse()
    {
        // Arrange
        var inactiveKineticChainType = KineticChainTypeTestBuilder.Compound()
            .IsActive(false)
            .Build();

        var id = KineticChainTypeId.ParseOrEmpty(TestIds.KineticChainTypeIds.Compound);

        _cacheServiceMock
            .Setup(c => c.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<KineticChainTypeId>()))
            .ReturnsAsync(inactiveKineticChainType);

        // Act
        var result = await _service.ExistsAsync(id);

        // Assert
        Assert.False(result);
    }
}
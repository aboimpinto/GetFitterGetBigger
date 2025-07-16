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
using GetFitterGetBigger.API.Services.Base;
using GetFitterGetBigger.API.Services.Implementations;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;
using GetFitterGetBigger.API.Tests.TestConstants;
using Microsoft.Extensions.Logging;
using Moq;
using Olimpo.EntityFramework.Persistency;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services;

public class MuscleRoleServiceTests
{
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _unitOfWorkProviderMock;
    private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _readOnlyUnitOfWorkMock;
    private readonly Mock<IMuscleRoleRepository> _repositoryMock;
    private readonly Mock<IEternalCacheService> _cacheServiceMock;
    private readonly Mock<ILogger<MuscleRoleService>> _loggerMock;
    private readonly MuscleRoleService _service;

    public MuscleRoleServiceTests()
    {
        _unitOfWorkProviderMock = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _readOnlyUnitOfWorkMock = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _repositoryMock = new Mock<IMuscleRoleRepository>();
        _cacheServiceMock = new Mock<IEternalCacheService>();
        _loggerMock = new Mock<ILogger<MuscleRoleService>>();

        _unitOfWorkProviderMock
            .Setup(p => p.CreateReadOnly())
            .Returns(_readOnlyUnitOfWorkMock.Object);

        _readOnlyUnitOfWorkMock
            .Setup(uow => uow.GetRepository<IMuscleRoleRepository>())
            .Returns(_repositoryMock.Object);

        _service = new MuscleRoleService(
            _unitOfWorkProviderMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllActiveAsync_ReturnsSuccessWithAllMuscleRoles()
    {
        // Arrange
        var muscleRoles = new List<MuscleRole>
        {
            MuscleRoleTestBuilder.Create(TestIds.MuscleRoleIds.Primary, MuscleRoleTestConstants.Values.Primary, MuscleRoleTestConstants.Descriptions.Primary),
            MuscleRoleTestBuilder.Create(TestIds.MuscleRoleIds.Secondary, MuscleRoleTestConstants.Values.Secondary, MuscleRoleTestConstants.Descriptions.Secondary)
        };

        _cacheServiceMock
            .Setup(c => c.GetAsync<IEnumerable<ReferenceDataDto>>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<IEnumerable<ReferenceDataDto>>.Miss());

        _repositoryMock
            .Setup(r => r.GetAllActiveAsync())
            .ReturnsAsync(muscleRoles);

        _cacheServiceMock
            .Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ReferenceDataDto>>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.GetAllActiveAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data.Count());
        Assert.Empty(result.Errors);
        _repositoryMock.Verify(r => r.GetAllActiveAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var id = MuscleRoleId.From(Guid.Parse(TestIds.MuscleRoleIds.Primary.Replace("musclerole-", "")));
        var entity = MuscleRoleTestBuilder.Create(TestIds.MuscleRoleIds.Primary, MuscleRoleTestConstants.Values.Primary, MuscleRoleTestConstants.Descriptions.Primary);

        _cacheServiceMock
            .Setup(c => c.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(entity);

        _cacheServiceMock
            .Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<ReferenceDataDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        Assert.True(result.IsSuccess);
        var dto = result.Data;
        Assert.Equal(entity.Id, dto.Id);
        Assert.Equal(entity.Value, dto.Value);
        Assert.Equal(entity.Description, dto.Description);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ReturnsValidationError()
    {
        // Arrange & Act
        var result = await _service.GetByIdAsync(MuscleRoleId.Empty);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
        Assert.Contains(MuscleRoleErrorMessages.InvalidIdFormat, result.Errors.First());
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var id = MuscleRoleId.New();

        _cacheServiceMock
            .Setup(c => c.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<MuscleRoleId>()))
            .ReturnsAsync(MuscleRole.Empty);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
    }

    [Fact]
    public async Task GetByValueAsync_WithValidValue_ReturnsSuccess()
    {
        // Arrange
        var value = MuscleRoleTestConstants.Values.Primary;
        var entity = MuscleRoleTestBuilder.Create(TestIds.MuscleRoleIds.Primary, value, "Primary muscle");

        _cacheServiceMock
            .Setup(c => c.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

        _repositoryMock
            .Setup(r => r.GetByValueAsync(value))
            .ReturnsAsync(entity);

        _cacheServiceMock
            .Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<ReferenceDataDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.GetByValueAsync(value);

        // Assert
        Assert.True(result.IsSuccess);
        var dto = result.Data;
        Assert.Equal(entity.Id, dto.Id);
        Assert.Equal(value, dto.Value);
        Assert.Equal(entity.Description, dto.Description);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task GetByValueAsync_WithNonExistentValue_ReturnsNotFound()
    {
        // Arrange
        var value = MuscleRoleTestConstants.Values.NonExistent;

        _cacheServiceMock
            .Setup(c => c.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

        _repositoryMock
            .Setup(r => r.GetByValueAsync(value))
            .ReturnsAsync(MuscleRole.Empty);

        // Act
        var result = await _service.GetByValueAsync(value);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
        Assert.Contains("not found", result.Errors.First(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetByValueAsync_WithEmptyValue_ReturnsValidationError()
    {
        // Arrange & Act
        var result = await _service.GetByValueAsync("");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
        Assert.Contains(MuscleRoleErrorMessages.ValueCannotBeEmpty, result.Errors.First());
    }

    [Fact]
    public async Task ExistsAsync_WhenExists_ReturnsTrue()
    {
        // Arrange
        var id = MuscleRoleId.New();
        var entity = MuscleRoleTestBuilder.Create(TestIds.MuscleRoleIds.Primary, MuscleRoleTestConstants.Values.Primary, MuscleRoleTestConstants.Descriptions.Primary);

        _cacheServiceMock
            .Setup(c => c.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(entity);

        _cacheServiceMock
            .Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<ReferenceDataDto>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ExistsAsync(id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_WhenNotExists_ReturnsFalse()
    {
        // Arrange
        var id = MuscleRoleId.New();

        _cacheServiceMock
            .Setup(c => c.GetAsync<ReferenceDataDto>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<ReferenceDataDto>.Miss());

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(MuscleRole.Empty);

        // Act
        var result = await _service.ExistsAsync(id);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetAllActiveAsync_WithCacheHit_ReturnsFromCache()
    {
        // Arrange
        var cachedData = new List<ReferenceDataDto>
        {
            new() { Id = TestIds.MuscleRoleIds.Primary.ToString(), Value = MuscleRoleTestConstants.Values.Primary, Description = MuscleRoleTestConstants.Descriptions.Primary },
            new() { Id = TestIds.MuscleRoleIds.Secondary.ToString(), Value = MuscleRoleTestConstants.Values.Secondary, Description = MuscleRoleTestConstants.Descriptions.Secondary }
        };

        _cacheServiceMock
            .Setup(c => c.GetAsync<IEnumerable<ReferenceDataDto>>(It.IsAny<string>()))
            .ReturnsAsync(CacheResult<IEnumerable<ReferenceDataDto>>.Hit(cachedData));

        // Act
        var result = await _service.GetAllActiveAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data.Count());
        _repositoryMock.Verify(r => r.GetAllActiveAsync(), Times.Never);
    }
}
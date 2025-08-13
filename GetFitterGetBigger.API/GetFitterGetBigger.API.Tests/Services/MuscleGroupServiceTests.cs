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
using GetFitterGetBigger.API.Services.Commands.MuscleGroup;
using GetFitterGetBigger.API.Services.Implementations;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;
using static GetFitterGetBigger.API.Tests.TestBuilders.TestIds;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Tests.Services;

public class MuscleGroupServiceTests
{
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
    private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
    private readonly Mock<IWritableUnitOfWork<FitnessDbContext>> _mockWritableUnitOfWork;
    private readonly Mock<IMuscleGroupRepository> _mockRepository;
    private readonly Mock<ILogger<MuscleGroupService>> _mockLogger;
    private readonly Mock<IBodyPartService> _mockBodyPartService;
    private readonly MuscleGroupService _service;
    
    private readonly List<MuscleGroup> _testData;
    
    public MuscleGroupServiceTests()
    {
        _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _mockWritableUnitOfWork = new Mock<IWritableUnitOfWork<FitnessDbContext>>();
        _mockRepository = new Mock<IMuscleGroupRepository>();
        _mockLogger = new Mock<ILogger<MuscleGroupService>>();
        _mockBodyPartService = new Mock<IBodyPartService>();
        
        _mockUnitOfWorkProvider
            .Setup(x => x.CreateReadOnly())
            .Returns(_mockReadOnlyUnitOfWork.Object);
            
        _mockUnitOfWorkProvider
            .Setup(x => x.CreateWritable())
            .Returns(_mockWritableUnitOfWork.Object);
            
        _mockReadOnlyUnitOfWork
            .Setup(x => x.GetRepository<IMuscleGroupRepository>())
            .Returns(_mockRepository.Object);
            
        _mockWritableUnitOfWork
            .Setup(x => x.GetRepository<IMuscleGroupRepository>())
            .Returns(_mockRepository.Object);
        _service = new MuscleGroupService(
            _mockUnitOfWorkProvider.Object,
            _mockLogger.Object,
            _mockBodyPartService.Object);
            
        _testData = new List<MuscleGroup>
        {
            MuscleGroupTestBuilder.Chest().Build(),
            MuscleGroupTestBuilder.Back().Build(),
            MuscleGroupTestBuilder.Shoulders().Build(),
            MuscleGroupTestBuilder.Quadriceps().Build()
        };
    }
    
    #region GetAllAsync Tests
    
    [Fact]
    public async Task GetAllAsync_WhenDataExists_ReturnsAllMuscleGroups()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(_testData);
            
        // Act
        var result = await _service.GetAllAsync();
        
        Assert.True(result.IsSuccess);
        Assert.Equal(_testData.Count, result.Data.Count());
        _mockRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }
    
    [Fact]
    public async Task GetAllAsync_WhenRepositoryReturnsEmpty_ReturnsEmptyList()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<MuscleGroup>());
            
        // Act
        var result = await _service.GetAllAsync();
        
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data);
    }
    
    #endregion
    
    #region GetByIdAsync Tests
    
    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsCorrectMuscleGroup()
    {
        // Arrange
        var muscleGroup = _testData.First();
        
        _mockRepository
            .Setup(x => x.GetByIdAsync(muscleGroup.Id))
            .ReturnsAsync(muscleGroup);
            
        // Act
        var result = await _service.GetByIdAsync(muscleGroup.Id);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(muscleGroup.Name, result.Data.Name);
        _mockRepository.Verify(x => x.GetByIdAsync(muscleGroup.Id), Times.Once);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ReturnsValidationFailed()
    {
        // Act
        var result = await _service.GetByIdAsync(MuscleGroupId.Empty);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
        _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<MuscleGroupId>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var id = MuscleGroupId.ParseOrEmpty(MuscleGroupIds.Chest);
        
        _mockRepository
            .Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(MuscleGroup.Empty);
            
        // Act
        var result = await _service.GetByIdAsync(id);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
        Assert.True(result.Data.IsEmpty);
    }
    
    #endregion
    
    #region GetByNameAsync Tests
    
    [Fact]
    public async Task GetByNameAsync_WithValidName_ReturnsCorrectMuscleGroup()
    {
        // Arrange
        var muscleGroup = _testData.First();
        
        _mockRepository
            .Setup(x => x.GetByNameAsync(muscleGroup.Name))
            .ReturnsAsync(muscleGroup);
            
        // Act
        var result = await _service.GetByNameAsync(muscleGroup.Name);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(muscleGroup.Name, result.Data.Name);
        _mockRepository.Verify(x => x.GetByNameAsync(muscleGroup.Name), Times.Once);
    }
    
    [Fact]
    public async Task GetByNameAsync_WithEmptyName_ReturnsFailure()
    {
        // Act
        var result = await _service.GetByNameAsync("");
        
        Assert.False(result.IsSuccess);
        Assert.Contains(MuscleGroupErrorMessages.Validation.NameCannotBeEmptyForSearch, result.Errors.First());
        _mockRepository.Verify(x => x.GetByNameAsync(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByNameAsync_WithNullName_ReturnsFailure()
    {
        // Act
        var result = await _service.GetByNameAsync(null!);
        
        Assert.False(result.IsSuccess);
        Assert.Contains(MuscleGroupErrorMessages.Validation.NameCannotBeEmptyForSearch, result.Errors.First());
        _mockRepository.Verify(x => x.GetByNameAsync(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByNameAsync_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetByNameAsync("NonExistent"))
            .ReturnsAsync(MuscleGroup.Empty);
            
        // Act
        var result = await _service.GetByNameAsync("NonExistent");
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
        Assert.True(result.Data.IsEmpty);
    }
    
    [Fact]
    public async Task GetByNameAsync_WithInactiveMuscleGroup_ReturnsNotFound()
    {
        // Arrange
        var inactiveMuscleGroup = MuscleGroupTestBuilder.Custom()
            .WithName("Inactive")
            .IsInactive()
            .Build();
            
        _mockRepository
            .Setup(x => x.GetByNameAsync("Inactive"))
            .ReturnsAsync(inactiveMuscleGroup);
            
        // Act
        var result = await _service.GetByNameAsync("Inactive");
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
        Assert.True(result.Data.IsEmpty);
    }
    
    #endregion
    
    #region GetByBodyPartAsync Tests
    
    [Fact]
    public async Task GetByBodyPartAsync_WithValidBodyPartId_ReturnsCorrectMuscleGroups()
    {
        // Arrange
        var bodyPartId = BodyPartId.ParseOrEmpty(BodyPartIds.Chest);
        var muscleGroups = _testData.Where(mg => mg.BodyPartId == bodyPartId).ToList();
        
        _mockRepository
            .Setup(x => x.GetByBodyPartAsync(bodyPartId))
            .ReturnsAsync(muscleGroups);
            
        // Act
        var result = await _service.GetByBodyPartAsync(bodyPartId);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(muscleGroups.Count, result.Data.Count());
        _mockRepository.Verify(x => x.GetByBodyPartAsync(bodyPartId), Times.Once);
    }
    
    [Fact]
    public async Task GetByBodyPartAsync_WithEmptyBodyPartId_ReturnsFailure()
    {
        // Act
        var result = await _service.GetByBodyPartAsync(BodyPartId.Empty);
        
        Assert.False(result.IsSuccess);
        Assert.Contains(MuscleGroupErrorMessages.Validation.BodyPartIdCannotBeEmptyForSearch, result.Errors.First());
        _mockRepository.Verify(x => x.GetByBodyPartAsync(It.IsAny<BodyPartId>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByBodyPartAsync_WhenNoMuscleGroupsFound_ReturnsEmptyList()
    {
        // Arrange
        var bodyPartId = BodyPartId.ParseOrEmpty(BodyPartIds.Back);
        
        _mockRepository
            .Setup(x => x.GetByBodyPartAsync(bodyPartId))
            .ReturnsAsync(new List<MuscleGroup>());
            
        // Act
        var result = await _service.GetByBodyPartAsync(bodyPartId);
        
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data);
    }
    
    #endregion
    
    #region CreateAsync Tests
    
    [Fact]
    public async Task CreateAsync_WithValidCommand_CreatesSuccessfully()
    {
        // Arrange
        var command = new CreateMuscleGroupCommand
            {
            Name = "New Muscle",
            BodyPartId = BodyPartId.ParseOrEmpty(BodyPartIds.Chest)
        };
        
        var createdEntity = MuscleGroupTestBuilder.Custom()
            .WithName(command.Name)
            .WithBodyPartId(command.BodyPartId)
            .Build();
            
        _mockBodyPartService
            .Setup(x => x.ExistsAsync(command.BodyPartId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)));
            
        _mockRepository
            .Setup(x => x.ExistsByNameAsync(command.Name.Trim(), It.IsAny<MuscleGroupId?>()))
            .ReturnsAsync(false);
            
        _mockRepository
            .Setup(x => x.CreateAsync(It.IsAny<MuscleGroup>()))
            .ReturnsAsync(createdEntity);
            
        // Act
        var result = await _service.CreateAsync(command);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(command.Name, result.Data.Name);
        _mockWritableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }
    
    [Fact]
    public async Task CreateAsync_WithNullCommand_ReturnsValidationError()
    {
        // Act
        var result = await _service.CreateAsync(null!);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
        Assert.Contains(MuscleGroupErrorMessages.Validation.RequestCannotBeNull, result.Errors.First());
        _mockRepository.Verify(x => x.CreateAsync(It.IsAny<MuscleGroup>()), Times.Never);
    }
    
    [Fact]
    public async Task CreateAsync_WithEmptyName_ReturnsValidationError()
    {
        // Arrange
        var command = new CreateMuscleGroupCommand
            {
            Name = "",
            BodyPartId = BodyPartId.ParseOrEmpty(BodyPartIds.Chest)
        };
        
        // Act
        var result = await _service.CreateAsync(command);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
        Assert.Contains(MuscleGroupErrorMessages.Validation.NameCannotBeEmpty, result.Errors.First());
        _mockRepository.Verify(x => x.CreateAsync(It.IsAny<MuscleGroup>()), Times.Never);
    }
    
    [Fact]
    public async Task CreateAsync_WithNameTooLong_ReturnsValidationError()
    {
        // Arrange
        var command = new CreateMuscleGroupCommand
            {
            Name = new string('a', 101),
            BodyPartId = BodyPartId.ParseOrEmpty(BodyPartIds.Chest)
        };
        
        // Act
        var result = await _service.CreateAsync(command);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
        Assert.Contains(MuscleGroupErrorMessages.Validation.NameTooLong, result.Errors.First());
        _mockRepository.Verify(x => x.CreateAsync(It.IsAny<MuscleGroup>()), Times.Never);
    }
    
    [Fact]
    public async Task CreateAsync_WithEmptyBodyPartId_ReturnsValidationError()
    {
        // Arrange
        var command = new CreateMuscleGroupCommand
            {
            Name = "Valid Name",
            BodyPartId = BodyPartId.Empty
        };
        
        // Act
        var result = await _service.CreateAsync(command);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
        Assert.Contains(MuscleGroupErrorMessages.Validation.BodyPartIdRequired, result.Errors.First());
        _mockRepository.Verify(x => x.CreateAsync(It.IsAny<MuscleGroup>()), Times.Never);
    }
    
    [Fact]
    public async Task CreateAsync_WithNonExistentBodyPart_ReturnsValidationError()
    {
        // Arrange
        var command = new CreateMuscleGroupCommand
            {
            Name = "Valid Name",
            BodyPartId = BodyPartId.ParseOrEmpty(BodyPartIds.Chest)
        };
        
        _mockBodyPartService
            .Setup(x => x.ExistsAsync(command.BodyPartId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(false)));
            
        // Act
        var result = await _service.CreateAsync(command);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
        _mockRepository.Verify(x => x.CreateAsync(It.IsAny<MuscleGroup>()), Times.Never);
    }
    
    [Fact]
    public async Task CreateAsync_WithDuplicateName_ReturnsAlreadyExists()
    {
        // Arrange
        var command = new CreateMuscleGroupCommand
            {
            Name = "Existing",
            BodyPartId = BodyPartId.ParseOrEmpty(BodyPartIds.Chest)
        };
        
        _mockBodyPartService
            .Setup(x => x.ExistsAsync(command.BodyPartId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)));
            
        _mockRepository
            .Setup(x => x.ExistsByNameAsync(command.Name.Trim(), It.IsAny<MuscleGroupId?>()))
            .ReturnsAsync(true);
            
        // Act
        var result = await _service.CreateAsync(command);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.AlreadyExists, result.PrimaryErrorCode);
        _mockRepository.Verify(x => x.CreateAsync(It.IsAny<MuscleGroup>()), Times.Never);
    }
    
    #endregion
    
    #region UpdateAsync Tests
    
    [Fact]
    public async Task UpdateAsync_WithValidCommand_UpdatesSuccessfully()
    {
        // Arrange
        var existingMuscleGroup = _testData.First();
        var command = new UpdateMuscleGroupCommand
            {
            Name = "Updated Name",
            BodyPartId = BodyPartId.ParseOrEmpty(BodyPartIds.Back)
        };
        
        var updatedEntity = MuscleGroupTestBuilder.Custom()
            .WithId(existingMuscleGroup.Id)
            .WithName(command.Name)
            .WithBodyPartId(command.BodyPartId)
            .Build();
        
        // Mock ExistsAsync check for entity existence
        _mockRepository
            .Setup(x => x.ExistsAsync(existingMuscleGroup.Id))
            .ReturnsAsync(true);
            
        _mockBodyPartService
            .Setup(x => x.ExistsAsync(command.BodyPartId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)));
            
        _mockRepository
            .Setup(x => x.ExistsByNameAsync(command.Name.Trim(), existingMuscleGroup.Id))
            .ReturnsAsync(false);
            
        _mockRepository
            .Setup(x => x.GetByIdAsync(existingMuscleGroup.Id))
            .ReturnsAsync(existingMuscleGroup);
            
        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<MuscleGroup>()))
            .ReturnsAsync(updatedEntity);
            
        // Act
        var result = await _service.UpdateAsync(existingMuscleGroup.Id, command);
        
        Assert.True(result.IsSuccess);
        Assert.Equal(command.Name, result.Data.Name);
        _mockWritableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }
    
    [Fact]
    public async Task UpdateAsync_WithEmptyId_ReturnsValidationFailed()
    {
        // Arrange
        var command = new UpdateMuscleGroupCommand
            {
            Name = "Updated",
            BodyPartId = BodyPartId.ParseOrEmpty(BodyPartIds.Chest)
        };
        
        // Act
        var result = await _service.UpdateAsync(MuscleGroupId.Empty, command);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<MuscleGroup>()), Times.Never);
    }
    
    [Fact]
    public async Task UpdateAsync_WithNonExistentMuscleGroup_ReturnsNotFound()
    {
        // Arrange
        var id = MuscleGroupId.ParseOrEmpty(MuscleGroupIds.Chest);
        var command = new UpdateMuscleGroupCommand
            {
            Name = "Updated",
            BodyPartId = BodyPartId.ParseOrEmpty(BodyPartIds.Chest)
        };
        
        _mockRepository
            .Setup(x => x.ExistsAsync(id))
            .ReturnsAsync(false);
            
        // Mock the BodyPartService even though it shouldn't be called in this scenario
        // This prevents NullReferenceException due to validation execution order
        _mockBodyPartService
            .Setup(x => x.ExistsAsync(command.BodyPartId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)));
            
        // Act
        var result = await _service.UpdateAsync(id, command);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<MuscleGroup>()), Times.Never);
    }
    
    [Fact]
    public async Task UpdateAsync_WithDuplicateName_ReturnsAlreadyExists()
    {
        // Arrange
        var existingMuscleGroup = _testData.First();
        var command = new UpdateMuscleGroupCommand
            {
            Name = "Duplicate",
            BodyPartId = BodyPartId.ParseOrEmpty(BodyPartIds.Chest)
        };
        
        // Mock ExistsAsync check for entity existence
        _mockRepository
            .Setup(x => x.ExistsAsync(existingMuscleGroup.Id))
            .ReturnsAsync(true);
            
        _mockBodyPartService
            .Setup(x => x.ExistsAsync(command.BodyPartId))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(BooleanResultDto.Create(true)));
            
        _mockRepository
            .Setup(x => x.ExistsByNameAsync(command.Name.Trim(), existingMuscleGroup.Id))
            .ReturnsAsync(true);
            
        // Act
        var result = await _service.UpdateAsync(existingMuscleGroup.Id, command);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.AlreadyExists, result.PrimaryErrorCode);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<MuscleGroup>()), Times.Never);
    }
    
    #endregion
    
    #region DeleteAsync Tests
    
    [Fact]
    public async Task DeleteAsync_WithValidId_DeletesSuccessfully()
    {
        // Arrange
        var muscleGroup = _testData.First();
        
        // Mock ExistsAsync check for entity existence
        _mockRepository
            .Setup(x => x.ExistsAsync(muscleGroup.Id))
            .ReturnsAsync(true);
            
        _mockRepository
            .Setup(x => x.CanDeactivateAsync(muscleGroup.Id))
            .ReturnsAsync(true);
            
        _mockRepository
            .Setup(x => x.DeactivateAsync(muscleGroup.Id))
            .ReturnsAsync(true);
            
        // Act
        var result = await _service.DeleteAsync(muscleGroup.Id);
        
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
        _mockWritableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }
    
    [Fact]
    public async Task DeleteAsync_WithEmptyId_ReturnsValidationFailed()
    {
        // Act
        var result = await _service.DeleteAsync(MuscleGroupId.Empty);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
        _mockRepository.Verify(x => x.DeactivateAsync(It.IsAny<MuscleGroupId>()), Times.Never);
    }
    
    [Fact]
    public async Task DeleteAsync_WhenMuscleGroupNotFound_ReturnsNotFound()
    {
        // Arrange
        var id = MuscleGroupId.ParseOrEmpty(MuscleGroupIds.Chest);
        
        _mockRepository
            .Setup(x => x.ExistsAsync(id))
            .ReturnsAsync(false);
            
        // Act
        var result = await _service.DeleteAsync(id);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
        _mockRepository.Verify(x => x.DeactivateAsync(It.IsAny<MuscleGroupId>()), Times.Never);
    }
    
    [Fact]
    public async Task DeleteAsync_WhenMuscleGroupInUse_ReturnsDependencyExists()
    {
        // Arrange
        var muscleGroup = _testData.First();
        
        // Mock ExistsAsync check for entity existence
        _mockRepository
            .Setup(x => x.ExistsAsync(muscleGroup.Id))
            .ReturnsAsync(true);
            
        _mockRepository
            .Setup(x => x.CanDeactivateAsync(muscleGroup.Id))
            .ReturnsAsync(false);
            
        // Act
        var result = await _service.DeleteAsync(muscleGroup.Id);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.DependencyExists, result.PrimaryErrorCode);
        _mockRepository.Verify(x => x.DeactivateAsync(It.IsAny<MuscleGroupId>()), Times.Never);
    }
    
    #endregion
    
    #region ExistsAsync Tests
    
    [Fact]
    public async Task ExistsAsync_WithExistingId_ReturnsSuccess()
    {
        // Arrange
        var muscleGroup = _testData.First();
        
        _mockRepository
            .Setup(x => x.ExistsAsync(muscleGroup.Id))
            .ReturnsAsync(true);
            
        // Act
        var result = await _service.CheckExistsAsync(muscleGroup.Id);
        
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }
    
    [Fact]
    public async Task ExistsAsync_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var id = MuscleGroupId.ParseOrEmpty(MuscleGroupIds.Chest);
        
        _mockRepository
            .Setup(x => x.ExistsAsync(id))
            .ReturnsAsync(false);
            
        // Act
        var result = await _service.CheckExistsAsync(id);
        
        Assert.True(result.IsSuccess);
        Assert.False(result.Data);
    }
    
    [Fact]
    public async Task ExistsAsync_WithEmptyId_ReturnsValidationFailed()
    {
        // Act
        var result = await _service.CheckExistsAsync(MuscleGroupId.Empty);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
    }
    
    #endregion
    
    
    #region Helper Methods
    
    private static MuscleGroupDto MapToDto(MuscleGroup entity)
    {
        return new MuscleGroupDto
        {
            Id = entity.Id.ToString(),
            Name = entity.Name,
            BodyPartId = entity.BodyPartId.ToString(),
            BodyPartName = entity.BodyPart?.Value,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
    
    #endregion
}
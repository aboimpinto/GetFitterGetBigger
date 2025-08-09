using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Commands.SetConfigurations;
using GetFitterGetBigger.API.Services.Implementations;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Olimpo.EntityFramework.Persistency;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services;

public class SetConfigurationServiceTests
{
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _unitOfWorkProvider;
    private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _readOnlyUnitOfWork;
    private readonly Mock<IWritableUnitOfWork<FitnessDbContext>> _writableUnitOfWork;
    private readonly Mock<ISetConfigurationRepository> _repository;
    private readonly Mock<ILogger<SetConfigurationService>> _logger;
    private readonly ISetConfigurationService _service;

    private readonly SetConfigurationId _validSetConfigurationId = SetConfigurationId.New();
    private readonly WorkoutTemplateExerciseId _validWorkoutTemplateExerciseId = WorkoutTemplateExerciseId.New();
    private readonly WorkoutTemplateId _validWorkoutTemplateId = WorkoutTemplateId.New();
    private readonly UserId _validUserId = UserId.New();

    public SetConfigurationServiceTests()
    {
        _unitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _readOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _writableUnitOfWork = new Mock<IWritableUnitOfWork<FitnessDbContext>>();
        _repository = new Mock<ISetConfigurationRepository>();
        _logger = new Mock<ILogger<SetConfigurationService>>();

        _unitOfWorkProvider.Setup(x => x.CreateReadOnly()).Returns(_readOnlyUnitOfWork.Object);
        _unitOfWorkProvider.Setup(x => x.CreateWritable()).Returns(_writableUnitOfWork.Object);
        _readOnlyUnitOfWork.Setup(x => x.GetRepository<ISetConfigurationRepository>()).Returns(_repository.Object);
        _writableUnitOfWork.Setup(x => x.GetRepository<ISetConfigurationRepository>()).Returns(_repository.Object);

        _service = new SetConfigurationService(_unitOfWorkProvider.Object, _logger.Object);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ShouldReturnValidationFailure()
    {
        // Act
        var result = await _service.GetByIdAsync(SetConfigurationId.Empty);

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidIdButNotFound_ShouldReturnNotFoundFailure()
    {
        // Arrange
        _repository.Setup(x => x.GetByIdAsync(_validSetConfigurationId))
            .ReturnsAsync((SetConfiguration?)null);

        // Act
        var result = await _service.GetByIdAsync(_validSetConfigurationId);

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidIdAndFound_ShouldReturnSuccess()
    {
        // Arrange
        var setConfiguration = CreateValidSetConfiguration();
        _repository.Setup(x => x.GetByIdAsync(_validSetConfigurationId))
            .ReturnsAsync(setConfiguration);

        // Act
        var result = await _service.GetByIdAsync(_validSetConfigurationId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(_validSetConfigurationId.ToString(), result.Data.Id);
        Assert.Equal(1, result.Data.SetNumber);
        Assert.Equal("10-12", result.Data.TargetReps);
        Assert.Equal(50m, result.Data.TargetWeight);
        Assert.Equal(60, result.Data.RestSeconds);
    }

    #endregion

    #region GetByWorkoutTemplateExerciseAsync Tests

    [Fact]
    public async Task GetByWorkoutTemplateExerciseAsync_WithEmptyId_ShouldReturnValidationFailure()
    {
        // Act
        var result = await _service.GetByWorkoutTemplateExerciseAsync(WorkoutTemplateExerciseId.Empty);

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetByWorkoutTemplateExerciseAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var setConfigurations = new[]
        {
            CreateValidSetConfiguration(1),
            CreateValidSetConfiguration(2),
            CreateValidSetConfiguration(3)
        };
        _repository.Setup(x => x.GetByWorkoutTemplateExerciseAsync(_validWorkoutTemplateExerciseId))
            .ReturnsAsync(setConfigurations);

        // Act
        var result = await _service.GetByWorkoutTemplateExerciseAsync(_validWorkoutTemplateExerciseId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(3, result.Data.Count());
        Assert.Equal(1, result.Data.First().SetNumber);
        Assert.Equal(3, result.Data.Last().SetNumber);
    }

    #endregion

    #region GetByWorkoutTemplateAsync Tests

    [Fact]
    public async Task GetByWorkoutTemplateAsync_WithEmptyId_ShouldReturnValidationFailure()
    {
        // Act
        var result = await _service.GetByWorkoutTemplateAsync(WorkoutTemplateId.Empty);

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetByWorkoutTemplateAsync_WithValidId_ShouldReturnSuccess()
    {
        // Arrange
        var setConfigurations = new[]
        {
            CreateValidSetConfiguration(1),
            CreateValidSetConfiguration(2)
        };
        _repository.Setup(x => x.GetByWorkoutTemplateAsync(_validWorkoutTemplateId))
            .ReturnsAsync(setConfigurations);

        // Act
        var result = await _service.GetByWorkoutTemplateAsync(_validWorkoutTemplateId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count());
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithNullCommand_ShouldReturnValidationFailure()
    {
        // Act
        var result = await _service.CreateAsync(null!);

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidCommand_ShouldReturnValidationFailure()
    {
        // Arrange
        var command = new CreateSetConfigurationCommand
        {
            WorkoutTemplateExerciseId = WorkoutTemplateExerciseId.Empty,
            UserId = _validUserId,
            RestSeconds = 60
        };

        // Act
        var result = await _service.CreateAsync(command);

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task CreateAsync_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var command = new CreateSetConfigurationCommand
        {
            WorkoutTemplateExerciseId = _validWorkoutTemplateExerciseId,
            UserId = _validUserId,
            TargetReps = "10-12",
            TargetWeight = 50m,
            RestSeconds = 60
        };

        var createdSetConfiguration = CreateValidSetConfiguration();
        _repository.Setup(x => x.GetMaxSetNumberAsync(_validWorkoutTemplateExerciseId))
            .ReturnsAsync(0);
        _repository.Setup(x => x.AddAsync(It.IsAny<SetConfiguration>()))
            .ReturnsAsync(createdSetConfiguration);

        // Act
        var result = await _service.CreateAsync(command);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("10-12", result.Data.TargetReps);
        Assert.Equal(50m, result.Data.TargetWeight);
        Assert.Equal(60, result.Data.RestSeconds);
        _writableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }

    #endregion

    #region CreateBulkAsync Tests

    [Fact]
    public async Task CreateBulkAsync_WithNullCommand_ShouldReturnValidationFailure()
    {
        // Act
        var result = await _service.CreateBulkAsync(null!);

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task CreateBulkAsync_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var command = new CreateBulkSetConfigurationsCommand
            {
            WorkoutTemplateExerciseId = _validWorkoutTemplateExerciseId,
            UserId = _validUserId,
            SetConfigurations = new[]
            {
                new SetConfigurationData
                {
                    SetNumber = 1,
                    TargetReps = "10-12",
                    TargetWeight = 50m,
                    RestSeconds = 60
                },
                new SetConfigurationData
                {
                    SetNumber = 2,
                    TargetReps = "8-10",
                    TargetWeight = 55m,
                    RestSeconds = 90
                }
            }
        };

        var createdSetConfigurations = new[]
        {
            CreateValidSetConfiguration(1),
            CreateValidSetConfiguration(2)
        };
        _repository.Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<SetConfiguration>>()))
            .ReturnsAsync(createdSetConfigurations);

        // Act
        var result = await _service.CreateBulkAsync(command);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count());
        _writableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithNullCommand_ShouldReturnValidationFailure()
    {
        // Act
        var result = await _service.UpdateAsync(null!);

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task UpdateAsync_WithNotFoundId_ShouldReturnNotFoundFailure()
    {
        // Arrange
        var command = new UpdateSetConfigurationCommand
        {
            SetConfigurationId = _validSetConfigurationId,
            UserId = _validUserId,
            TargetReps = "12-15",
            RestSeconds = 90
        };

        _repository.Setup(x => x.GetByIdAsync(_validSetConfigurationId))
            .ReturnsAsync((SetConfiguration?)null);

        // Act
        var result = await _service.UpdateAsync(command);

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task UpdateAsync_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var existingSetConfiguration = CreateValidSetConfiguration();
        var command = new UpdateSetConfigurationCommand
        {
            SetConfigurationId = _validSetConfigurationId,
            UserId = _validUserId,
            TargetReps = "12-15",
            RestSeconds = 90
        };

        var updatedSetConfiguration = existingSetConfiguration with
        {
            TargetReps = "12-15",
            RestSeconds = 90
        };

        _repository.Setup(x => x.GetByIdAsync(_validSetConfigurationId))
            .ReturnsAsync(existingSetConfiguration);
        _repository.Setup(x => x.UpdateAsync(It.IsAny<SetConfiguration>()))
            .ReturnsAsync(updatedSetConfiguration);

        // Act
        var result = await _service.UpdateAsync(command);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("12-15", result.Data.TargetReps);
        Assert.Equal(90, result.Data.RestSeconds);
        _writableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }

    #endregion

    #region UpdateBulkAsync Tests

    [Fact]
    public async Task UpdateBulkAsync_WithNullCommand_ShouldReturnValidationFailure()
    {
        // Act
        var result = await _service.UpdateBulkAsync(null!);

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
        Assert.Equal(0, result.Data);
    }

    [Fact]
    public async Task UpdateBulkAsync_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var existingSetConfiguration = CreateValidSetConfiguration();
        var command = new UpdateBulkSetConfigurationsCommand
        {
            WorkoutTemplateExerciseId = _validWorkoutTemplateExerciseId,
            UserId = _validUserId,
            SetConfigurationUpdates = new[]
            {
                new SetConfigurationUpdateData
                {
                    SetConfigurationId = _validSetConfigurationId,
                    TargetReps = "12-15",
                    RestSeconds = 90
                }
            }
        };

        _repository.Setup(x => x.GetByIdAsync(_validSetConfigurationId))
            .ReturnsAsync(existingSetConfiguration);
        _repository.Setup(x => x.UpdateRangeAsync(It.IsAny<IEnumerable<SetConfiguration>>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.UpdateBulkAsync(command);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Data);
        _writableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }

    #endregion

    #region ReorderSetsAsync Tests

    [Fact]
    public async Task ReorderSetsAsync_WithNullCommand_ShouldReturnValidationFailure()
    {
        // Act
        var result = await _service.ReorderSetsAsync(null!);

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
        Assert.False(result.Data);
    }

    [Fact]
    public async Task ReorderSetsAsync_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var command = new ReorderSetConfigurationsCommand
        {
            WorkoutTemplateExerciseId = _validWorkoutTemplateExerciseId,
            UserId = _validUserId,
            SetReorders = new Dictionary<SetConfigurationId, int>
            {
                { _validSetConfigurationId, 2 }
            }
        };

        _repository.Setup(x => x.ReorderSetsAsync(_validWorkoutTemplateExerciseId, command.SetReorders))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ReorderSetsAsync(command);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
        _writableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithEmptyId_ShouldReturnValidationFailure()
    {
        // Act
        var result = await _service.DeleteAsync(SetConfigurationId.Empty);

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
        Assert.False(result.Data);
    }


    [Fact]
    public async Task DeleteAsync_WithNotFoundId_ShouldReturnNotFoundFailure()
    {
        // Arrange
        _repository.Setup(x => x.GetByIdAsync(_validSetConfigurationId))
            .ReturnsAsync((SetConfiguration?)null);

        // Act
        var result = await _service.DeleteAsync(_validSetConfigurationId);

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
        Assert.False(result.Data);
    }

    [Fact]
    public async Task DeleteAsync_WithValidParameters_ShouldReturnSuccess()
    {
        // Arrange
        var existingSetConfiguration = CreateValidSetConfiguration();
        _repository.Setup(x => x.GetByIdAsync(_validSetConfigurationId))
            .ReturnsAsync(existingSetConfiguration);
        _repository.Setup(x => x.DeleteAsync(_validSetConfigurationId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(_validSetConfigurationId);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
        _writableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }

    #endregion

    #region DeleteByWorkoutTemplateExerciseAsync Tests

    [Fact]
    public async Task DeleteByWorkoutTemplateExerciseAsync_WithEmptyId_ShouldReturnValidationFailure()
    {
        // Act
        var result = await _service.DeleteByWorkoutTemplateExerciseAsync(WorkoutTemplateExerciseId.Empty);

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
        Assert.Equal(0, result.Data);
    }

    [Fact]
    public async Task DeleteByWorkoutTemplateExerciseAsync_WithValidParameters_ShouldReturnSuccess()
    {
        // Arrange
        _repository.Setup(x => x.DeleteByWorkoutTemplateExerciseAsync(_validWorkoutTemplateExerciseId))
            .ReturnsAsync(3);

        // Act
        var result = await _service.DeleteByWorkoutTemplateExerciseAsync(_validWorkoutTemplateExerciseId);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Data);
        _writableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_WithEmptyId_ShouldReturnFalse()
    {
        // Act
        var result = await _service.ExistsAsync(SetConfigurationId.Empty);

        Assert.True(result.IsSuccess);
            Assert.False(result.Data);
    }

    [Fact]
    public async Task ExistsAsync_WithValidIdFound_ShouldReturnTrue()
    {
        // Arrange
        var existingSetConfiguration = CreateValidSetConfiguration();
        _repository.Setup(x => x.GetByIdAsync(_validSetConfigurationId))
            .ReturnsAsync(existingSetConfiguration);

        // Act
        var result = await _service.ExistsAsync(_validSetConfigurationId);

        Assert.True(result.IsSuccess);
            Assert.True(result.Data);
    }

    [Fact]
    public async Task ExistsAsync_WithValidIdNotFound_ShouldReturnFalse()
    {
        // Arrange
        _repository.Setup(x => x.GetByIdAsync(_validSetConfigurationId))
            .ReturnsAsync((SetConfiguration?)null);

        // Act
        var result = await _service.ExistsAsync(_validSetConfigurationId);

        Assert.True(result.IsSuccess);
            Assert.False(result.Data);
    }

    [Fact]
    public async Task ExistsAsync_WithWorkoutTemplateExerciseIdAndSetNumber_WithEmptyId_ShouldReturnFalse()
    {
        // Act
        var result = await _service.ExistsAsync(WorkoutTemplateExerciseId.Empty, 1);

        Assert.True(result.IsSuccess);
            Assert.False(result.Data);
    }

    [Fact]
    public async Task ExistsAsync_WithWorkoutTemplateExerciseIdAndSetNumber_WithInvalidSetNumber_ShouldReturnFalse()
    {
        // Act
        var result = await _service.ExistsAsync(_validWorkoutTemplateExerciseId, 0);

        Assert.True(result.IsSuccess);
            Assert.False(result.Data);
    }

    [Fact]
    public async Task ExistsAsync_WithWorkoutTemplateExerciseIdAndSetNumber_WithValidParameters_ShouldReturnTrue()
    {
        // Arrange
        _repository.Setup(x => x.ExistsAsync(_validWorkoutTemplateExerciseId, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ExistsAsync(_validWorkoutTemplateExerciseId, 1);

        Assert.True(result.IsSuccess);
            Assert.True(result.Data);
    }

    #endregion

    #region Helper Methods

    private SetConfiguration CreateValidSetConfiguration(int setNumber = 1)
    {
        var entityResult = SetConfiguration.Handler.Create(
            _validSetConfigurationId,
            _validWorkoutTemplateExerciseId,
            setNumber,
            "10-12",
            50m,
            null,
            60);

        return entityResult.Value!;
    }

    #endregion
}
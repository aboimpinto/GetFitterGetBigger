using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Implementations;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Olimpo.EntityFramework.Persistency;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services;

public class WorkoutObjectiveServiceTests
{
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _unitOfWorkProviderMock;
    private readonly Mock<IEternalCacheService> _cacheServiceMock;
    private readonly Mock<ILogger<WorkoutObjectiveService>> _loggerMock;
    private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _unitOfWorkMock;
    private readonly Mock<IWorkoutObjectiveRepository> _repositoryMock;
    private readonly WorkoutObjectiveService _service;

    public WorkoutObjectiveServiceTests()
    {
        _unitOfWorkProviderMock = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _cacheServiceMock = new Mock<IEternalCacheService>();
        _loggerMock = new Mock<ILogger<WorkoutObjectiveService>>();
        _unitOfWorkMock = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _repositoryMock = new Mock<IWorkoutObjectiveRepository>();

        _unitOfWorkProviderMock.Setup(x => x.CreateReadOnly())
            .Returns(_unitOfWorkMock.Object);
        
        _unitOfWorkMock.Setup(x => x.GetRepository<IWorkoutObjectiveRepository>())
            .Returns(_repositoryMock.Object);

        _service = new WorkoutObjectiveService(
            _unitOfWorkProviderMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var workoutObjectiveId = WorkoutObjectiveId.From(Guid.NewGuid());
        var workoutObjective = WorkoutObjective.Handler.Create(
            workoutObjectiveId,
            "Muscular Strength",
            "Build maximum strength through heavy loads and low repetitions",
            1,
            true).Value;

        _repositoryMock.Setup(x => x.GetByIdAsync(workoutObjectiveId))
            .ReturnsAsync(workoutObjective);

        // Act
        var result = await _service.GetByIdAsync(workoutObjectiveId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(workoutObjectiveId.ToString(), result.Data.Id);
        Assert.Equal("Muscular Strength", result.Data.Value);
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ReturnsValidationFailed()
    {
        // Arrange
        var emptyId = WorkoutObjectiveId.Empty;

        // Act
        var result = await _service.GetByIdAsync(emptyId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
    }

    [Fact]
    public async Task GetAllActiveAsync_ReturnsAllActiveObjectives()
    {
        // Arrange
        var workoutObjectives = new List<WorkoutObjective>
        {
            WorkoutObjective.Handler.Create(
                WorkoutObjectiveId.From(Guid.NewGuid()),
                "Muscular Strength",
                "Build maximum strength through heavy loads and low repetitions",
                1,
                true).Value,
            WorkoutObjective.Handler.Create(
                WorkoutObjectiveId.From(Guid.NewGuid()),
                "Muscular Hypertrophy",
                "Increase muscle size through moderate loads and volume",
                2,
                true).Value
        };

        _repositoryMock.Setup(x => x.GetAllActiveAsync())
            .ReturnsAsync(workoutObjectives);

        // Act
        var result = await _service.GetAllActiveAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data.Count());
        Assert.All(result.Data, x => Assert.False(string.IsNullOrEmpty(x.Value)));
    }
}
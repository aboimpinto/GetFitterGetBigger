using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplate;
using GetFitterGetBigger.API.Services.Implementations;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Olimpo.EntityFramework.Persistency;

namespace GetFitterGetBigger.API.Tests.Services;

public class WorkoutTemplateServiceTests
{
    private readonly Mock<IUnitOfWorkProvider<FitnessDbContext>> _mockUnitOfWorkProvider;
    private readonly Mock<IReadOnlyUnitOfWork<FitnessDbContext>> _mockReadOnlyUnitOfWork;
    private readonly Mock<IWritableUnitOfWork<FitnessDbContext>> _mockWritableUnitOfWork;
    private readonly Mock<IWorkoutTemplateRepository> _mockRepository;
    private readonly Mock<IWorkoutStateRepository> _mockWorkoutStateRepository;
    private readonly Mock<IWorkoutStateService> _mockWorkoutStateService;
    private readonly Mock<IExerciseService> _mockExerciseService;
    private readonly Mock<IWorkoutTemplateExerciseService> _mockWorkoutTemplateExerciseService;
    private readonly Mock<ILogger<WorkoutTemplateService>> _mockLogger;
    private readonly WorkoutTemplateService _service;
    
    private readonly WorkoutTemplate _testTemplate;
    private readonly WorkoutTemplateId _testTemplateId;
    private readonly UserId _testUserId;
    
    public WorkoutTemplateServiceTests()
    {
        _mockUnitOfWorkProvider = new Mock<IUnitOfWorkProvider<FitnessDbContext>>();
        _mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        _mockWritableUnitOfWork = new Mock<IWritableUnitOfWork<FitnessDbContext>>();
        _mockRepository = new Mock<IWorkoutTemplateRepository>();
        _mockWorkoutStateRepository = new Mock<IWorkoutStateRepository>();
        _mockLogger = new Mock<ILogger<WorkoutTemplateService>>();
        
        _mockUnitOfWorkProvider
            .Setup(x => x.CreateReadOnly())
            .Returns(_mockReadOnlyUnitOfWork.Object);
            
        _mockUnitOfWorkProvider
            .Setup(x => x.CreateWritable())
            .Returns(_mockWritableUnitOfWork.Object);
            
        _mockReadOnlyUnitOfWork
            .Setup(x => x.GetRepository<IWorkoutTemplateRepository>())
            .Returns(_mockRepository.Object);
            
        _mockReadOnlyUnitOfWork
            .Setup(x => x.GetRepository<IWorkoutStateRepository>())
            .Returns(_mockWorkoutStateRepository.Object);
            
        _mockWritableUnitOfWork
            .Setup(x => x.GetRepository<IWorkoutTemplateRepository>())
            .Returns(_mockRepository.Object);
            
        _mockWorkoutStateService = new Mock<IWorkoutStateService>();
        _mockExerciseService = new Mock<IExerciseService>();
        _mockWorkoutTemplateExerciseService = new Mock<IWorkoutTemplateExerciseService>();
        
        _service = new WorkoutTemplateService(
            _mockUnitOfWorkProvider.Object, 
            _mockWorkoutStateService.Object, 
            _mockExerciseService.Object,
            _mockWorkoutTemplateExerciseService.Object,
            _mockLogger.Object);
        
        // Setup test data
        _testTemplateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        _testUserId = UserId.ParseOrEmpty(TestIds.UserIds.JohnDoe);
        _testTemplate = new WorkoutTemplateBuilder()
            .WithId(_testTemplateId)
            .WithName("Test Workout Template")
            .WithDescription("Test Description")
            .WithCreatedBy(_testUserId)
            .Build();
            
        // Setup WorkoutState mock to return Draft state
        var draftWorkoutState = WorkoutStateTestBuilder.Default()
            .WithValue("Draft")
            .WithDescription("Draft state for new workout templates")
            .Build();
            
        _mockWorkoutStateRepository
            .Setup(x => x.GetByValueAsync("Draft"))
            .ReturnsAsync(draftWorkoutState);
            
        // Setup WorkoutStateService mock
        var draftWorkoutStateDto = new WorkoutStateDto
        {
            Id = draftWorkoutState.Id.ToString(),
            Value = draftWorkoutState.Value,
            Description = draftWorkoutState.Description
        };
            
        _mockWorkoutStateService
            .Setup(x => x.GetByValueAsync("Draft"))
            .ReturnsAsync(ServiceResult<WorkoutStateDto>.Success(draftWorkoutStateDto));
    }
    
    #region GetById Tests
    
    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetByIdWithDetailsAsync(_testTemplateId))
            .ReturnsAsync(_testTemplate);
            
        // Act
        var result = await _service.GetByIdAsync(_testTemplateId);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(_testTemplate.Name, result.Data.Name);
        Assert.Equal(_testTemplate.Description, result.Data.Description);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ReturnsFailure()
    {
        // Arrange
        var emptyId = WorkoutTemplateId.Empty;
        
        // Act
        var result = await _service.GetByIdAsync(emptyId);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Contains("GUID format", result.Errors.First());
        _mockRepository.Verify(x => x.GetByIdWithDetailsAsync(It.IsAny<WorkoutTemplateId>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsFailure()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetByIdWithDetailsAsync(_testTemplateId))
            .ReturnsAsync(WorkoutTemplate.Empty);
            
        // Act
        var result = await _service.GetByIdAsync(_testTemplateId);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Contains("not found", result.Errors.First());
    }
    
    #endregion
    
    #region GetPagedByCreator Tests
    
    [Fact]
    public async Task GetPagedByCreatorAsync_WithValidCreatorId_ReturnsPagedResponse()
    {
        // Arrange
        var templates = new List<WorkoutTemplate> { _testTemplate };
        _mockRepository
            .Setup(x => x.GetPagedByCreatorAsync(_testUserId, 1, 20, false))
            .ReturnsAsync((templates, 1));
            
        // Act
        var result = await _service.GetPagedByCreatorAsync(_testUserId, 1, 20);
        
        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(20, result.PageSize);
    }
    
    [Fact]
    public async Task GetPagedByCreatorAsync_WithEmptyCreatorId_ReturnsEmptyResponse()
    {
        // Arrange
        var emptyUserId = UserId.Empty;
        
        // Act
        var result = await _service.GetPagedByCreatorAsync(emptyUserId, 1, 20);
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        _mockRepository.Verify(x => x.GetPagedByCreatorAsync(It.IsAny<UserId>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
    }
    
    #endregion
    
    #region Create Tests
    
    [Fact]
    public async Task CreateAsync_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateWorkoutTemplateCommand
        {
            Name = "New Workout",
            Description = "New Description",
            CategoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.Strength),
            DifficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Beginner),
            EstimatedDurationMinutes = 60,
            Tags = new List<string> { "test", "workout" },
            IsPublic = true,
            CreatedBy = _testUserId
        };
        
        _mockRepository
            .Setup(x => x.ExistsByNameAsync(command.Name, command.CreatedBy, WorkoutTemplateId.Empty))
            .ReturnsAsync(false);
            
        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<WorkoutTemplate>()))
            .ReturnsAsync(_testTemplate);
            
        // Act
        var result = await _service.CreateAsync(command);
        
        // Assert
        if (!result.IsSuccess)
        {
            var errors = string.Join(", ", result.Errors);
            Assert.True(result.IsSuccess, $"Create failed with errors: {errors}");
        }
        Assert.NotNull(result.Data);
        _mockWritableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }
    
    [Fact]
    public async Task CreateAsync_WithNullCommand_ReturnsFailure()
    {
        // Act
        var result = await _service.CreateAsync(null!);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Create command cannot be null", result.Errors);
    }
    
    [Fact]
    public async Task CreateAsync_WithDuplicateName_ReturnsFailure()
    {
        // Arrange
        var command = new CreateWorkoutTemplateCommand
        {
            Name = "Existing Workout",
            Description = "Description",
            CategoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.Strength),
            DifficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Beginner),
            EstimatedDurationMinutes = 60,
            Tags = new List<string>(),
            IsPublic = true,
            CreatedBy = _testUserId
        };
        
        _mockRepository
            .Setup(x => x.ExistsByNameAsync(command.Name, command.CreatedBy, WorkoutTemplateId.Empty))
            .ReturnsAsync(true);
            
        // Act
        var result = await _service.CreateAsync(command);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("already exists", result.Errors.First());
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<WorkoutTemplate>()), Times.Never);
    }
    
    [Fact]
    public async Task CreateAsync_WithInvalidName_ReturnsFailure()
    {
        // Arrange
        var command = new CreateWorkoutTemplateCommand
        {
            Name = "AB", // Too short
            Description = "Description",
            CategoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.Strength),
            DifficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Beginner),
            EstimatedDurationMinutes = 60,
            Tags = new List<string>(),
            IsPublic = true,
            CreatedBy = _testUserId
        };
        
        // Act
        var result = await _service.CreateAsync(command);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Name must be between 3 and 100 characters", result.Errors);
    }
    
    #endregion
    
    #region Update Tests
    
    [Fact]
    public async Task UpdateAsync_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        var command = new UpdateWorkoutTemplateCommand
        {
            Id = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.Template1),
            Name = "Updated Workout",
            Description = "Updated Description",
            CategoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.Strength),
            DifficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Intermediate),
            UpdatedBy = UserId.ParseOrEmpty(TestIds.UserIds.PersonalTrainer),
            EstimatedDurationMinutes = 45,
            Tags = new List<string> { "updated" },
            IsPublic = false
        };
        
        _mockRepository
            .Setup(x => x.GetByIdWithDetailsAsync(_testTemplateId))
            .ReturnsAsync(_testTemplate);
            
        _mockRepository
            .Setup(x => x.GetByIdAsync(_testTemplateId))
            .ReturnsAsync(_testTemplate);
            
        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<WorkoutTemplate>()))
            .ReturnsAsync(_testTemplate);
            
        // Act
        var result = await _service.UpdateAsync(_testTemplateId, command);
        
        // Assert
        Assert.True(result.IsSuccess);
        _mockWritableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }
    
    [Fact]
    public async Task UpdateAsync_WhenTemplateNotFound_ReturnsFailure()
    {
        // Arrange
        var command = new UpdateWorkoutTemplateCommand
        {
            Id = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.Template1),
            Name = "Updated Workout",
            Description = "Updated Description",
            CategoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.Strength),
            DifficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Intermediate),
            UpdatedBy = UserId.ParseOrEmpty(TestIds.UserIds.PersonalTrainer),
            EstimatedDurationMinutes = 45,
            Tags = new List<string>(),
            IsPublic = false
        };
        
        _mockRepository
            .Setup(x => x.GetByIdWithDetailsAsync(_testTemplateId))
            .ReturnsAsync(WorkoutTemplate.Empty);
            
        // Act
        var result = await _service.UpdateAsync(_testTemplateId, command);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("not found", result.Errors.First());
    }
    
    #endregion
    
    #region ChangeState Tests
    
    [Fact]
    public async Task ChangeStateAsync_WithValidState_ReturnsSuccess()
    {
        // Arrange
        var newStateId = WorkoutStateId.ParseOrEmpty(TestIds.WorkoutStateIds.Production);
        
        _mockRepository
            .Setup(x => x.GetByIdWithDetailsAsync(_testTemplateId))
            .ReturnsAsync(_testTemplate);
            
        _mockRepository
            .Setup(x => x.GetByIdAsync(_testTemplateId))
            .ReturnsAsync(_testTemplate);
            
        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<WorkoutTemplate>()))
            .ReturnsAsync(_testTemplate);
            
        // Act
        var result = await _service.ChangeStateAsync(_testTemplateId, newStateId);
        
        // Assert
        Assert.True(result.IsSuccess);
        _mockWritableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }
    
    [Fact]
    public async Task ChangeStateAsync_WithEmptyStateId_ReturnsFailure()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetByIdWithDetailsAsync(_testTemplateId))
            .ReturnsAsync(_testTemplate);
            
        // Act
        var result = await _service.ChangeStateAsync(_testTemplateId, WorkoutStateId.Empty);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("GUID format", result.Errors.First());
    }
    
    #endregion
    
    #region Delete Tests
    
    [Fact]
    public async Task SoftDeleteAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetByIdWithDetailsAsync(_testTemplateId))
            .ReturnsAsync(_testTemplate);
            
        _mockRepository
            .Setup(x => x.SoftDeleteAsync(_testTemplateId))
            .ReturnsAsync(true);
            
        // Act
        var result = await _service.SoftDeleteAsync(_testTemplateId);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
        _mockWritableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }
    
    [Fact]
    public async Task DeleteAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetByIdWithDetailsAsync(_testTemplateId))
            .ReturnsAsync(_testTemplate);
            
        _mockRepository
            .Setup(x => x.DeleteAsync(_testTemplateId))
            .ReturnsAsync(true);
            
        // Act
        var result = await _service.DeleteAsync(_testTemplateId);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
        _mockWritableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }
    
    #endregion
    
    #region Filtering Tests
    
    [Fact]
    public async Task GetByNamePatternAsync_WithValidPattern_ReturnsSuccess()
    {
        // Arrange
        var pattern = "test";
        var templates = new List<WorkoutTemplate> { _testTemplate };
        
        _mockRepository
            .Setup(x => x.GetByNamePatternAsync(pattern, UserId.Empty, false))
            .ReturnsAsync(templates);
            
        // Act
        var result = await _service.GetByNamePatternAsync(pattern);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data);
    }
    
    [Fact]
    public async Task GetByNamePatternAsync_WithEmptyPattern_ReturnsEmptyList()
    {
        // Act
        var result = await _service.GetByNamePatternAsync("");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data);
        _mockRepository.Verify(x => x.GetByNamePatternAsync(It.IsAny<string>(), It.IsAny<UserId>(), It.IsAny<bool>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByCategoryAsync_WithValidCategoryId_ReturnsSuccess()
    {
        // Arrange
        var categoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.Strength);
        var templates = new List<WorkoutTemplate> { _testTemplate };
        
        _mockRepository
            .Setup(x => x.GetByCategoryAsync(categoryId, UserId.Empty, false))
            .ReturnsAsync(templates);
            
        // Act
        var result = await _service.GetByCategoryAsync(categoryId);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data);
    }
    
    [Fact]
    public async Task GetByCategoryAsync_WithEmptyCategoryId_ReturnsFailure()
    {
        // Act
        var result = await _service.GetByCategoryAsync(WorkoutCategoryId.Empty);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("GUID format", result.Errors.First());
    }
    
    [Fact]
    public async Task GetByObjectiveAsync_WithValidObjectiveId_ReturnsSuccess()
    {
        // Arrange
        var objectiveId = WorkoutObjectiveId.ParseOrEmpty(TestIds.WorkoutObjectiveIds.BuildMuscle);
        var templates = new List<WorkoutTemplate> { _testTemplate };
        
        _mockRepository
            .Setup(x => x.GetByObjectiveAsync(objectiveId, UserId.Empty, false))
            .ReturnsAsync(templates);
            
        // Act
        var result = await _service.GetByObjectiveAsync(objectiveId);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data);
    }
    
    [Fact]
    public async Task GetByDifficultyAsync_WithValidDifficultyId_ReturnsSuccess()
    {
        // Arrange
        var difficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Beginner);
        var templates = new List<WorkoutTemplate> { _testTemplate };
        
        _mockRepository
            .Setup(x => x.GetByDifficultyAsync(difficultyId, UserId.Empty, false))
            .ReturnsAsync(templates);
            
        // Act
        var result = await _service.GetByDifficultyAsync(difficultyId);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data);
    }
    
    [Fact]
    public async Task GetByExerciseAsync_WithValidExerciseId_ReturnsSuccess()
    {
        // Arrange
        var exerciseId = ExerciseId.ParseOrEmpty(TestIds.ExerciseIds.BenchPress);
        var templates = new List<WorkoutTemplate> { _testTemplate };
        
        _mockRepository
            .Setup(x => x.GetByExerciseAsync(exerciseId, UserId.Empty, false))
            .ReturnsAsync(templates);
            
        // Act
        var result = await _service.GetByExerciseAsync(exerciseId);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data);
    }
    
    #endregion
    
    #region Duplicate Tests
    
    [Fact]
    public async Task DuplicateAsync_WithValidParameters_ReturnsSuccess()
    {
        // Arrange
        var newName = "Duplicated Workout";
        var newCreatorId = UserId.ParseOrEmpty(TestIds.UserIds.JaneDoe);
        
        _mockRepository
            .Setup(x => x.GetByIdWithDetailsAsync(_testTemplateId))
            .ReturnsAsync(_testTemplate);
            
        _mockRepository
            .Setup(x => x.ExistsByNameAsync(newName, newCreatorId, WorkoutTemplateId.Empty))
            .ReturnsAsync(false);
            
        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<WorkoutTemplate>()))
            .ReturnsAsync(_testTemplate);
            
        // Act
        var result = await _service.DuplicateAsync(_testTemplateId, newName, newCreatorId);
        
        // Assert
        Assert.True(result.IsSuccess);
        _mockWritableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }
    
    [Fact]
    public async Task DuplicateAsync_WithEmptyName_ReturnsFailure()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetByIdWithDetailsAsync(_testTemplateId))
            .ReturnsAsync(_testTemplate);
            
        // Act
        var result = await _service.DuplicateAsync(_testTemplateId, "", _testUserId);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Template name is required", result.Errors);
    }
    
    [Fact]
    public async Task DuplicateAsync_WithEmptyCreatorId_ReturnsFailure()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetByIdWithDetailsAsync(_testTemplateId))
            .ReturnsAsync(_testTemplate);
            
        // Act
        var result = await _service.DuplicateAsync(_testTemplateId, "New Name", UserId.Empty);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Creator ID is required", result.Errors);
    }
    
    #endregion
    
    #region Exists Tests
    
    [Fact]
    public async Task ExistsAsync_WithExistingId_ReturnsTrue()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.ExistsAsync(_testTemplateId))
            .ReturnsAsync(true);
            
        // Act
        var result = await _service.ExistsAsync(_testTemplateId);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public async Task ExistsAsync_WithEmptyId_ReturnsFalse()
    {
        // Act
        var result = await _service.ExistsAsync(WorkoutTemplateId.Empty);
        
        // Assert
        Assert.False(result);
        _mockRepository.Verify(x => x.ExistsAsync(It.IsAny<WorkoutTemplateId>()), Times.Never);
    }
    
    [Fact]
    public async Task ExistsAsync_WithStringId_ParsesAndChecks()
    {
        // Arrange
        var stringId = TestIds.WorkoutTemplateIds.BasicTemplate;
        _mockRepository
            .Setup(x => x.ExistsAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(true);
            
        // Act
        var result = await _service.ExistsAsync(stringId);
        
        // Assert
        Assert.True(result);
        _mockRepository.Verify(x => x.ExistsAsync(_testTemplateId), Times.Once);
    }
    
    [Fact]
    public async Task ExistsByNameAsync_WithExistingName_ReturnsTrue()
    {
        // Arrange
        var name = "Existing Workout";
        _mockRepository
            .Setup(x => x.ExistsByNameAsync(name, _testUserId, WorkoutTemplateId.Empty))
            .ReturnsAsync(true);
            
        // Act
        var result = await _service.ExistsByNameAsync(name, _testUserId);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public async Task ExistsByNameAsync_WithEmptyName_ReturnsFalse()
    {
        // Act
        var result = await _service.ExistsByNameAsync("", _testUserId);
        
        // Assert
        Assert.False(result);
        _mockRepository.Verify(x => x.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<UserId>(), It.IsAny<WorkoutTemplateId>()), Times.Never);
    }
    
    #endregion
}
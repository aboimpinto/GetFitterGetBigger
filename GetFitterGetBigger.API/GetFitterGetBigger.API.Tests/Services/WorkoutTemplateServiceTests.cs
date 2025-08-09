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
using GetFitterGetBigger.API.Tests.TestHelpers;
using Microsoft.Extensions.Logging;
using Moq;
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
            .Setup(x => x.CreateReadOnly())
            .Returns(_mockReadOnlyUnitOfWork.Object);
            
        _mockReadOnlyUnitOfWork
            .Setup(x => x.GetRepository<IWorkoutTemplateRepository>())
            .Returns(_mockRepository.Object);
            
        _mockReadOnlyUnitOfWork
            .Setup(x => x.GetRepository<IWorkoutTemplateRepository>())
            .Returns(_mockRepository.Object);
            
        _mockUnitOfWorkProvider
            .Setup(x => x.CreateWritable())
            .Returns(_mockWritableUnitOfWork.Object);
            
        _mockWritableUnitOfWork
            .Setup(x => x.GetRepository<IWorkoutTemplateRepository>())
            .Returns(_mockRepository.Object);
            
        _mockWritableUnitOfWork
            .Setup(x => x.CommitAsync())
            .Returns(Task.CompletedTask);
            
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
        
        // Create a workout state for testing
        var productionState = WorkoutStateTestBuilder.Production().Build();
            
        _testTemplate = new WorkoutTemplateBuilder()
            .WithId(_testTemplateId)
            .WithName("Test Workout Template")
            .WithDescription("Test Description")
            .WithCategoryId(TestIds.WorkoutCategoryIds.Strength)
            .WithDifficultyId(TestIds.DifficultyLevelIds.Beginner)
            .WithWorkoutStateId(TestIds.WorkoutStateIds.Production)
            .WithWorkoutState(productionState)
            .Build();
            
        // Setup WorkoutState mock to return Draft state
        var draftWorkoutState = WorkoutStateTestBuilder.Default()
            .WithValue("Draft")
            .WithDescription("Draft state for new workout templates")
            .Build();
            
        // _mockWorkoutStateRepository setup needed
            
        // Setup WorkoutStateService mock
        var draftWorkoutStateDto = new WorkoutStateDto
            {
            Id = draftWorkoutState.Id.ToString(),
            Value = draftWorkoutState.Value,
            Description = draftWorkoutState.Description
        };
            
        // _mockWorkoutStateService setup needed
    }
    
    #region GetById Tests
    
    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(_testTemplate);
            
        // Act
        var result = await _service.GetByIdAsync(_testTemplateId);
        
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
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.InvalidFormat, result.PrimaryErrorCode);
        _mockRepository.Verify(x => x.GetByIdWithDetailsAsync(It.IsAny<WorkoutTemplateId>()), Times.Never);
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsFailure()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync((WorkoutTemplate)null!);
            
        // Act
        var result = await _service.GetByIdAsync(_testTemplateId);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
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
            IsPublic = true
        };
        
        _mockRepository
            .Setup(x => x.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(false);
            
        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<WorkoutTemplate>()))
            .ReturnsAsync(_testTemplate);
            
        // Setup WorkoutStateService mock for Draft state
        var draftWorkoutStateDto = new WorkoutStateDto
        {
            Id = TestIds.WorkoutStateIds.Draft,
            Value = "Draft",
            Description = "Draft state for new workout templates"
        };
        
        _mockWorkoutStateService
            .Setup(x => x.GetByValueAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<WorkoutStateDto>.Success(draftWorkoutStateDto));
            
        // Act
        var result = await _service.CreateAsync(command);
        
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
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
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
            IsPublic = true
        };
        
        _mockRepository
            .Setup(x => x.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(true);
            
        // Act
        var result = await _service.CreateAsync(command);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.AlreadyExists, result.PrimaryErrorCode);
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
            IsPublic = true
        };
        
        // Act
        var result = await _service.CreateAsync(command);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
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
            EstimatedDurationMinutes = 45,
            Tags = new List<string> { "updated" },
            IsPublic = false
        };
        
        // Mock GetByIdWithDetailsAsync for the existence check in UpdateAsync
        _mockRepository
            .Setup(x => x.GetByIdWithDetailsAsync(_testTemplateId))
            .ReturnsAsync(_testTemplate);
            
        // Mock GetByIdAsync for loading the existing template during update
        _mockRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(_testTemplate);
            
        // Mock ExistsByNameAsync to allow name update (return false = name doesn't exist yet)
        _mockRepository
            .Setup(x => x.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(false);
            
        // Mock UpdateAsync to return updated template
        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<WorkoutTemplate>()))
            .ReturnsAsync(_testTemplate);
            
        // Act
        var result = await _service.UpdateAsync(_testTemplateId, command);
        
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
            EstimatedDurationMinutes = 45,
            Tags = new List<string>(),
            IsPublic = false
        };
        
        _mockRepository
            .Setup(x => x.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(true);
            
        // Act
        var result = await _service.UpdateAsync(_testTemplateId, command);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
    }
    
    #endregion
    
    #region ChangeState Tests
    
    [Fact]
    public async Task ChangeStateAsync_WithValidState_ReturnsSuccess()
    {
        // Arrange
        var newStateId = WorkoutStateId.ParseOrEmpty(TestIds.WorkoutStateIds.Production);
        
        _mockRepository
            .Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(_testTemplate);
            
        _mockRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(_testTemplate);
            
        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<WorkoutTemplate>()))
            .ReturnsAsync(_testTemplate);
            
        // Act
        var result = await _service.ChangeStateAsync(_testTemplateId, newStateId);
        
        Assert.True(result.IsSuccess);
        _mockWritableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }
    
    [Fact]
    public async Task ChangeStateAsync_WithEmptyStateId_ReturnsFailure()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(_testTemplate);
            
        // Act
        var result = await _service.ChangeStateAsync(_testTemplateId, WorkoutStateId.Empty);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.InvalidFormat, result.PrimaryErrorCode);
    }
    
    #endregion
    
    #region Delete Tests
    
    [Fact]
    public async Task SoftDeleteAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(_testTemplate);
            
        _mockRepository
            .Setup(x => x.SoftDeleteAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(true);
            
        // Act
        var result = await _service.SoftDeleteAsync(_testTemplateId);
        
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
        _mockWritableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }
    
    [Fact]
    public async Task DeleteAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(_testTemplate);
            
        _mockRepository
            .Setup(x => x.DeleteAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(true);
            
        // Act
        var result = await _service.DeleteAsync(_testTemplateId);
        
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
        _mockWritableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }
    
    #endregion
    
    #region Filtering Tests
    
    [Fact]
    public async Task SearchAsync_WithEmptyParameters_ReturnsAllResults()
    {
        // Arrange
        var templates = new List<WorkoutTemplate> { _testTemplate }.BuildAsyncQueryable();
        
        _mockRepository
            .Setup(x => x.GetWorkoutTemplatesQueryable())
            .Returns(templates);
        
        _mockRepository
            .Setup(x => x.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(true);
            
        // Act
        var result = await _service.SearchAsync(
            namePattern: string.Empty,
            categoryId: WorkoutCategoryId.Empty,
            objectiveId: WorkoutObjectiveId.Empty,
            difficultyId: DifficultyLevelId.Empty,
            stateId: WorkoutStateId.Empty,
            page: 1,
            pageSize: 20,
            sortBy: "name",
            sortOrder: "asc");
        
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data.Items);
        _mockRepository.Verify(x => x.GetWorkoutTemplatesQueryable(), Times.Once);
    }
    
    [Fact]
    public async Task SearchAsync_WithCategoryFilter_ReturnsFilteredResults()
    {
        // Arrange
        var categoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.Strength);
        var templates = new List<WorkoutTemplate> { _testTemplate }.BuildAsyncQueryable();
        
        _mockRepository
            .Setup(x => x.GetWorkoutTemplatesQueryable())
            .Returns(templates);
        
        _mockRepository
            .Setup(x => x.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(true);
            
        // Act
        var result = await _service.SearchAsync(
            namePattern: string.Empty,
            categoryId: categoryId,
            objectiveId: WorkoutObjectiveId.Empty,
            difficultyId: DifficultyLevelId.Empty,
            stateId: WorkoutStateId.Empty,
            page: 1,
            pageSize: 20,
            sortBy: "name",
            sortOrder: "asc");
        
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data.Items);
    }
    
    [Fact]
    public async Task SearchAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var productionState = WorkoutStateTestBuilder.Default()
            .WithValue("PRODUCTION")
            .WithDescription("Production state")
            .Build();
            
        var templates = Enumerable.Range(1, 30)
            .Select(i => new WorkoutTemplateBuilder()
                .WithId(WorkoutTemplateId.New())
                .WithName($"Template {i}")
                .WithWorkoutState(productionState)
                .Build())
            .ToList()
            .BuildAsyncQueryable();
            
        _mockRepository
            .Setup(x => x.GetWorkoutTemplatesQueryable())
            .Returns(templates);
            
        _mockRepository
            .Setup(x => x.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(true);
            
        // Act
        var result = await _service.SearchAsync(
            namePattern: string.Empty,
            categoryId: WorkoutCategoryId.Empty,
            objectiveId: WorkoutObjectiveId.Empty,
            difficultyId: DifficultyLevelId.Empty,
            stateId: WorkoutStateId.Empty,
            page: 2,
            pageSize: 10,
            sortBy: "name",
            sortOrder: "asc");
        
        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Data.Items.Count());
        Assert.Equal(30, result.Data.TotalCount);
        Assert.Equal(2, result.Data.CurrentPage);
        Assert.Equal(3, result.Data.TotalPages);
    }
    
    [Fact]
    public async Task SearchAsync_WithObjectiveFilter_ReturnsFilteredResults()
    {
        // Arrange
        var objectiveId = WorkoutObjectiveId.ParseOrEmpty(TestIds.WorkoutObjectiveIds.BuildMuscle);
        
        // Create a template with the objective we're searching for
        var productionState = WorkoutStateTestBuilder.Default()
            .WithValue("PRODUCTION")
            .WithDescription("Production state")
            .Build();
            
        var objective = WorkoutTemplateObjective.Handler.Create(_testTemplateId, objectiveId).Value!;
        var templateWithObjective = new WorkoutTemplateBuilder()
            .WithId(_testTemplateId)
            .WithName("Test Workout Template")
            .WithDescription("Test Description")
            .WithCategoryId(TestIds.WorkoutCategoryIds.Strength)
            .WithDifficultyId(TestIds.DifficultyLevelIds.Beginner)
            .WithWorkoutStateId(TestIds.WorkoutStateIds.Production)
            .WithWorkoutState(productionState)
            .WithObjective(objective)
            .Build();
            
        var templates = new List<WorkoutTemplate> { templateWithObjective }.BuildAsyncQueryable();
        
        _mockRepository
            .Setup(x => x.GetWorkoutTemplatesQueryable())
            .Returns(templates);
        
        _mockRepository
            .Setup(x => x.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(true);
            
        // Act
        var result = await _service.SearchAsync(
            namePattern: string.Empty,
            categoryId: WorkoutCategoryId.Empty,
            objectiveId: objectiveId,
            difficultyId: DifficultyLevelId.Empty,
            stateId: WorkoutStateId.Empty,
            page: 1,
            pageSize: 20,
            sortBy: "name",
            sortOrder: "asc");
        
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data.Items);
    }
    
    [Fact]
    public async Task SearchAsync_WithDifficultyFilter_ReturnsFilteredResults()
    {
        // Arrange
        var difficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Beginner);
        var templates = new List<WorkoutTemplate> { _testTemplate }.BuildAsyncQueryable();
        
        _mockRepository
            .Setup(x => x.GetWorkoutTemplatesQueryable())
            .Returns(templates);
        
        _mockRepository
            .Setup(x => x.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(true);
            
        // Act
        var result = await _service.SearchAsync(
            namePattern: string.Empty,
            categoryId: WorkoutCategoryId.Empty,
            objectiveId: WorkoutObjectiveId.Empty,
            difficultyId: difficultyId,
            stateId: WorkoutStateId.Empty,
            page: 1,
            pageSize: 20,
            sortBy: "name",
            sortOrder: "asc");
        
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data.Items);
    }
    
    [Fact]
    public async Task SearchAsync_WithMultipleFilters_ExcludingNamePattern_ReturnsFilteredResults()
    {
        // Note: Testing multiple filters WITHOUT name pattern because MockQueryable 
        // doesn't support EF.Functions.ILike. Name pattern testing is covered in integration tests.
        
        // Arrange
        var categoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.Strength);
        var difficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Beginner);
        var stateId = WorkoutStateId.ParseOrEmpty(TestIds.WorkoutStateIds.Production);
        var templates = new List<WorkoutTemplate> { _testTemplate }.BuildAsyncQueryable();
        
        _mockRepository
            .Setup(x => x.GetWorkoutTemplatesQueryable())
            .Returns(templates);
        
        _mockRepository
            .Setup(x => x.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(true);
            
        // Act - test without name pattern
        var result = await _service.SearchAsync(
            namePattern: string.Empty,  // No name pattern to avoid ILike
            categoryId: categoryId,
            objectiveId: WorkoutObjectiveId.Empty,
            difficultyId: difficultyId,
            stateId: stateId,
            page: 1,
            pageSize: 20,
            sortBy: "name",
            sortOrder: "desc");
        
        Assert.True(result.IsSuccess);
        Assert.Single(result.Data.Items);
        Assert.Equal(_testTemplate.Name, result.Data.Items.First().Name);
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
            .Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(_testTemplate);
            
        _mockRepository
            .Setup(x => x.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(false);
            
        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<WorkoutTemplate>()))
            .ReturnsAsync(_testTemplate);
            
        // Act
        var result = await _service.DuplicateAsync(_testTemplateId, newName);
        
        Assert.True(result.IsSuccess);
        _mockWritableUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }
    
    [Fact]
    public async Task DuplicateAsync_WithEmptyName_ReturnsFailure()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(_testTemplate);
            
        // Act
        var result = await _service.DuplicateAsync(_testTemplateId, "");
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.ValidationFailed, result.PrimaryErrorCode);
    }
    
    
    #endregion
    
    #region Exists Tests
    
    [Fact]
    public async Task ExistsAsync_WithExistingId_ReturnsTrue()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.ExistsAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(true);
            
        // Act
        var result = await _service.ExistsAsync(_testTemplateId);
        
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }
    
    [Fact]
    public async Task ExistsAsync_WithEmptyId_ReturnsFalse()
    {
        // Act
        var result = await _service.ExistsAsync(WorkoutTemplateId.Empty);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.InvalidFormat, result.PrimaryErrorCode);
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
        var result = await _service.ExistsAsync(WorkoutTemplateId.ParseOrEmpty(stringId));
        
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
        _mockRepository.Verify(x => x.ExistsAsync(_testTemplateId), Times.Once);
    }
    
    [Fact]
    public async Task ExistsByNameAsync_WithExistingName_ReturnsTrue()
    {
        // Arrange
        var name = "Existing Workout";
        _mockRepository
            .Setup(x => x.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(true);
            
        // Act
        var result = await _service.ExistsByNameAsync(name);
        
        Assert.True(result);
    }
    
    [Fact]
    public async Task ExistsByNameAsync_WithEmptyName_ReturnsFalse()
    {
        // Act
        var result = await _service.ExistsByNameAsync("");
        
        Assert.False(result);
        _mockRepository.Verify(x => x.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<WorkoutTemplateId>()), Times.Never);
    }
    
    #endregion
}
using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplate;
using GetFitterGetBigger.API.Services.Implementations;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Tests.TestBuilders;
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;
using GetFitterGetBigger.API.Tests.Services.Extensions;
using GetFitterGetBigger.API.Tests.TestHelpers;
using Moq;
using Moq.AutoMock;

namespace GetFitterGetBigger.API.Tests.Services;

public class WorkoutTemplateServiceTests
{
    
    #region GetById Tests
    
    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        const string expectedName = "Test Workout Template";
        const string expectedDescription = "Test Description";
        
        var productionState = WorkoutStateTestBuilder.Production().Build();
        var testTemplate = new WorkoutTemplateBuilder()
            .WithId(templateId)
            .WithName(expectedName)
            .WithDescription(expectedDescription)
            .WithCategoryId(TestIds.WorkoutCategoryIds.Strength)
            .WithDifficultyId(TestIds.DifficultyLevelIds.Beginner)
            .WithWorkoutStateId(TestIds.WorkoutStateIds.Production)
            .WithWorkoutState(productionState)
            .Build();
        
        automocker
            .SetupWorkoutTemplateUnitOfWork()
            .SetupWorkoutTemplateGetByIdWithDetails(testTemplate);
        
        // Act
        var result = await testee.GetByIdAsync(templateId);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Name.Should().Be(expectedName);
        result.Data.Description.Should().Be(expectedDescription);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ReturnsInvalidFormatError()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var emptyId = WorkoutTemplateId.Empty;
        
        // Act
        var result = await testee.GetByIdAsync(emptyId);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Data.IsEmpty.Should().BeTrue(because: "empty ID should result in Empty DTO");
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.InvalidFormat);
        
        automocker.VerifyWorkoutTemplateGetByIdWithDetailsNeverCalled();
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        
        automocker
            .SetupWorkoutTemplateUnitOfWork()
            .SetupWorkoutTemplateGetByIdWithDetails(WorkoutTemplate.Empty);
        
        // Act
        var result = await testee.GetByIdAsync(templateId);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Data.IsEmpty.Should().BeTrue();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.NotFound);
    }
    
    #endregion
    
    
    #region Create Tests
    
    [Fact]
    public async Task CreateAsync_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        const string workoutName = "New Workout";
        const string workoutDescription = "New Description";
        
        var command = new CreateWorkoutTemplateCommand
        {
            Name = workoutName,
            Description = workoutDescription,
            CategoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.Strength),
            DifficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Beginner),
            EstimatedDurationMinutes = 60,
            Tags = ["test", "workout"],
            IsPublic = true
        };
        
        var draftWorkoutStateDto = new WorkoutStateDto
        {
            Id = TestIds.WorkoutStateIds.Draft,
            Value = "Draft",
            Description = "Draft state for new workout templates"
        };
        
        var createdTemplate = new WorkoutTemplateBuilder()
            .WithName(workoutName)
            .WithDescription(workoutDescription)
            .Build();
        
        automocker
            .SetupWorkoutTemplateUnitOfWork()
            .SetupWorkoutTemplateExists(workoutName, false)
            .SetupWorkoutTemplateAdd(createdTemplate)
            .SetupWorkoutStateService(draftWorkoutStateDto);
        
        // Act
        var result = await testee.CreateAsync(command);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.IsEmpty.Should().BeFalse();
        result.Data.Name.Should().Be(workoutName);
        
        automocker.VerifyWorkoutTemplateAddOnce();
    }
    
    [Fact]
    public async Task CreateAsync_WithNullCommand_ReturnsValidationFailedError()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        // Act
        var result = await testee.CreateAsync(null!);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Data.IsEmpty.Should().BeTrue();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        
        automocker.VerifyWorkoutTemplateAddNeverCalled();
    }
    
    [Fact]
    public async Task CreateAsync_WithDuplicateName_ReturnsAlreadyExistsError()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        const string existingName = "Existing Workout";
        
        var command = new CreateWorkoutTemplateCommand
        {
            Name = existingName,
            Description = "Description",
            CategoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.Strength),
            DifficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Beginner),
            EstimatedDurationMinutes = 60,
            Tags = [],
            IsPublic = true
        };
        
        automocker
            .SetupWorkoutTemplateUnitOfWork()
            .SetupWorkoutTemplateExists(existingName, true);
        
        // Act
        var result = await testee.CreateAsync(command);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Data.IsEmpty.Should().BeTrue();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.AlreadyExists);
        
        automocker.VerifyWorkoutTemplateAddNeverCalled();
    }
    
    [Fact]
    public async Task CreateAsync_WithInvalidName_ReturnsValidationFailedError()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var command = new CreateWorkoutTemplateCommand
        {
            Name = "AB", // Too short
            Description = "Description",
            CategoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.Strength),
            DifficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Beginner),
            EstimatedDurationMinutes = 60,
            Tags = [],
            IsPublic = true
        };
        
        // Act
        var result = await testee.CreateAsync(command);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Data.IsEmpty.Should().BeTrue();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        
        automocker.VerifyWorkoutTemplateRepositoryNotAccessedForValidationFailure();
    }
    
    #endregion
    
    #region Update Tests
    
    [Fact]
    public async Task UpdateAsync_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.Template1);
        const string originalName = "Original Workout";
        const string updatedName = "Updated Workout";
        const string updatedDescription = "Updated Description";
        
        var command = new UpdateWorkoutTemplateCommand
        {
            Id = templateId,
            Name = updatedName,
            Description = updatedDescription,
            CategoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.Strength),
            DifficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Intermediate),
            EstimatedDurationMinutes = 45,
            Tags = ["updated"],
            IsPublic = false
        };
        
        var existingTemplate = new WorkoutTemplateBuilder()
            .WithId(templateId)
            .WithName(originalName)
            .Build();
            
        var updatedTemplate = new WorkoutTemplateBuilder()
            .WithId(templateId)
            .WithName(updatedName)
            .WithDescription(updatedDescription)
            .Build();
        
        // Setup different returns for multiple calls
        var getByIdCallCount = 0;
        automocker.GetMock<IWorkoutTemplateRepository>()
            .Setup(r => r.GetByIdAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(() => {
                getByIdCallCount++;
                return getByIdCallCount == 1 ? existingTemplate : updatedTemplate;
            });
        
        automocker
            .SetupWorkoutTemplateUnitOfWork()
            .SetupWorkoutTemplateGetByIdWithDetails(existingTemplate)
            .SetupWorkoutTemplateExistsById(templateId, true)  // Template exists for update
            .SetupWorkoutTemplateExists(updatedName, false)
            .SetupWorkoutTemplateQueryable([existingTemplate])  // Setup for name uniqueness check
            .SetupWorkoutTemplateUpdate(updatedTemplate);
        
        // Act
        var result = await testee.UpdateAsync(templateId, command);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.IsEmpty.Should().BeFalse();
        result.Data.Name.Should().Be(updatedName);
        result.Data.Description.Should().Be(updatedDescription);
        
        automocker.VerifyWorkoutTemplateUpdateOnce();
    }
    
    [Fact]
    public async Task UpdateAsync_WhenTemplateNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.Template1);
        
        var command = new UpdateWorkoutTemplateCommand
        {
            Id = templateId,
            Name = "Updated Workout",
            Description = "Updated Description",
            CategoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.Strength),
            DifficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Intermediate),
            EstimatedDurationMinutes = 45,
            Tags = [],
            IsPublic = false
        };
        
        automocker
            .SetupWorkoutTemplateUnitOfWork()
            .SetupWorkoutTemplateGetByIdWithDetails(WorkoutTemplate.Empty);
        
        // Act
        var result = await testee.UpdateAsync(templateId, command);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Data.IsEmpty.Should().BeTrue();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.NotFound);
        
        automocker.VerifyWorkoutTemplateUpdateNeverCalled();
    }
    
    #endregion
    
    #region ChangeState Tests
    
    [Fact]
    public async Task ChangeStateAsync_WithValidState_ReturnsSuccess()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        var newStateId = WorkoutStateId.ParseOrEmpty(TestIds.WorkoutStateIds.Production);
        
        var existingTemplate = new WorkoutTemplateBuilder()
            .WithId(templateId)
            .WithName("Test Template")
            .WithWorkoutStateId(TestIds.WorkoutStateIds.Draft)
            .Build();
        
        var updatedTemplate = new WorkoutTemplateBuilder()
            .WithId(templateId)
            .WithName("Test Template")
            .WithWorkoutStateId(TestIds.WorkoutStateIds.Production)
            .Build();
        
        // Setup different returns for multiple calls
        var getByIdCallCount = 0;
        automocker.GetMock<IWorkoutTemplateRepository>()
            .Setup(r => r.GetByIdAsync(It.IsAny<WorkoutTemplateId>()))
            .ReturnsAsync(() => {
                getByIdCallCount++;
                return getByIdCallCount == 1 ? existingTemplate : updatedTemplate;
            });
        
        automocker
            .SetupWorkoutTemplateUnitOfWork()
            .SetupWorkoutTemplateGetByIdWithDetails(existingTemplate)
            .SetupWorkoutTemplateUpdate(updatedTemplate);
        
        // Act
        var result = await testee.ChangeStateAsync(templateId, newStateId);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.IsEmpty.Should().BeFalse();
        
        automocker.VerifyWorkoutTemplateUpdateOnce();
    }
    
    [Fact]
    public async Task ChangeStateAsync_WithEmptyStateId_ReturnsInvalidFormatError()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        
        var existingTemplate = new WorkoutTemplateBuilder()
            .WithId(templateId)
            .WithName("Test Template")
            .Build();
        
        automocker
            .SetupWorkoutTemplateUnitOfWork()
            .SetupWorkoutTemplateGetByIdWithDetails(existingTemplate);
        
        // Act
        var result = await testee.ChangeStateAsync(templateId, WorkoutStateId.Empty);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Data.IsEmpty.Should().BeTrue();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.InvalidFormat);
        
        automocker.VerifyWorkoutTemplateUpdateNeverCalled();
    }
    
    #endregion
    
    #region Delete Tests
    
    [Fact]
    public async Task SoftDeleteAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        
        var existingTemplate = new WorkoutTemplateBuilder()
            .WithId(templateId)
            .WithName("Test Template")
            .Build();
        
        automocker
            .SetupWorkoutTemplateUnitOfWork()
            .SetupWorkoutTemplateGetByIdWithDetails(existingTemplate)
            .SetupWorkoutTemplateSoftDelete(true);
        
        // Act
        var result = await testee.SoftDeleteAsync(templateId);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
        
        automocker.VerifyWorkoutTemplateSoftDeleteOnce(templateId);
    }
    
    [Fact]
    public async Task SoftDeleteAsync_WhenTemplateNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        
        automocker
            .SetupWorkoutTemplateUnitOfWork()
            .SetupWorkoutTemplateGetByIdWithDetails(null!);
        
        // Act
        var result = await testee.SoftDeleteAsync(templateId);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse(because: "template not found should return failure");
        result.Data.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.NotFound);
        
        automocker.VerifyWorkoutTemplateSoftDeleteNeverCalled();
    }
    
    [Fact]
    public async Task DeleteAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        
        var existingTemplate = new WorkoutTemplateBuilder()
            .WithId(templateId)
            .WithName("Test Template")
            .Build();
        
        automocker
            .SetupWorkoutTemplateUnitOfWork()
            .SetupWorkoutTemplateGetByIdWithDetails(existingTemplate)
            .SetupWorkoutTemplateDelete(true);
        
        // Act
        var result = await testee.DeleteAsync(templateId);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
        
        automocker.VerifyWorkoutTemplateDeleteOnce(templateId);
    }
    
    [Fact]
    public async Task DeleteAsync_WhenTemplateNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        
        automocker
            .SetupWorkoutTemplateUnitOfWork()
            .SetupWorkoutTemplateGetByIdWithDetails(null!);
        
        // Act
        var result = await testee.DeleteAsync(templateId);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse(because: "template not found should return failure");
        result.Data.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.NotFound);
        
        automocker.VerifyWorkoutTemplateDeleteNeverCalled();
    }
    
    #endregion
    
    #region Filtering Tests
    
    [Fact]
    public async Task SearchAsync_WithEmptyParameters_ReturnsAllResults()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var testTemplate = new WorkoutTemplateBuilder()
            .WithName("Test Template")
            .WithWorkoutState(WorkoutStateTestBuilder.Production().Build())
            .Build();
            
        var templates = new List<WorkoutTemplate> { testTemplate }.BuildAsyncQueryable();
        
        automocker
            .SetupWorkoutTemplateUnitOfWork()
            .SetupWorkoutTemplateQueryable(templates);
        
        // Act
        var result = await testee.SearchAsync(
            namePattern: string.Empty,
            categoryId: WorkoutCategoryId.Empty,
            objectiveId: WorkoutObjectiveId.Empty,
            difficultyId: DifficultyLevelId.Empty,
            stateId: WorkoutStateId.Empty,
            page: 1,
            pageSize: 20,
            sortBy: "name",
            sortOrder: "asc");
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Items.Should().HaveCount(1);
        
        automocker.VerifyWorkoutTemplateQueryableOnce();
    }
    
    [Fact]
    public async Task SearchAsync_WithCategoryFilter_ReturnsFilteredResults()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var categoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.Strength);
        
        var testTemplate = new WorkoutTemplateBuilder()
            .WithName("Strength Template")
            .WithCategoryId(TestIds.WorkoutCategoryIds.Strength)
            .WithWorkoutState(WorkoutStateTestBuilder.Production().Build())
            .Build();
            
        var templates = new List<WorkoutTemplate> { testTemplate }.BuildAsyncQueryable();
        
        automocker
            .SetupWorkoutTemplateUnitOfWork()
            .SetupWorkoutTemplateQueryable(templates);
        
        // Act
        var result = await testee.SearchAsync(
            namePattern: string.Empty,
            categoryId: categoryId,
            objectiveId: WorkoutObjectiveId.Empty,
            difficultyId: DifficultyLevelId.Empty,
            stateId: WorkoutStateId.Empty,
            page: 1,
            pageSize: 20,
            sortBy: "name",
            sortOrder: "asc");
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Items.Should().HaveCount(1);
    }
    
    [Fact]
    public async Task SearchAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
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
            .ToList();
        
        automocker
            .SetupWorkoutTemplateUnitOfWork()
            .SetupWorkoutTemplateQueryable(templates);
        
        // Act
        var result = await testee.SearchAsync(
            namePattern: string.Empty,
            categoryId: WorkoutCategoryId.Empty,
            objectiveId: WorkoutObjectiveId.Empty,
            difficultyId: DifficultyLevelId.Empty,
            stateId: WorkoutStateId.Empty,
            page: 2,
            pageSize: 10,
            sortBy: "name",
            sortOrder: "asc");
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Items.Should().HaveCount(10);
        result.Data.TotalCount.Should().Be(30);
        result.Data.CurrentPage.Should().Be(2);
        result.Data.TotalPages.Should().Be(3);
    }
    
    [Fact]
    public async Task SearchAsync_WithObjectiveFilter_ReturnsFilteredResults()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var objectiveId = WorkoutObjectiveId.ParseOrEmpty(TestIds.WorkoutObjectiveIds.BuildMuscle);
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        
        var productionState = WorkoutStateTestBuilder.Default()
            .WithValue("PRODUCTION")
            .WithDescription("Production state")
            .Build();
            
        var objective = WorkoutTemplateObjective.Handler.Create(templateId, objectiveId).Value!;
        var templateWithObjective = new WorkoutTemplateBuilder()
            .WithId(templateId)
            .WithName("Test Workout Template")
            .WithDescription("Test Description")
            .WithCategoryId(TestIds.WorkoutCategoryIds.Strength)
            .WithDifficultyId(TestIds.DifficultyLevelIds.Beginner)
            .WithWorkoutStateId(TestIds.WorkoutStateIds.Production)
            .WithWorkoutState(productionState)
            .WithObjective(objective)
            .Build();
            
        var templates = new List<WorkoutTemplate> { templateWithObjective };
        
        automocker
            .SetupWorkoutTemplateUnitOfWork()
            .SetupWorkoutTemplateQueryable(templates);
        
        // Act
        var result = await testee.SearchAsync(
            namePattern: string.Empty,
            categoryId: WorkoutCategoryId.Empty,
            objectiveId: objectiveId,
            difficultyId: DifficultyLevelId.Empty,
            stateId: WorkoutStateId.Empty,
            page: 1,
            pageSize: 20,
            sortBy: "name",
            sortOrder: "asc");
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Items.Should().HaveCount(1);
    }
    
    [Fact]
    public async Task SearchAsync_WithDifficultyFilter_ReturnsFilteredResults()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var difficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Beginner);
        
        var testTemplate = new WorkoutTemplateBuilder()
            .WithName("Beginner Template")
            .WithDifficultyId(TestIds.DifficultyLevelIds.Beginner)
            .WithWorkoutState(WorkoutStateTestBuilder.Production().Build())
            .Build();
            
        var templates = new List<WorkoutTemplate> { testTemplate };
        
        automocker
            .SetupWorkoutTemplateUnitOfWork()
            .SetupWorkoutTemplateQueryable(templates);
        
        // Act
        var result = await testee.SearchAsync(
            namePattern: string.Empty,
            categoryId: WorkoutCategoryId.Empty,
            objectiveId: WorkoutObjectiveId.Empty,
            difficultyId: difficultyId,
            stateId: WorkoutStateId.Empty,
            page: 1,
            pageSize: 20,
            sortBy: "name",
            sortOrder: "asc");
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Items.Should().HaveCount(1);
    }
    
    [Fact]
    public async Task SearchAsync_WithMultipleFilters_ExcludingNamePattern_ReturnsFilteredResults()
    {
        // Note: Testing multiple filters WITHOUT name pattern because MockQueryable 
        // doesn't support EF.Functions.ILike. Name pattern testing is covered in integration tests.
        
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var categoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.Strength);
        var difficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Beginner);
        var stateId = WorkoutStateId.ParseOrEmpty(TestIds.WorkoutStateIds.Production);
        
        const string templateName = "Multi-Filter Template";
        
        var testTemplate = new WorkoutTemplateBuilder()
            .WithName(templateName)
            .WithCategoryId(TestIds.WorkoutCategoryIds.Strength)
            .WithDifficultyId(TestIds.DifficultyLevelIds.Beginner)
            .WithWorkoutStateId(TestIds.WorkoutStateIds.Production)
            .WithWorkoutState(WorkoutStateTestBuilder.Production().Build())
            .Build();
            
        var templates = new List<WorkoutTemplate> { testTemplate };
        
        automocker
            .SetupWorkoutTemplateUnitOfWork()
            .SetupWorkoutTemplateQueryable(templates);
        
        // Act - test without name pattern
        var result = await testee.SearchAsync(
            namePattern: string.Empty,  // No name pattern to avoid ILike
            categoryId: categoryId,
            objectiveId: WorkoutObjectiveId.Empty,
            difficultyId: difficultyId,
            stateId: stateId,
            page: 1,
            pageSize: 20,
            sortBy: "name",
            sortOrder: "desc");
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Items.Should().HaveCount(1);
        result.Data.Items.First().Name.Should().Be(templateName);
    }
    
    #endregion
    
    #region Duplicate Tests
    
    [Fact]
    public async Task DuplicateAsync_WithValidParameters_ReturnsSuccess()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        const string originalName = "Original Template";
        const string newName = "Duplicated Workout";
        
        var originalTemplate = new WorkoutTemplateBuilder()
            .WithId(templateId)
            .WithName(originalName)
            .WithDescription("Original Description")
            .Build();
            
        var duplicatedTemplate = new WorkoutTemplateBuilder()
            .WithId(WorkoutTemplateId.New())
            .WithName(newName)
            .WithDescription("Original Description")
            .Build();
        
        automocker
            .SetupWorkoutTemplateUnitOfWork()
            .SetupWorkoutTemplateGetByIdWithDetails(originalTemplate)
            .SetupWorkoutTemplateExists(newName, false)
            .SetupWorkoutTemplateAdd(duplicatedTemplate);
        
        // Act
        var result = await testee.DuplicateAsync(templateId, newName);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.IsEmpty.Should().BeFalse();
        result.Data.Name.Should().Be(newName);
        
        automocker.VerifyWorkoutTemplateAddOnce();
    }
    
    [Fact]
    public async Task DuplicateAsync_WithEmptyName_ReturnsValidationFailedError()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        
        var existingTemplate = new WorkoutTemplateBuilder()
            .WithId(templateId)
            .WithName("Original Template")
            .Build();
        
        automocker
            .SetupWorkoutTemplateUnitOfWork()
            .SetupWorkoutTemplateGetByIdWithDetails(existingTemplate);
        
        // Act
        var result = await testee.DuplicateAsync(templateId, "");
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Data.IsEmpty.Should().BeTrue();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        
        automocker.VerifyWorkoutTemplateAddNeverCalled();
    }
    
    [Fact]
    public async Task DuplicateAsync_WhenTemplateNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        const string newName = "Duplicated Workout";
        
        automocker
            .SetupWorkoutTemplateUnitOfWork()
            .SetupWorkoutTemplateGetByIdWithDetails(WorkoutTemplate.Empty);
        
        // Act
        var result = await testee.DuplicateAsync(templateId, newName);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Data.IsEmpty.Should().BeTrue();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.NotFound);
        
        automocker.VerifyWorkoutTemplateAddNeverCalled();
    }
    
    #endregion
    
    #region Exists Tests
    
    [Fact]
    public async Task ExistsAsync_WithExistingId_ReturnsTrue()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        
        automocker
            .SetupWorkoutTemplateUnitOfWork()
            .SetupWorkoutTemplateExistsById(templateId, true);
        
        // Act
        var result = await testee.ExistsAsync(templateId);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
        
        automocker.VerifyWorkoutTemplateExistsByIdOnce(templateId);
    }
    
    [Fact]
    public async Task ExistsAsync_WithNonExistentId_ReturnsFalse()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        
        automocker
            .SetupWorkoutTemplateUnitOfWork()
            .SetupWorkoutTemplateExistsById(templateId, false);
        
        // Act
        var result = await testee.ExistsAsync(templateId);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeFalse();
        
        automocker.VerifyWorkoutTemplateExistsByIdOnce(templateId);
    }
    
    [Fact]
    public async Task ExistsAsync_WithEmptyId_ReturnsInvalidFormatError()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        // Act
        var result = await testee.ExistsAsync(WorkoutTemplateId.Empty);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Data.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.InvalidFormat);
        
        automocker.VerifyWorkoutTemplateRepositoryNotAccessedForValidationFailure();
    }
    
    [Fact]
    public async Task ExistsByNameAsync_WithExistingName_ReturnsTrue()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        const string existingName = "Existing Workout";
        
        automocker
            .SetupWorkoutTemplateUnitOfWork()
            .SetupWorkoutTemplateExists(existingName, true);
        
        // Act
        var result = await testee.ExistsByNameAsync(existingName);
        
        // Assert
        result.Should().BeTrue();
        
        automocker.VerifyWorkoutTemplateExistsByNameOnce(existingName);
    }
    
    [Fact]
    public async Task ExistsByNameAsync_WithNonExistentName_ReturnsFalse()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        const string nonExistentName = "Non-Existent Workout";
        
        automocker
            .SetupWorkoutTemplateUnitOfWork()
            .SetupWorkoutTemplateExists(nonExistentName, false);
        
        // Act
        var result = await testee.ExistsByNameAsync(nonExistentName);
        
        // Assert
        result.Should().BeFalse();
        
        automocker.VerifyWorkoutTemplateExistsByNameOnce(nonExistentName);
    }
    
    [Fact]
    public async Task ExistsByNameAsync_WithEmptyName_ReturnsFalse()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        // Act
        var result = await testee.ExistsByNameAsync("");
        
        // Assert
        result.Should().BeFalse(because: "empty names should not be considered as existing");
        
        // Verify that repository was not called for empty names
        automocker.VerifyWorkoutTemplateRepositoryNotAccessedForValidationFailure();
    }
    
    #endregion
}
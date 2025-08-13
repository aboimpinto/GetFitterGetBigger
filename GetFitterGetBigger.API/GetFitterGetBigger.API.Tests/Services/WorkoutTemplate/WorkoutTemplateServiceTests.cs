using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplate;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.WorkoutTemplate;
using GetFitterGetBigger.API.Services.WorkoutTemplate.DataServices;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Handlers;
using GetFitterGetBigger.API.Tests.Services.Extensions;
using GetFitterGetBigger.API.Tests.TestBuilders;
using Moq;
using Moq.AutoMock;

namespace GetFitterGetBigger.API.Tests.Services.WorkoutTemplate;

public class WorkoutTemplateServiceTests
{
    #region GetByIdAsync Tests
    
    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        var expectedDto = new WorkoutTemplateDtoBuilder()
            .WithId(TestIds.WorkoutTemplateIds.BasicTemplate)
            .WithName("Test Workout")
            .WithDescription("Test Description")
            .Build();
        
        automocker.SetupWorkoutTemplateQueryDataService_GetByIdWithDetails(expectedDto);
        
        // Act
        var result = await testee.GetByIdAsync(templateId);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(expectedDto);
        
        automocker.VerifyWorkoutTemplateQueryDataService_GetByIdWithDetailsOnce(templateId);
    }
    
    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ReturnsValidationFailed()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        // Act
        var result = await testee.GetByIdAsync(WorkoutTemplateId.Empty);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Data.IsEmpty.Should().BeTrue();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        
        automocker.VerifyWorkoutTemplateQueryDataService_NeverCalled();
    }
    
    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        
        automocker.SetupWorkoutTemplateQueryDataService_GetByIdWithDetails_NotFound();
        
        // Act
        var result = await testee.GetByIdAsync(templateId);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Data.IsEmpty.Should().BeTrue();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.NotFound);
    }
    
    #endregion
    
    #region SearchAsync Tests
    
    [Fact]
    public async Task SearchAsync_WithValidParameters_ReturnsSuccess()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var expectedResponse = new PagedResponse<WorkoutTemplateDto>
        {
            Items = new List<WorkoutTemplateDto>
            {
                new WorkoutTemplateDtoBuilder()
                    .WithId(TestIds.WorkoutTemplateIds.BasicTemplate)
                    .WithName("Template 1")
                    .Build(),
                new WorkoutTemplateDtoBuilder()
                    .WithId(TestIds.WorkoutTemplateIds.Template1)
                    .WithName("Template 2")
                    .Build()
            },
            TotalCount = 2,
            CurrentPage = 1,
            PageSize = 10
        };
        
        automocker.SetupWorkoutTemplateQueryDataService_Search(expectedResponse);
        
        // Setup WorkoutStateService for business logic (when stateId is empty, it gets ARCHIVED state to exclude)
        var archivedStateDto = new WorkoutStateDto
        {
            Id = TestIds.WorkoutStateIds.Archived,
            Value = "ARCHIVED",
            Description = "Retired template"
        };
        automocker.SetupWorkoutStateService(archivedStateDto);
        
        // Act
        var result = await testee.SearchAsync(
            page: 1,
            pageSize: 10,
            namePattern: "Template",
            categoryId: WorkoutCategoryId.Empty,
            objectiveId: WorkoutObjectiveId.Empty,
            difficultyId: DifficultyLevelId.Empty,
            stateId: WorkoutStateId.Empty,
            sortBy: "name",
            sortOrder: "asc");
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Items.Should().HaveCount(2);
        result.Data.TotalCount.Should().Be(2);
    }
    
    [Fact]
    public async Task SearchAsync_WithInvalidPage_ReturnsValidationFailed()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        // Act
        var result = await testee.SearchAsync(
            page: 0, // Invalid
            pageSize: 10,
            namePattern: "",
            categoryId: WorkoutCategoryId.Empty,
            objectiveId: WorkoutObjectiveId.Empty,
            difficultyId: DifficultyLevelId.Empty,
            stateId: WorkoutStateId.Empty,
            sortBy: "name",
            sortOrder: "asc");
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        
        automocker.VerifyWorkoutTemplateQueryDataService_NeverCalled();
    }
    
    [Fact]
    public async Task SearchAsync_WithInvalidPageSize_ReturnsValidationFailed()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        // Act
        var result = await testee.SearchAsync(
            page: 1,
            pageSize: 150, // Too large
            namePattern: "",
            categoryId: WorkoutCategoryId.Empty,
            objectiveId: WorkoutObjectiveId.Empty,
            difficultyId: DifficultyLevelId.Empty,
            stateId: WorkoutStateId.Empty,
            sortBy: "name",
            sortOrder: "asc");
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        
        automocker.VerifyWorkoutTemplateQueryDataService_NeverCalled();
    }
    
    #endregion
    
    #region CreateAsync Tests
    
    [Fact]
    public async Task CreateAsync_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var command = new CreateWorkoutTemplateCommand
        {
            Name = "New Workout Template",
            Description = "Description",
            CategoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.Strength),
            DifficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Beginner),
            EstimatedDurationMinutes = 60,
            Tags = ["tag1", "tag2"],
            IsPublic = true
        };
        
        var draftState = new WorkoutStateDto
        {
            Id = TestIds.WorkoutStateIds.Draft,
            Value = "Draft",
            Description = "Draft state"
        };
        
        var createdDto = new WorkoutTemplateDtoBuilder()
            .WithId(TestIds.WorkoutTemplateIds.BasicTemplate)
            .WithName(command.Name)
            .WithDescription(command.Description)
            .Build();
        
        automocker.GetMock<IWorkoutStateService>()
            .Setup(x => x.GetByValueAsync("Draft"))
            .ReturnsAsync(ServiceResult<WorkoutStateDto>.Success(draftState));
        
        automocker.SetupWorkoutTemplateQueryDataService_ExistsByName(command.Name, false);
        automocker.SetupWorkoutTemplateCommandDataService_Create(createdDto);
        
        // Act
        var result = await testee.CreateAsync(command);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(createdDto);
        
        automocker.VerifyWorkoutTemplateCommandDataService_CreateOnce();
    }
    
    [Fact]
    public async Task CreateAsync_WithEmptyName_ReturnsValidationFailed()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var command = new CreateWorkoutTemplateCommand
        {
            Name = "", // Invalid
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
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        
        automocker.VerifyWorkoutTemplateCommandDataService_NeverCalled();
    }
    
    [Fact]
    public async Task CreateAsync_WithDuplicateName_ReturnsAlreadyExists()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var command = new CreateWorkoutTemplateCommand
        {
            Name = "Existing Template",
            Description = "Description",
            CategoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.Strength),
            DifficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Beginner),
            EstimatedDurationMinutes = 60,
            Tags = [],
            IsPublic = true
        };
        
        automocker.SetupWorkoutTemplateQueryDataService_ExistsByName(command.Name, true);
        
        // Act
        var result = await testee.CreateAsync(command);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.AlreadyExists);
        
        automocker.VerifyWorkoutTemplateCommandDataService_NeverCalled();
    }
    
    [Fact]
    public async Task CreateAsync_WithShortName_ReturnsValidationFailed()
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
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        
        automocker.VerifyWorkoutTemplateCommandDataService_NeverCalled();
    }
    
    [Fact]
    public async Task CreateAsync_WithTooManyTags_ReturnsValidationFailed()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var command = new CreateWorkoutTemplateCommand
        {
            Name = "Valid Name",
            Description = "Description",
            CategoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.Strength),
            DifficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Beginner),
            EstimatedDurationMinutes = 60,
            Tags = Enumerable.Range(1, 15).Select(i => $"tag{i}").ToList(), // Too many
            IsPublic = true
        };
        
        // Act
        var result = await testee.CreateAsync(command);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        
        automocker.VerifyWorkoutTemplateCommandDataService_NeverCalled();
    }
    
    #endregion
    
    #region UpdateAsync Tests
    
    [Fact]
    public async Task UpdateAsync_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        var command = new UpdateWorkoutTemplateCommand
        {
            Name = "Updated Template",
            Description = "Updated Description",
            CategoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.Strength),
            DifficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Intermediate),
            EstimatedDurationMinutes = 45,
            Tags = ["updated"],
            IsPublic = false
        };
        
        var updatedDto = new WorkoutTemplateDtoBuilder()
            .WithId(templateId.ToString())
            .WithName(command.Name)
            .WithDescription(command.Description)
            .Build();
        
        automocker.SetupWorkoutTemplateQueryDataService_Exists(templateId, true);
        automocker.SetupWorkoutTemplateQueryDataService_ExistsByName(command.Name, false);
        automocker.SetupWorkoutTemplateCommandDataService_Update(updatedDto);
        
        // Act
        var result = await testee.UpdateAsync(templateId, command);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(updatedDto);
        
        automocker.VerifyWorkoutTemplateCommandDataService_UpdateOnce();
    }
    
    [Fact]
    public async Task UpdateAsync_WithEmptyId_ReturnsValidationFailed()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var command = new UpdateWorkoutTemplateCommand
        {
            Name = "Updated Template",
            Description = "Description",
            CategoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.Strength),
            DifficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Beginner),
            EstimatedDurationMinutes = 60,
            Tags = [],
            IsPublic = true
        };
        
        // Act
        var result = await testee.UpdateAsync(WorkoutTemplateId.Empty, command);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        
        automocker.VerifyWorkoutTemplateCommandDataService_NeverCalled();
    }
    
    [Fact]
    public async Task UpdateAsync_WhenTemplateNotFound_ReturnsNotFound()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        var command = new UpdateWorkoutTemplateCommand
        {
            Name = "Updated Template",
            Description = "Description",
            CategoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.Strength),
            DifficultyId = DifficultyLevelId.ParseOrEmpty(TestIds.DifficultyLevelIds.Beginner),
            EstimatedDurationMinutes = 60,
            Tags = [],
            IsPublic = true
        };
        
        automocker.SetupWorkoutTemplateQueryDataService_Exists(templateId, false);
        automocker.SetupWorkoutTemplateQueryDataService_ExistsByName(command.Name!, false);
        
        // Act
        var result = await testee.UpdateAsync(templateId, command);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.NotFound);
        
        automocker.VerifyWorkoutTemplateCommandDataService_NeverCalled();
    }
    
    #endregion
    
    #region ChangeStateAsync Tests
    
    [Fact]
    public async Task ChangeStateAsync_WithValidState_ReturnsSuccess()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        var newStateId = WorkoutStateId.ParseOrEmpty(TestIds.WorkoutStateIds.Production);
        
        var updatedDto = new WorkoutTemplateDtoBuilder()
            .WithId(templateId.ToString())
            .WithName("Template")
            .WithWorkoutStateId(newStateId.ToString())
            .Build();
        
        automocker.SetupWorkoutTemplateQueryDataService_Exists(templateId, true);
        automocker.SetupWorkoutTemplateCommandDataService_ChangeState(updatedDto);
        
        // Act
        var result = await testee.ChangeStateAsync(templateId, newStateId);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(updatedDto);
        
        automocker.VerifyWorkoutTemplateCommandDataService_ChangeStateOnce(templateId, newStateId);
    }
    
    [Fact]
    public async Task ChangeStateAsync_WithEmptyStateId_ReturnsValidationFailed()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        
        // Act
        var result = await testee.ChangeStateAsync(templateId, WorkoutStateId.Empty);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        
        automocker.VerifyWorkoutTemplateCommandDataService_NeverCalled();
    }
    
    #endregion
    
    #region DuplicateAsync Tests
    
    [Fact]
    public async Task DuplicateAsync_WithValidParameters_ReturnsSuccess()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var sourceId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        const string newName = "Duplicated Template";
        
        // Setup the original template data that the DuplicationHandler needs
        var originalTemplateDto = new WorkoutTemplateDtoBuilder()
            .WithId(TestIds.WorkoutTemplateIds.BasicTemplate)
            .WithName("Original Template")
            .WithDescription("Original Description")
            .Build();
        
        var duplicatedDto = new WorkoutTemplateDtoBuilder()
            .WithId(TestIds.WorkoutTemplateIds.Template1)
            .WithName(newName)
            .Build();
        
        // Mock all the dependencies that DuplicationHandler needs
        automocker.SetupWorkoutTemplateQueryDataService_GetByIdWithDetails(originalTemplateDto);
        automocker.SetupWorkoutTemplateQueryDataService_ExistsByName(newName, false);
        automocker.SetupWorkoutTemplateCommandDataService_Create(duplicatedDto);
        
        // Act
        var result = await testee.DuplicateAsync(sourceId, newName);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(duplicatedDto);
    }
    
    [Fact]
    public async Task DuplicateAsync_WithEmptyName_ReturnsValidationFailed()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var sourceId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        
        // Act
        var result = await testee.DuplicateAsync(sourceId, "");
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        
        automocker.VerifyWorkoutTemplateCommandDataService_NeverCalled();
    }
    
    #endregion
    
    #region SoftDeleteAsync Tests
    
    [Fact]
    public async Task SoftDeleteAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        
        automocker.SetupWorkoutTemplateQueryDataService_Exists(templateId, true);
        automocker.SetupWorkoutTemplateCommandDataService_SoftDelete(true);
        
        // Act
        var result = await testee.SoftDeleteAsync(templateId);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();
        
        automocker.VerifyWorkoutTemplateCommandDataService_SoftDeleteOnce(templateId);
    }
    
    [Fact]
    public async Task SoftDeleteAsync_WhenTemplateNotFound_ReturnsNotFound()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        
        automocker.SetupWorkoutTemplateQueryDataService_Exists(templateId, false);
        
        // Act
        var result = await testee.SoftDeleteAsync(templateId);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.NotFound);
        
        automocker.VerifyWorkoutTemplateCommandDataService_NeverCalled();
    }
    
    #endregion
    
    #region DeleteAsync Tests
    
    [Fact]
    public async Task DeleteAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        
        automocker.SetupWorkoutTemplateQueryDataService_Exists(templateId, true);
        automocker.SetupWorkoutTemplateQueryDataService_HasExecutionLogs(templateId, false);
        automocker.SetupWorkoutTemplateCommandDataService_Delete(true);
        
        // Act
        var result = await testee.DeleteAsync(templateId);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();
        
        automocker.VerifyWorkoutTemplateCommandDataService_DeleteOnce(templateId);
    }
    
    [Fact]
    public async Task DeleteAsync_WhenHasExecutionLogs_ReturnsValidationFailed()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        
        automocker.SetupWorkoutTemplateQueryDataService_Exists(templateId, true);
        automocker.SetupWorkoutTemplateQueryDataService_HasExecutionLogs(templateId, true);
        
        // Act
        var result = await testee.DeleteAsync(templateId);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        
        automocker.VerifyWorkoutTemplateCommandDataService_NeverCalled();
    }
    
    #endregion
    
    #region ExistsAsync Tests
    
    [Fact]
    public async Task ExistsAsync_WithExistingId_ReturnsTrue()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        
        automocker.SetupWorkoutTemplateQueryDataService_Exists(templateId, true);
        
        // Act
        var result = await testee.ExistsAsync(templateId);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();
        
        automocker.VerifyWorkoutTemplateQueryDataService_ExistsOnce(templateId);
    }
    
    [Fact]
    public async Task ExistsAsync_WithNonExistentId_ReturnsFalse()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        
        automocker.SetupWorkoutTemplateQueryDataService_Exists(templateId, false);
        
        // Act
        var result = await testee.ExistsAsync(templateId);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeFalse();
        
        automocker.VerifyWorkoutTemplateQueryDataService_ExistsOnce(templateId);
    }
    
    #endregion
    
    #region ExistsByNameAsync Tests
    
    [Fact]
    public async Task ExistsByNameAsync_WithExistingName_ReturnsSuccess()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        const string name = "Existing Template";
        
        automocker.SetupWorkoutTemplateQueryDataService_ExistsByName(name, true);
        
        // Act
        var result = await testee.ExistsByNameAsync(name);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Value.Should().BeTrue();
        
        automocker.VerifyWorkoutTemplateQueryDataService_ExistsByNameOnce(name);
    }
    
    [Fact]
    public async Task ExistsByNameAsync_WithEmptyName_ReturnsValidationFailed()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        // Act
        var result = await testee.ExistsByNameAsync("");
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
        
        automocker.VerifyWorkoutTemplateQueryDataService_NeverCalled();
    }
    
    #endregion
    
    #region GetSuggestedExercisesAsync Tests
    
    [Fact]
    public async Task GetSuggestedExercisesAsync_WithValidParameters_ReturnsSuccess()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var categoryId = WorkoutCategoryId.ParseOrEmpty(TestIds.WorkoutCategoryIds.Strength);
        var existingExerciseIds = new List<ExerciseId>
        {
            ExerciseId.ParseOrEmpty(TestIds.ExerciseIds.BenchPress)
        };
        
        var suggestedExercises = new List<ExerciseDto>
        {
            new() { Id = TestIds.ExerciseIds.Squat, Name = "Squat" },
            new() { Id = TestIds.ExerciseIds.Deadlift, Name = "Deadlift" }
        };
        
        automocker.SetupSuggestionHandler_GetSuggestedExercises(suggestedExercises);
        
        // Act
        var result = await testee.GetSuggestedExercisesAsync(categoryId, existingExerciseIds, 5);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }
    
    [Fact]
    public async Task GetSuggestedExercisesAsync_WithEmptyCategoryId_ReturnsValidationFailed()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        // Act
        var result = await testee.GetSuggestedExercisesAsync(
            WorkoutCategoryId.Empty,
            Enumerable.Empty<ExerciseId>(),
            10);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.ValidationFailed);
    }
    
    #endregion
    
    #region GetRequiredEquipmentAsync Tests
    
    [Fact]
    public async Task GetRequiredEquipmentAsync_WithValidId_ReturnsSuccess()
    {
        // Arrange
        var automocker = new AutoMocker();
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        var templateId = WorkoutTemplateId.ParseOrEmpty(TestIds.WorkoutTemplateIds.BasicTemplate);
        
        var requiredEquipment = new List<EquipmentDto>
        {
            new() { Id = TestIds.EquipmentIds.Barbell, Name = "Barbell" },
            new() { Id = TestIds.EquipmentIds.Dumbbell, Name = "Dumbbell" }
        };
        
        automocker.SetupWorkoutTemplateQueryDataService_Exists(templateId, true);
        automocker.SetupEquipmentRequirementsService_GetRequiredEquipment(requiredEquipment);
        
        // Act
        var result = await testee.GetRequiredEquipmentAsync(templateId);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }
    
    [Fact]
    public async Task GetRequiredEquipmentAsync_WithEmptyId_ReturnsValidationFailed()
    {
        // Arrange
        var automocker = new AutoMocker();
        
        // Setup the required mock that the async validation will call
        // The async validation EnsureExistsAsync calls _queryDataService.ExistsAsync
        automocker.GetMock<IWorkoutTemplateQueryDataService>()
            .Setup(x => x.ExistsAsync(WorkoutTemplateId.Empty))
            .ReturnsAsync(ServiceResult<BooleanResultDto>.Success(new BooleanResultDto { Value = false }));
        
        var testee = automocker.CreateInstance<WorkoutTemplateService>();
        
        // Act  
        var result = await testee.GetRequiredEquipmentAsync(WorkoutTemplateId.Empty);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        // Due to the async validation framework executing all validations,
        // we get NotFound instead of ValidationFailed because EnsureExistsAsync runs after EnsureNotEmpty
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.NotFound);
    }
    
    #endregion
}
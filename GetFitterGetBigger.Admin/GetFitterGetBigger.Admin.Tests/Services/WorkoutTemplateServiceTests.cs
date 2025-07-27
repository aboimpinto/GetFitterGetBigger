using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.Results;
using GetFitterGetBigger.Admin.Models.Errors;
using GetFitterGetBigger.Admin.Models.ReferenceData;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Services.DataProviders;
using GetFitterGetBigger.Admin.Builders;
using Microsoft.Extensions.Logging;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.Services
{
    public class WorkoutTemplateServiceTests
    {
        private readonly Mock<IWorkoutTemplateDataProvider> _dataProviderMock;
        private readonly Mock<ILogger<WorkoutTemplateService>> _loggerMock;
        private readonly WorkoutTemplateService _workoutTemplateService;

        public WorkoutTemplateServiceTests()
        {
            _dataProviderMock = new Mock<IWorkoutTemplateDataProvider>();
            _loggerMock = new Mock<ILogger<WorkoutTemplateService>>();

            _workoutTemplateService = new WorkoutTemplateService(
                _dataProviderMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetWorkoutTemplatesAsync_WithValidFilter_ReturnsPagedResult()
        {
            // Arrange
            var filter = new WorkoutTemplateFilterDtoBuilder()
                .WithNamePattern("strength")
                .WithPageSize(10)
                .Build();

            var expectedData = new WorkoutTemplatePagedResultDtoBuilder()
                .WithItems(
                    new WorkoutTemplateDtoBuilder()
                        .WithName("Upper Body Strength")
                        .WithDifficulty("difficultylevel-intermediate", "Intermediate")
                        .Build(),
                    new WorkoutTemplateDtoBuilder()
                        .WithName("Lower Body Strength")
                        .WithDifficulty("difficultylevel-advanced", "Advanced")
                        .Build()
                )
                .WithTotalCount(2)
                .Build();

            _dataProviderMock.Setup(x => x.GetWorkoutTemplatesAsync(It.IsAny<WorkoutTemplateFilterDto>()))
                .ReturnsAsync(DataServiceResult<WorkoutTemplatePagedResultDto>.Success(expectedData));

            // Act
            var result = await _workoutTemplateService.GetWorkoutTemplatesAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Items.Should().HaveCount(2);
            result.Data.TotalCount.Should().Be(2);
            result.Data.Items[0].Name.Should().Be("Upper Body Strength");
            
            _dataProviderMock.Verify(x => x.GetWorkoutTemplatesAsync(It.Is<WorkoutTemplateFilterDto>(f => 
                f.NamePattern == "strength" && f.PageSize == 10)), Times.Once);
        }

        [Fact]
        public async Task GetWorkoutTemplatesAsync_WithLargePageSize_CapsAt100()
        {
            // Arrange
            var filter = new WorkoutTemplateFilterDtoBuilder()
                .WithPageSize(150)
                .Build();

            var expectedData = new WorkoutTemplatePagedResultDtoBuilder().Build();
            
            _dataProviderMock.Setup(x => x.GetWorkoutTemplatesAsync(It.IsAny<WorkoutTemplateFilterDto>()))
                .ReturnsAsync(DataServiceResult<WorkoutTemplatePagedResultDto>.Success(expectedData));

            // Act
            var result = await _workoutTemplateService.GetWorkoutTemplatesAsync(filter);
            
            // Assert success
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();

            // Assert
            _dataProviderMock.Verify(x => x.GetWorkoutTemplatesAsync(It.Is<WorkoutTemplateFilterDto>(f => 
                f.PageSize == 100)), Times.Once);
            
            _loggerMock.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("exceeds limits")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetWorkoutTemplatesAsync_WhenDataProviderFails_ReturnsFailureResult()
        {
            // Arrange
            var filter = new WorkoutTemplateFilterDtoBuilder().Build();
            _dataProviderMock.Setup(x => x.GetWorkoutTemplatesAsync(It.IsAny<WorkoutTemplateFilterDto>()))
                .ReturnsAsync(DataServiceResult<WorkoutTemplatePagedResultDto>.NetworkError("Connection failed"));

            // Act
            var result = await _workoutTemplateService.GetWorkoutTemplatesAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
            result.Errors.First().Message.Should().Contain("Connection failed");
        }

        [Fact]
        public async Task GetWorkoutTemplateByIdAsync_WhenNotInCache_FetchesFromDataProvider()
        {
            // Arrange
            var templateId = "template-123";
            var expectedTemplate = new WorkoutTemplateDtoBuilder()
                .WithId(templateId)
                .WithName("Test Template")
                .Build();
            
            _dataProviderMock.Setup(x => x.GetWorkoutTemplateByIdAsync(templateId))
                .ReturnsAsync(DataServiceResult<WorkoutTemplateDto>.Success(expectedTemplate));

            // Act
            var result = await _workoutTemplateService.GetWorkoutTemplateByIdAsync(templateId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Id.Should().Be(templateId);
            result.Data.Name.Should().Be("Test Template");
            result.Data.IsEmpty.Should().BeFalse();
        }

        // Test removed: GetWorkoutTemplateByIdAsync_WhenInCache_ReturnsCachedValue
        // Caching is now handled in the data layer, not the business layer

        [Fact]
        public async Task GetWorkoutTemplateByIdAsync_WithEmptyId_ReturnsValidationError()
        {
            // Arrange
            var templateId = "";

            // Act
            var result = await _workoutTemplateService.GetWorkoutTemplateByIdAsync(templateId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.First().Code.Should().Be(ServiceErrorCode.ValidationRequired);
            result.Errors.First().Message.Should().Contain("Value is required");
            
            _dataProviderMock.Verify(x => x.GetWorkoutTemplateByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetWorkoutTemplateByIdAsync_WhenNotFound_ReturnsEmptyTemplate()
        {
            // Arrange
            var templateId = "nonexistent-id";
            _dataProviderMock.Setup(x => x.GetWorkoutTemplateByIdAsync(templateId))
                .ReturnsAsync(DataServiceResult<WorkoutTemplateDto>.NotFound("Template"));

            // Act
            var result = await _workoutTemplateService.GetWorkoutTemplateByIdAsync(templateId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.IsEmpty.Should().BeTrue();
            result.Data.Id.Should().Be(string.Empty);
            result.Data.Name.Should().Be(string.Empty);
            
            // No longer logging at the service level since HttpDataProviderBase already logs HTTP operations
            _dataProviderMock.Verify(x => x.GetWorkoutTemplateByIdAsync(templateId), Times.Once);
        }

        [Fact]
        public async Task CreateWorkoutTemplateAsync_WithValidTemplate_ReturnsSuccessResult()
        {
            // Arrange
            var createDto = new CreateWorkoutTemplateDtoBuilder()
                .WithName("New Template")
                .Build();
            
            var createdTemplate = new WorkoutTemplateDtoBuilder()
                .WithId("new-id")
                .WithName("New Template")
                .Build();
            
            _dataProviderMock.Setup(x => x.CreateWorkoutTemplateAsync(It.IsAny<CreateWorkoutTemplateDto>()))
                .ReturnsAsync(DataServiceResult<WorkoutTemplateDto>.Success(createdTemplate));

            // Act
            var result = await _workoutTemplateService.CreateWorkoutTemplateAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be("new-id");
            result.Data.Name.Should().Be("New Template");
        }

        [Fact]
        public async Task CreateWorkoutTemplateAsync_WithEmptyName_ReturnsValidationError()
        {
            // Arrange
            var createDto = new CreateWorkoutTemplateDtoBuilder()
                .WithName("")
                .Build();

            // Act
            var result = await _workoutTemplateService.CreateWorkoutTemplateAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.First().Code.Should().Be(ServiceErrorCode.ValidationRequired);
            result.Errors.First().Message.Should().Contain("required");
        }

        [Fact]
        public async Task CreateWorkoutTemplateAsync_WithLongName_ReturnsValidationError()
        {
            // Arrange
            var createDto = new CreateWorkoutTemplateDtoBuilder()
                .WithName(new string('a', 101))
                .Build();

            // Act
            var result = await _workoutTemplateService.CreateWorkoutTemplateAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.First().Code.Should().Be(ServiceErrorCode.ValidationOutOfRange);
            result.Errors.First().Message.Should().Contain("Template name");
            result.Errors.First().Message.Should().Contain("100 characters");
        }

        [Fact]
        public async Task CreateWorkoutTemplateAsync_WhenConflict_ReturnsDuplicateNameError()
        {
            // Arrange
            var createDto = new CreateWorkoutTemplateDtoBuilder()
                .WithName("Existing Template")
                .Build();
            
            _dataProviderMock.Setup(x => x.CreateWorkoutTemplateAsync(It.IsAny<CreateWorkoutTemplateDto>()))
                .ReturnsAsync(DataServiceResult<WorkoutTemplateDto>.Failure(new DataError(DataErrorCode.Conflict, "Template already exists")));

            // Act
            var result = await _workoutTemplateService.CreateWorkoutTemplateAsync(createDto);
            
            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors.First().Code.Should().Be(ServiceErrorCode.DuplicateName);
            result.Errors.First().Message.Should().Contain("Existing Template");
            result.Errors.First().Message.Should().Contain("already exists");
        }

        [Fact]
        public async Task UpdateWorkoutTemplateAsync_WithValidTemplate_ReturnsSuccessResult()
        {
            // Arrange
            var templateId = "template-123";
            var updateDto = new UpdateWorkoutTemplateDtoBuilder()
                .WithName("Updated Template")
                .Build();
            
            var updatedTemplate = new WorkoutTemplateDtoBuilder()
                .WithId(templateId)
                .WithName("Updated Template")
                .Build();
            
            _dataProviderMock.Setup(x => x.UpdateWorkoutTemplateAsync(templateId, It.IsAny<UpdateWorkoutTemplateDto>()))
                .ReturnsAsync(DataServiceResult<WorkoutTemplateDto>.Success(updatedTemplate));

            // Act
            var result = await _workoutTemplateService.UpdateWorkoutTemplateAsync(templateId, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Name.Should().Be("Updated Template");
        }

        [Fact]
        public async Task UpdateWorkoutTemplateAsync_WithEmptyName_ReturnsValidationError()
        {
            // Arrange
            var templateId = "template-123";
            var updateDto = new UpdateWorkoutTemplateDtoBuilder()
                .WithName("")
                .Build();

            // Act
            var result = await _workoutTemplateService.UpdateWorkoutTemplateAsync(templateId, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
            result.Errors.First().Code.Should().Be(ServiceErrorCode.ValidationRequired);
        }

        [Fact]
        public async Task UpdateWorkoutTemplateAsync_WhenNotFound_ReturnsTemplateNotFoundError()
        {
            // Arrange
            var templateId = "non-existent";
            var updateDto = new UpdateWorkoutTemplateDtoBuilder()
                .WithName("Updated Template")
                .Build();
            
            _dataProviderMock.Setup(x => x.UpdateWorkoutTemplateAsync(templateId, It.IsAny<UpdateWorkoutTemplateDto>()))
                .ReturnsAsync(DataServiceResult<WorkoutTemplateDto>.Failure(
                    new DataError(DataErrorCode.NotFound, "Template not found")));

            // Act
            var result = await _workoutTemplateService.UpdateWorkoutTemplateAsync(templateId, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
            result.Errors.First().Code.Should().Be(ServiceErrorCode.TemplateNotFound);
        }

        [Fact]
        public async Task DeleteWorkoutTemplateAsync_WhenSuccessful_ReturnsSuccessResult()
        {
            // Arrange
            var templateId = "template-123";
            _dataProviderMock.Setup(x => x.DeleteWorkoutTemplateAsync(templateId))
                .ReturnsAsync(DataServiceResult<bool>.Success(true));

            // Act
            var result = await _workoutTemplateService.DeleteWorkoutTemplateAsync(templateId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();
            
            _loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Deleted workout template")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteWorkoutTemplateAsync_WithEmptyId_ReturnsValidationError()
        {
            // Arrange
            var templateId = "";

            // Act
            var result = await _workoutTemplateService.DeleteWorkoutTemplateAsync(templateId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
            result.Errors.First().Code.Should().Be(ServiceErrorCode.ValidationRequired);
        }

        [Fact]
        public async Task DeleteWorkoutTemplateAsync_WhenNotFound_ReturnsTemplateNotFoundError()
        {
            // Arrange
            var templateId = "non-existent";
            _dataProviderMock.Setup(x => x.DeleteWorkoutTemplateAsync(templateId))
                .ReturnsAsync(DataServiceResult<bool>.Failure(
                    new DataError(DataErrorCode.NotFound, "Template not found")));

            // Act
            var result = await _workoutTemplateService.DeleteWorkoutTemplateAsync(templateId);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
            result.Errors.First().Code.Should().Be(ServiceErrorCode.TemplateNotFound);
        }

        [Fact]
        public async Task ChangeWorkoutTemplateStateAsync_WithValidData_ReturnsSuccessResult()
        {
            // Arrange
            var templateId = "template-123";
            var changeStateDto = new ChangeWorkoutStateDto { WorkoutStateId = "state-active" };
            
            var updatedTemplate = new WorkoutTemplateDtoBuilder()
                .WithId(templateId)
                .WithWorkoutState("state-active", "Active")
                .Build();
            
            _dataProviderMock.Setup(x => x.ChangeWorkoutTemplateStateAsync(templateId, It.IsAny<ChangeWorkoutStateDto>()))
                .ReturnsAsync(DataServiceResult<WorkoutTemplateDto>.Success(updatedTemplate));

            // Act
            var result = await _workoutTemplateService.ChangeWorkoutTemplateStateAsync(templateId, changeStateDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.WorkoutState.Id.Should().Be("state-active");
        }

        [Fact]
        public async Task ChangeWorkoutTemplateStateAsync_WithEmptyStateId_ReturnsValidationError()
        {
            // Arrange
            var templateId = "template-123";
            var changeStateDto = new ChangeWorkoutStateDto { WorkoutStateId = "" };

            // Act
            var result = await _workoutTemplateService.ChangeWorkoutTemplateStateAsync(templateId, changeStateDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
            result.Errors.First().Code.Should().Be(ServiceErrorCode.ValidationRequired);
        }

        [Fact]
        public async Task ChangeWorkoutTemplateStateAsync_WhenNotFound_ReturnsTemplateNotFoundError()
        {
            // Arrange
            var templateId = "non-existent";
            var changeStateDto = new ChangeWorkoutStateDto { WorkoutStateId = "state-active" };
            
            _dataProviderMock.Setup(x => x.ChangeWorkoutTemplateStateAsync(templateId, It.IsAny<ChangeWorkoutStateDto>()))
                .ReturnsAsync(DataServiceResult<WorkoutTemplateDto>.Failure(
                    new DataError(DataErrorCode.NotFound, "Template not found")));

            // Act
            var result = await _workoutTemplateService.ChangeWorkoutTemplateStateAsync(templateId, changeStateDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
            result.Errors.First().Code.Should().Be(ServiceErrorCode.TemplateNotFound);
        }

        [Fact]
        public async Task DuplicateWorkoutTemplateAsync_WithValidData_ReturnsSuccessResult()
        {
            // Arrange
            var templateId = "template-123";
            var duplicateDto = new DuplicateWorkoutTemplateDto { NewName = "Duplicated Template" };
            
            var duplicatedTemplate = new WorkoutTemplateDtoBuilder()
                .WithId("template-456")
                .WithName("Duplicated Template")
                .Build();
            
            _dataProviderMock.Setup(x => x.DuplicateWorkoutTemplateAsync(templateId, It.IsAny<DuplicateWorkoutTemplateDto>()))
                .ReturnsAsync(DataServiceResult<WorkoutTemplateDto>.Success(duplicatedTemplate));

            // Act
            var result = await _workoutTemplateService.DuplicateWorkoutTemplateAsync(templateId, duplicateDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Name.Should().Be("Duplicated Template");
        }

        [Fact]
        public async Task DuplicateWorkoutTemplateAsync_WithEmptyName_ReturnsValidationError()
        {
            // Arrange
            var templateId = "template-123";
            var duplicateDto = new DuplicateWorkoutTemplateDto { NewName = "" };

            // Act
            var result = await _workoutTemplateService.DuplicateWorkoutTemplateAsync(templateId, duplicateDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
            result.Errors.First().Code.Should().Be(ServiceErrorCode.ValidationRequired);
        }

        [Fact]
        public async Task DuplicateWorkoutTemplateAsync_WithLongName_ReturnsValidationError()
        {
            // Arrange
            var templateId = "template-123";
            var duplicateDto = new DuplicateWorkoutTemplateDto { NewName = new string('A', 101) };

            // Act
            var result = await _workoutTemplateService.DuplicateWorkoutTemplateAsync(templateId, duplicateDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
            result.Errors.First().Code.Should().Be(ServiceErrorCode.ValidationOutOfRange);
        }

        [Fact]
        public async Task DuplicateWorkoutTemplateAsync_WhenOriginalNotFound_ReturnsTemplateNotFoundError()
        {
            // Arrange
            var templateId = "non-existent";
            var duplicateDto = new DuplicateWorkoutTemplateDto { NewName = "Duplicated Template" };
            
            _dataProviderMock.Setup(x => x.DuplicateWorkoutTemplateAsync(templateId, It.IsAny<DuplicateWorkoutTemplateDto>()))
                .ReturnsAsync(DataServiceResult<WorkoutTemplateDto>.Failure(
                    new DataError(DataErrorCode.NotFound, "Original template not found")));

            // Act
            var result = await _workoutTemplateService.DuplicateWorkoutTemplateAsync(templateId, duplicateDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
            result.Errors.First().Code.Should().Be(ServiceErrorCode.TemplateNotFound);
        }

        [Fact]
        public async Task DuplicateWorkoutTemplateAsync_WhenNameConflict_ReturnsDuplicateNameError()
        {
            // Arrange
            var templateId = "template-123";
            var duplicateDto = new DuplicateWorkoutTemplateDto { NewName = "Existing Template" };
            
            _dataProviderMock.Setup(x => x.DuplicateWorkoutTemplateAsync(templateId, It.IsAny<DuplicateWorkoutTemplateDto>()))
                .ReturnsAsync(DataServiceResult<WorkoutTemplateDto>.Failure(
                    new DataError(DataErrorCode.Conflict, "Name already exists")));

            // Act
            var result = await _workoutTemplateService.DuplicateWorkoutTemplateAsync(templateId, duplicateDto);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
            result.Errors.First().Code.Should().Be(ServiceErrorCode.DuplicateName);
        }

    }
}
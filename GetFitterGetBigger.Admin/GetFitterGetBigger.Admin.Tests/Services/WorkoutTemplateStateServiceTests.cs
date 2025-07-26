using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.ReferenceData;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Builders;
using GetFitterGetBigger.Admin.Tests.Builders;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.Services
{
    public class WorkoutTemplateStateServiceTests
    {
        private readonly Mock<IWorkoutTemplateService> _workoutTemplateServiceMock;
        private readonly WorkoutTemplateStateService _stateService;

        public WorkoutTemplateStateServiceTests()
        {
            _workoutTemplateServiceMock = new Mock<IWorkoutTemplateService>();
            _stateService = new WorkoutTemplateStateService(_workoutTemplateServiceMock.Object);
        }

        [Fact]
        public async Task InitializeAsync_LoadsReferenceDataAndTemplates()
        {
            // Arrange
            var categories = ReferenceDataDtoBuilder.BuildList(3).ToList();
            var difficultyLevels = ReferenceDataDtoBuilder.BuildList(3).ToList();
            var states = new List<ReferenceDataDto>
            {
                new ReferenceDataDto { Id = "state-1", Value = "DRAFT", Description = "Draft state" },
                new ReferenceDataDto { Id = "state-2", Value = "PRODUCTION", Description = "Production state" },
                new ReferenceDataDto { Id = "state-3", Value = "ARCHIVED", Description = "Archived state" }
            };
            var objectives = ReferenceDataDtoBuilder.BuildList(4).ToList();
            var templates = new WorkoutTemplatePagedResultDtoBuilder()
                .WithItems(new WorkoutTemplateDtoBuilder().Build())
                .Build();

            _workoutTemplateServiceMock.Setup(x => x.GetWorkoutCategoriesAsync()).Returns(Task.FromResult(categories));
            _workoutTemplateServiceMock.Setup(x => x.GetDifficultyLevelsAsync()).Returns(Task.FromResult(difficultyLevels));
            _workoutTemplateServiceMock.Setup(x => x.GetWorkoutStatesAsync()).Returns(Task.FromResult(states));
            _workoutTemplateServiceMock.Setup(x => x.GetWorkoutObjectivesAsync()).Returns(Task.FromResult(objectives));
            _workoutTemplateServiceMock.Setup(x => x.GetWorkoutTemplatesAsync(It.IsAny<WorkoutTemplateFilterDto>()))
                .ReturnsAsync(templates);

            // Act
            await _stateService.InitializeAsync();

            // Assert
            _stateService.WorkoutCategories.Should().HaveCount(3);
            _stateService.DifficultyLevels.Should().HaveCount(3);
            _stateService.WorkoutStates.Should().HaveCount(3);
            _stateService.WorkoutObjectives.Should().HaveCount(4);
            _stateService.CurrentPage.Should().NotBeNull();
            _stateService.IsLoadingReferenceData.Should().BeFalse();
            _stateService.IsLoading.Should().BeFalse();
        }

        [Fact]
        public async Task LoadWorkoutTemplatesAsync_WithFilter_UpdatesCurrentPageAndFilter()
        {
            // Arrange
            var filter = new WorkoutTemplateFilterDtoBuilder()
                .WithNamePattern("strength")
                .WithPageSize(20)
                .Build();

            var expectedTemplates = new WorkoutTemplatePagedResultDtoBuilder()
                .WithItems(
                    new WorkoutTemplateDtoBuilder().WithName("Upper Body Strength").Build(),
                    new WorkoutTemplateDtoBuilder().WithName("Core Strength").Build()
                )
                .Build();

            _workoutTemplateServiceMock.Setup(x => x.GetWorkoutTemplatesAsync(It.IsAny<WorkoutTemplateFilterDto>()))
                .ReturnsAsync(expectedTemplates);

            // Act
            await _stateService.LoadWorkoutTemplatesAsync(filter);

            // Assert
            _stateService.CurrentPage.Should().NotBeNull();
            _stateService.CurrentPage!.Items.Should().HaveCount(2);
            _stateService.CurrentFilter.NamePattern.Should().Be("strength");
            _stateService.CurrentFilter.PageSize.Should().Be(20);
            _stateService.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public async Task LoadWorkoutTemplateByIdAsync_WithValidId_LoadsTemplate()
        {
            // Arrange
            var templateId = "workouttemplate-123";
            var expectedTemplate = new WorkoutTemplateDtoBuilder()
                .WithId(templateId)
                .WithName("Test Template")
                .Build();

            _workoutTemplateServiceMock.Setup(x => x.GetWorkoutTemplateByIdAsync(templateId))
                .ReturnsAsync(expectedTemplate);

            // Act
            await _stateService.LoadWorkoutTemplateByIdAsync(templateId);

            // Assert
            _stateService.SelectedTemplate.Should().NotBeNull();
            _stateService.SelectedTemplate!.Id.Should().Be(templateId);
            _stateService.SelectedTemplate!.Name.Should().Be("Test Template");
            _stateService.ErrorMessage.Should().BeNull();
            _stateService.IsLoadingTemplate.Should().BeFalse();
        }

        [Fact]
        public async Task CreateWorkoutTemplateAsync_Success_RefreshesCurrentPage()
        {
            // Arrange
            var createDto = new CreateWorkoutTemplateDtoBuilder()
                .WithName("New Template")
                .Build();

            var createdTemplate = new WorkoutTemplateDtoBuilder()
                .WithName("New Template")
                .Build();

            var refreshedPage = new WorkoutTemplatePagedResultDtoBuilder()
                .WithItems(createdTemplate)
                .Build();

            _workoutTemplateServiceMock.Setup(x => x.CreateWorkoutTemplateAsync(createDto))
                .ReturnsAsync(createdTemplate);
            _workoutTemplateServiceMock.Setup(x => x.GetWorkoutTemplatesAsync(It.IsAny<WorkoutTemplateFilterDto>()))
                .ReturnsAsync(refreshedPage);

            // Act
            await _stateService.CreateWorkoutTemplateAsync(createDto);

            // Assert
            _workoutTemplateServiceMock.Verify(x => x.CreateWorkoutTemplateAsync(createDto), Times.Once);
            _workoutTemplateServiceMock.Verify(x => x.GetWorkoutTemplatesAsync(It.IsAny<WorkoutTemplateFilterDto>()), Times.Once);
            _stateService.CurrentPage!.Items.Should().Contain(t => t.Name == "New Template");
            _stateService.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public async Task UpdateWorkoutTemplateAsync_UpdatesSelectedTemplate()
        {
            // Arrange
            var templateId = "workouttemplate-123";
            var selectedTemplate = new WorkoutTemplateDtoBuilder()
                .WithId(templateId)
                .WithName("Original Name")
                .Build();
            
            // Set the selected template
            _workoutTemplateServiceMock.Setup(x => x.GetWorkoutTemplateByIdAsync(templateId))
                .ReturnsAsync(selectedTemplate);
            await _stateService.LoadWorkoutTemplateByIdAsync(templateId);
            
            // Verify that the template was loaded
            _stateService.SelectedTemplate.Should().NotBeNull();
            _stateService.SelectedTemplate!.Id.Should().Be(templateId);

            var updateDto = new UpdateWorkoutTemplateDto
            {
                Name = "Updated Name",
                Description = "Updated description",
                CategoryId = "cat-1",
                DifficultyId = "diff-1",
                EstimatedDurationMinutes = 45,
                IsPublic = true
            };

            var updatedTemplate = new WorkoutTemplateDtoBuilder()
                .WithId(templateId)
                .WithName("Updated Name")
                .Build();

            _workoutTemplateServiceMock.Setup(x => x.UpdateWorkoutTemplateAsync(templateId, updateDto))
                .ReturnsAsync(updatedTemplate);
            _workoutTemplateServiceMock.Setup(x => x.GetWorkoutTemplatesAsync(It.IsAny<WorkoutTemplateFilterDto>()))
                .ReturnsAsync(new WorkoutTemplatePagedResultDtoBuilder().Build());

            // Act
            await _stateService.UpdateWorkoutTemplateAsync(templateId, updateDto);

            // Assert
            _stateService.SelectedTemplate.Should().NotBeNull();
            _stateService.SelectedTemplate!.Name.Should().Be("Updated Name");
            _stateService.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public async Task DeleteWorkoutTemplateAsync_ClearsSelectedTemplateIfDeleted()
        {
            // Arrange
            var templateId = "workouttemplate-123";
            var selectedTemplate = new WorkoutTemplateDtoBuilder()
                .WithId(templateId)
                .Build();

            _workoutTemplateServiceMock.Setup(x => x.GetWorkoutTemplateByIdAsync(templateId))
                .ReturnsAsync(selectedTemplate);
            
            // Load the template first
            await _stateService.LoadWorkoutTemplateByIdAsync(templateId);

            _workoutTemplateServiceMock.Setup(x => x.DeleteWorkoutTemplateAsync(templateId))
                .Returns(Task.CompletedTask);
            _workoutTemplateServiceMock.Setup(x => x.GetWorkoutTemplatesAsync(It.IsAny<WorkoutTemplateFilterDto>()))
                .ReturnsAsync(new WorkoutTemplatePagedResultDtoBuilder().Build());

            // Act
            await _stateService.DeleteWorkoutTemplateAsync(templateId);

            // Assert
            _stateService.SelectedTemplate.Should().BeNull();
            _workoutTemplateServiceMock.Verify(x => x.DeleteWorkoutTemplateAsync(templateId), Times.Once);
        }

        [Fact]
        public async Task ChangeWorkoutTemplateStateAsync_UpdatesTemplateState()
        {
            // Arrange
            var templateId = "workouttemplate-123";
            var changeStateDto = new ChangeWorkoutStateDto { WorkoutStateId = "state-2" };

            var updatedTemplate = new WorkoutTemplateDtoBuilder()
                .WithId(templateId)
                .WithWorkoutState("state-2", "PRODUCTION")
                .Build();

            _workoutTemplateServiceMock.Setup(x => x.ChangeWorkoutTemplateStateAsync(templateId, changeStateDto))
                .ReturnsAsync(updatedTemplate);
            _workoutTemplateServiceMock.Setup(x => x.GetWorkoutTemplatesAsync(It.IsAny<WorkoutTemplateFilterDto>()))
                .ReturnsAsync(new WorkoutTemplatePagedResultDtoBuilder().Build());

            // Act
            await _stateService.ChangeWorkoutTemplateStateAsync(templateId, changeStateDto);

            // Assert
            _workoutTemplateServiceMock.Verify(x => x.ChangeWorkoutTemplateStateAsync(templateId, changeStateDto), Times.Once);
        }

        [Fact]
        public async Task DuplicateWorkoutTemplateAsync_RefreshesCurrentPage()
        {
            // Arrange
            var templateId = "workouttemplate-123";
            var duplicateDto = new DuplicateWorkoutTemplateDto { NewName = "Copy of Template" };

            var duplicatedTemplate = new WorkoutTemplateDtoBuilder()
                .WithName("Copy of Template")
                .Build();

            _workoutTemplateServiceMock.Setup(x => x.DuplicateWorkoutTemplateAsync(templateId, duplicateDto))
                .ReturnsAsync(duplicatedTemplate);
            _workoutTemplateServiceMock.Setup(x => x.GetWorkoutTemplatesAsync(It.IsAny<WorkoutTemplateFilterDto>()))
                .ReturnsAsync(new WorkoutTemplatePagedResultDtoBuilder().Build());

            // Act
            await _stateService.DuplicateWorkoutTemplateAsync(templateId, duplicateDto);

            // Assert
            _workoutTemplateServiceMock.Verify(x => x.DuplicateWorkoutTemplateAsync(templateId, duplicateDto), Times.Once);
            _workoutTemplateServiceMock.Verify(x => x.GetWorkoutTemplatesAsync(It.IsAny<WorkoutTemplateFilterDto>()), Times.Once);
        }

        [Fact]
        public void StoreReturnPage_CreatesFilterCopy()
        {
            // Arrange
            var currentFilter = new WorkoutTemplateFilterDtoBuilder()
                .WithPage(3)
                .WithNamePattern("test")
                .Build();
            _stateService.CurrentFilter.Page = 3;
            _stateService.CurrentFilter.NamePattern = "test";

            // Act
            _stateService.StoreReturnPage();

            // Assert
            _stateService.HasStoredPage.Should().BeTrue();
        }

        [Fact]
        public async Task LoadWorkoutTemplatesWithStoredPageAsync_UsesStoredFilter()
        {
            // Arrange
            var storedFilter = new WorkoutTemplateFilterDtoBuilder()
                .WithPage(5)
                .WithNamePattern("stored")
                .Build();
            
            _stateService.CurrentFilter.Page = 5;
            _stateService.CurrentFilter.NamePattern = "stored";
            _stateService.StoreReturnPage();

            _workoutTemplateServiceMock.Setup(x => x.GetWorkoutTemplatesAsync(It.IsAny<WorkoutTemplateFilterDto>()))
                .ReturnsAsync(new WorkoutTemplatePagedResultDtoBuilder().Build());

            // Act
            await _stateService.LoadWorkoutTemplatesWithStoredPageAsync();

            // Assert
            _workoutTemplateServiceMock.Verify(x => x.GetWorkoutTemplatesAsync(
                It.Is<WorkoutTemplateFilterDto>(f => f.Page == 5 && f.NamePattern == "stored")), Times.Once);
            _stateService.HasStoredPage.Should().BeFalse();
        }

        [Fact]
        public void ClearError_ClearsErrorMessage()
        {
            // Arrange
            // Simulate an error
            _workoutTemplateServiceMock.Setup(x => x.GetWorkoutTemplatesAsync(It.IsAny<WorkoutTemplateFilterDto>()))
                .ThrowsAsync(new Exception("Test error"));
            
            // This will set an error
            _ = _stateService.LoadWorkoutTemplatesAsync();

            // Act
            _stateService.ClearError();

            // Assert
            _stateService.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public void OnChange_Event_FiredWhenStateChanges()
        {
            // Arrange
            var stateChangedCount = 0;
            _stateService.OnChange += () => stateChangedCount++;

            // Act
            _stateService.ClearSelectedTemplate();

            // Assert
            stateChangedCount.Should().Be(1);
        }
    }
}
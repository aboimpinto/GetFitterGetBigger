using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.ReferenceData;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Services.DataProviders;
using GetFitterGetBigger.Admin.Services.Stores;
using GetFitterGetBigger.Admin.Tests.TestHelpers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using WorkoutTemplatesPage = GetFitterGetBigger.Admin.Components.Pages.WorkoutTemplates.WorkoutTemplates;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.WorkoutTemplates
{
    public class WorkoutTemplatesPageIntegrationTests : WorkoutTemplateTestBase
    {
        private readonly Mock<IWorkoutTemplateListStore> _listStoreMock;
        private readonly Mock<IWorkoutReferenceDataStore> _referenceStoreMock;
        private readonly Mock<IGenericReferenceDataService> _genericReferenceDataServiceMock;

        public WorkoutTemplatesPageIntegrationTests()
        {
            _listStoreMock = new Mock<IWorkoutTemplateListStore>();
            _referenceStoreMock = new Mock<IWorkoutReferenceDataStore>();
            _genericReferenceDataServiceMock = new Mock<IGenericReferenceDataService>();

            // Register mocks
            Services.AddSingleton(_listStoreMock.Object);
            Services.AddSingleton(_referenceStoreMock.Object);
            Services.AddSingleton(_genericReferenceDataServiceMock.Object);

            // Setup default behavior
            _listStoreMock.Setup(x => x.CurrentFilter).Returns(new WorkoutTemplateFilterDto { PageSize = 10 });
            _listStoreMock.Setup(x => x.LoadTemplatesAsync(It.IsAny<WorkoutTemplateFilterDto?>())).Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task WorkoutStatesDropdown_ShouldBePopulated_WhenReferenceDataIsLoaded()
        {
            // Arrange
            var expectedStates = new List<ReferenceDataDto>
            {
                new() { Id = "state-1", Value = "DRAFT" },
                new() { Id = "state-2", Value = "PRODUCTION" },
                new() { Id = "state-3", Value = "ARCHIVED" }
            };

            // Setup the reference store
            _referenceStoreMock.Setup(x => x.IsLoaded).Returns(false); // Not loaded initially
            _referenceStoreMock.Setup(x => x.WorkoutStates).Returns(expectedStates);
            _referenceStoreMock.Setup(x => x.WorkoutCategories).Returns(new List<ReferenceDataDto>());
            _referenceStoreMock.Setup(x => x.DifficultyLevels).Returns(new List<ReferenceDataDto>());
            _referenceStoreMock.Setup(x => x.LoadReferenceDataAsync()).Returns(Task.CompletedTask);

            // Setup generic reference data service to return states
            _genericReferenceDataServiceMock.Setup(x => x.GetReferenceDataAsync<WorkoutStates>())
                .ReturnsAsync(expectedStates);

            // Act
            var cut = RenderComponent<WorkoutTemplatesPage>();
            await cut.InvokeAsync(() => Task.CompletedTask); // Let the component finish initialization

            // Assert
            var stateDropdown = cut.Find("[data-testid=\"state-select\"]");
            var options = stateDropdown.QuerySelectorAll("option");

            options.Should().HaveCount(4); // "All States" + 3 states
            options[0].TextContent.Should().Be("All States");
            options[1].TextContent.Should().Be("DRAFT");
            options[2].TextContent.Should().Be("PRODUCTION");
            options[3].TextContent.Should().Be("ARCHIVED");

            // Verify the reference data was loaded
            _referenceStoreMock.Verify(x => x.LoadReferenceDataAsync(), Times.Once);
        }

        [Fact]
        public async Task WorkoutStatesDropdown_ShouldCallGenericReferenceDataService_NotDataProvider()
        {
            // Arrange
            var expectedStates = new List<ReferenceDataDto>
            {
                new() { Id = "state-1", Value = "DRAFT" },
                new() { Id = "state-2", Value = "PRODUCTION" }
            };

            // Setup generic reference data service to return workout states
            _genericReferenceDataServiceMock.Setup(x => x.GetReferenceDataAsync<WorkoutStates>())
                .ReturnsAsync(expectedStates);

            // Create mocks for WorkoutTemplateService dependencies
            var dataProviderMock = new Mock<IWorkoutTemplateDataProvider>();
            var workoutRefServiceMock = new Mock<IWorkoutReferenceDataService>();
            var loggerMock = new Mock<ILogger<WorkoutTemplateService>>();
            
            // Setup other required methods
            workoutRefServiceMock.Setup(x => x.GetWorkoutCategoriesAsync())
                .ReturnsAsync(new List<WorkoutCategoryDto>());
            workoutRefServiceMock.Setup(x => x.GetWorkoutObjectivesAsync())
                .ReturnsAsync(new List<ReferenceDataDto>());
            
            // Create a real WorkoutTemplateService with mocked dependencies
            var workoutTemplateService = new WorkoutTemplateService(
                dataProviderMock.Object,
                loggerMock.Object);

            // Replace the service in DI
            Services.AddSingleton<IWorkoutTemplateService>(workoutTemplateService);

            // Create a real WorkoutReferenceDataStore
            var storeLoggerMock = new Mock<ILogger<WorkoutReferenceDataStore>>();
            var referenceStore = new WorkoutReferenceDataStore(
                _genericReferenceDataServiceMock.Object,
                workoutRefServiceMock.Object,
                storeLoggerMock.Object);

            Services.AddSingleton<IWorkoutReferenceDataStore>(referenceStore);

            // Act
            var cut = RenderComponent<WorkoutTemplatesPage>();
            await cut.InvokeAsync(() => Task.CompletedTask); // Let the component finish initialization

            // Assert
            // The generic reference data service should have been called
            _genericReferenceDataServiceMock.Verify(x => x.GetReferenceDataAsync<WorkoutStates>(), Times.Once);
        }

        [Fact]
        public void WorkoutStatesDropdown_ShouldBeEmpty_WhenReferenceDataFailsToLoad()
        {
            // Arrange
            _referenceStoreMock.Setup(x => x.IsLoaded).Returns(true); // Already "loaded" but with no data
            _referenceStoreMock.Setup(x => x.WorkoutStates).Returns(Enumerable.Empty<ReferenceDataDto>());
            _referenceStoreMock.Setup(x => x.WorkoutCategories).Returns(new List<ReferenceDataDto>());
            _referenceStoreMock.Setup(x => x.DifficultyLevels).Returns(new List<ReferenceDataDto>());

            // Act
            var cut = RenderComponent<WorkoutTemplatesPage>();

            // Assert
            var stateDropdown = cut.Find("[data-testid=\"state-select\"]");
            var options = stateDropdown.QuerySelectorAll("option");

            options.Should().HaveCount(1); // Only "All States"
            options[0].TextContent.Should().Be("All States");
        }
    }
}
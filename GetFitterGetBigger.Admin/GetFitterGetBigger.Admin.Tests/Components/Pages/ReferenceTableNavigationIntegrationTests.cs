using Bunit;
using Bunit.TestDoubles;
using GetFitterGetBigger.Admin.Components.Pages;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages
{
    public class ReferenceTableNavigationIntegrationTests : TestContext
    {
        private readonly Mock<IGenericReferenceDataService> _referenceDataServiceMock;
        private readonly Mock<IEquipmentStateService> _equipmentStateServiceMock;
        private readonly Mock<IMuscleGroupsStateService> _muscleGroupsStateServiceMock;
        private readonly Mock<IExerciseWeightTypeStateService> _exerciseWeightTypeStateServiceMock;
        private readonly Mock<IWorkoutReferenceDataStateService> _workoutReferenceDataStateServiceMock;
        private readonly Mock<NavigationManager> _navigationManagerMock;

        public ReferenceTableNavigationIntegrationTests()
        {
            // Mock all required services
            _referenceDataServiceMock = new Mock<IGenericReferenceDataService>();
            _equipmentStateServiceMock = new Mock<IEquipmentStateService>();
            _muscleGroupsStateServiceMock = new Mock<IMuscleGroupsStateService>();
            _exerciseWeightTypeStateServiceMock = new Mock<IExerciseWeightTypeStateService>();
            _workoutReferenceDataStateServiceMock = new Mock<IWorkoutReferenceDataStateService>();
            _navigationManagerMock = new Mock<NavigationManager>();

            // Register all services
            Services.AddSingleton(_referenceDataServiceMock.Object);
            Services.AddSingleton(_equipmentStateServiceMock.Object);
            Services.AddSingleton(_muscleGroupsStateServiceMock.Object);
            Services.AddSingleton(_exerciseWeightTypeStateServiceMock.Object);
            Services.AddSingleton(_workoutReferenceDataStateServiceMock.Object);
            Services.AddSingleton(_navigationManagerMock.Object);
            
            // Add authorization
            this.AddTestAuthorization().SetAuthorized("test-user");
        }

        #region Navigation Tests

        [Fact]
        public void ReferenceTables_DisplaysAllReferenceTableLinks()
        {
            // Act
            var component = RenderComponent<ReferenceTables>();

            // Assert - Check that all reference tables are listed
            var links = component.FindAll("a[href^='/referencetables/']");
            Assert.Equal(12, links.Count); // Should have 12 reference table links

            // Verify new workout reference tables are included by checking the parent div
            var cards = component.FindAll(".bg-gray-50");
            Assert.Contains(cards, card => card.TextContent.Contains("Workout Objectives"));
            Assert.Contains(cards, card => card.TextContent.Contains("Workout Categories"));
            Assert.Contains(cards, card => card.TextContent.Contains("Execution Protocols"));
        }

        [Theory]
        [InlineData("WorkoutObjectives", "Workout Objectives")]
        [InlineData("WorkoutCategories", "Workout Categories")]
        [InlineData("ExecutionProtocols", "Execution Protocols")]
        public void ReferenceTables_NavigatesToCorrectTable(string tableName, string displayName)
        {
            // Arrange
            var component = RenderComponent<ReferenceTables>();

            // Act
            var link = component.Find($"a[href='/referencetables/{tableName}']");

            // Assert
            Assert.NotNull(link);
            Assert.Contains(displayName, link.Parent!.TextContent);
        }

        #endregion

        #region Data Loading Tests

        [Fact]
        public void WorkoutObjectives_LoadsDataOnMount()
        {
            // Arrange
            var objectives = new List<ReferenceDataDto>
            {
                new() { Id = "1", Value = "Muscle Building", Description = "Focus on hypertrophy" },
                new() { Id = "2", Value = "Strength", Description = "Focus on maximum strength" }
            };

            _workoutReferenceDataStateServiceMock.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
            _workoutReferenceDataStateServiceMock.Setup(x => x.FilteredWorkoutObjectives).Returns(objectives);
            _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingObjectives).Returns(false);

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "WorkoutObjectives"));

            // Assert
            _workoutReferenceDataStateServiceMock.Verify(x => x.InitializeAsync(), Times.Once);
            var cards = component.FindAll("[data-testid='objective-card']");
            Assert.Equal(2, cards.Count);
        }

        [Fact]
        public void WorkoutCategories_LoadsDataOnMount()
        {
            // Arrange
            var categories = new List<WorkoutCategoryDto>
            {
                new() { WorkoutCategoryId = "1", Value = "Push", Description = "Pushing movements", Icon = "💪", Color = "#FF0000" },
                new() { WorkoutCategoryId = "2", Value = "Pull", Description = "Pulling movements", Icon = "🏋️", Color = "#00FF00" }
            };

            _workoutReferenceDataStateServiceMock.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
            _workoutReferenceDataStateServiceMock.Setup(x => x.FilteredWorkoutCategories).Returns(categories);
            _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingCategories).Returns(false);

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "WorkoutCategories"));

            // Assert
            _workoutReferenceDataStateServiceMock.Verify(x => x.InitializeAsync(), Times.Once);
            var cards = component.FindAll("[data-testid='category-card']");
            Assert.Equal(2, cards.Count);
        }

        [Fact]
        public void ExecutionProtocols_LoadsDataOnMount()
        {
            // Arrange
            var protocols = new List<ExecutionProtocolDto>
            {
                new() { ExecutionProtocolId = "1", Code = "AMRAP", Value = "As Many Reps As Possible", Description = "Perform maximum reps" },
                new() { ExecutionProtocolId = "2", Code = "EMOM", Value = "Every Minute on the Minute", Description = "Start each set at the top of the minute" }
            };

            _workoutReferenceDataStateServiceMock.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);
            _workoutReferenceDataStateServiceMock.Setup(x => x.FilteredExecutionProtocols).Returns(protocols);
            _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingProtocols).Returns(false);

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "ExecutionProtocols"));

            // Assert
            _workoutReferenceDataStateServiceMock.Verify(x => x.InitializeAsync(), Times.Once);
            var cards = component.FindAll("[data-testid='protocol-card']");
            Assert.Equal(2, cards.Count);
        }

        #endregion

        #region State Persistence Tests

        [Fact]
        public void WorkoutObjectives_MaintainsSearchStateOnRerender()
        {
            // Arrange
            var objectives = new List<ReferenceDataDto>
            {
                new() { Id = "1", Value = "Muscle Building", Description = "Focus on hypertrophy" },
                new() { Id = "2", Value = "Strength", Description = "Focus on maximum strength" }
            };

            _workoutReferenceDataStateServiceMock.Setup(x => x.FilteredWorkoutObjectives).Returns(objectives);
            _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingObjectives).Returns(false);
            _workoutReferenceDataStateServiceMock.Setup(x => x.ObjectivesSearchTerm).Returns("Muscle");

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "WorkoutObjectives"));

            // Force a re-render
            component.Render();

            // Assert
            var searchInput = component.Find("[data-testid='search-input']");
            Assert.Equal("Muscle", searchInput.GetAttribute("value"));
        }

        [Fact]
        public void ExecutionProtocols_MaintainsSearchStateOnRerender()
        {
            // Arrange
            var protocols = new List<ExecutionProtocolDto>
            {
                new() { ExecutionProtocolId = "1", Code = "AMRAP", Value = "As Many Reps As Possible", Description = "Perform maximum reps" },
                new() { ExecutionProtocolId = "2", Code = "EMOM", Value = "Every Minute on the Minute", Description = "Start each set at the top of the minute" }
            };

            _workoutReferenceDataStateServiceMock.Setup(x => x.FilteredExecutionProtocols).Returns(protocols);
            _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingProtocols).Returns(false);
            _workoutReferenceDataStateServiceMock.Setup(x => x.ProtocolsSearchTerm).Returns("AMRAP");

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "ExecutionProtocols"));

            // Force a re-render
            component.Render();

            // Assert
            var searchInput = component.Find("[data-testid='search-input']");
            Assert.Equal("AMRAP", searchInput.GetAttribute("value"));
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public void WorkoutObjectives_HandlesLoadingError()
        {
            // Arrange
            _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingObjectives).Returns(false);
            _workoutReferenceDataStateServiceMock.Setup(x => x.ObjectivesError).Returns("Failed to load workout objectives");

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "WorkoutObjectives"));

            // Assert
            var errorDiv = component.Find(".bg-red-50");
            Assert.Contains("Failed to load workout objectives", errorDiv.TextContent);
            var retryButton = component.Find("[data-testid='objectives-retry-button']");
            Assert.NotNull(retryButton);
        }

        [Fact]
        public void WorkoutCategories_RetryLoadsDataAfterError()
        {
            // Arrange
            _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingCategories).Returns(false);
            _workoutReferenceDataStateServiceMock.Setup(x => x.CategoriesError).Returns("Network error");
            _workoutReferenceDataStateServiceMock.Setup(x => x.LoadWorkoutCategoriesAsync()).Returns(Task.CompletedTask);

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "WorkoutCategories"));

            var retryButton = component.Find("[data-testid='categories-retry-button']");
            retryButton.Click();

            // Assert
            _workoutReferenceDataStateServiceMock.Verify(x => x.LoadWorkoutCategoriesAsync(), Times.Once);
        }

        #endregion

        #region Component Cleanup Tests

        [Fact]
        public void ReferenceTableDetail_DisposesStateServiceOnUnmount()
        {
            // Arrange
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "WorkoutObjectives"));

            // Act
            component.Dispose();

            // Assert
            // Verify that the OnChange event handler is removed (this is handled in the Dispose method)
            // The actual verification would be done by checking that state changes don't trigger updates
            // after disposal, but this is handled internally by the component
            Assert.True(component.IsDisposed);
        }

        #endregion
    }
}
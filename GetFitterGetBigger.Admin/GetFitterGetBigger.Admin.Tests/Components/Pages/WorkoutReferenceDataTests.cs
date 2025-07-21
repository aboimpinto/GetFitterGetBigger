using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Pages;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Bunit.TestDoubles;
using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages
{
    public class WorkoutReferenceDataTests : TestContext
    {
        private readonly Mock<IReferenceDataService> _referenceDataServiceMock;
        private readonly Mock<IEquipmentStateService> _equipmentStateServiceMock;
        private readonly Mock<IMuscleGroupsStateService> _muscleGroupsStateServiceMock;
        private readonly Mock<IExerciseWeightTypeStateService> _exerciseWeightTypeStateServiceMock;
        private readonly Mock<IWorkoutReferenceDataStateService> _workoutReferenceDataStateServiceMock;
        private readonly Mock<NavigationManager> _navigationManagerMock;

        public WorkoutReferenceDataTests()
        {
            _referenceDataServiceMock = new Mock<IReferenceDataService>();
            _equipmentStateServiceMock = new Mock<IEquipmentStateService>();
            _muscleGroupsStateServiceMock = new Mock<IMuscleGroupsStateService>();
            _exerciseWeightTypeStateServiceMock = new Mock<IExerciseWeightTypeStateService>();
            _workoutReferenceDataStateServiceMock = new Mock<IWorkoutReferenceDataStateService>();
            _navigationManagerMock = new Mock<NavigationManager>();

            Services.AddSingleton(_referenceDataServiceMock.Object);
            Services.AddSingleton(_equipmentStateServiceMock.Object);
            Services.AddSingleton(_muscleGroupsStateServiceMock.Object);
            Services.AddSingleton(_exerciseWeightTypeStateServiceMock.Object);
            Services.AddSingleton(_workoutReferenceDataStateServiceMock.Object);
            Services.AddSingleton(_navigationManagerMock.Object);
            
            this.AddTestAuthorization().SetAuthorized("test-user");
        }

        #region Workout Objectives Tests

        [Fact]
        public void WorkoutObjectives_RendersCorrectTitle()
        {
            // Arrange
            var objectives = new List<ReferenceDataDto>
            {
                new() { Id = Guid.NewGuid().ToString(), Value = "Muscle Building", Description = "Build muscle mass" }
            };
            _workoutReferenceDataStateServiceMock.Setup(x => x.FilteredWorkoutObjectives).Returns(objectives);
            _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingObjectives).Returns(false);
            _workoutReferenceDataStateServiceMock.Setup(x => x.ObjectivesError).Returns((string?)null);

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "WorkoutObjectives"));

            // Assert
            component.Find("h2").TextContent.Should().Be("Workout Objectives");
            component.Find("p").TextContent.Should().Be("View workout objectives for training programs");
        }

        [Fact]
        public void WorkoutObjectives_ShowsLoadingState()
        {
            // Arrange
            _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingObjectives).Returns(true);
            _workoutReferenceDataStateServiceMock.Setup(x => x.FilteredWorkoutObjectives).Returns(new List<ReferenceDataDto>());

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "WorkoutObjectives"));

            // Assert - Now using skeleton instead of spinner
            component.Find(".animate-pulse").Should().NotBeNull();
            var skeletonCards = component.FindAll(".bg-gray-50");
            skeletonCards.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public void WorkoutObjectives_DisplaysErrorMessage()
        {
            // Arrange
            const string errorMessage = "Failed to load objectives";
            _workoutReferenceDataStateServiceMock.Setup(x => x.ObjectivesError).Returns(errorMessage);
            _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingObjectives).Returns(false);
            _workoutReferenceDataStateServiceMock.Setup(x => x.FilteredWorkoutObjectives).Returns(new List<ReferenceDataDto>());

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "WorkoutObjectives"));

            // Assert
            var errorDiv = component.Find(".bg-red-50");
            errorDiv.TextContent.Should().Contain(errorMessage);
            errorDiv.QuerySelector("[data-testid='objectives-retry-button']").Should().NotBeNull();
        }

        [Fact]
        public void WorkoutObjectives_DisplaysObjectivesInCards()
        {
            // Arrange
            var objectives = new List<ReferenceDataDto>
            {
                new() { Id = Guid.NewGuid().ToString(), Value = "Muscle Building", Description = "Build muscle mass and strength" },
                new() { Id = Guid.NewGuid().ToString(), Value = "Fat Loss", Description = "Reduce body fat percentage" },
                new() { Id = Guid.NewGuid().ToString(), Value = "Endurance", Description = "Improve cardiovascular fitness" }
            };
            _workoutReferenceDataStateServiceMock.Setup(x => x.FilteredWorkoutObjectives).Returns(objectives);
            _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingObjectives).Returns(false);
            _workoutReferenceDataStateServiceMock.Setup(x => x.ObjectivesError).Returns((string?)null);

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "WorkoutObjectives"));

            // Assert
            var cards = component.FindAll("[data-testid='objective-card']");
            cards.Should().HaveCount(3);

            // Check first card
            var firstCard = cards[0];
            firstCard.QuerySelector("h3")!.TextContent.Should().Be("Muscle Building");
            firstCard.TextContent.Should().Contain("Build muscle mass and strength");
            firstCard.QuerySelector("[data-testid='objective-view-details-button']").Should().NotBeNull();
        }

        [Fact]
        public void WorkoutObjectives_ShowsEmptyStateWhenNoData()
        {
            // Arrange
            _workoutReferenceDataStateServiceMock.Setup(x => x.FilteredWorkoutObjectives).Returns(new List<ReferenceDataDto>());
            _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingObjectives).Returns(false);
            _workoutReferenceDataStateServiceMock.Setup(x => x.ObjectivesError).Returns((string?)null);

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "WorkoutObjectives"));

            // Assert
            component.Find(".text-center").TextContent.Should().Contain("No workout objectives found.");
        }

        [Fact]
        public void WorkoutObjectives_CallsLoadObjectivesOnRetryButton()
        {
            // Arrange
            const string errorMessage = "Test error";
            _workoutReferenceDataStateServiceMock.Setup(x => x.ObjectivesError).Returns(errorMessage);
            _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingObjectives).Returns(false);
            _workoutReferenceDataStateServiceMock.Setup(x => x.FilteredWorkoutObjectives).Returns(new List<ReferenceDataDto>());
            _workoutReferenceDataStateServiceMock.Setup(x => x.LoadWorkoutObjectivesAsync()).Returns(Task.CompletedTask);

            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "WorkoutObjectives"));

            // Act
            var retryButton = component.Find("[data-testid='objectives-retry-button']");
            retryButton.Click();

            // Assert
            _workoutReferenceDataStateServiceMock.Verify(x => x.LoadWorkoutObjectivesAsync(), Times.Once);
        }

        [Fact]
        public void WorkoutObjectives_ResponsiveGridLayout()
        {
            // Arrange
            var objectives = new List<ReferenceDataDto>
            {
                new() { Id = Guid.NewGuid().ToString(), Value = "Test", Description = "Test" }
            };
            _workoutReferenceDataStateServiceMock.Setup(x => x.FilteredWorkoutObjectives).Returns(objectives);
            _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingObjectives).Returns(false);
            _workoutReferenceDataStateServiceMock.Setup(x => x.ObjectivesError).Returns((string?)null);

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "WorkoutObjectives"));

            // Assert
            var gridContainer = component.Find("[data-testid='objectives-grid']");
            (gridContainer.GetAttribute("class") ?? "").Should().Contain("grid-cols-1")
                .And.Contain("sm:grid-cols-2")
                .And.Contain("lg:grid-cols-3")
                .And.Contain("xl:grid-cols-4");
        }

        #endregion

        #region Workout Categories Tests

        [Fact]
        public void WorkoutCategories_RendersCorrectTitle()
        {
            // Arrange
            var categories = new List<WorkoutCategoryDto>
            {
                new() { WorkoutCategoryId = "1", Value = "Strength", Color = "#FF0000", PrimaryMuscleGroups = "Chest, Back" }
            };
            _workoutReferenceDataStateServiceMock.Setup(x => x.FilteredWorkoutCategories).Returns(categories);
            _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingCategories).Returns(false);
            _workoutReferenceDataStateServiceMock.Setup(x => x.CategoriesError).Returns((string?)null);

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "WorkoutCategories"));

            // Assert
            component.Find("h2").TextContent.Should().Be("Workout Categories");
            component.Find("p").TextContent.Should().Be("View workout categories and their muscle group associations");
        }

        [Fact]
        public void WorkoutCategories_ShowsLoadingState()
        {
            // Arrange
            _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingCategories).Returns(true);
            _workoutReferenceDataStateServiceMock.Setup(x => x.FilteredWorkoutCategories).Returns(new List<WorkoutCategoryDto>());

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "WorkoutCategories"));

            // Assert - Now using skeleton instead of spinner
            component.Find(".animate-pulse").Should().NotBeNull();
            var skeletonCards = component.FindAll(".bg-gray-50");
            skeletonCards.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public void WorkoutCategories_DisplaysErrorMessage()
        {
            // Arrange
            const string errorMessage = "Failed to load categories";
            _workoutReferenceDataStateServiceMock.Setup(x => x.CategoriesError).Returns(errorMessage);
            _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingCategories).Returns(false);
            _workoutReferenceDataStateServiceMock.Setup(x => x.FilteredWorkoutCategories).Returns(new List<WorkoutCategoryDto>());

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "WorkoutCategories"));

            // Assert
            var errorDiv = component.Find(".bg-red-50");
            errorDiv.TextContent.Should().Contain(errorMessage);
            errorDiv.QuerySelector("[data-testid='categories-retry-button']").Should().NotBeNull();
        }

        [Fact]
        public void WorkoutCategories_DisplaysCategoriesInCards()
        {
            // Arrange
            var categories = new List<WorkoutCategoryDto>
            {
                new() { WorkoutCategoryId = "1", Value = "Strength", Color = "#FF0000", Icon = "<svg></svg>", PrimaryMuscleGroups = "Chest, Back" },
                new() { WorkoutCategoryId = "2", Value = "Cardio", Color = "#00FF00", PrimaryMuscleGroups = "Full Body" }
            };
            _workoutReferenceDataStateServiceMock.Setup(x => x.FilteredWorkoutCategories).Returns(categories);
            _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingCategories).Returns(false);
            _workoutReferenceDataStateServiceMock.Setup(x => x.CategoriesError).Returns((string?)null);

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "WorkoutCategories"));

            // Assert
            var cards = component.FindAll("[data-testid='category-card']");
            cards.Should().HaveCount(2);

            // Check first card
            var firstCard = cards[0];
            firstCard.QuerySelector("h3")!.TextContent.Should().Be("Strength");
            firstCard.QuerySelector("[data-testid='muscle-groups']")!.TextContent.Should().Be("Chest, Back");
            firstCard.QuerySelector("[data-testid='category-view-details-button']").Should().NotBeNull();
        }

        [Fact]
        public void WorkoutCategories_ShowsColorAndIcon()
        {
            // Arrange
            var categories = new List<WorkoutCategoryDto>
            {
                new() { WorkoutCategoryId = "1", Value = "Strength", Color = "#FF0000", Icon = "<svg><circle/></svg>", PrimaryMuscleGroups = "Chest" }
            };
            _workoutReferenceDataStateServiceMock.Setup(x => x.FilteredWorkoutCategories).Returns(categories);
            _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingCategories).Returns(false);
            _workoutReferenceDataStateServiceMock.Setup(x => x.CategoriesError).Returns((string?)null);

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "WorkoutCategories"));

            // Assert
            var card = component.Find("[data-testid='category-card']");
            card.GetAttribute("style")?.Should().Contain("border-color: #FF000030");
            
            var iconContainer = card.QuerySelector("[data-testid='category-icon']");
            iconContainer?.InnerHtml.Should().Contain("<svg>");
        }

        [Fact]
        public void WorkoutCategories_ShowsInitialWhenNoIcon()
        {
            // Arrange
            var categories = new List<WorkoutCategoryDto>
            {
                new() { WorkoutCategoryId = "1", Value = "Cardio", Color = "#00FF00" }
            };
            _workoutReferenceDataStateServiceMock.Setup(x => x.FilteredWorkoutCategories).Returns(categories);
            _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingCategories).Returns(false);
            _workoutReferenceDataStateServiceMock.Setup(x => x.CategoriesError).Returns((string?)null);

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "WorkoutCategories"));

            // Assert
            var card = component.Find("[data-testid='category-card']");
            var initialSpan = card.QuerySelector(".text-2xl.font-bold");
            initialSpan?.TextContent.Should().Be("C"); // First letter of "Cardio"
        }

        [Fact]
        public void WorkoutCategories_CallsLoadCategoriesOnRetryButton()
        {
            // Arrange
            const string errorMessage = "Test error";
            _workoutReferenceDataStateServiceMock.Setup(x => x.CategoriesError).Returns(errorMessage);
            _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingCategories).Returns(false);
            _workoutReferenceDataStateServiceMock.Setup(x => x.FilteredWorkoutCategories).Returns(new List<WorkoutCategoryDto>());
            _workoutReferenceDataStateServiceMock.Setup(x => x.LoadWorkoutCategoriesAsync()).Returns(Task.CompletedTask);

            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "WorkoutCategories"));

            // Act
            var retryButton = component.Find("[data-testid='categories-retry-button']");
            retryButton.Click();

            // Assert
            _workoutReferenceDataStateServiceMock.Verify(x => x.LoadWorkoutCategoriesAsync(), Times.Once);
        }

        #endregion

        #region Execution Protocols Tests

        [Fact]
        public void ExecutionProtocols_RendersCorrectTitle()
        {
            // Arrange
            var protocols = new List<ExecutionProtocolDto>
            {
                new() { ExecutionProtocolId = "1", Value = "Standard", Code = "STD", TimeBase = true, RepBase = false }
            };
            _workoutReferenceDataStateServiceMock.Setup(x => x.FilteredExecutionProtocols).Returns(protocols);
            _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingProtocols).Returns(false);
            _workoutReferenceDataStateServiceMock.Setup(x => x.ProtocolsError).Returns((string?)null);

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "ExecutionProtocols"));

            // Assert
            component.Find("h2").TextContent.Should().Be("Execution Protocols");
            component.Find("p").TextContent.Should().Be("View exercise execution protocols and timing patterns");
        }

        [Fact]
        public void ExecutionProtocols_ShowsLoadingState()
        {
            // Arrange
            _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingProtocols).Returns(true);
            _workoutReferenceDataStateServiceMock.Setup(x => x.FilteredExecutionProtocols).Returns(new List<ExecutionProtocolDto>());

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "ExecutionProtocols"));

            // Assert - Now using skeleton instead of spinner
            component.Find(".animate-pulse").Should().NotBeNull();
            var skeletonCards = component.FindAll(".bg-gray-50");
            skeletonCards.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public void ExecutionProtocols_DisplaysErrorMessage()
        {
            // Arrange
            const string errorMessage = "Failed to load protocols";
            _workoutReferenceDataStateServiceMock.Setup(x => x.ProtocolsError).Returns(errorMessage);
            _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingProtocols).Returns(false);
            _workoutReferenceDataStateServiceMock.Setup(x => x.FilteredExecutionProtocols).Returns(new List<ExecutionProtocolDto>());

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "ExecutionProtocols"));

            // Assert
            var errorDiv = component.Find(".bg-red-50");
            errorDiv.TextContent.Should().Contain(errorMessage);
            errorDiv.QuerySelector("[data-testid='protocols-retry-button']").Should().NotBeNull();
        }

        [Fact]
        public void ExecutionProtocols_DisplaysProtocolsInCards()
        {
            // Arrange
            var protocols = new List<ExecutionProtocolDto>
            {
                new() { ExecutionProtocolId = "1", Value = "Standard", Code = "STD", Description = "Standard protocol", TimeBase = true, RepBase = false, IntensityLevel = "Medium" },
                new() { ExecutionProtocolId = "2", Value = "HIIT", Code = "HIIT", Description = "High intensity", TimeBase = true, RepBase = false, IntensityLevel = "High" }
            };
            _workoutReferenceDataStateServiceMock.Setup(x => x.FilteredExecutionProtocols).Returns(protocols);
            _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingProtocols).Returns(false);
            _workoutReferenceDataStateServiceMock.Setup(x => x.ProtocolsError).Returns((string?)null);

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "ExecutionProtocols"));

            // Assert
            var cards = component.FindAll("[data-testid='protocol-card']");
            cards.Should().HaveCount(2);

            // Check first card
            var firstCard = cards[0];
            firstCard.QuerySelector("h3")!.TextContent.Should().Be("Standard");
            firstCard.QuerySelector("span")!.TextContent.Should().Be("STD");
            firstCard.TextContent.Should().Contain("Standard protocol");
            firstCard.TextContent.Should().Contain("Time-Based:");
            firstCard.TextContent.Should().Contain("Yes");
            firstCard.TextContent.Should().Contain("Rep-Based:");
            firstCard.TextContent.Should().Contain("No");
        }

        [Fact]
        public void ExecutionProtocols_ShowsCorrectIntensityBadgeColors()
        {
            // Arrange
            var protocols = new List<ExecutionProtocolDto>
            {
                new() { ExecutionProtocolId = "1", Value = "Low", Code = "LOW", IntensityLevel = "low" },
                new() { ExecutionProtocolId = "2", Value = "Medium", Code = "MED", IntensityLevel = "medium" },
                new() { ExecutionProtocolId = "3", Value = "High", Code = "HIGH", IntensityLevel = "high" }
            };
            _workoutReferenceDataStateServiceMock.Setup(x => x.FilteredExecutionProtocols).Returns(protocols);
            _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingProtocols).Returns(false);
            _workoutReferenceDataStateServiceMock.Setup(x => x.ProtocolsError).Returns((string?)null);

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "ExecutionProtocols"));

            // Assert
            var cards = component.FindAll("[data-testid='protocol-card']");
            
            // Low intensity - green
            cards[0].InnerHtml.Should().Contain("bg-green-100 text-green-800");
            
            // Medium intensity - yellow
            cards[1].InnerHtml.Should().Contain("bg-yellow-100 text-yellow-800");
            
            // High intensity - red
            cards[2].InnerHtml.Should().Contain("bg-red-100 text-red-800");
        }

        [Fact]
        public void ExecutionProtocols_CallsLoadProtocolsOnRetryButton()
        {
            // Arrange
            const string errorMessage = "Test error";
            _workoutReferenceDataStateServiceMock.Setup(x => x.ProtocolsError).Returns(errorMessage);
            _workoutReferenceDataStateServiceMock.Setup(x => x.IsLoadingProtocols).Returns(false);
            _workoutReferenceDataStateServiceMock.Setup(x => x.FilteredExecutionProtocols).Returns(new List<ExecutionProtocolDto>());
            _workoutReferenceDataStateServiceMock.Setup(x => x.LoadExecutionProtocolsAsync()).Returns(Task.CompletedTask);

            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "ExecutionProtocols"));

            // Act
            var retryButton = component.Find("[data-testid='protocols-retry-button']");
            retryButton.Click();

            // Assert
            _workoutReferenceDataStateServiceMock.Verify(x => x.LoadExecutionProtocolsAsync(), Times.Once);
        }

        #endregion

        #region Common Tests

        [Fact]
        public void WorkoutReferenceData_CallsInitializeOnLoad()
        {
            // Arrange
            _workoutReferenceDataStateServiceMock.Setup(x => x.InitializeAsync()).Returns(Task.CompletedTask);

            // Act
            RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "WorkoutObjectives"));

            // Assert
            _workoutReferenceDataStateServiceMock.Verify(x => x.InitializeAsync(), Times.Once);
        }

        [Fact]
        public void WorkoutReferenceData_SubscribesToStateChanges()
        {
            // Arrange
            Action? stateChangeHandler = null;
            _workoutReferenceDataStateServiceMock.SetupAdd(x => x.OnChange += It.IsAny<Action>())
                .Callback<Action>(handler => stateChangeHandler = handler);

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "WorkoutCategories"));

            // Assert
            stateChangeHandler.Should().NotBeNull();
            _workoutReferenceDataStateServiceMock.VerifyAdd(x => x.OnChange += It.IsAny<Action>(), Times.Once);
        }

        [Fact]
        public void WorkoutReferenceData_UnsubscribesFromStateChangesOnDispose()
        {
            // Arrange
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "ExecutionProtocols"));

            // Act
            component.Dispose();

            // Assert
            // The component should unsubscribe when disposed
            _workoutReferenceDataStateServiceMock.VerifyAdd(x => x.OnChange += It.IsAny<Action>(), Times.Once);
        }

        #endregion
    }
}
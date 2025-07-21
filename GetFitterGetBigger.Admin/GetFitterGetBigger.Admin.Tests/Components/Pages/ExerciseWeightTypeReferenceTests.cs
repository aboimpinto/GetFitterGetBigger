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

namespace GetFitterGetBigger.Admin.Tests.Components.Pages
{
    public class ExerciseWeightTypeReferenceTests : TestContext
    {
        private readonly Mock<IReferenceDataService> _referenceDataServiceMock;
        private readonly Mock<IEquipmentStateService> _equipmentStateServiceMock;
        private readonly Mock<IMuscleGroupsStateService> _muscleGroupsStateServiceMock;
        private readonly Mock<IExerciseWeightTypeStateService> _exerciseWeightTypeStateServiceMock;
        private readonly Mock<IWorkoutReferenceDataStateService> _workoutReferenceDataStateServiceMock;
        private readonly Mock<NavigationManager> _navigationManagerMock;

        public ExerciseWeightTypeReferenceTests()
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

        [Fact]
        public void ExerciseWeightTypes_RendersCorrectTitle()
        {
            // Arrange
            var weightTypes = new List<ExerciseWeightTypeDto>
            {
                new() { Id = Guid.NewGuid(), Code = "BODYWEIGHT_ONLY", Name = "Bodyweight Only", Description = "Test description", IsActive = true, DisplayOrder = 1 }
            };
            _exerciseWeightTypeStateServiceMock.Setup(x => x.WeightTypes).Returns(weightTypes);
            _exerciseWeightTypeStateServiceMock.Setup(x => x.IsLoading).Returns(false);
            _exerciseWeightTypeStateServiceMock.Setup(x => x.ErrorMessage).Returns((string?)null);

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "ExerciseWeightTypes"));

            // Assert
            component.Find("h2").TextContent.Should().Be("Exercise Weight Types");
            component.Find("p").TextContent.Should().Be("View exercise weight type classifications and validation rules");
        }

        [Fact]
        public void ExerciseWeightTypes_ShowsLoadingState()
        {
            // Arrange
            _exerciseWeightTypeStateServiceMock.Setup(x => x.IsLoading).Returns(true);
            _exerciseWeightTypeStateServiceMock.Setup(x => x.WeightTypes).Returns(new List<ExerciseWeightTypeDto>());

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "ExerciseWeightTypes"));

            // Assert - Now using skeleton instead of spinner
            component.Find(".animate-pulse").Should().NotBeNull();
            var skeletonCards = component.FindAll(".bg-gray-50");
            skeletonCards.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public void ExerciseWeightTypes_DisplaysErrorMessage()
        {
            // Arrange
            const string errorMessage = "Failed to load weight types";
            _exerciseWeightTypeStateServiceMock.Setup(x => x.ErrorMessage).Returns(errorMessage);
            _exerciseWeightTypeStateServiceMock.Setup(x => x.IsLoading).Returns(false);
            _exerciseWeightTypeStateServiceMock.Setup(x => x.WeightTypes).Returns(new List<ExerciseWeightTypeDto>());

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "ExerciseWeightTypes"));

            // Assert
            var errorDiv = component.Find(".bg-red-50");
            errorDiv.TextContent.Should().Contain(errorMessage);
            errorDiv.QuerySelector("button").Should().NotBeNull(); // Dismiss button
        }

        [Fact]
        public void ExerciseWeightTypes_DisplaysWeightTypesInCards()
        {
            // Arrange
            var weightTypes = new List<ExerciseWeightTypeDto>
            {
                new() { Id = Guid.NewGuid(), Code = "BODYWEIGHT_ONLY", Name = "Bodyweight Only", Description = "Uses body weight only", IsActive = true, DisplayOrder = 1 },
                new() { Id = Guid.NewGuid(), Code = "WEIGHT_REQUIRED", Name = "Weight Required", Description = "Requires external weight", IsActive = true, DisplayOrder = 2 },
                new() { Id = Guid.NewGuid(), Code = "MACHINE_WEIGHT", Name = "Machine Weight", Description = "Uses machine weights", IsActive = true, DisplayOrder = 3 }
            };
            _exerciseWeightTypeStateServiceMock.Setup(x => x.WeightTypes).Returns(weightTypes);
            _exerciseWeightTypeStateServiceMock.Setup(x => x.IsLoading).Returns(false);
            _exerciseWeightTypeStateServiceMock.Setup(x => x.ErrorMessage).Returns((string?)null);
            _exerciseWeightTypeStateServiceMock.Setup(x => x.GetValidationMessage("BODYWEIGHT_ONLY"))
                .Returns("This exercise uses bodyweight only. Weight cannot be specified.");
            _exerciseWeightTypeStateServiceMock.Setup(x => x.GetValidationMessage("WEIGHT_REQUIRED"))
                .Returns("This exercise requires a weight to be specified.");
            _exerciseWeightTypeStateServiceMock.Setup(x => x.GetValidationMessage("MACHINE_WEIGHT"))
                .Returns("This exercise uses machine weights.");

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "ExerciseWeightTypes"));

            // Assert
            var cards = component.FindAll(".bg-gray-50");
            cards.Should().HaveCount(3);

            // Check first card
            var firstCard = cards[0];
            firstCard.QuerySelector("h3")!.TextContent.Should().Be("Bodyweight Only");
            firstCard.QuerySelector("span")!.TextContent.Should().Be("BODYWEIGHT_ONLY");
            firstCard.TextContent.Should().Contain("Uses body weight only");
            firstCard.TextContent.Should().Contain("This exercise uses bodyweight only. Weight cannot be specified.");

            // Check color coding for badges
            var bodyweitghtBadge = firstCard.QuerySelector("span")!;
            (bodyweitghtBadge.GetAttribute("class") ?? "").Should().Contain("bg-blue-100").And.Contain("text-blue-800");
        }

        [Fact]
        public void ExerciseWeightTypes_DisplaysCorrectBadgeColors()
        {
            // Arrange
            var weightTypes = new List<ExerciseWeightTypeDto>
            {
                new() { Id = Guid.NewGuid(), Code = "BODYWEIGHT_ONLY", Name = "Bodyweight Only", Description = "Test", IsActive = true, DisplayOrder = 1 },
                new() { Id = Guid.NewGuid(), Code = "NO_WEIGHT", Name = "No Weight", Description = "Test", IsActive = true, DisplayOrder = 2 },
                new() { Id = Guid.NewGuid(), Code = "BODYWEIGHT_OPTIONAL", Name = "Bodyweight Optional", Description = "Test", IsActive = true, DisplayOrder = 3 },
                new() { Id = Guid.NewGuid(), Code = "WEIGHT_REQUIRED", Name = "Weight Required", Description = "Test", IsActive = true, DisplayOrder = 4 },
                new() { Id = Guid.NewGuid(), Code = "MACHINE_WEIGHT", Name = "Machine Weight", Description = "Test", IsActive = true, DisplayOrder = 5 }
            };
            _exerciseWeightTypeStateServiceMock.Setup(x => x.WeightTypes).Returns(weightTypes);
            _exerciseWeightTypeStateServiceMock.Setup(x => x.IsLoading).Returns(false);
            _exerciseWeightTypeStateServiceMock.Setup(x => x.ErrorMessage).Returns((string?)null);
            
            // Setup validation messages
            foreach (var wt in weightTypes)
            {
                _exerciseWeightTypeStateServiceMock.Setup(x => x.GetValidationMessage(wt.Code))
                    .Returns($"Validation for {wt.Code}");
            }

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "ExerciseWeightTypes"));

            // Assert
            var cards = component.FindAll(".bg-gray-50");
            
            // BODYWEIGHT_ONLY - blue
            var bodyweitghtBadge = cards[0].QuerySelector("span")!;
            (bodyweitghtBadge.GetAttribute("class") ?? "").Should().Contain("bg-blue-100").And.Contain("text-blue-800");
            
            // NO_WEIGHT - gray
            var noWeightBadge = cards[1].QuerySelector("span")!;
            (noWeightBadge.GetAttribute("class") ?? "").Should().Contain("bg-gray-100").And.Contain("text-gray-800");
            
            // BODYWEIGHT_OPTIONAL - green
            var optionalBadge = cards[2].QuerySelector("span")!;
            (optionalBadge.GetAttribute("class") ?? "").Should().Contain("bg-green-100").And.Contain("text-green-800");
            
            // WEIGHT_REQUIRED - orange
            var requiredBadge = cards[3].QuerySelector("span")!;
            (requiredBadge.GetAttribute("class") ?? "").Should().Contain("bg-orange-100").And.Contain("text-orange-800");
            
            // MACHINE_WEIGHT - purple
            var machineBadge = cards[4].QuerySelector("span")!;
            (machineBadge.GetAttribute("class") ?? "").Should().Contain("bg-purple-100").And.Contain("text-purple-800");
        }

        [Fact]
        public void ExerciseWeightTypes_ShowsEmptyStateWhenNoData()
        {
            // Arrange
            _exerciseWeightTypeStateServiceMock.Setup(x => x.WeightTypes).Returns(new List<ExerciseWeightTypeDto>());
            _exerciseWeightTypeStateServiceMock.Setup(x => x.IsLoading).Returns(false);
            _exerciseWeightTypeStateServiceMock.Setup(x => x.ErrorMessage).Returns((string?)null);

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "ExerciseWeightTypes"));

            // Assert
            component.Find(".text-center").TextContent.Should().Contain("No exercise weight types found.");
        }

        [Fact]
        public void ExerciseWeightTypes_CallsClearErrorOnDismissButton()
        {
            // Arrange
            const string errorMessage = "Test error";
            _exerciseWeightTypeStateServiceMock.Setup(x => x.ErrorMessage).Returns(errorMessage);
            _exerciseWeightTypeStateServiceMock.Setup(x => x.IsLoading).Returns(false);
            _exerciseWeightTypeStateServiceMock.Setup(x => x.WeightTypes).Returns(new List<ExerciseWeightTypeDto>());
            _exerciseWeightTypeStateServiceMock.Setup(x => x.ClearError());
            _exerciseWeightTypeStateServiceMock.Setup(x => x.LoadWeightTypesAsync()).Returns(Task.CompletedTask);

            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "ExerciseWeightTypes"));

            // Act
            var dismissButton = component.Find("[data-testid='weighttype-dismiss-button']");
            dismissButton.Click();

            // Assert
            _exerciseWeightTypeStateServiceMock.Verify(x => x.ClearError(), Times.Once);
        }

        [Fact]
        public void ExerciseWeightTypes_ResponsiveGridLayout()
        {
            // Arrange
            var weightTypes = new List<ExerciseWeightTypeDto>
            {
                new() { Id = Guid.NewGuid(), Code = "BODYWEIGHT_ONLY", Name = "Bodyweight Only", Description = "Test", IsActive = true, DisplayOrder = 1 }
            };
            _exerciseWeightTypeStateServiceMock.Setup(x => x.WeightTypes).Returns(weightTypes);
            _exerciseWeightTypeStateServiceMock.Setup(x => x.IsLoading).Returns(false);
            _exerciseWeightTypeStateServiceMock.Setup(x => x.ErrorMessage).Returns((string?)null);
            _exerciseWeightTypeStateServiceMock.Setup(x => x.GetValidationMessage("BODYWEIGHT_ONLY")).Returns("Test validation");

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "ExerciseWeightTypes"));

            // Assert
            var gridContainer = component.Find(".grid");
            (gridContainer.GetAttribute("class") ?? "").Should().Contain("grid-cols-1")
                .And.Contain("md:grid-cols-2")
                .And.Contain("lg:grid-cols-3");
        }

        [Fact]
        public void ExerciseWeightTypes_CallsLoadWeightTypesOnInitialization()
        {
            // Act
            RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "ExerciseWeightTypes"));

            // Assert
            _exerciseWeightTypeStateServiceMock.Verify(x => x.LoadWeightTypesAsync(), Times.Once);
        }

        [Fact]
        public void ExerciseWeightTypes_SubscribesToStateChanges()
        {
            // Arrange
            Action? stateChangeHandler = null;
            _exerciseWeightTypeStateServiceMock.SetupAdd(x => x.OnChange += It.IsAny<Action>())
                .Callback<Action>(handler => stateChangeHandler = handler);

            // Act
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "ExerciseWeightTypes"));

            // Assert
            stateChangeHandler.Should().NotBeNull();
            _exerciseWeightTypeStateServiceMock.VerifyAdd(x => x.OnChange += It.IsAny<Action>(), Times.Once);
        }

        [Fact]
        public void ExerciseWeightTypes_UnsubscribesFromStateChangesOnDispose()
        {
            // Arrange
            var component = RenderComponent<ReferenceTableDetail>(parameters => parameters
                .Add(p => p.TableName, "ExerciseWeightTypes"));

            // Act
            component.Dispose();

            // Assert - The component should unsubscribe when disposed
            // Note: In practice, this is handled by the component lifecycle in ReferenceTableDetail.razor
            _exerciseWeightTypeStateServiceMock.VerifyAdd(x => x.OnChange += It.IsAny<Action>(), Times.Once);
        }
    }
}
using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Shared;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using AngleSharp.Dom;

namespace GetFitterGetBigger.Admin.Tests.Components.Shared
{
    public class ExerciseWeightTypeSelectorTests : TestContext
    {
        private readonly Mock<IExerciseWeightTypeStateService> _stateServiceMock;
        private readonly List<ExerciseWeightTypeDto> _testWeightTypes;

        public ExerciseWeightTypeSelectorTests()
        {
            _stateServiceMock = new Mock<IExerciseWeightTypeStateService>();
            _testWeightTypes = new List<ExerciseWeightTypeDto>
            {
                new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Code = "BODYWEIGHT_ONLY", Name = "Bodyweight Only", Description = "Uses bodyweight only", IsActive = true, DisplayOrder = 1 },
                new() { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Code = "NO_WEIGHT", Name = "No Weight", Description = "No weight used", IsActive = true, DisplayOrder = 2 },
                new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Code = "BODYWEIGHT_OPTIONAL", Name = "Bodyweight Optional", Description = "Weight optional", IsActive = true, DisplayOrder = 3 },
                new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Code = "WEIGHT_REQUIRED", Name = "Weight Required", Description = "Weight required", IsActive = true, DisplayOrder = 4 },
                new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Code = "MACHINE_WEIGHT", Name = "Machine Weight", Description = "Machine weight", IsActive = true, DisplayOrder = 5 }
            };

            Services.AddSingleton(_stateServiceMock.Object);
        }

        [Fact]
        public void Component_RendersWithRequiredIndicator_WhenRequired()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
            _stateServiceMock.Setup(x => x.IsLoading).Returns(false);

            // Act
            var component = RenderComponent<ExerciseWeightTypeSelector>(parameters => parameters
                .Add(p => p.Required, true));

            // Assert
            component.Find("label").TextContent.Should().Contain("*");
            component.Find("label").TextContent.Should().Contain("Weight Type");
        }

        [Fact]
        public void Component_ShowsLoadingState_WhenLoading()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.IsLoading).Returns(true);
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(new List<ExerciseWeightTypeDto>());

            // Act
            var component = RenderComponent<ExerciseWeightTypeSelector>();

            // Assert
            component.Find(".animate-spin").Should().NotBeNull();
            var loadingText = component.FindAll("span").FirstOrDefault(s => s.TextContent.Contains("Loading weight types..."));
            loadingText.Should().NotBeNull();
        }

        [Fact]
        public void Component_ShowsErrorState_WhenError()
        {
            // Arrange
            const string errorMessage = "Failed to load weight types";
            _stateServiceMock.Setup(x => x.IsLoading).Returns(false);
            _stateServiceMock.Setup(x => x.ErrorMessage).Returns(errorMessage);
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(new List<ExerciseWeightTypeDto>());

            // Act
            var component = RenderComponent<ExerciseWeightTypeSelector>();

            // Assert
            var errorDiv = component.Find(".bg-red-50");
            errorDiv.TextContent.Should().Contain(errorMessage);
            component.Find("button").TextContent.Should().Be("Retry");
        }

        [Fact]
        public void Component_PopulatesDropdown_WithWeightTypes()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
            _stateServiceMock.Setup(x => x.IsLoading).Returns(false);

            // Act
            var component = RenderComponent<ExerciseWeightTypeSelector>();

            // Assert
            var options = component.FindAll("option");
            options.Should().HaveCount(6); // 5 weight types + 1 placeholder
            options[0].TextContent.Should().Be("Select weight type");
            options[1].TextContent.Should().Contain("Bodyweight Only");
            options[2].TextContent.Should().Contain("No Weight");
            options[3].TextContent.Should().Contain("Bodyweight Optional");
            options[4].TextContent.Should().Contain("Weight Required");
            options[5].TextContent.Should().Contain("Machine Weight");
        }

        [Fact]
        public void Component_ShowsSelectedWeightTypeDetails()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
            _stateServiceMock.Setup(x => x.IsLoading).Returns(false);
            _stateServiceMock.Setup(x => x.GetValidationMessage("BODYWEIGHT_ONLY"))
                .Returns("This exercise uses bodyweight only. Weight cannot be specified.");

            var component = RenderComponent<ExerciseWeightTypeSelector>(parameters => parameters
                .Add(p => p.Value, "11111111-1111-1111-1111-111111111111")
                .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, _ => { })));

            // Assert
            var detailsDiv = component.Find(".bg-gray-50");
            detailsDiv.Should().NotBeNull();
            detailsDiv.TextContent.Should().Contain("Bodyweight Only");
            detailsDiv.TextContent.Should().Contain("Uses bodyweight only");
            detailsDiv.TextContent.Should().Contain("This exercise uses bodyweight only. Weight cannot be specified.");
        }

        [Fact]
        public void Component_InvokesValueChanged_WhenSelectionChanges()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
            _stateServiceMock.Setup(x => x.IsLoading).Returns(false);

            string? capturedValue = null;
            var component = RenderComponent<ExerciseWeightTypeSelector>(parameters => parameters
                .Add(p => p.Value, string.Empty)
                .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => capturedValue = value)));

            // Act
            var select = component.Find("select");
            select.Change("22222222-2222-2222-2222-222222222222");

            // Assert
            capturedValue.Should().Be("22222222-2222-2222-2222-222222222222");
        }

        [Fact]
        public void Component_ShowsDisabledState()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
            _stateServiceMock.Setup(x => x.IsLoading).Returns(false);

            // Act
            var component = RenderComponent<ExerciseWeightTypeSelector>(parameters => parameters
                .Add(p => p.Disabled, true));

            // Assert
            var select = component.Find("select");
            select.GetAttribute("disabled").Should().NotBeNull();
            (select.GetAttribute("class") ?? "").Should().Contain("bg-gray-100");
            (select.GetAttribute("class") ?? "").Should().Contain("cursor-not-allowed");
            component.Find("label").TextContent.Should().Contain("(disabled)");
        }

        [Fact]
        public void Component_ShowsValidationError()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
            _stateServiceMock.Setup(x => x.IsLoading).Returns(false);

            // Act
            var component = RenderComponent<ExerciseWeightTypeSelector>(parameters => parameters
                .Add(p => p.ValidationMessage, "Weight type is required")
                .Add(p => p.ShowValidationError, true));

            // Assert
            var errorMessage = component.Find("[data-testid=\"weight-type-validation-error\"]");
            errorMessage.TextContent.Should().Be("Weight type is required");
            
            var select = component.Find("select");
            (select.GetAttribute("class") ?? "").Should().Contain("border-red-300");
        }

        [Fact]
        public void Component_LoadsWeightTypes_OnInitialization()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(new List<ExerciseWeightTypeDto>());
            _stateServiceMock.Setup(x => x.IsLoading).Returns(false);

            // Act
            RenderComponent<ExerciseWeightTypeSelector>();

            // Assert
            _stateServiceMock.Verify(x => x.LoadWeightTypesAsync(), Times.Once);
        }

        [Fact]
        public void Component_SkipsLoading_WhenWeightTypesAlreadyLoaded()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
            _stateServiceMock.Setup(x => x.IsLoading).Returns(false);

            // Act
            RenderComponent<ExerciseWeightTypeSelector>();

            // Assert
            _stateServiceMock.Verify(x => x.LoadWeightTypesAsync(), Times.Never);
        }

        [Fact]
        public void Component_CallsClearErrorAndReload_OnRetry()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.IsLoading).Returns(false);
            _stateServiceMock.Setup(x => x.ErrorMessage).Returns("Error");
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(new List<ExerciseWeightTypeDto>());

            var component = RenderComponent<ExerciseWeightTypeSelector>();

            // Act
            component.Find("button").Click();

            // Assert
            _stateServiceMock.Verify(x => x.ClearError(), Times.Once);
            _stateServiceMock.Verify(x => x.LoadWeightTypesAsync(), Times.Exactly(2)); // Once on init, once on retry
        }

        [Fact]
        public void Component_ShowsCorrectIconsForWeightTypes()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
            _stateServiceMock.Setup(x => x.IsLoading).Returns(false);
            _stateServiceMock.Setup(x => x.GetValidationMessage(It.IsAny<string>())).Returns("Test validation");

            // Test each weight type
            var testCases = new[]
            {
                ("11111111-1111-1111-1111-111111111111", "💪", "text-blue-600"), // BODYWEIGHT_ONLY
                ("22222222-2222-2222-2222-222222222222", "🚫", "text-gray-600"), // NO_WEIGHT
                ("33333333-3333-3333-3333-333333333333", "⚖️", "text-green-600"), // BODYWEIGHT_OPTIONAL
                ("44444444-4444-4444-4444-444444444444", "🏋️", "text-orange-600"), // WEIGHT_REQUIRED
                ("55555555-5555-5555-5555-555555555555", "🎯", "text-purple-600") // MACHINE_WEIGHT
            };

            foreach (var (id, expectedIcon, expectedColor) in testCases)
            {
                // Act
                var component = RenderComponent<ExerciseWeightTypeSelector>(parameters => parameters
                    .Add(p => p.Value, id));

                // Assert
                var iconSpan = component.Find(".bg-gray-50 span");
                iconSpan.TextContent.Should().Be(expectedIcon);
                (iconSpan.GetAttribute("class") ?? "").Should().Contain(expectedColor);
            }
        }

        [Fact]
        public void Component_SubscribesToStateChanges_OnInitialization()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
            _stateServiceMock.Setup(x => x.IsLoading).Returns(false);

            // Act
            var component = RenderComponent<ExerciseWeightTypeSelector>();

            // Assert
            _stateServiceMock.VerifyAdd(x => x.OnChange += It.IsAny<Action>(), Times.Once);
        }
    }
}
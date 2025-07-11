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
    public class WeightInputFieldTests : TestContext
    {
        private readonly Mock<IExerciseWeightTypeStateService> _stateServiceMock;
        private readonly List<ExerciseWeightTypeDto> _testWeightTypes;

        public WeightInputFieldTests()
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
        public void Component_HidesWeightInput_ForBodyweightOnly()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
            _stateServiceMock.Setup(x => x.RequiresWeightInput("BODYWEIGHT_ONLY")).Returns(false);

            // Act
            var component = RenderComponent<WeightInputField>(parameters => parameters
                .Add(p => p.WeightTypeId, "11111111-1111-1111-1111-111111111111"));

            // Assert
            component.FindAll("[data-testid=\"weight-input\"]").Should().BeEmpty();
            var notApplicableDiv = component.Find("[data-testid=\"weight-not-applicable\"]");
            notApplicableDiv.Should().NotBeNull();
            notApplicableDiv.TextContent.Should().Contain("This exercise uses bodyweight only - no external weight can be added");
        }

        [Fact]
        public void Component_HidesWeightInput_ForNoWeight()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
            _stateServiceMock.Setup(x => x.RequiresWeightInput("NO_WEIGHT")).Returns(false);

            // Act
            var component = RenderComponent<WeightInputField>(parameters => parameters
                .Add(p => p.WeightTypeId, "22222222-2222-2222-2222-222222222222"));

            // Assert
            component.FindAll("[data-testid=\"weight-input\"]").Should().BeEmpty();
            var notApplicableDiv = component.Find("[data-testid=\"weight-not-applicable\"]");
            notApplicableDiv.TextContent.Should().Contain("This exercise does not use weight as a metric");
        }

        [Fact]
        public void Component_ShowsWeightInput_ForBodyweightOptional()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
            _stateServiceMock.Setup(x => x.RequiresWeightInput("BODYWEIGHT_OPTIONAL")).Returns(true);

            // Act
            var component = RenderComponent<WeightInputField>(parameters => parameters
                .Add(p => p.WeightTypeId, "33333333-3333-3333-3333-333333333333"));

            // Assert
            var input = component.Find("[data-testid=\"weight-input\"]");
            input.Should().NotBeNull();
            input.GetAttribute("placeholder").Should().Contain("Optional");
            
            // Should not show required indicator
            component.FindAll(".text-red-500").Should().BeEmpty();
            
            // Check help text
            var helpText = component.Find("#weight-help");
            helpText.TextContent.Should().Contain("You can add weight to make this exercise harder");
        }

        [Fact]
        public void Component_ShowsRequiredIndicator_ForWeightRequired()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
            _stateServiceMock.Setup(x => x.RequiresWeightInput("WEIGHT_REQUIRED")).Returns(true);

            // Act
            var component = RenderComponent<WeightInputField>(parameters => parameters
                .Add(p => p.WeightTypeId, "44444444-4444-4444-4444-444444444444"));

            // Assert
            var requiredIndicator = component.Find(".text-red-500");
            requiredIndicator.Should().NotBeNull();
            requiredIndicator.TextContent.Should().Be("*");
            
            var input = component.Find("[data-testid=\"weight-input\"]");
            input.GetAttribute("placeholder").Should().Contain("Required");
        }

        [Fact]
        public void Component_ShowsRequiredIndicator_ForMachineWeight()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
            _stateServiceMock.Setup(x => x.RequiresWeightInput("MACHINE_WEIGHT")).Returns(true);

            // Act
            var component = RenderComponent<WeightInputField>(parameters => parameters
                .Add(p => p.WeightTypeId, "55555555-5555-5555-5555-555555555555"));

            // Assert
            var requiredIndicator = component.Find(".text-red-500");
            requiredIndicator.Should().NotBeNull();
            
            var input = component.Find("[data-testid=\"weight-input\"]");
            input.GetAttribute("placeholder").Should().Contain("Stack weight");
            
            var helpText = component.Find("#weight-help");
            helpText.TextContent.Should().Contain("Enter the weight setting on the machine's stack");
        }

        [Fact]
        public void Component_ShowsWeightInput_WhenNoWeightTypeSelected()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);

            // Act
            var component = RenderComponent<WeightInputField>(parameters => parameters
                .Add(p => p.WeightTypeId, string.Empty));

            // Assert
            var input = component.Find("[data-testid=\"weight-input\"]");
            input.Should().NotBeNull();
            component.FindAll("[data-testid=\"weight-not-applicable\"]").Should().BeEmpty();
        }

        [Fact]
        public void Component_ConvertsUnits_FromKgToLbs()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
            _stateServiceMock.Setup(x => x.RequiresWeightInput("WEIGHT_REQUIRED")).Returns(true);
            
            decimal? capturedWeight = null;
            bool? capturedUseMetric = null;
            
            var component = RenderComponent<WeightInputField>(parameters => parameters
                .Add(p => p.WeightTypeId, "44444444-4444-4444-4444-444444444444")
                .Add(p => p.Weight, 100m)
                .Add(p => p.UseMetricUnits, true)
                .Add(p => p.WeightChanged, EventCallback.Factory.Create<decimal?>(this, value => capturedWeight = value))
                .Add(p => p.UseMetricUnitsChanged, EventCallback.Factory.Create<bool>(this, value => capturedUseMetric = value)));

            // Act
            var toggleButton = component.Find("[data-testid=\"unit-toggle\"]");
            toggleButton.Click();

            // Assert
            capturedUseMetric.Should().Be(false);
            capturedWeight.Should().BeApproximately(220m, 1m); // 100kg = ~220lbs
            // Button text doesn't change because the parent controls UseMetricUnits
        }

        [Fact]
        public void Component_ConvertsUnits_FromLbsToKg()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
            _stateServiceMock.Setup(x => x.RequiresWeightInput("WEIGHT_REQUIRED")).Returns(true);
            
            decimal? capturedWeight = null;
            bool? capturedUseMetric = null;
            
            var component = RenderComponent<WeightInputField>(parameters => parameters
                .Add(p => p.WeightTypeId, "44444444-4444-4444-4444-444444444444")
                .Add(p => p.Weight, 220m)
                .Add(p => p.UseMetricUnits, false)
                .Add(p => p.WeightChanged, EventCallback.Factory.Create<decimal?>(this, value => capturedWeight = value))
                .Add(p => p.UseMetricUnitsChanged, EventCallback.Factory.Create<bool>(this, value => capturedUseMetric = value)));

            // Act
            var toggleButton = component.Find("[data-testid=\"unit-toggle\"]");
            toggleButton.Click();

            // Assert
            capturedUseMetric.Should().Be(true);
            capturedWeight.Should().BeApproximately(100m, 0.5m); // 220lbs = ~100kg (with rounding)
            // Button text doesn't change because the parent controls UseMetricUnits
        }

        [Fact]
        public void Component_ShowsConversion_WhenShowConversionIsTrue()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
            _stateServiceMock.Setup(x => x.RequiresWeightInput("WEIGHT_REQUIRED")).Returns(true);

            // Act
            var component = RenderComponent<WeightInputField>(parameters => parameters
                .Add(p => p.WeightTypeId, "44444444-4444-4444-4444-444444444444")
                .Add(p => p.Weight, 50m)
                .Add(p => p.UseMetricUnits, true)
                .Add(p => p.ShowConversion, true));

            // Assert
            var conversionText = component.Find(".text-blue-600");
            conversionText.TextContent.Should().Contain("≈");
            conversionText.TextContent.Should().Contain("110 lbs"); // 50kg ≈ 110lbs
        }

        [Fact]
        public void Component_HidesConversion_WhenShowConversionIsFalse()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
            _stateServiceMock.Setup(x => x.RequiresWeightInput("WEIGHT_REQUIRED")).Returns(true);

            // Act
            var component = RenderComponent<WeightInputField>(parameters => parameters
                .Add(p => p.WeightTypeId, "44444444-4444-4444-4444-444444444444")
                .Add(p => p.Weight, 50m)
                .Add(p => p.ShowConversion, false));

            // Assert
            component.FindAll(".text-blue-600").Where(e => e.TextContent.Contains("≈")).Should().BeEmpty();
        }

        [Fact]
        public void Component_ShowsValidationError()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
            _stateServiceMock.Setup(x => x.RequiresWeightInput("WEIGHT_REQUIRED")).Returns(true);

            // Act
            var component = RenderComponent<WeightInputField>(parameters => parameters
                .Add(p => p.WeightTypeId, "44444444-4444-4444-4444-444444444444")
                .Add(p => p.ValidationMessage, "Weight is required for this exercise"));

            // Assert
            var errorMessage = component.Find("[data-testid=\"weight-validation-error\"]");
            errorMessage.TextContent.Should().Be("Weight is required for this exercise");
            
            var input = component.Find("[data-testid=\"weight-input\"]");
            (input.GetAttribute("class") ?? "").Should().Contain("border-red-300");
        }

        [Fact]
        public void Component_DisablesInput_WhenDisabled()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
            _stateServiceMock.Setup(x => x.RequiresWeightInput("WEIGHT_REQUIRED")).Returns(true);

            // Act
            var component = RenderComponent<WeightInputField>(parameters => parameters
                .Add(p => p.WeightTypeId, "44444444-4444-4444-4444-444444444444")
                .Add(p => p.Disabled, true));

            // Assert
            var input = component.Find("[data-testid=\"weight-input\"]");
            input.GetAttribute("disabled").Should().NotBeNull();
            (input.GetAttribute("class") ?? "").Should().Contain("bg-gray-100");
            (input.GetAttribute("class") ?? "").Should().Contain("cursor-not-allowed");
            
            var toggleButton = component.Find("[data-testid=\"unit-toggle\"]");
            toggleButton.GetAttribute("disabled").Should().NotBeNull();
        }

        [Fact]
        public void Component_UpdatesWeight_OnInputChange()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
            _stateServiceMock.Setup(x => x.RequiresWeightInput("WEIGHT_REQUIRED")).Returns(true);
            
            decimal? capturedWeight = null;
            var component = RenderComponent<WeightInputField>(parameters => parameters
                .Add(p => p.WeightTypeId, "44444444-4444-4444-4444-444444444444")
                .Add(p => p.WeightChanged, EventCallback.Factory.Create<decimal?>(this, value => capturedWeight = value)));

            // Act
            var input = component.Find("[data-testid=\"weight-input\"]");
            input.Change("75.5");

            // Assert
            capturedWeight.Should().Be(75.5m);
        }

        [Fact]
        public void Component_SetsProperStepValue_ForMetricUnits()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
            _stateServiceMock.Setup(x => x.RequiresWeightInput("WEIGHT_REQUIRED")).Returns(true);

            // Act
            var component = RenderComponent<WeightInputField>(parameters => parameters
                .Add(p => p.WeightTypeId, "44444444-4444-4444-4444-444444444444")
                .Add(p => p.UseMetricUnits, true));

            // Assert
            var input = component.Find("[data-testid=\"weight-input\"]");
            input.GetAttribute("step").Should().Be("0.5");
        }

        [Fact]
        public void Component_SetsProperStepValue_ForImperialUnits()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
            _stateServiceMock.Setup(x => x.RequiresWeightInput("WEIGHT_REQUIRED")).Returns(true);

            // Act
            var component = RenderComponent<WeightInputField>(parameters => parameters
                .Add(p => p.WeightTypeId, "44444444-4444-4444-4444-444444444444")
                .Add(p => p.UseMetricUnits, false));

            // Assert
            var input = component.Find("[data-testid=\"weight-input\"]");
            input.GetAttribute("step").Should().Be("1");
        }

        [Fact]
        public void Component_HandlesNullWeightValue()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
            _stateServiceMock.Setup(x => x.RequiresWeightInput("BODYWEIGHT_OPTIONAL")).Returns(true);

            // Act
            var component = RenderComponent<WeightInputField>(parameters => parameters
                .Add(p => p.WeightTypeId, "33333333-3333-3333-3333-333333333333")
                .Add(p => p.Weight, null));

            // Assert
            var input = component.Find("[data-testid=\"weight-input\"]");
            // Blazor doesn't set value attribute for empty number inputs
            (input.GetAttribute("value") ?? "").Should().BeEmpty();
            component.FindAll(".text-blue-600").Where(e => e.TextContent.Contains("≈")).Should().BeEmpty();
        }

        [Fact]
        public void Component_HandlesZeroWeight_ForConversion()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
            _stateServiceMock.Setup(x => x.RequiresWeightInput("BODYWEIGHT_OPTIONAL")).Returns(true);

            // Act
            var component = RenderComponent<WeightInputField>(parameters => parameters
                .Add(p => p.WeightTypeId, "33333333-3333-3333-3333-333333333333")
                .Add(p => p.Weight, 0m)
                .Add(p => p.ShowConversion, true));

            // Assert
            component.FindAll(".text-blue-600").Where(e => e.TextContent.Contains("≈")).Should().BeEmpty();
        }

        [Fact]
        public void Component_ShowsCorrectPlaceholder_ForEachWeightType()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
            _stateServiceMock.Setup(x => x.RequiresWeightInput(It.IsAny<string>())).Returns(true);

            var testCases = new[]
            {
                ("33333333-3333-3333-3333-333333333333", true, "Optional (kg)"),
                ("33333333-3333-3333-3333-333333333333", false, "Optional (lbs)"),
                ("44444444-4444-4444-4444-444444444444", true, "Required (kg)"),
                ("44444444-4444-4444-4444-444444444444", false, "Required (lbs)"),
                ("55555555-5555-5555-5555-555555555555", true, "Stack weight (kg)"),
                ("55555555-5555-5555-5555-555555555555", false, "Stack weight (lbs)")
            };

            foreach (var (weightTypeId, useMetric, expectedPlaceholder) in testCases)
            {
                // Act
                var component = RenderComponent<WeightInputField>(parameters => parameters
                    .Add(p => p.WeightTypeId, weightTypeId)
                    .Add(p => p.UseMetricUnits, useMetric));

                // Assert
                var input = component.Find("[data-testid=\"weight-input\"]");
                input.GetAttribute("placeholder").Should().Be(expectedPlaceholder);
            }
        }

        [Fact]
        public void Component_PreservesWeightValue_WhenToggleUnitsIsDisabled()
        {
            // Arrange
            _stateServiceMock.Setup(x => x.WeightTypes).Returns(_testWeightTypes);
            _stateServiceMock.Setup(x => x.RequiresWeightInput("WEIGHT_REQUIRED")).Returns(true);
            
            decimal? capturedWeight = null;
            bool? capturedUseMetric = null;
            
            var component = RenderComponent<WeightInputField>(parameters => parameters
                .Add(p => p.WeightTypeId, "44444444-4444-4444-4444-444444444444")
                .Add(p => p.Weight, 100m)
                .Add(p => p.Disabled, true)
                .Add(p => p.WeightChanged, EventCallback.Factory.Create<decimal?>(this, value => capturedWeight = value))
                .Add(p => p.UseMetricUnitsChanged, EventCallback.Factory.Create<bool>(this, value => capturedUseMetric = value)));

            // Act
            var toggleButton = component.Find("[data-testid=\"unit-toggle\"]");
            toggleButton.Click(); // Should not trigger any changes

            // Assert
            capturedWeight.Should().BeNull(); // No change event fired
            capturedUseMetric.Should().BeNull(); // No change event fired
        }
    }
}
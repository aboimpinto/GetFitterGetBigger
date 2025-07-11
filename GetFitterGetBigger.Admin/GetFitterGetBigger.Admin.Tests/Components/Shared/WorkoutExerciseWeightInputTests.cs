using Bunit;
using Microsoft.Extensions.DependencyInjection;
using GetFitterGetBigger.Admin.Components.Shared;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Builders;
using Moq;
using FluentAssertions;
using AngleSharp.Dom;

namespace GetFitterGetBigger.Admin.Tests.Components.Shared
{
    public class WorkoutExerciseWeightInputTests : TestContext
    {
        private readonly Mock<IExerciseWeightTypeStateService> _weightTypeStateServiceMock;

        public WorkoutExerciseWeightInputTests()
        {
            _weightTypeStateServiceMock = new Mock<IExerciseWeightTypeStateService>();
            Services.AddSingleton(_weightTypeStateServiceMock.Object);
        }

        [Fact]
        public void WorkoutExerciseWeightInput_WithNoExercise_DisplaysNoExerciseMessage()
        {
            var component = RenderComponent<WorkoutExerciseWeightInput>();

            var message = component.Find("[data-testid='no-exercise-message']");
            message.TextContent.Should().Contain("Select an exercise to configure weight");
        }

        [Fact]
        public void WorkoutExerciseWeightInput_WithExerciseWithoutWeightType_DisplaysNoWeightTypeMessage()
        {
            var exercise = new ExerciseDtoBuilder()
                .WithName("Test Exercise")
                .WithWeightType((ExerciseWeightTypeDto?)null)
                .Build();

            var component = RenderComponent<WorkoutExerciseWeightInput>(parameters => parameters
                .Add(p => p.Exercise, exercise));

            var message = component.Find("[data-testid='no-exercise-message']");
            message.TextContent.Should().Contain("Exercise weight type not available");
        }

        [Fact]
        public void WorkoutExerciseWeightInput_WithBodyweightOnlyExercise_HidesWeightInput()
        {
            var weightType = new ExerciseWeightTypeDtoBuilder()
                .WithCode("BODYWEIGHT_ONLY")
                .WithName("Bodyweight Only")
                .Build();

            var exercise = new ExerciseDtoBuilder()
                .WithName("Push-ups")
                .WithWeightType(weightType)
                .Build();

            var component = RenderComponent<WorkoutExerciseWeightInput>(parameters => parameters
                .Add(p => p.Exercise, exercise));

            component.FindAll("[data-testid='weight-input-field']").Should().BeEmpty();
            
            var noWeightMessage = component.Find("[data-testid='no-weight-message']");
            noWeightMessage.TextContent.Should().Contain("Uses bodyweight only - no additional weight needed");
        }

        [Fact]
        public void WorkoutExerciseWeightInput_WithNoWeightExercise_HidesWeightInput()
        {
            var weightType = new ExerciseWeightTypeDtoBuilder()
                .WithCode("NO_WEIGHT")
                .WithName("No Weight")
                .Build();

            var exercise = new ExerciseDtoBuilder()
                .WithName("Stretching")
                .WithWeightType(weightType)
                .Build();

            var component = RenderComponent<WorkoutExerciseWeightInput>(parameters => parameters
                .Add(p => p.Exercise, exercise));

            component.FindAll("[data-testid='weight-input-field']").Should().BeEmpty();
            
            var noWeightMessage = component.Find("[data-testid='no-weight-message']");
            noWeightMessage.TextContent.Should().Contain("No weight required for this exercise");
        }

        [Fact]
        public void WorkoutExerciseWeightInput_WithBodyweightOptionalExercise_ShowsOptionalWeightInput()
        {
            var weightType = new ExerciseWeightTypeDtoBuilder()
                .WithCode("BODYWEIGHT_OPTIONAL")
                .WithName("Bodyweight Optional")
                .Build();

            var exercise = new ExerciseDtoBuilder()
                .WithName("Pull-ups")
                .WithWeightType(weightType)
                .Build();

            var component = RenderComponent<WorkoutExerciseWeightInput>(parameters => parameters
                .Add(p => p.Exercise, exercise));

            var weightInput = component.Find("[data-testid='weight-input-field']");
            weightInput.Should().NotBeNull();
            weightInput.GetAttribute("placeholder").Should().Contain("Enter additional weight (optional)");
            
            // Should not show required asterisk
            component.FindAll("span.text-red-500").Should().BeEmpty();
        }

        [Fact]
        public void WorkoutExerciseWeightInput_WithWeightRequiredExercise_ShowsRequiredWeightInput()
        {
            var weightType = new ExerciseWeightTypeDtoBuilder()
                .WithCode("WEIGHT_REQUIRED")
                .WithName("Weight Required")
                .Build();

            var exercise = new ExerciseDtoBuilder()
                .WithName("Barbell Squat")
                .WithWeightType(weightType)
                .Build();

            var component = RenderComponent<WorkoutExerciseWeightInput>(parameters => parameters
                .Add(p => p.Exercise, exercise));

            var weightInput = component.Find("[data-testid='weight-input-field']");
            weightInput.Should().NotBeNull();
            weightInput.GetAttribute("placeholder").Should().Contain("Enter weight amount");
            
            // Should show required asterisk
            var requiredIndicator = component.Find("span.text-red-500");
            requiredIndicator.TextContent.Should().Contain("*");
        }

        [Fact]
        public void WorkoutExerciseWeightInput_WithMachineWeightExercise_ShowsRequiredMachineWeightInput()
        {
            var weightType = new ExerciseWeightTypeDtoBuilder()
                .WithCode("MACHINE_WEIGHT")
                .WithName("Machine Weight")
                .Build();

            var exercise = new ExerciseDtoBuilder()
                .WithName("Leg Press")
                .WithWeightType(weightType)
                .Build();

            var component = RenderComponent<WorkoutExerciseWeightInput>(parameters => parameters
                .Add(p => p.Exercise, exercise));

            var weightInput = component.Find("[data-testid='weight-input-field']");
            weightInput.Should().NotBeNull();
            weightInput.GetAttribute("placeholder").Should().Contain("Enter machine weight/setting");
            
            // Should show required asterisk
            var requiredIndicator = component.Find("span.text-red-500");
            requiredIndicator.TextContent.Should().Contain("*");
        }

        [Fact]
        public void WorkoutExerciseWeightInput_WhenDisabled_DisablesAllInputs()
        {
            var weightType = new ExerciseWeightTypeDtoBuilder()
                .WithCode("WEIGHT_REQUIRED")
                .WithName("Weight Required")
                .Build();

            var exercise = new ExerciseDtoBuilder()
                .WithName("Barbell Squat")
                .WithWeightType(weightType)
                .Build();

            var component = RenderComponent<WorkoutExerciseWeightInput>(parameters => parameters
                .Add(p => p.Exercise, exercise)
                .Add(p => p.Disabled, true));

            var weightInput = component.Find("[data-testid='weight-input-field']");
            weightInput.HasAttribute("disabled").Should().BeTrue();
            
            // Unit toggle should be hidden when disabled
            component.FindAll("[data-testid='weight-unit-select']").Should().BeEmpty();
        }

        [Fact]
        public void WorkoutExerciseWeightInput_ShowsHelpIconWithTooltip()
        {
            var weightType = new ExerciseWeightTypeDtoBuilder()
                .WithCode("WEIGHT_REQUIRED")
                .WithName("Weight Required")
                .Build();

            var exercise = new ExerciseDtoBuilder()
                .WithName("Barbell Squat")
                .WithWeightType(weightType)
                .Build();

            var component = RenderComponent<WorkoutExerciseWeightInput>(parameters => parameters
                .Add(p => p.Exercise, exercise));

            var helpIcon = component.Find("[data-testid='weight-help-icon']");
            helpIcon.Should().NotBeNull();
            
            var tooltip = component.Find("[data-testid='weight-help-tooltip']");
            tooltip.TextContent.Should().Contain("Enter the weight you'll be lifting");
        }

        [Fact]
        public void WorkoutExerciseWeightInput_ShowsContextualHelpForBodyweightOptional()
        {
            var weightType = new ExerciseWeightTypeDtoBuilder()
                .WithCode("BODYWEIGHT_OPTIONAL")
                .WithName("Bodyweight Optional")
                .Build();

            var exercise = new ExerciseDtoBuilder()
                .WithName("Pull-ups")
                .WithWeightType(weightType)
                .Build();

            var component = RenderComponent<WorkoutExerciseWeightInput>(parameters => parameters
                .Add(p => p.Exercise, exercise));

            var contextualHelp = component.Find("[data-testid='contextual-help']");
            contextualHelp.TextContent.Should().Contain("This exercise can be performed with just bodyweight or with additional weight");
        }

        [Fact]
        public void WorkoutExerciseWeightInput_HidesContextualHelpWhenDisabled()
        {
            var weightType = new ExerciseWeightTypeDtoBuilder()
                .WithCode("BODYWEIGHT_OPTIONAL")
                .WithName("Bodyweight Optional")
                .Build();

            var exercise = new ExerciseDtoBuilder()
                .WithName("Pull-ups")
                .WithWeightType(weightType)
                .Build();

            var component = RenderComponent<WorkoutExerciseWeightInput>(parameters => parameters
                .Add(p => p.Exercise, exercise)
                .Add(p => p.ShowContextualHelp, false));

            component.FindAll("[data-testid='contextual-help']").Should().BeEmpty();
        }

        [Fact]
        public void WorkoutExerciseWeightInput_WithUnitToggle_ShowsUnitSelector()
        {
            var weightType = new ExerciseWeightTypeDtoBuilder()
                .WithCode("WEIGHT_REQUIRED")
                .WithName("Weight Required")
                .Build();

            var exercise = new ExerciseDtoBuilder()
                .WithName("Barbell Squat")
                .WithWeightType(weightType)
                .Build();

            var component = RenderComponent<WorkoutExerciseWeightInput>(parameters => parameters
                .Add(p => p.Exercise, exercise)
                .Add(p => p.ShowUnitToggle, true));

            var unitSelect = component.Find("[data-testid='weight-unit-select']");
            unitSelect.Should().NotBeNull();
            
            var options = unitSelect.QuerySelectorAll("option");
            options.Should().HaveCount(2);
            options[0].TextContent.Should().Be("kg");
            options[1].TextContent.Should().Be("lbs");
        }

        [Fact]
        public void WorkoutExerciseWeightInput_WithoutUnitToggle_ShowsStaticUnit()
        {
            var weightType = new ExerciseWeightTypeDtoBuilder()
                .WithCode("WEIGHT_REQUIRED")
                .WithName("Weight Required")
                .Build();

            var exercise = new ExerciseDtoBuilder()
                .WithName("Barbell Squat")
                .WithWeightType(weightType)
                .Build();

            var component = RenderComponent<WorkoutExerciseWeightInput>(parameters => parameters
                .Add(p => p.Exercise, exercise)
                .Add(p => p.ShowUnitToggle, false)
                .Add(p => p.WeightUnit, "kg"));

            component.FindAll("[data-testid='weight-unit-select']").Should().BeEmpty();
            component.Markup.Should().Contain("kg");
        }

        [Fact]
        public void WorkoutExerciseWeightInput_ValidatesRequiredWeight()
        {
            var weightType = new ExerciseWeightTypeDtoBuilder()
                .WithCode("WEIGHT_REQUIRED")
                .WithName("Weight Required")
                .Build();

            var exercise = new ExerciseDtoBuilder()
                .WithName("Barbell Squat")
                .WithWeightType(weightType)
                .Build();

            var component = RenderComponent<WorkoutExerciseWeightInput>(parameters => parameters
                .Add(p => p.Exercise, exercise)
                .Add(p => p.WeightValue, null));

            var weightInput = component.Find("[data-testid='weight-input-field']");
            weightInput.Blur();

            var validationError = component.Find("[data-testid='weight-validation-error']");
            validationError.TextContent.Should().Contain("Weight is required for this exercise");
        }

        [Fact]
        public void WorkoutExerciseWeightInput_ValidatesMinimumWeight()
        {
            var weightType = new ExerciseWeightTypeDtoBuilder()
                .WithCode("WEIGHT_REQUIRED")
                .WithName("Weight Required")
                .Build();

            var exercise = new ExerciseDtoBuilder()
                .WithName("Barbell Squat")
                .WithWeightType(weightType)
                .Build();

            var component = RenderComponent<WorkoutExerciseWeightInput>(parameters => parameters
                .Add(p => p.Exercise, exercise)
                .Add(p => p.WeightValue, 0.05m)
                .Add(p => p.WeightUnit, "kg"));

            var weightInput = component.Find("[data-testid='weight-input-field']");
            weightInput.Blur();

            var validationError = component.Find("[data-testid='weight-validation-error']");
            validationError.TextContent.Should().Contain("Weight must be at least 0.1 kg");
        }

        [Fact]
        public void WorkoutExerciseWeightInput_ValidatesMaximumWeight()
        {
            var weightType = new ExerciseWeightTypeDtoBuilder()
                .WithCode("WEIGHT_REQUIRED")
                .WithName("Weight Required")
                .Build();

            var exercise = new ExerciseDtoBuilder()
                .WithName("Barbell Squat")
                .WithWeightType(weightType)
                .Build();

            var component = RenderComponent<WorkoutExerciseWeightInput>(parameters => parameters
                .Add(p => p.Exercise, exercise)
                .Add(p => p.WeightValue, 1500m)
                .Add(p => p.WeightUnit, "kg"));

            var weightInput = component.Find("[data-testid='weight-input-field']");
            weightInput.Blur();

            var validationError = component.Find("[data-testid='weight-validation-error']");
            validationError.TextContent.Should().Contain("Weight cannot exceed 1000 kg");
        }

        [Fact]
        public void WorkoutExerciseWeightInput_ValidatesMachineWeightLimits()
        {
            var weightType = new ExerciseWeightTypeDtoBuilder()
                .WithCode("MACHINE_WEIGHT")
                .WithName("Machine Weight")
                .Build();

            var exercise = new ExerciseDtoBuilder()
                .WithName("Leg Press")
                .WithWeightType(weightType)
                .Build();

            var component = RenderComponent<WorkoutExerciseWeightInput>(parameters => parameters
                .Add(p => p.Exercise, exercise)
                .Add(p => p.WeightValue, 600m)
                .Add(p => p.WeightUnit, "kg"));

            var weightInput = component.Find("[data-testid='weight-input-field']");
            weightInput.Blur();

            var validationError = component.Find("[data-testid='weight-validation-error']");
            validationError.TextContent.Should().Contain("Weight cannot exceed 500 kg");
        }

        [Fact]
        public void WorkoutExerciseWeightInput_AllowsOptionalWeightToBeEmpty()
        {
            var weightType = new ExerciseWeightTypeDtoBuilder()
                .WithCode("BODYWEIGHT_OPTIONAL")
                .WithName("Bodyweight Optional")
                .Build();

            var exercise = new ExerciseDtoBuilder()
                .WithName("Pull-ups")
                .WithWeightType(weightType)
                .Build();

            var component = RenderComponent<WorkoutExerciseWeightInput>(parameters => parameters
                .Add(p => p.Exercise, exercise)
                .Add(p => p.WeightValue, null));

            var weightInput = component.Find("[data-testid='weight-input-field']");
            weightInput.Blur();

            // Should not show validation error for empty optional weight
            component.FindAll("[data-testid='weight-validation-error']").Should().BeEmpty();
        }

        [Fact]
        public void WorkoutExerciseWeightInput_ClearsValidationWhenWeightBecomesValid()
        {
            var weightType = new ExerciseWeightTypeDtoBuilder()
                .WithCode("WEIGHT_REQUIRED")
                .WithName("Weight Required")
                .Build();

            var exercise = new ExerciseDtoBuilder()
                .WithName("Barbell Squat")
                .WithWeightType(weightType)
                .Build();

            var component = RenderComponent<WorkoutExerciseWeightInput>(parameters => parameters
                .Add(p => p.Exercise, exercise)
                .Add(p => p.WeightValue, null));

            var weightInput = component.Find("[data-testid='weight-input-field']");
            
            // First trigger validation error
            weightInput.Blur();
            component.FindAll("[data-testid='weight-validation-error']").Should().HaveCount(1);

            // Then provide valid weight
            component.SetParametersAndRender(parameters => parameters
                .Add(p => p.WeightValue, 50m));
            
            weightInput.Blur();
            component.FindAll("[data-testid='weight-validation-error']").Should().BeEmpty();
        }
    }
}
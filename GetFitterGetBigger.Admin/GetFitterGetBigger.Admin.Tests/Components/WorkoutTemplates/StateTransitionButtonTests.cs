using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.WorkoutTemplates;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Builders;
using Microsoft.AspNetCore.Components;
using AngleSharp.Dom;

namespace GetFitterGetBigger.Admin.Tests.Components.WorkoutTemplates
{
    public class StateTransitionButtonTests : TestContext
    {
        private WorkoutTemplateDto CreateTestTemplate(string stateValue)
        {
            return new WorkoutTemplateDtoBuilder()
                .WithId("template-1")
                .WithName("Test Template")
                .WithWorkoutState($"state-{stateValue.ToLower()}", stateValue)
                .Build();
        }

        private ReferenceDataDto CreateTargetState(string stateValue)
        {
            return new ReferenceDataDto 
            { 
                Id = $"state-{stateValue.ToLower()}", 
                Value = stateValue, 
                Description = $"{stateValue} state" 
            };
        }

        [Fact]
        public void Component_RendersButton_WhenTransitionIsValid()
        {
            // Arrange
            var template = CreateTestTemplate("DRAFT");
            var targetState = CreateTargetState("PRODUCTION");

            // Act
            var component = RenderComponent<StateTransitionButton>(parameters => parameters
                .Add(p => p.WorkoutTemplate, template)
                .Add(p => p.TargetState, targetState)
                .Add(p => p.ButtonText, "Move to Production"));

            // Assert
            var button = component.Find("[data-testid=\"state-transition-button\"]");
            button.Should().NotBeNull();
            button.TextContent.Should().Contain("Move to Production");
        }

        [Fact]
        public void Component_DoesNotRender_WhenTransitionIsInvalid()
        {
            // Arrange - Invalid transition: PRODUCTION to DRAFT
            var template = CreateTestTemplate("PRODUCTION");
            var targetState = CreateTargetState("DRAFT");

            // Act
            var component = RenderComponent<StateTransitionButton>(parameters => parameters
                .Add(p => p.WorkoutTemplate, template)
                .Add(p => p.TargetState, targetState));

            // Assert
            component.FindAll("[data-testid=\"state-transition-button\"]").Should().BeEmpty();
        }

        [Theory]
        [InlineData("DRAFT", "PRODUCTION", true)]
        [InlineData("DRAFT", "ARCHIVED", true)]
        [InlineData("PRODUCTION", "ARCHIVED", true)]
        [InlineData("ARCHIVED", "DRAFT", true)]
        [InlineData("PRODUCTION", "DRAFT", false)]
        [InlineData("ARCHIVED", "PRODUCTION", false)]
        [InlineData("DRAFT", "DRAFT", false)]
        public void CanTransition_ReturnsCorrectValue_ForStateTransitions(string currentState, string targetState, bool expectedResult)
        {
            // Arrange
            var template = CreateTestTemplate(currentState);
            var target = CreateTargetState(targetState);
            var component = RenderComponent<StateTransitionButton>(parameters => parameters
                .Add(p => p.WorkoutTemplate, template)
                .Add(p => p.TargetState, target));

            // Act
            var canTransition = component.Instance.CanTransition();

            // Assert
            canTransition.Should().Be(expectedResult);
        }

        [Fact]
        public void Component_ShowsConfirmationDialog_WhenButtonClicked()
        {
            // Arrange
            var template = CreateTestTemplate("DRAFT");
            var targetState = CreateTargetState("PRODUCTION");
            var component = RenderComponent<StateTransitionButton>(parameters => parameters
                .Add(p => p.WorkoutTemplate, template)
                .Add(p => p.TargetState, targetState)
                .Add(p => p.RequireConfirmation, true));

            // Act
            component.Find("[data-testid=\"state-transition-button\"]").Click();

            // Assert
            var dialog = component.Find("[data-testid=\"confirm-dialog-backdrop\"]");
            dialog.Should().NotBeNull();
            var message = component.Find("[data-testid=\"confirm-dialog-message\"]");
            message.TextContent.Should().Contain("Are you sure you want to move this template to PRODUCTION?");
        }

        [Fact]
        public void Component_DoesNotShowDialog_WhenRequireConfirmationIsFalse()
        {
            // Arrange
            var stateChanged = false;
            var template = CreateTestTemplate("DRAFT");
            var targetState = CreateTargetState("PRODUCTION");
            var component = RenderComponent<StateTransitionButton>(parameters => parameters
                .Add(p => p.WorkoutTemplate, template)
                .Add(p => p.TargetState, targetState)
                .Add(p => p.RequireConfirmation, false)
                .Add(p => p.OnStateChanged, EventCallback.Factory.Create<ReferenceDataDto>(this, (state) => stateChanged = true)));

            // Act
            component.Find("[data-testid=\"state-transition-button\"]").Click();

            // Assert
            component.FindAll("[data-testid=\"confirm-dialog-backdrop\"]").Should().BeEmpty();
            stateChanged.Should().BeTrue();
        }

        [Fact]
        public void ConfirmButton_InvokesCallback_WhenClicked()
        {
            // Arrange
            ReferenceDataDto? receivedState = null;
            var template = CreateTestTemplate("DRAFT");
            var targetState = CreateTargetState("PRODUCTION");
            var component = RenderComponent<StateTransitionButton>(parameters => parameters
                .Add(p => p.WorkoutTemplate, template)
                .Add(p => p.TargetState, targetState)
                .Add(p => p.OnStateChanged, EventCallback.Factory.Create<ReferenceDataDto>(this, (state) => receivedState = state)));

            // Act
            component.Find("[data-testid=\"state-transition-button\"]").Click();
            component.Find("[data-testid=\"confirm-button\"]").Click();

            // Assert
            receivedState.Should().NotBeNull();
            receivedState!.Value.Should().Be("PRODUCTION");
            component.FindAll("[data-testid=\"confirm-dialog-backdrop\"]").Should().BeEmpty();
        }

        [Fact]
        public void CancelButton_ClosesDialog_WithoutInvokingCallback()
        {
            // Arrange
            var callbackInvoked = false;
            var template = CreateTestTemplate("DRAFT");
            var targetState = CreateTargetState("PRODUCTION");
            var component = RenderComponent<StateTransitionButton>(parameters => parameters
                .Add(p => p.WorkoutTemplate, template)
                .Add(p => p.TargetState, targetState)
                .Add(p => p.OnStateChanged, EventCallback.Factory.Create<ReferenceDataDto>(this, (_) => callbackInvoked = true)));

            // Act
            component.Find("[data-testid=\"state-transition-button\"]").Click();
            component.Find("[data-testid=\"cancel-button\"]").Click();

            // Assert
            component.FindAll("[data-testid=\"confirm-dialog-backdrop\"]").Should().BeEmpty();
            callbackInvoked.Should().BeFalse();
        }

        [Fact]
        public void Component_ShowsCorrectIcon_ForEachTargetState()
        {
            // Arrange
            var testCases = new[]
            {
                ("PRODUCTION", "🚀"),
                ("ARCHIVED", "📦"),
                ("DRAFT", "📝")
            };

            var template = CreateTestTemplate("DRAFT");

            foreach (var (state, expectedIcon) in testCases)
            {
                // Act
                var targetState = CreateTargetState(state);
                var component = RenderComponent<StateTransitionButton>(parameters => parameters
                    .Add(p => p.WorkoutTemplate, template)
                    .Add(p => p.TargetState, targetState)
                    .Add(p => p.ShowIcon, true));

                // Assert
                if (component.FindAll("[data-testid=\"state-transition-button\"]").Any())
                {
                    var button = component.Find("[data-testid=\"state-transition-button\"]");
                    button.TextContent.Should().Contain(expectedIcon);
                }
            }
        }

        [Fact]
        public void Component_HidesIcon_WhenShowIconIsFalse()
        {
            // Arrange
            var template = CreateTestTemplate("DRAFT");
            var targetState = CreateTargetState("PRODUCTION");

            // Act
            var component = RenderComponent<StateTransitionButton>(parameters => parameters
                .Add(p => p.WorkoutTemplate, template)
                .Add(p => p.TargetState, targetState)
                .Add(p => p.ShowIcon, false));

            // Assert
            var button = component.Find("[data-testid=\"state-transition-button\"]");
            button.TextContent.Should().NotContain("🚀");
        }

        [Fact]
        public void Component_AppliesCorrectSizeClasses()
        {
            // Arrange
            var template = CreateTestTemplate("DRAFT");
            var targetState = CreateTargetState("PRODUCTION");

            // Test Small
            var smallComponent = RenderComponent<StateTransitionButton>(parameters => parameters
                .Add(p => p.WorkoutTemplate, template)
                .Add(p => p.TargetState, targetState)
                .Add(p => p.Size, StateTransitionButton.ButtonSize.Small));

            var smallButton = smallComponent.Find("[data-testid=\"state-transition-button\"]");
            var smallClasses = smallButton.GetAttribute("class") ?? "";
            smallClasses.Should().Contain("px-3");
            smallClasses.Should().Contain("py-1.5");

            // Test Medium (default)
            var mediumComponent = RenderComponent<StateTransitionButton>(parameters => parameters
                .Add(p => p.WorkoutTemplate, template)
                .Add(p => p.TargetState, targetState));

            var mediumButton = mediumComponent.Find("[data-testid=\"state-transition-button\"]");
            var mediumClasses = mediumButton.GetAttribute("class") ?? "";
            mediumClasses.Should().Contain("px-4");
            mediumClasses.Should().Contain("py-2");

            // Test Large
            var largeComponent = RenderComponent<StateTransitionButton>(parameters => parameters
                .Add(p => p.WorkoutTemplate, template)
                .Add(p => p.TargetState, targetState)
                .Add(p => p.Size, StateTransitionButton.ButtonSize.Large));

            var largeButton = largeComponent.Find("[data-testid=\"state-transition-button\"]");
            var largeClasses = largeButton.GetAttribute("class") ?? "";
            largeClasses.Should().Contain("px-5");
            largeClasses.Should().Contain("py-3");
        }

        [Fact]
        public void Component_AppliesCorrectVariantStyles()
        {
            // Arrange
            var template = CreateTestTemplate("DRAFT");
            var targetState = CreateTargetState("PRODUCTION");

            // Test Primary
            var primaryComponent = RenderComponent<StateTransitionButton>(parameters => parameters
                .Add(p => p.WorkoutTemplate, template)
                .Add(p => p.TargetState, targetState)
                .Add(p => p.Variant, StateTransitionButton.ButtonVariant.Primary));

            var primaryButton = primaryComponent.Find("[data-testid=\"state-transition-button\"]");
            var primaryClasses = primaryButton.GetAttribute("class") ?? "";
            primaryClasses.Should().Contain("bg-blue-600");

            // Test Danger
            var dangerComponent = RenderComponent<StateTransitionButton>(parameters => parameters
                .Add(p => p.WorkoutTemplate, template)
                .Add(p => p.TargetState, CreateTargetState("ARCHIVED"))
                .Add(p => p.Variant, StateTransitionButton.ButtonVariant.Danger));

            var dangerButton = dangerComponent.Find("[data-testid=\"state-transition-button\"]");
            var dangerClasses = dangerButton.GetAttribute("class") ?? "";
            dangerClasses.Should().Contain("bg-red-600");
        }

        [Fact]
        public void Component_DisablesButton_WhenIsDisabledIsTrue()
        {
            // Arrange
            var template = CreateTestTemplate("DRAFT");
            var targetState = CreateTargetState("PRODUCTION");

            // Act
            var component = RenderComponent<StateTransitionButton>(parameters => parameters
                .Add(p => p.WorkoutTemplate, template)
                .Add(p => p.TargetState, targetState)
                .Add(p => p.IsDisabled, true));

            // Assert
            var button = component.Find("[data-testid=\"state-transition-button\"]");
            button.GetAttribute("disabled").Should().NotBeNull();
            button.GetAttribute("title").Should().Be("State transition not allowed");
        }

        [Fact]
        public void Component_ShowsCorrectConfirmationMessage_ForEachTransition()
        {
            // Arrange
            var testCases = new[]
            {
                ("DRAFT", "PRODUCTION", "Are you sure you want to move this template to PRODUCTION? This will make it available for use and restrict editing."),
                ("PRODUCTION", "ARCHIVED", "Are you sure you want to archive this template? It will no longer be available for use."),
                ("ARCHIVED", "DRAFT", "Are you sure you want to move this template back to DRAFT? This will allow editing again.")
            };

            foreach (var (currentState, targetState, expectedMessage) in testCases)
            {
                var template = CreateTestTemplate(currentState);
                var target = CreateTargetState(targetState);
                var component = RenderComponent<StateTransitionButton>(parameters => parameters
                    .Add(p => p.WorkoutTemplate, template)
                    .Add(p => p.TargetState, target));

                if (component.FindAll("[data-testid=\"state-transition-button\"]").Any())
                {
                    // Act
                    component.Find("[data-testid=\"state-transition-button\"]").Click();

                    // Assert
                    var message = component.Find("[data-testid=\"confirm-dialog-message\"]");
                    message.TextContent.Should().Be(expectedMessage);
                    
                    // Clean up
                    component.Find("[data-testid=\"cancel-button\"]").Click();
                }
            }
        }

        [Fact]
        public void Component_ShowsLoadingSpinner_WhenTransitioning()
        {
            // Arrange
            var template = CreateTestTemplate("DRAFT");
            var targetState = CreateTargetState("PRODUCTION");
            var component = RenderComponent<StateTransitionButton>(parameters => parameters
                .Add(p => p.WorkoutTemplate, template)
                .Add(p => p.TargetState, targetState));

            // Act
            component.Find("[data-testid=\"state-transition-button\"]").Click();
            component.Instance.isTransitioning = true;
            component.Render();

            // Assert
            var spinner = component.Find("[data-testid=\"loading-spinner\"]");
            spinner.Should().NotBeNull();
            spinner.GetAttribute("class").Should().Contain("animate-spin");
        }

        [Fact]
        public void Component_HasProperAriaAttributes()
        {
            // Arrange
            var template = CreateTestTemplate("DRAFT");
            var targetState = CreateTargetState("PRODUCTION");

            // Act
            var component = RenderComponent<StateTransitionButton>(parameters => parameters
                .Add(p => p.WorkoutTemplate, template)
                .Add(p => p.TargetState, targetState));

            // Assert
            var button = component.Find("[data-testid=\"state-transition-button\"]");
            button.GetAttribute("aria-label").Should().Be("Change state to PRODUCTION");

            // Check dialog attributes
            component.Find("[data-testid=\"state-transition-button\"]").Click();
            var dialog = component.Find("[data-testid=\"confirm-dialog-backdrop\"]");
            dialog.GetAttribute("role").Should().Be("dialog");
            dialog.GetAttribute("aria-modal").Should().Be("true");
            dialog.GetAttribute("aria-labelledby").Should().Be("confirm-dialog-title");
        }

        [Fact]
        public void Component_HandlesCaseInsensitiveStates()
        {
            // Arrange
            var template = new WorkoutTemplateDtoBuilder()
                .WithWorkoutState("state-draft", "draft") // lowercase
                .Build();
            var targetState = new ReferenceDataDto { Id = "state-production", Value = "production", Description = "Production state" };

            // Act
            var component = RenderComponent<StateTransitionButton>(parameters => parameters
                .Add(p => p.WorkoutTemplate, template)
                .Add(p => p.TargetState, targetState));

            // Assert - Should still render because transition logic handles case-insensitive
            var button = component.Find("[data-testid=\"state-transition-button\"]");
            button.Should().NotBeNull();
        }
    }
}
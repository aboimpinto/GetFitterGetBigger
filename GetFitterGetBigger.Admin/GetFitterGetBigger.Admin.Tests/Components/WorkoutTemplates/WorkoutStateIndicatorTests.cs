using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.WorkoutTemplates;
using GetFitterGetBigger.Admin.Models.Dtos;
using AngleSharp.Dom;

namespace GetFitterGetBigger.Admin.Tests.Components.WorkoutTemplates
{
    public class WorkoutStateIndicatorTests : TestContext
    {
        [Fact]
        public void Component_RendersWithWorkoutState()
        {
            // Arrange
            var workoutState = new ReferenceDataDto { Id = "state-1", Value = "DRAFT", Description = "Draft state" };

            // Act
            var component = RenderComponent<WorkoutStateIndicator>(parameters => parameters
                .Add(p => p.WorkoutState, workoutState));

            // Assert
            var indicator = component.Find("[data-testid=\"workout-state-indicator\"]");
            indicator.Should().NotBeNull();
            indicator.TextContent.Should().Contain("DRAFT");
        }

        [Fact]
        public void Component_ShowsCorrectStylesForEachState()
        {
            // Arrange
            var testCases = new[]
            {
                (new ReferenceDataDto { Id = "state-1", Value = "DRAFT", Description = "Draft state" }, "bg-yellow-100", "text-yellow-800", "border-yellow-200", "📝"),
                (new ReferenceDataDto { Id = "state-2", Value = "PRODUCTION", Description = "Production state" }, "bg-green-100", "text-green-800", "border-green-200", "✅"),
                (new ReferenceDataDto { Id = "state-3", Value = "ARCHIVED", Description = "Archived state" }, "bg-gray-100", "text-gray-800", "border-gray-200", "📦")
            };

            foreach (var (state, bgColor, textColor, borderColor, icon) in testCases)
            {
                // Act
                var component = RenderComponent<WorkoutStateIndicator>(parameters => parameters
                    .Add(p => p.WorkoutState, state));

                // Assert
                var indicator = component.Find("[data-testid=\"workout-state-indicator\"]");
                var classes = indicator.GetAttribute("class") ?? "";
                classes.Should().Contain(bgColor);
                classes.Should().Contain(textColor);
                classes.Should().Contain(borderColor);
                indicator.TextContent.Should().Contain(icon);
            }
        }

        [Fact]
        public void Component_HandlesUnknownStateGracefully()
        {
            // Arrange
            var unknownState = new ReferenceDataDto { Id = "state-99", Value = "UNKNOWN", Description = "Unknown state" };

            // Act
            var component = RenderComponent<WorkoutStateIndicator>(parameters => parameters
                .Add(p => p.WorkoutState, unknownState));

            // Assert
            var indicator = component.Find("[data-testid=\"workout-state-indicator\"]");
            indicator.TextContent.Should().Contain("UNKNOWN");
            indicator.TextContent.Should().Contain("❓"); // Default icon
            var classes = indicator.GetAttribute("class") ?? "";
            classes.Should().Contain("bg-gray-100"); // Default colors
            classes.Should().Contain("text-gray-800");
            classes.Should().Contain("border-gray-200");
        }

        [Fact]
        public void Component_HidesIcon_WhenShowIconIsFalse()
        {
            // Arrange
            var workoutState = new ReferenceDataDto { Id = "state-1", Value = "DRAFT", Description = "Draft state" };

            // Act
            var component = RenderComponent<WorkoutStateIndicator>(parameters => parameters
                .Add(p => p.WorkoutState, workoutState)
                .Add(p => p.ShowIcon, false));

            // Assert
            var indicator = component.Find("[data-testid=\"workout-state-indicator\"]");
            indicator.TextContent.Should().NotContain("📝");
            indicator.TextContent.Should().Contain("DRAFT");
        }

        [Fact]
        public void Component_RendersDifferentSizes()
        {
            // Arrange
            var workoutState = new ReferenceDataDto { Id = "state-1", Value = "DRAFT", Description = "Draft state" };

            // Test Small
            var smallComponent = RenderComponent<WorkoutStateIndicator>(parameters => parameters
                .Add(p => p.WorkoutState, workoutState)
                .Add(p => p.Size, WorkoutStateIndicator.IndicatorSize.Small));

            var smallIndicator = smallComponent.Find("[data-testid=\"workout-state-indicator\"]");
            var smallClasses = smallIndicator.GetAttribute("class") ?? "";
            smallClasses.Should().Contain("px-2");
            smallClasses.Should().Contain("py-0.5");

            // Test Medium (default)
            var mediumComponent = RenderComponent<WorkoutStateIndicator>(parameters => parameters
                .Add(p => p.WorkoutState, workoutState));

            var mediumIndicator = mediumComponent.Find("[data-testid=\"workout-state-indicator\"]");
            var mediumClasses = mediumIndicator.GetAttribute("class") ?? "";
            mediumClasses.Should().Contain("px-3");
            mediumClasses.Should().Contain("py-1");

            // Test Large
            var largeComponent = RenderComponent<WorkoutStateIndicator>(parameters => parameters
                .Add(p => p.WorkoutState, workoutState)
                .Add(p => p.Size, WorkoutStateIndicator.IndicatorSize.Large));

            var largeIndicator = largeComponent.Find("[data-testid=\"workout-state-indicator\"]");
            var largeClasses = largeIndicator.GetAttribute("class") ?? "";
            largeClasses.Should().Contain("px-4");
            largeClasses.Should().Contain("py-2");
        }

        [Fact]
        public void Component_ShowsTooltip_WhenShowTooltipIsTrue()
        {
            // Arrange
            var workoutState = new ReferenceDataDto { Id = "state-1", Value = "DRAFT", Description = "Draft state" };

            // Act
            var component = RenderComponent<WorkoutStateIndicator>(parameters => parameters
                .Add(p => p.WorkoutState, workoutState));

            // Assert
            var indicator = component.Find("[data-testid=\"workout-state-indicator\"]");
            indicator.GetAttribute("title").Should().Be("Draft state");
        }

        [Fact]
        public void Component_HidesTooltip_WhenShowTooltipIsFalse()
        {
            // Arrange
            var workoutState = new ReferenceDataDto { Id = "state-1", Value = "DRAFT", Description = "Draft state" };

            // Act
            var component = RenderComponent<WorkoutStateIndicator>(parameters => parameters
                .Add(p => p.WorkoutState, workoutState)
                .Add(p => p.ShowTooltip, false));

            // Assert
            var indicator = component.Find("[data-testid=\"workout-state-indicator\"]");
            indicator.GetAttribute("title").Should().BeNullOrEmpty();
        }

        [Fact]
        public void Component_HasProperAriaLabel()
        {
            // Arrange
            var workoutState = new ReferenceDataDto { Id = "state-2", Value = "PRODUCTION", Description = "Production state" };

            // Act
            var component = RenderComponent<WorkoutStateIndicator>(parameters => parameters
                .Add(p => p.WorkoutState, workoutState));

            // Assert
            var indicator = component.Find("[data-testid=\"workout-state-indicator\"]");
            indicator.GetAttribute("aria-label").Should().Be("Workout state: PRODUCTION");
        }

        [Fact]
        public void Component_RendersNothing_WhenWorkoutStateIsNull()
        {
            // Act
            var component = RenderComponent<WorkoutStateIndicator>();

            // Assert
            component.FindAll("[data-testid=\"workout-state-indicator\"]").Should().BeEmpty();
        }

        [Fact]
        public void Component_HandlesCaseInsensitiveStates()
        {
            // Arrange
            var testCases = new[]
            {
                new ReferenceDataDto { Id = "state-1", Value = "draft", Description = "Draft state" },
                new ReferenceDataDto { Id = "state-2", Value = "Draft", Description = "Draft state" },
                new ReferenceDataDto { Id = "state-3", Value = "DRAFT", Description = "Draft state" }
            };

            foreach (var state in testCases)
            {
                // Act
                var component = RenderComponent<WorkoutStateIndicator>(parameters => parameters
                    .Add(p => p.WorkoutState, state));

                // Assert
                var indicator = component.Find("[data-testid=\"workout-state-indicator\"]");
                var classes = indicator.GetAttribute("class") ?? "";
                classes.Should().Contain("bg-yellow-100");
                classes.Should().Contain("text-yellow-800");
                indicator.TextContent.Should().Contain("📝");
            }
        }

        [Fact]
        public void GetStateStyles_ReturnsCorrectValues()
        {
            // Arrange
            var component = RenderComponent<WorkoutStateIndicator>();
            var instance = component.Instance;

            // Act & Assert
            var draftStyle = instance.GetStateStyles("DRAFT");
            draftStyle.bgColor.Should().Be("bg-yellow-100");
            draftStyle.textColor.Should().Be("text-yellow-800");
            draftStyle.borderColor.Should().Be("border-yellow-200");
            draftStyle.icon.Should().Be("📝");

            var productionStyle = instance.GetStateStyles("PRODUCTION");
            productionStyle.bgColor.Should().Be("bg-green-100");
            productionStyle.textColor.Should().Be("text-green-800");
            productionStyle.borderColor.Should().Be("border-green-200");
            productionStyle.icon.Should().Be("✅");

            var archivedStyle = instance.GetStateStyles("ARCHIVED");
            archivedStyle.bgColor.Should().Be("bg-gray-100");
            archivedStyle.textColor.Should().Be("text-gray-800");
            archivedStyle.borderColor.Should().Be("border-gray-200");
            archivedStyle.icon.Should().Be("📦");

            var unknownStyle = instance.GetStateStyles("UNKNOWN");
            unknownStyle.bgColor.Should().Be("bg-gray-100");
            unknownStyle.textColor.Should().Be("text-gray-800");
            unknownStyle.borderColor.Should().Be("border-gray-200");
            unknownStyle.icon.Should().Be("❓");
        }

        [Fact]
        public void GetSizeClasses_ReturnsCorrectValues()
        {
            // Arrange
            var component = RenderComponent<WorkoutStateIndicator>();
            var instance = component.Instance;

            // Act & Assert
            var smallSize = instance.GetSizeClasses(WorkoutStateIndicator.IndicatorSize.Small);
            smallSize.Container.Should().Be("px-2 py-0.5");
            smallSize.Text.Should().Be("text-xs font-medium");
            smallSize.Icon.Should().Be("text-xs");

            var mediumSize = instance.GetSizeClasses(WorkoutStateIndicator.IndicatorSize.Medium);
            mediumSize.Container.Should().Be("px-3 py-1");
            mediumSize.Text.Should().Be("text-sm font-medium");
            mediumSize.Icon.Should().Be("text-sm");

            var largeSize = instance.GetSizeClasses(WorkoutStateIndicator.IndicatorSize.Large);
            largeSize.Container.Should().Be("px-4 py-2");
            largeSize.Text.Should().Be("text-base font-semibold");
            largeSize.Icon.Should().Be("text-base");
        }
    }
}
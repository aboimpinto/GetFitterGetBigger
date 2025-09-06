using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Pages.Exercises.ExerciseLinks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Generic;
using System.Linq;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.Exercises.ExerciseLinks
{
    public class ExerciseContextSelectorTests : TestContext
    {
        [Fact]
        public void ExerciseContextSelector_RendersTabsForAllContexts()
        {
            // Arrange
            var contexts = new List<string> { "Workout", "Warmup", "Cooldown" };

            // Act
            var component = RenderComponent<ExerciseContextSelector>(parameters => parameters
                .Add(p => p.Contexts, contexts)
                .Add(p => p.ActiveContext, "Workout"));

            // Assert
            component.Find("[data-testid='context-selector']").Should().NotBeNull();
            
            // Should have 3 tabs
            var tabs = component.FindAll("[role='tab']");
            tabs.Should().HaveCount(3);
            
            // Check tab content
            component.Markup.Should().Contain("As Workout Exercise");
            component.Markup.Should().Contain("As Warmup Exercise");
            component.Markup.Should().Contain("As Cooldown Exercise");
        }

        [Fact]
        public void ExerciseContextSelector_ActiveTabHasCorrectStyling()
        {
            // Arrange
            var contexts = new List<string> { "Workout", "Warmup", "Cooldown" };

            // Act
            var component = RenderComponent<ExerciseContextSelector>(parameters => parameters
                .Add(p => p.Contexts, contexts)
                .Add(p => p.ActiveContext, "Warmup"));

            // Assert
            var activeTab = component.Find("[data-testid='context-tab-warmup']");
            activeTab.GetAttribute("aria-selected").Should().Be("true");
            activeTab.GetAttribute("class").Should().Contain("text-blue-700");
            activeTab.GetAttribute("class").Should().Contain("bg-gradient-to-b");
            activeTab.GetAttribute("class").Should().Contain("border-blue-600");
        }

        [Fact]
        public void ExerciseContextSelector_InactiveTabsHaveCorrectStyling()
        {
            // Arrange
            var contexts = new List<string> { "Workout", "Warmup", "Cooldown" };

            // Act
            var component = RenderComponent<ExerciseContextSelector>(parameters => parameters
                .Add(p => p.Contexts, contexts)
                .Add(p => p.ActiveContext, "Warmup"));

            // Assert
            var workoutTab = component.Find("[data-testid='context-tab-workout']");
            workoutTab.GetAttribute("aria-selected").Should().Be("false");
            workoutTab.GetAttribute("class").Should().Contain("text-gray-600");
            workoutTab.GetAttribute("class").Should().Contain("bg-white");
            workoutTab.GetAttribute("class").Should().Contain("border-transparent");

            var cooldownTab = component.Find("[data-testid='context-tab-cooldown']");
            cooldownTab.GetAttribute("aria-selected").Should().Be("false");
            cooldownTab.GetAttribute("class").Should().Contain("text-gray-600");
        }

        [Fact]
        public void ExerciseContextSelector_CallsOnContextChangeWhenTabClicked()
        {
            // Arrange
            var contexts = new List<string> { "Workout", "Warmup", "Cooldown" };
            string? changedContext = null;

            var component = RenderComponent<ExerciseContextSelector>(parameters => parameters
                .Add(p => p.Contexts, contexts)
                .Add(p => p.ActiveContext, "Workout")
                .Add(p => p.OnContextChange, EventCallback.Factory.Create<string>(this, c => changedContext = c)));

            // Act
            var warmupTab = component.Find("[data-testid='context-tab-warmup']");
            warmupTab.Click();

            // Assert
            changedContext.Should().Be("Warmup");
        }

        [Fact]
        public void ExerciseContextSelector_DoesNotCallOnContextChangeWhenActiveTabClicked()
        {
            // Arrange
            var contexts = new List<string> { "Workout", "Warmup", "Cooldown" };
            string? changedContext = null;

            var component = RenderComponent<ExerciseContextSelector>(parameters => parameters
                .Add(p => p.Contexts, contexts)
                .Add(p => p.ActiveContext, "Workout")
                .Add(p => p.OnContextChange, EventCallback.Factory.Create<string>(this, c => changedContext = c)));

            // Act
            var workoutTab = component.Find("[data-testid='context-tab-workout']");
            workoutTab.Click();

            // Assert
            changedContext.Should().BeNull();
        }

        [Fact]
        public void ExerciseContextSelector_HandlesEnterKeyPress()
        {
            // Arrange
            var contexts = new List<string> { "Workout", "Warmup", "Cooldown" };
            string? changedContext = null;

            var component = RenderComponent<ExerciseContextSelector>(parameters => parameters
                .Add(p => p.Contexts, contexts)
                .Add(p => p.ActiveContext, "Workout")
                .Add(p => p.OnContextChange, EventCallback.Factory.Create<string>(this, c => changedContext = c)));

            // Act
            var warmupTab = component.Find("[data-testid='context-tab-warmup']");
            warmupTab.KeyDown("Enter");

            // Assert
            changedContext.Should().Be("Warmup");
        }

        [Fact]
        public void ExerciseContextSelector_HandlesSpaceKeyPress()
        {
            // Arrange
            var contexts = new List<string> { "Workout", "Warmup", "Cooldown" };
            string? changedContext = null;

            var component = RenderComponent<ExerciseContextSelector>(parameters => parameters
                .Add(p => p.Contexts, contexts)
                .Add(p => p.ActiveContext, "Workout")
                .Add(p => p.OnContextChange, EventCallback.Factory.Create<string>(this, c => changedContext = c)));

            // Act
            var warmupTab = component.Find("[data-testid='context-tab-warmup']");
            warmupTab.KeyDown(" ");

            // Assert
            changedContext.Should().Be("Warmup");
        }

        [Fact]
        public void ExerciseContextSelector_HandlesArrowRightKeyNavigation()
        {
            // Arrange
            var contexts = new List<string> { "Workout", "Warmup", "Cooldown" };
            string? changedContext = null;

            var component = RenderComponent<ExerciseContextSelector>(parameters => parameters
                .Add(p => p.Contexts, contexts)
                .Add(p => p.ActiveContext, "Workout")
                .Add(p => p.OnContextChange, EventCallback.Factory.Create<string>(this, c => changedContext = c)));

            // Act
            var workoutTab = component.Find("[data-testid='context-tab-workout']");
            workoutTab.KeyDown("ArrowRight");

            // Assert
            changedContext.Should().Be("Warmup");
        }

        [Fact]
        public void ExerciseContextSelector_HandlesArrowLeftKeyNavigation()
        {
            // Arrange
            var contexts = new List<string> { "Workout", "Warmup", "Cooldown" };
            string? changedContext = null;

            var component = RenderComponent<ExerciseContextSelector>(parameters => parameters
                .Add(p => p.Contexts, contexts)
                .Add(p => p.ActiveContext, "Warmup")
                .Add(p => p.OnContextChange, EventCallback.Factory.Create<string>(this, c => changedContext = c)));

            // Act
            var warmupTab = component.Find("[data-testid='context-tab-warmup']");
            warmupTab.KeyDown("ArrowLeft");

            // Assert
            changedContext.Should().Be("Workout");
        }

        [Fact]
        public void ExerciseContextSelector_HandlesHomeKeyNavigation()
        {
            // Arrange
            var contexts = new List<string> { "Workout", "Warmup", "Cooldown" };
            string? changedContext = null;

            var component = RenderComponent<ExerciseContextSelector>(parameters => parameters
                .Add(p => p.Contexts, contexts)
                .Add(p => p.ActiveContext, "Cooldown")
                .Add(p => p.OnContextChange, EventCallback.Factory.Create<string>(this, c => changedContext = c)));

            // Act
            var cooldownTab = component.Find("[data-testid='context-tab-cooldown']");
            cooldownTab.KeyDown("Home");

            // Assert
            changedContext.Should().Be("Workout");
        }

        [Fact]
        public void ExerciseContextSelector_HandlesEndKeyNavigation()
        {
            // Arrange
            var contexts = new List<string> { "Workout", "Warmup", "Cooldown" };
            string? changedContext = null;

            var component = RenderComponent<ExerciseContextSelector>(parameters => parameters
                .Add(p => p.Contexts, contexts)
                .Add(p => p.ActiveContext, "Workout")
                .Add(p => p.OnContextChange, EventCallback.Factory.Create<string>(this, c => changedContext = c)));

            // Act
            var workoutTab = component.Find("[data-testid='context-tab-workout']");
            workoutTab.KeyDown("End");

            // Assert
            changedContext.Should().Be("Cooldown");
        }

        [Fact]
        public void ExerciseContextSelector_DoesNotNavigateBeyondBounds_ArrowRight()
        {
            // Arrange
            var contexts = new List<string> { "Workout", "Warmup", "Cooldown" };
            string? changedContext = null;

            var component = RenderComponent<ExerciseContextSelector>(parameters => parameters
                .Add(p => p.Contexts, contexts)
                .Add(p => p.ActiveContext, "Cooldown")
                .Add(p => p.OnContextChange, EventCallback.Factory.Create<string>(this, c => changedContext = c)));

            // Act
            var cooldownTab = component.Find("[data-testid='context-tab-cooldown']");
            cooldownTab.KeyDown("ArrowRight");

            // Assert
            changedContext.Should().BeNull(); // Should not change when at last tab
        }

        [Fact]
        public void ExerciseContextSelector_DoesNotNavigateBeyondBounds_ArrowLeft()
        {
            // Arrange
            var contexts = new List<string> { "Workout", "Warmup", "Cooldown" };
            string? changedContext = null;

            var component = RenderComponent<ExerciseContextSelector>(parameters => parameters
                .Add(p => p.Contexts, contexts)
                .Add(p => p.ActiveContext, "Workout")
                .Add(p => p.OnContextChange, EventCallback.Factory.Create<string>(this, c => changedContext = c)));

            // Act
            var workoutTab = component.Find("[data-testid='context-tab-workout']");
            workoutTab.KeyDown("ArrowLeft");

            // Assert
            changedContext.Should().BeNull(); // Should not change when at first tab
        }

        [Fact]
        public void ExerciseContextSelector_HasCorrectAriaAttributes()
        {
            // Arrange
            var contexts = new List<string> { "Workout", "Warmup", "Cooldown" };

            // Act
            var component = RenderComponent<ExerciseContextSelector>(parameters => parameters
                .Add(p => p.Contexts, contexts)
                .Add(p => p.ActiveContext, "Warmup"));

            // Assert
            var tablist = component.Find("[role='tablist']");
            tablist.GetAttribute("aria-label").Should().Be("Exercise contexts");

            var workoutTab = component.Find("[data-testid='context-tab-workout']");
            workoutTab.GetAttribute("role").Should().Be("tab");
            workoutTab.GetAttribute("aria-selected").Should().Be("false");
            workoutTab.GetAttribute("aria-controls").Should().Be("context-panel-workout");

            var warmupTab = component.Find("[data-testid='context-tab-warmup']");
            warmupTab.GetAttribute("aria-selected").Should().Be("true");
            warmupTab.GetAttribute("aria-controls").Should().Be("context-panel-warmup");
        }

        [Fact]
        public void ExerciseContextSelector_DisabledTabsHaveCorrectStyling()
        {
            // Arrange
            var contexts = new List<string> { "Workout", "Warmup", "Cooldown" };

            // Act
            var component = RenderComponent<ExerciseContextSelector>(parameters => parameters
                .Add(p => p.Contexts, contexts)
                .Add(p => p.ActiveContext, "Workout")
                .Add(p => p.Disabled, true));

            // Assert
            var tabs = component.FindAll("[role='tab']");
            foreach (var tab in tabs)
            {
                tab.HasAttribute("disabled").Should().BeTrue();
                tab.GetAttribute("class").Should().Contain("cursor-not-allowed");
                tab.GetAttribute("class").Should().Contain("opacity-50");
            }
        }

        [Fact]
        public void ExerciseContextSelector_DisabledTabsDoNotRespondToClicks()
        {
            // Arrange
            var contexts = new List<string> { "Workout", "Warmup", "Cooldown" };
            string? changedContext = null;

            var component = RenderComponent<ExerciseContextSelector>(parameters => parameters
                .Add(p => p.Contexts, contexts)
                .Add(p => p.ActiveContext, "Workout")
                .Add(p => p.Disabled, true)
                .Add(p => p.OnContextChange, EventCallback.Factory.Create<string>(this, c => changedContext = c)));

            // Act
            var warmupTab = component.Find("[data-testid='context-tab-warmup']");
            warmupTab.Click();

            // Assert
            changedContext.Should().BeNull();
        }

        [Fact]
        public void ExerciseContextSelector_DisabledTabsDoNotRespondToKeyboard()
        {
            // Arrange
            var contexts = new List<string> { "Workout", "Warmup", "Cooldown" };
            string? changedContext = null;

            var component = RenderComponent<ExerciseContextSelector>(parameters => parameters
                .Add(p => p.Contexts, contexts)
                .Add(p => p.ActiveContext, "Workout")
                .Add(p => p.Disabled, true)
                .Add(p => p.OnContextChange, EventCallback.Factory.Create<string>(this, c => changedContext = c)));

            // Act
            var workoutTab = component.Find("[data-testid='context-tab-workout']");
            workoutTab.KeyDown("Enter");
            workoutTab.KeyDown("ArrowRight");

            // Assert
            changedContext.Should().BeNull();
        }

        [Fact]
        public void ExerciseContextSelector_HandlesCustomContextTypes()
        {
            // Arrange
            var contexts = new List<string> { "Workout", "Mobility" };

            // Act
            var component = RenderComponent<ExerciseContextSelector>(parameters => parameters
                .Add(p => p.Contexts, contexts)
                .Add(p => p.ActiveContext, "Workout"));

            // Assert
            component.Markup.Should().Contain("As Workout Exercise");
            component.Markup.Should().Contain("As Mobility Exercise");
        }

        [Fact]
        public void ExerciseContextSelector_WorksWithSingleContext()
        {
            // Arrange
            var contexts = new List<string> { "Workout" };

            // Act
            var component = RenderComponent<ExerciseContextSelector>(parameters => parameters
                .Add(p => p.Contexts, contexts)
                .Add(p => p.ActiveContext, "Workout"));

            // Assert
            var tabs = component.FindAll("[role='tab']");
            tabs.Should().HaveCount(1);
            
            var workoutTab = component.Find("[data-testid='context-tab-workout']");
            workoutTab.GetAttribute("aria-selected").Should().Be("true");
        }

        [Fact]
        public void ExerciseContextSelector_TabsHaveFocusManagement()
        {
            // Arrange
            var contexts = new List<string> { "Workout", "Warmup", "Cooldown" };

            // Act
            var component = RenderComponent<ExerciseContextSelector>(parameters => parameters
                .Add(p => p.Contexts, contexts)
                .Add(p => p.ActiveContext, "Workout"));

            // Assert
            var tabs = component.FindAll("[role='tab']");
            foreach (var tab in tabs)
            {
                tab.GetAttribute("class").Should().Contain("focus:outline-none");
                tab.GetAttribute("class").Should().Contain("focus:ring-2");
                tab.GetAttribute("class").Should().Contain("focus:ring-blue-500");
            }
        }
    }
}
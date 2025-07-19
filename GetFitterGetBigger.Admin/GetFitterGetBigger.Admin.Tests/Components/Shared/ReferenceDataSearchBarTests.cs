using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Threading.Tasks;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Components.Shared
{
    public class ReferenceDataSearchBarTests : TestContext
    {
        [Fact]
        public void ReferenceDataSearchBar_RendersCorrectly()
        {
            // Arrange & Act
            var component = RenderComponent<ReferenceDataSearchBar>(parameters => parameters
                .Add(p => p.Label, "Search Items")
                .Add(p => p.Placeholder, "Type to search..."));

            // Assert
            component.Find("label").TextContent.Should().Contain("Search Items");
            var input = component.Find("[data-testid='search-input']");
            input.Should().NotBeNull();
            input.GetAttribute("placeholder").Should().Be("Type to search...");
            component.FindAll("[data-testid='clear-button']").Should().BeEmpty();
        }

        [Fact]
        public void ReferenceDataSearchBar_ShowsClearButtonWhenHasValue()
        {
            // Arrange & Act
            var component = RenderComponent<ReferenceDataSearchBar>(parameters => parameters
                .Add(p => p.Value, "test search"));

            // Assert
            component.Find("[data-testid='search-input']").GetAttribute("value").Should().Be("test search");
            component.Find("[data-testid='clear-button']").Should().NotBeNull();
        }

        [Fact]
        public void ReferenceDataSearchBar_ClearButtonClearsValue()
        {
            // Arrange
            var valueChangedValue = "";
            var component = RenderComponent<ReferenceDataSearchBar>(parameters => parameters
                .Add(p => p.Value, "test search")
                .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => valueChangedValue = value))
                .Add(p => p.DebounceDelay, 0));

            // Act
            component.Find("[data-testid='clear-button']").Click();

            // Assert
            valueChangedValue.Should().Be("");
            component.Find("[data-testid='search-input']").GetAttribute("value").Should().Be("");
            component.FindAll("[data-testid='clear-button']").Should().BeEmpty();
        }

        [Fact]
        public void ReferenceDataSearchBar_NotifiesValueChangeImmediatelyWhenNoDebounce()
        {
            // Arrange
            var valueChangedValue = "";
            var component = RenderComponent<ReferenceDataSearchBar>(parameters => parameters
                .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => valueChangedValue = value))
                .Add(p => p.DebounceDelay, 0));

            // Act
            component.Find("[data-testid='search-input']").Input("new value");

            // Assert
            valueChangedValue.Should().Be("new value");
        }

        [Fact]
        public async Task ReferenceDataSearchBar_DebouncesInputWhenDelaySet()
        {
            // Arrange
            var valueChangedCount = 0;
            var lastValue = "";
            var component = RenderComponent<ReferenceDataSearchBar>(parameters => parameters
                .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => 
                {
                    valueChangedCount++;
                    lastValue = value;
                }))
                .Add(p => p.DebounceDelay, 100));

            // Act - Type multiple characters quickly
            var input = component.Find("[data-testid='search-input']");
            input.Input("a");
            input.Input("ab");
            input.Input("abc");

            // Assert - Value not changed immediately
            valueChangedCount.Should().Be(0);

            // Wait for debounce
            await Task.Delay(150);
            component.WaitForState(() => valueChangedCount > 0, TimeSpan.FromSeconds(1));

            // Should only get one notification with final value
            valueChangedCount.Should().Be(1);
            lastValue.Should().Be("abc");
        }

        [Fact]
        public async Task ReferenceDataSearchBar_EnterKeyTriggersImmediateUpdate()
        {
            // Arrange
            var valueChangedValue = "";
            var enterPressed = false;
            var component = RenderComponent<ReferenceDataSearchBar>(parameters => parameters
                .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => valueChangedValue = value))
                .Add(p => p.OnEnterPressed, EventCallback.Factory.Create(this, () => enterPressed = true))
                .Add(p => p.DebounceDelay, 1000)); // Long delay to ensure Enter bypasses it

            // Act
            var input = component.Find("[data-testid='search-input']");
            input.Input("search text");
            await input.TriggerEventAsync("onkeydown", new KeyboardEventArgs { Key = "Enter" });

            // Assert
            valueChangedValue.Should().Be("search text");
            enterPressed.Should().BeTrue();
        }

        [Fact]
        public async Task ReferenceDataSearchBar_EscapeKeyClearsValueWhenNotEmpty()
        {
            // Arrange
            var valueChangedValue = "";
            var escapePressed = false;
            var component = RenderComponent<ReferenceDataSearchBar>(parameters => parameters
                .Add(p => p.Value, "existing text")
                .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => valueChangedValue = value))
                .Add(p => p.OnEscapePressed, EventCallback.Factory.Create(this, () => escapePressed = true)));

            // Act
            await component.Find("[data-testid='search-input']").TriggerEventAsync("onkeydown", new KeyboardEventArgs { Key = "Escape" });

            // Assert
            valueChangedValue.Should().Be("");
            escapePressed.Should().BeFalse(); // Escape event not fired when clearing
        }

        [Fact]
        public async Task ReferenceDataSearchBar_EscapeKeyTriggersEventWhenEmpty()
        {
            // Arrange
            var escapePressed = false;
            var component = RenderComponent<ReferenceDataSearchBar>(parameters => parameters
                .Add(p => p.Value, "")
                .Add(p => p.OnEscapePressed, EventCallback.Factory.Create(this, () => escapePressed = true)));

            // Act
            await component.Find("[data-testid='search-input']").TriggerEventAsync("onkeydown", new KeyboardEventArgs { Key = "Escape" });

            // Assert
            escapePressed.Should().BeTrue();
        }

        [Fact]
        public void ReferenceDataSearchBar_ShowsResultCount()
        {
            // Arrange & Act
            var component = RenderComponent<ReferenceDataSearchBar>(parameters => parameters
                .Add(p => p.ShowResultCount, true)
                .Add(p => p.ResultCount, 5));

            // Assert
            var resultCount = component.Find("[data-testid='result-count']");
            resultCount.TextContent.Should().Contain("5 results found");
        }

        [Fact]
        public void ReferenceDataSearchBar_ShowsNoResultsMessage()
        {
            // Arrange & Act
            var component = RenderComponent<ReferenceDataSearchBar>(parameters => parameters
                .Add(p => p.ShowResultCount, true)
                .Add(p => p.ResultCount, 0));

            // Assert
            var resultCount = component.Find("[data-testid='result-count']");
            resultCount.TextContent.Should().Contain("No results found");
        }

        [Fact]
        public void ReferenceDataSearchBar_ShowsSingularResultMessage()
        {
            // Arrange & Act
            var component = RenderComponent<ReferenceDataSearchBar>(parameters => parameters
                .Add(p => p.ShowResultCount, true)
                .Add(p => p.ResultCount, 1));

            // Assert
            var resultCount = component.Find("[data-testid='result-count']");
            resultCount.TextContent.Should().Contain("1 result found");
        }

        [Fact]
        public void ReferenceDataSearchBar_UpdatesValueWhenParameterChanges()
        {
            // Arrange
            var component = RenderComponent<ReferenceDataSearchBar>(parameters => parameters
                .Add(p => p.Value, "initial"));

            // Act
            component.SetParametersAndRender(parameters => parameters
                .Add(p => p.Value, "updated"));

            // Assert
            component.Find("[data-testid='search-input']").GetAttribute("value").Should().Be("updated");
        }

        [Fact]
        public void ReferenceDataSearchBar_DisposesTimerProperly()
        {
            // Arrange
            var component = RenderComponent<ReferenceDataSearchBar>(parameters => parameters
                .Add(p => p.DebounceDelay, 100));

            // Act
            component.Dispose();

            // Assert - Should not throw
            component.IsDisposed.Should().BeTrue();
        }
    }
}
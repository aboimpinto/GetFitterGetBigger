using Bunit;
using GetFitterGetBigger.Admin.Components.Shared;
using Xunit;
using FluentAssertions;
using System.Linq;

namespace GetFitterGetBigger.Admin.Tests.Components.Shared
{
    public class LoadingIndicatorTests : TestContext
    {
        [Fact]
        public void Should_render_with_default_values()
        {
            // Act
            var cut = RenderComponent<LoadingIndicator>();
            
            // Assert
            var container = cut.Find("[data-testid='loading-indicator']");
            container.Should().NotBeNull();
            container.GetAttribute("class")?.Should().Contain("py-8");
            
            var spinner = container.QuerySelector(".animate-spin");
            spinner.Should().NotBeNull();
            var spinnerClasses = spinner.GetAttribute("class") ?? "";
            spinnerClasses.Should().Contain("h-12");
            spinnerClasses.Should().Contain("w-12");
            spinnerClasses.Should().Contain("border-blue-600");
        }

        [Fact]
        public void Should_render_with_custom_size()
        {
            // Act
            var cut = RenderComponent<LoadingIndicator>(parameters => parameters
                .Add(p => p.Size, "8"));
            
            // Assert
            var spinner = cut.Find(".animate-spin");
            var spinnerClasses = spinner.GetAttribute("class") ?? "";
            spinnerClasses.Should().Contain("h-8");
            spinnerClasses.Should().Contain("w-8");
        }

        [Fact]
        public void Should_render_with_custom_color()
        {
            // Act
            var cut = RenderComponent<LoadingIndicator>(parameters => parameters
                .Add(p => p.Color, "red"));
            
            // Assert
            var spinner = cut.Find(".animate-spin");
            spinner.GetAttribute("class")?.Should().Contain("border-red-600");
        }

        [Fact]
        public void Should_render_with_custom_padding()
        {
            // Act
            var cut = RenderComponent<LoadingIndicator>(parameters => parameters
                .Add(p => p.PaddingY, "4"));
            
            // Assert
            var container = cut.Find("[data-testid='loading-indicator']");
            container.GetAttribute("class")?.Should().Contain("py-4");
        }

        [Fact]
        public void Should_render_message_when_provided()
        {
            // Act
            var cut = RenderComponent<LoadingIndicator>(parameters => parameters
                .Add(p => p.Message, "Loading data..."));
            
            // Assert
            var message = cut.Find("span.ml-3.text-gray-600");
            message.TextContent.Should().Be("Loading data...");
        }

        [Fact]
        public void Should_not_render_message_when_not_provided()
        {
            // Act
            var cut = RenderComponent<LoadingIndicator>();
            
            // Assert
            cut.FindAll("span.ml-3.text-gray-600").Should().BeEmpty();
        }

        [Fact]
        public void Should_render_with_all_custom_parameters()
        {
            // Act
            var cut = RenderComponent<LoadingIndicator>(parameters => parameters
                .Add(p => p.Size, "16")
                .Add(p => p.Color, "green")
                .Add(p => p.PaddingY, "12")
                .Add(p => p.Message, "Please wait..."));
            
            // Assert
            var container = cut.Find("[data-testid='loading-indicator']");
            container.GetAttribute("class")?.Should().Contain("py-12");
            
            var spinner = container.QuerySelector(".animate-spin");
            var spinnerClasses = spinner.GetAttribute("class") ?? "";
            spinnerClasses.Should().Contain("h-16");
            spinnerClasses.Should().Contain("w-16");
            spinnerClasses.Should().Contain("border-green-600");
            
            var message = container.QuerySelector("span.ml-3.text-gray-600");
            message.TextContent.Should().Be("Please wait...");
        }

        [Fact]
        public void Should_have_proper_accessibility_attributes()
        {
            // Act
            var cut = RenderComponent<LoadingIndicator>();
            
            // Assert
            var container = cut.Find("[data-testid='loading-indicator']");
            container.Should().NotBeNull();
            
            // The spinner should be centered
            var containerClasses = container.GetAttribute("class") ?? "";
            containerClasses.Should().Contain("flex");
            containerClasses.Should().Contain("items-center");
            containerClasses.Should().Contain("justify-center");
        }

        [Fact]
        public void Should_apply_spinner_animation_classes()
        {
            // Act
            var cut = RenderComponent<LoadingIndicator>();
            
            // Assert
            var spinner = cut.Find(".animate-spin");
            var spinnerClasses = spinner.GetAttribute("class") ?? "";
            spinnerClasses.Should().Contain("rounded-full");
            spinnerClasses.Should().Contain("border-b-2");
        }
    }
}
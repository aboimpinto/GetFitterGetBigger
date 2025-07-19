using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Shared;
using GetFitterGetBigger.Admin.Models.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Threading.Tasks;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Components.Shared
{
    public class ReferenceDataDetailModalTests : TestContext
    {
        [Fact]
        public void ReferenceDataDetailModal_RendersCorrectly()
        {
            // Arrange & Act
            var component = RenderComponent<ReferenceDataDetailModal<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Title, "Test Details")
                .Add(p => p.Item, new ReferenceDataDto { Id = "1", Value = "Test Item", Description = "Test Description" }));

            // Assert
            component.Find("[data-testid='modal-title']").TextContent.Should().Contain("Test Details");
            component.Find("[data-testid='detail-modal-backdrop']").Should().NotBeNull();
            component.Find("[data-testid='detail-modal-content']").Should().NotBeNull();
            component.Find("[data-testid='close-button']").Should().NotBeNull();
            component.Find("[data-testid='footer-close-button']").Should().NotBeNull();
        }

        [Fact]
        public void ReferenceDataDetailModal_ShowsLoadingWhenItemIsNull()
        {
            // Arrange & Act
            var component = RenderComponent<ReferenceDataDetailModal<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Title, "Loading Details")
                .Add(p => p.Item, null));

            // Assert
            var body = component.Find("[data-testid='modal-body']");
            body.TextContent.Should().Contain("Loading...");
            body.QuerySelector(".animate-spin").Should().NotBeNull();
        }

        [Fact]
        public void ReferenceDataDetailModal_RendersContentTemplate()
        {
            // Arrange
            var item = new ReferenceDataDto { Id = "1", Value = "Test Item", Description = "Test Description" };
            
            // Act
            var component = RenderComponent<ReferenceDataDetailModal<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Title, "Item Details")
                .Add(p => p.Item, item)
                .Add(p => p.ContentTemplate, context => builder =>
                {
                    builder.OpenElement(0, "div");
                    builder.AddAttribute(1, "data-testid", "custom-content");
                    builder.AddContent(2, $"Name: {context.Value}, Description: {context.Description}");
                    builder.CloseElement();
                }));

            // Assert
            var customContent = component.Find("[data-testid='custom-content']");
            customContent.TextContent.Should().Contain("Name: Test Item");
            customContent.TextContent.Should().Contain("Description: Test Description");
        }

        [Fact]
        public void ReferenceDataDetailModal_ShowsNoTemplateMessage()
        {
            // Arrange & Act
            var component = RenderComponent<ReferenceDataDetailModal<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Item, new ReferenceDataDto { Id = "1", Value = "Test" }));

            // Assert
            component.Find("[data-testid='modal-body']").TextContent.Should().Contain("No content template provided");
        }

        [Fact]
        public void ReferenceDataDetailModal_CloseButtonTriggersOnClose()
        {
            // Arrange
            var closeCalled = false;
            var component = RenderComponent<ReferenceDataDetailModal<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Item, new ReferenceDataDto { Id = "1", Value = "Test" })
                .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => closeCalled = true)));

            // Act
            component.Find("[data-testid='close-button']").Click();

            // Assert
            closeCalled.Should().BeTrue();
        }

        [Fact]
        public void ReferenceDataDetailModal_FooterCloseButtonTriggersOnClose()
        {
            // Arrange
            var closeCalled = false;
            var component = RenderComponent<ReferenceDataDetailModal<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Item, new ReferenceDataDto { Id = "1", Value = "Test" })
                .Add(p => p.ShowFooter, true)
                .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => closeCalled = true)));

            // Act
            component.Find("[data-testid='footer-close-button']").Click();

            // Assert
            closeCalled.Should().BeTrue();
        }

        [Fact]
        public void ReferenceDataDetailModal_BackdropClickClosesWhenEnabled()
        {
            // Arrange
            var closeCalled = false;
            var component = RenderComponent<ReferenceDataDetailModal<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Item, new ReferenceDataDto { Id = "1", Value = "Test" })
                .Add(p => p.CloseOnBackdropClick, true)
                .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => closeCalled = true)));

            // Act
            component.Find("[data-testid='detail-modal-backdrop']").Click();

            // Assert
            closeCalled.Should().BeTrue();
        }

        [Fact]
        public void ReferenceDataDetailModal_BackdropClickDoesNotCloseWhenDisabled()
        {
            // Arrange
            var closeCalled = false;
            var component = RenderComponent<ReferenceDataDetailModal<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Item, new ReferenceDataDto { Id = "1", Value = "Test" })
                .Add(p => p.CloseOnBackdropClick, false)
                .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => closeCalled = true)));

            // Act
            component.Find("[data-testid='detail-modal-backdrop']").Click();

            // Assert
            closeCalled.Should().BeFalse();
        }


        [Fact]
        public void ReferenceDataDetailModal_EscapeKeyClosesWhenEnabled()
        {
            // Arrange
            var closeCalled = false;
            var component = RenderComponent<ReferenceDataDetailModal<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Item, new ReferenceDataDto { Id = "1", Value = "Test" })
                .Add(p => p.CloseOnEscape, true)
                .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => closeCalled = true)));

            // Act
            component.Find("[data-testid='detail-modal-backdrop']").KeyDown(new KeyboardEventArgs { Key = "Escape" });

            // Assert
            closeCalled.Should().BeTrue();
        }

        [Fact]
        public void ReferenceDataDetailModal_EscapeKeyDoesNotCloseWhenDisabled()
        {
            // Arrange
            var closeCalled = false;
            var component = RenderComponent<ReferenceDataDetailModal<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Item, new ReferenceDataDto { Id = "1", Value = "Test" })
                .Add(p => p.CloseOnEscape, false)
                .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => closeCalled = true)));

            // Act
            component.Find("[data-testid='detail-modal-backdrop']").KeyDown(new KeyboardEventArgs { Key = "Escape" });

            // Assert
            closeCalled.Should().BeFalse();
        }

        [Fact]
        public void ReferenceDataDetailModal_HidesFooterWhenShowFooterIsFalse()
        {
            // Arrange & Act
            var component = RenderComponent<ReferenceDataDetailModal<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Item, new ReferenceDataDto { Id = "1", Value = "Test" })
                .Add(p => p.ShowFooter, false));

            // Assert
            component.FindAll("[data-testid='footer-close-button']").Should().BeEmpty();
            component.FindAll(".border-t.border-gray-200").Should().BeEmpty();
        }

        [Fact]
        public void ReferenceDataDetailModal_RendersCustomFooterTemplate()
        {
            // Arrange & Act
            var component = RenderComponent<ReferenceDataDetailModal<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Item, new ReferenceDataDto { Id = "1", Value = "Test" })
                .Add(p => p.ShowFooter, true)
                .Add(p => p.FooterTemplate, builder =>
                {
                    builder.OpenElement(0, "button");
                    builder.AddAttribute(1, "data-testid", "custom-footer-button");
                    builder.AddContent(2, "Custom Action");
                    builder.CloseElement();
                }));

            // Assert
            component.Find("[data-testid='custom-footer-button']").TextContent.Should().Be("Custom Action");
            component.FindAll("[data-testid='footer-close-button']").Should().BeEmpty();
        }

        [Fact]
        public void ReferenceDataDetailModal_HasCorrectAriaAttributes()
        {
            // Arrange & Act
            var component = RenderComponent<ReferenceDataDetailModal<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Title, "Accessible Modal")
                .Add(p => p.Item, new ReferenceDataDto { Id = "1", Value = "Test" }));

            // Assert
            var backdrop = component.Find("[data-testid='detail-modal-backdrop']");
            backdrop.GetAttribute("role").Should().Be("dialog");
            backdrop.GetAttribute("aria-modal").Should().Be("true");
            backdrop.GetAttribute("aria-labelledby").Should().Be("modal-title");
            backdrop.GetAttribute("aria-describedby").Should().Be("modal-description");
            backdrop.GetAttribute("tabindex").Should().Be("-1");
        }
    }
}
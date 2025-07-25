using System.Collections.Generic;
using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Shared;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Components.Shared;

public class BreadcrumbTests : TestContext
{
    [Fact]
    public void Should_RenderAllBreadcrumbItems()
    {
        // Arrange
        var items = new List<Breadcrumb.BreadcrumbItem>
        {
            new() { Text = "Home", Href = "/" },
            new() { Text = "Products", Href = "/products" },
            new() { Text = "Product Detail" }
        };

        // Act
        var cut = RenderComponent<Breadcrumb>(parameters => parameters
            .Add(p => p.Items, items));

        // Assert
        var breadcrumbItems = cut.FindAll("[data-testid^='breadcrumb-item-']");
        breadcrumbItems.Should().HaveCount(3);
        
        breadcrumbItems[0].TextContent.Should().Be("Home");
        breadcrumbItems[1].TextContent.Should().Be("Products");
        breadcrumbItems[2].TextContent.Should().Be("Product Detail");
    }

    [Fact]
    public void Should_RenderLinksForNonLastItems()
    {
        // Arrange
        var items = new List<Breadcrumb.BreadcrumbItem>
        {
            new() { Text = "Home", Href = "/" },
            new() { Text = "Products", Href = "/products" },
            new() { Text = "Current Page" }
        };

        // Act
        var cut = RenderComponent<Breadcrumb>(parameters => parameters
            .Add(p => p.Items, items));

        // Assert
        // First two items should be links
        cut.Find("[data-testid='breadcrumb-item-0'] a").Should().NotBeNull();
        cut.Find("[data-testid='breadcrumb-item-0'] a").GetAttribute("href").Should().Be("/");
        
        cut.Find("[data-testid='breadcrumb-item-1'] a").Should().NotBeNull();
        cut.Find("[data-testid='breadcrumb-item-1'] a").GetAttribute("href").Should().Be("/products");
        
        // Last item should not be a link
        cut.FindAll("[data-testid='breadcrumb-item-2'] a").Should().BeEmpty();
        cut.Find("[data-testid='breadcrumb-item-2'] span").TextContent.Should().Be("Current Page");
    }

    [Fact]
    public void Should_NotRenderSeparatorForFirstItem()
    {
        // Arrange
        var items = new List<Breadcrumb.BreadcrumbItem>
        {
            new() { Text = "Home", Href = "/" },
            new() { Text = "Products", Href = "/products" }
        };

        // Act
        var cut = RenderComponent<Breadcrumb>(parameters => parameters
            .Add(p => p.Items, items));

        // Assert
        // Check that separators exist for all items except the first
        var separators = cut.FindAll("svg");
        separators.Should().HaveCount(1); // Only one separator between two items
    }

    [Fact]
    public void Should_HandleItemsWithoutHref()
    {
        // Arrange
        var items = new List<Breadcrumb.BreadcrumbItem>
        {
            new() { Text = "Home", Href = "/" },
            new() { Text = "No Link Item" }, // No href
            new() { Text = "Current" }
        };

        // Act
        var cut = RenderComponent<Breadcrumb>(parameters => parameters
            .Add(p => p.Items, items));

        // Assert
        // First item should be a link
        cut.Find("[data-testid='breadcrumb-item-0'] a").Should().NotBeNull();
        
        // Second item should be a span (no href provided)
        cut.FindAll("[data-testid='breadcrumb-item-1'] a").Should().BeEmpty();
        cut.Find("[data-testid='breadcrumb-item-1'] span").TextContent.Should().Be("No Link Item");
        
        // Last item should be a span with different styling
        cut.FindAll("[data-testid='breadcrumb-item-2'] a").Should().BeEmpty();
        var lastItemSpan = cut.Find("[data-testid='breadcrumb-item-2'] span");
        lastItemSpan.GetAttribute("class").Should().Contain("font-medium");
    }

    [Fact]
    public void Should_ApplyProperAriaLabel()
    {
        // Arrange
        var items = new List<Breadcrumb.BreadcrumbItem>
        {
            new() { Text = "Home", Href = "/" }
        };

        // Act
        var cut = RenderComponent<Breadcrumb>(parameters => parameters
            .Add(p => p.Items, items));

        // Assert
        var nav = cut.Find("nav");
        nav.GetAttribute("aria-label").Should().Be("breadcrumb");
    }

    [Fact]
    public void Should_HandleEmptyItemsList()
    {
        // Arrange
        var items = new List<Breadcrumb.BreadcrumbItem>();

        // Act
        var cut = RenderComponent<Breadcrumb>(parameters => parameters
            .Add(p => p.Items, items));

        // Assert
        cut.FindAll("[data-testid^='breadcrumb-item-']").Should().BeEmpty();
    }

    [Fact]
    public void Should_ApplyHoverClassToLinks()
    {
        // Arrange
        var items = new List<Breadcrumb.BreadcrumbItem>
        {
            new() { Text = "Home", Href = "/" },
            new() { Text = "Current" }
        };

        // Act
        var cut = RenderComponent<Breadcrumb>(parameters => parameters
            .Add(p => p.Items, items));

        // Assert
        var link = cut.Find("[data-testid='breadcrumb-item-0'] a");
        link.GetAttribute("class").Should().Contain("hover:text-blue-600");
    }
}
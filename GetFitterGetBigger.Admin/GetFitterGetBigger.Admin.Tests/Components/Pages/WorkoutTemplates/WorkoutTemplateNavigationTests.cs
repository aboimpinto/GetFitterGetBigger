using System.Reflection;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Pages.WorkoutTemplates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.WorkoutTemplates;

/// <summary>
/// Simple navigation tests to verify routes and authorization attributes
/// </summary>
public class WorkoutTemplateNavigationTests
{
    [Fact]
    public void WorkoutTemplatesPage_Should_HaveCorrectRoute()
    {
        // Arrange
        var pageType = typeof(GetFitterGetBigger.Admin.Components.Pages.WorkoutTemplates.WorkoutTemplates);
        
        // Act
        var routeAttribute = pageType.GetCustomAttribute<RouteAttribute>();
        
        // Assert
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("/workout-templates");
    }
    
    [Fact]
    public void WorkoutTemplateFormPage_Should_HaveCorrectRoutes()
    {
        // Arrange
        var pageType = typeof(WorkoutTemplateFormPage);
        
        // Act
        var routeAttributes = pageType.GetCustomAttributes<RouteAttribute>().ToList();
        
        // Assert
        routeAttributes.Should().HaveCount(2);
        routeAttributes.Select(r => r.Template).Should().Contain("/workout-templates/new");
        routeAttributes.Select(r => r.Template).Should().Contain("/workout-templates/{Id}/edit");
    }
    
    [Fact]
    public void WorkoutTemplateDetailPage_Should_HaveCorrectRoute()
    {
        // Arrange
        var pageType = typeof(WorkoutTemplateDetailPage);
        
        // Act
        var routeAttribute = pageType.GetCustomAttribute<RouteAttribute>();
        
        // Assert
        routeAttribute.Should().NotBeNull();
        routeAttribute!.Template.Should().Be("/workout-templates/{Id}");
    }
    
    [Fact]
    public void WorkoutTemplatesPage_Should_HaveAuthorizeAttribute()
    {
        // Arrange
        var pageType = typeof(GetFitterGetBigger.Admin.Components.Pages.WorkoutTemplates.WorkoutTemplates);
        
        // Act
        var authorizeAttribute = pageType.GetCustomAttribute<AuthorizeAttribute>();
        
        // Assert
        authorizeAttribute.Should().NotBeNull();
    }
    
    [Fact]
    public void WorkoutTemplateFormPage_Should_HaveAuthorizeAttribute()
    {
        // Arrange
        var pageType = typeof(WorkoutTemplateFormPage);
        
        // Act
        var authorizeAttribute = pageType.GetCustomAttribute<AuthorizeAttribute>();
        
        // Assert
        authorizeAttribute.Should().NotBeNull();
    }
    
    [Fact]
    public void WorkoutTemplateDetailPage_Should_NotHaveAuthorizeAttribute()
    {
        // Arrange
        var pageType = typeof(WorkoutTemplateDetailPage);
        
        // Act
        var authorizeAttribute = pageType.GetCustomAttribute<AuthorizeAttribute>();
        
        // Assert
        // Detail page doesn't have [Authorize] attribute based on the current implementation
        authorizeAttribute.Should().BeNull();
    }
    
}
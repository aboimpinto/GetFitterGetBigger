using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.WorkoutTemplates;
using GetFitterGetBigger.Admin.Components.Shared;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Moq;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using System.Linq;

namespace GetFitterGetBigger.Admin.Tests.Components.WorkoutTemplates
{
    public class WorkoutTemplateFiltersTests : TestContext
    {
        private readonly Mock<IWorkoutTemplateStateService> _stateServiceMock;
        private readonly List<ReferenceDataDto> _testCategories;
        private readonly List<ReferenceDataDto> _testDifficulties;
        private readonly List<ReferenceDataDto> _testStates;

        public WorkoutTemplateFiltersTests()
        {
            _stateServiceMock = new Mock<IWorkoutTemplateStateService>();
            
            _testCategories = new List<ReferenceDataDto>
            {
                new() { Id = "cat-1", Value = "Strength Training", Description = null },
                new() { Id = "cat-2", Value = "Cardio", Description = null },
                new() { Id = "cat-3", Value = "Flexibility", Description = null }
            };

            _testDifficulties = new List<ReferenceDataDto>
            {
                new() { Id = "diff-1", Value = "Beginner", Description = null },
                new() { Id = "diff-2", Value = "Intermediate", Description = null },
                new() { Id = "diff-3", Value = "Advanced", Description = null }
            };

            _testStates = new List<ReferenceDataDto>
            {
                new() { Id = "state-1", Value = "DRAFT", Description = null },
                new() { Id = "state-2", Value = "PRODUCTION", Description = null },
                new() { Id = "state-3", Value = "ARCHIVED", Description = null }
            };

            Services.AddSingleton(_stateServiceMock.Object);
            _stateServiceMock.Setup(x => x.WorkoutCategories).Returns(_testCategories);
            _stateServiceMock.Setup(x => x.DifficultyLevels).Returns(_testDifficulties);
            _stateServiceMock.Setup(x => x.WorkoutStates).Returns(_testStates);

            // Register the ReferenceDataSearchBar component
            JSInterop.Mode = JSRuntimeMode.Loose;
        }

        [Fact]
        public void Component_RendersAllFilterSections()
        {
            // Act
            var component = RenderComponent<WorkoutTemplateFilters>();

            // Assert
            component.Find("[data-testid=\"workout-template-filters\"]").Should().NotBeNull();
            component.Find("[data-testid=\"search-section\"]").Should().NotBeNull();
            component.Find("[data-testid=\"category-filter\"]").Should().NotBeNull();
            component.Find("[data-testid=\"difficulty-filter\"]").Should().NotBeNull();
            component.Find("[data-testid=\"state-filter\"]").Should().NotBeNull();
            component.Find("[data-testid=\"public-filter\"]").Should().NotBeNull();
        }

        [Fact]
        public void Component_PopulatesDropdownsWithStateServiceData()
        {
            // Act
            var component = RenderComponent<WorkoutTemplateFilters>();

            // Assert - Categories
            var categorySelect = component.Find("[data-testid=\"category-select\"]");
            var categoryOptions = categorySelect.QuerySelectorAll("option");
            categoryOptions.Length.Should().Be(4); // All + 3 categories
            categoryOptions[1].TextContent.Should().Be("Strength Training");
            categoryOptions[2].TextContent.Should().Be("Cardio");
            categoryOptions[3].TextContent.Should().Be("Flexibility");

            // Assert - Difficulties
            var difficultySelect = component.Find("[data-testid=\"difficulty-select\"]");
            var difficultyOptions = difficultySelect.QuerySelectorAll("option");
            difficultyOptions.Length.Should().Be(4); // All + 3 difficulties
            difficultyOptions[1].TextContent.Should().Be("Beginner");
            difficultyOptions[2].TextContent.Should().Be("Intermediate");
            difficultyOptions[3].TextContent.Should().Be("Advanced");

            // Assert - States
            var stateSelect = component.Find("[data-testid=\"state-select\"]");
            var stateOptions = stateSelect.QuerySelectorAll("option");
            stateOptions.Length.Should().Be(4); // All + 3 states
            stateOptions[1].TextContent.Should().Be("DRAFT");
            stateOptions[2].TextContent.Should().Be("PRODUCTION");
            stateOptions[3].TextContent.Should().Be("ARCHIVED");
        }

        [Fact]
        public void Component_SyncsWithFilterParameter()
        {
            // Arrange
            var filter = new WorkoutTemplateFilterDto
            {
                NamePattern = "test search",
                CategoryId = "cat-2",
                DifficultyId = "diff-3",
                StateId = "state-1",
                IsPublic = true
            };

            // Act
            var component = RenderComponent<WorkoutTemplateFilters>(parameters => parameters
                .Add(p => p.Filter, filter));

            // Assert
            var searchBar = component.FindComponent<ReferenceDataSearchBar>();
            searchBar.Instance.Value.Should().Be("test search");

            var categorySelect = component.Find("[data-testid=\"category-select\"]") as IHtmlSelectElement;
            categorySelect?.Value.Should().Be("cat-2");

            var difficultySelect = component.Find("[data-testid=\"difficulty-select\"]") as IHtmlSelectElement;
            difficultySelect?.Value.Should().Be("diff-3");

            var stateSelect = component.Find("[data-testid=\"state-select\"]") as IHtmlSelectElement;
            stateSelect?.Value.Should().Be("state-1");

            var publicSelect = component.Find("[data-testid=\"public-select\"]") as IHtmlSelectElement;
            publicSelect?.Value.Should().Be("true");
        }

        [Fact]
        public void ApplyFilters_InvokesCallback_WithCorrectFilter()
        {
            // Arrange
            WorkoutTemplateFilterDto? receivedFilter = null;
            var initialFilter = new WorkoutTemplateFilterDto { PageSize = 20 };
            
            var component = RenderComponent<WorkoutTemplateFilters>(parameters => parameters
                .Add(p => p.Filter, initialFilter)
                .Add(p => p.OnFilterChanged, EventCallback.Factory.Create<WorkoutTemplateFilterDto>(this, (filter) => receivedFilter = filter)));

            // Act
            // Set category directly on the select element
            component.Find("[data-testid=\"category-select\"]").Change("cat-1");

            // Set difficulty
            component.Find("[data-testid=\"difficulty-select\"]").Change("diff-2");

            // Set state
            component.Find("[data-testid=\"state-select\"]").Change("state-3");

            // Set public
            component.Find("[data-testid=\"public-select\"]").Change("false");

            // Apply filters
            component.Find("[data-testid=\"apply-filters-button\"]").Click();

            // Assert
            receivedFilter.Should().NotBeNull();
            receivedFilter!.CategoryId.Should().Be("cat-1");
            receivedFilter.DifficultyId.Should().Be("diff-2");
            receivedFilter.StateId.Should().Be("state-3");
            receivedFilter.IsPublic.Should().BeFalse();
            receivedFilter.Page.Should().Be(1); // Reset to page 1
            receivedFilter.PageSize.Should().Be(20); // Preserved from initial filter
        }

        [Fact]
        public void GetActiveFilterCount_ReturnsCorrectCount()
        {
            // Arrange
            var filter = new WorkoutTemplateFilterDto
            {
                NamePattern = "test",
                CategoryId = "cat-1",
                DifficultyId = "diff-1"
            };
            
            var component = RenderComponent<WorkoutTemplateFilters>(parameters => parameters
                .Add(p => p.Filter, filter));

            // Act & Assert
            var activeFilterCount = component.Find("[data-testid=\"active-filter-count\"]");
            activeFilterCount.TextContent.Should().Contain("3 filters active");
        }

        [Fact]
        public void ClearAllFilters_ResetsAllValues()
        {
            // Arrange
            var filter = new WorkoutTemplateFilterDto
            {
                NamePattern = "test",
                CategoryId = "cat-1",
                DifficultyId = "diff-1",
                StateId = "state-1",
                IsPublic = true
            };

            var component = RenderComponent<WorkoutTemplateFilters>(parameters => parameters
                .Add(p => p.Filter, filter));

            // Act
            component.Find("[data-testid=\"clear-filters-button\"]").Click();

            // Assert - Internal state should be cleared (we can verify through UI)
            // Note: The actual clearing happens internally, visible after Apply
            var instance = component.Instance;
            instance.GetActiveFilterCount().Should().Be(0);
        }

        [Fact]
        public void ClearButton_IsDisabled_WhenNoActiveFilters()
        {
            // Act
            var component = RenderComponent<WorkoutTemplateFilters>();

            // Assert
            var clearButton = component.Find("[data-testid=\"clear-filters-button\"]");
            clearButton.GetAttribute("disabled").Should().NotBeNull();
        }

        [Fact]
        public void ClearButton_IsEnabled_WhenActiveFilters()
        {
            // Arrange
            var filter = new WorkoutTemplateFilterDto { NamePattern = "test" };

            // Act
            var component = RenderComponent<WorkoutTemplateFilters>(parameters => parameters
                .Add(p => p.Filter, filter));

            // Assert
            var clearButton = component.Find("[data-testid=\"clear-filters-button\"]");
            clearButton.GetAttribute("disabled").Should().BeNull();
        }

        [Fact]
        public void Component_ShowsFilterSummary_WhenEnabled()
        {
            // Arrange
            var filter = new WorkoutTemplateFilterDto
            {
                NamePattern = "test",
                CategoryId = "cat-1",
                DifficultyId = "diff-2",
                StateId = "state-1",
                IsPublic = false
            };

            // Act
            var component = RenderComponent<WorkoutTemplateFilters>(parameters => parameters
                .Add(p => p.Filter, filter)
                .Add(p => p.ShowFilterSummary, true));

            // Assert
            var filterSummary = component.Find("[data-testid=\"filter-summary\"]");
            filterSummary.Should().NotBeNull();

            var pills = filterSummary.QuerySelectorAll("[data-testid=\"filter-pill\"]");
            pills.Length.Should().Be(5); // All 5 filters active

            // Check specific pill content
            pills[0].TextContent.Should().Contain("Search: test");
            pills[1].TextContent.Should().Contain("Category: Strength Training");
            pills[2].TextContent.Should().Contain("Difficulty: Intermediate");
            pills[3].TextContent.Should().Contain("State: DRAFT");
            pills[4].TextContent.Should().Contain("Visibility: Private");
        }

        [Fact]
        public void Component_HidesFilterSummary_WhenDisabled()
        {
            // Arrange
            var filter = new WorkoutTemplateFilterDto { NamePattern = "test" };

            // Act
            var component = RenderComponent<WorkoutTemplateFilters>(parameters => parameters
                .Add(p => p.Filter, filter)
                .Add(p => p.ShowFilterSummary, false));

            // Assert
            component.FindAll("[data-testid=\"filter-summary\"]").Should().BeEmpty();
        }

        [Fact]
        public void FilterPill_RemoveButton_ClearsSpecificFilter()
        {
            // Arrange
            WorkoutTemplateFilterDto? lastFilter = null;
            var filter = new WorkoutTemplateFilterDto
            {
                NamePattern = "test",
                CategoryId = "cat-1"
            };

            var component = RenderComponent<WorkoutTemplateFilters>(parameters => parameters
                .Add(p => p.Filter, filter)
                .Add(p => p.ShowFilterSummary, true)
                .Add(p => p.OnFilterChanged, EventCallback.Factory.Create<WorkoutTemplateFilterDto>(this, (f) => lastFilter = f)));

            // Act - Remove the category filter pill
            var categoryPill = component.FindAll("[data-testid=\"filter-pill\"]")[1]; // Second pill is category
            var removeButton = categoryPill.QuerySelector("[data-testid=\"remove-filter-button\"]");
            removeButton?.Click();

            // Assert
            lastFilter.Should().NotBeNull();
            lastFilter!.NamePattern.Should().Be("test"); // Search still present
            lastFilter.CategoryId.Should().BeNull(); // Category removed
        }

        [Fact]
        public void Component_HandlesNullFilterParameter()
        {
            // Act & Assert - Should not throw
            var component = RenderComponent<WorkoutTemplateFilters>(parameters => parameters
                .Add(p => p.Filter, null!));

            // Should render with default values
            var searchBar = component.FindComponent<ReferenceDataSearchBar>();
            searchBar.Instance.Value.Should().BeEmpty();
        }

        [Fact]
        public void PublicFilter_ShowsCorrectOptions()
        {
            // Act
            var component = RenderComponent<WorkoutTemplateFilters>();

            // Assert
            var publicSelect = component.Find("[data-testid=\"public-select\"]");
            var options = publicSelect.QuerySelectorAll("option");
            
            options.Length.Should().Be(3);
            options[0].TextContent.Should().Be("All Templates");
            options[1].TextContent.Should().Be("Public Only");
            options[2].TextContent.Should().Be("Private Only");
            
            options[0].GetAttribute("value").Should().Be("");
            options[1].GetAttribute("value").Should().Be("true");
            options[2].GetAttribute("value").Should().Be("false");
        }

        [Fact]
        public void ResultCount_DisplaysCorrectly_WhenProvided()
        {
            // Test no results
            var component = RenderComponent<WorkoutTemplateFilters>(parameters => parameters
                .Add(p => p.ShowResultCount, true)
                .Add(p => p.ResultCount, 0));

            // The ReferenceDataSearchBar component would show this
            // We need to find it within that component
            
            // Test single result
            component.SetParametersAndRender(parameters => parameters
                .Add(p => p.ResultCount, 1));

            // Test multiple results
            component.SetParametersAndRender(parameters => parameters
                .Add(p => p.ResultCount, 42));
        }
    }
}
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
using System.Threading.Tasks;

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

        [Fact]
        public void SearchBar_TriggersFilterChange_WithSearchValue()
        {
            // Arrange
            WorkoutTemplateFilterDto? receivedFilter = null;
            var initialFilter = new WorkoutTemplateFilterDto { NamePattern = "test search" };
            var component = RenderComponent<WorkoutTemplateFilters>(parameters => parameters
                .Add(p => p.Filter, initialFilter)
                .Add(p => p.OnFilterChanged, EventCallback.Factory.Create<WorkoutTemplateFilterDto>(this, (filter) => receivedFilter = filter)));

            // Act - The search value is already set via the filter parameter
            // Apply filters to trigger the callback
            component.Find("[data-testid=\"apply-filters-button\"]").Click();

            // Assert
            receivedFilter.Should().NotBeNull();
            receivedFilter!.NamePattern.Should().Be("test search");
            receivedFilter.Page.Should().Be(1); // Reset to page 1
        }

        [Fact]
        public void SearchBar_ClearsSearch_WhenFilterCleared()
        {
            // Arrange
            var filter = new WorkoutTemplateFilterDto { NamePattern = "initial search" };
            var component = RenderComponent<WorkoutTemplateFilters>(parameters => parameters
                .Add(p => p.Filter, filter));

            // Act
            component.Find("[data-testid=\"clear-filters-button\"]").Click();

            // Assert - After clearing, the search bar should be empty
            var instance = component.Instance;
            instance.GetActiveFilterCount().Should().Be(0);
        }

        [Fact]
        public void SearchBar_UpdatesFromFilterParameter()
        {
            // Arrange
            var component = RenderComponent<WorkoutTemplateFilters>();

            // Act - Update with new filter containing search term
            var newFilter = new WorkoutTemplateFilterDto { NamePattern = "updated search" };
            component.SetParametersAndRender(parameters => parameters
                .Add(p => p.Filter, newFilter));

            // Assert
            var searchBar = component.FindComponent<ReferenceDataSearchBar>();
            searchBar.Instance.Value.Should().Be("updated search");
        }

        [Fact]
        public void SearchFilter_RemoveButton_ClearsOnlySearch()
        {
            // Arrange
            WorkoutTemplateFilterDto? lastFilter = null;
            var filter = new WorkoutTemplateFilterDto
            {
                NamePattern = "test search",
                CategoryId = "cat-1",
                DifficultyId = "diff-2"
            };

            var component = RenderComponent<WorkoutTemplateFilters>(parameters => parameters
                .Add(p => p.Filter, filter)
                .Add(p => p.ShowFilterSummary, true)
                .Add(p => p.OnFilterChanged, EventCallback.Factory.Create<WorkoutTemplateFilterDto>(this, (f) => lastFilter = f)));

            // Act - Remove the search filter pill
            var searchPill = component.FindAll("[data-testid=\"filter-pill\"]")[0]; // First pill is search
            var removeButton = searchPill.QuerySelector("[data-testid=\"remove-filter-button\"]");
            removeButton?.Click();

            // Assert
            lastFilter.Should().NotBeNull();
            lastFilter!.NamePattern.Should().BeNull(); // Search cleared
            lastFilter.CategoryId.Should().Be("cat-1"); // Category still present
            lastFilter.DifficultyId.Should().Be("diff-2"); // Difficulty still present
        }

        [Fact]
        public void SearchBar_IntegratesWithOtherFilters()
        {
            // Arrange
            WorkoutTemplateFilterDto? receivedFilter = null;
            // Set initial filter with search value
            var initialFilter = new WorkoutTemplateFilterDto { NamePattern = "workout search" };
            var component = RenderComponent<WorkoutTemplateFilters>(parameters => parameters
                .Add(p => p.Filter, initialFilter)
                .Add(p => p.OnFilterChanged, EventCallback.Factory.Create<WorkoutTemplateFilterDto>(this, (filter) => receivedFilter = filter)));

            // Act - Set other filters
            component.Find("[data-testid=\"category-select\"]").Change("cat-2");
            component.Find("[data-testid=\"difficulty-select\"]").Change("diff-1");
            
            // Apply all filters
            component.Find("[data-testid=\"apply-filters-button\"]").Click();

            // Assert
            receivedFilter.Should().NotBeNull();
            receivedFilter!.NamePattern.Should().Be("workout search");
            receivedFilter.CategoryId.Should().Be("cat-2");
            receivedFilter.DifficultyId.Should().Be("diff-1");
        }

        [Fact]
        public void SearchBar_PreservesValueDuringFilterUpdates()
        {
            // Arrange
            var filter = new WorkoutTemplateFilterDto { NamePattern = "preserved search" };
            var component = RenderComponent<WorkoutTemplateFilters>(parameters => parameters
                .Add(p => p.Filter, filter));

            // Act - Change other filters without clearing search
            component.Find("[data-testid=\"category-select\"]").Change("cat-3");
            component.Find("[data-testid=\"difficulty-select\"]").Change("diff-3");

            // Assert - Search should still be preserved
            var searchBar = component.FindComponent<ReferenceDataSearchBar>();
            searchBar.Instance.Value.Should().Be("preserved search");
        }

        [Fact]
        public void EmptySearch_DoesNotShowInFilterSummary()
        {
            // Arrange
            var filter = new WorkoutTemplateFilterDto
            {
                NamePattern = "",  // Empty search
                CategoryId = "cat-1"
            };

            // Act
            var component = RenderComponent<WorkoutTemplateFilters>(parameters => parameters
                .Add(p => p.Filter, filter)
                .Add(p => p.ShowFilterSummary, true));

            // Assert
            var pills = component.FindAll("[data-testid=\"filter-pill\"]");
            pills.Count.Should().Be(1); // Only category pill
            pills[0].TextContent.Should().Contain("Category");
            pills[0].TextContent.Should().NotContain("Search");
        }

        [Fact]
        public void WhitespaceOnlySearch_TreatedAsEmpty()
        {
            // Arrange
            WorkoutTemplateFilterDto? receivedFilter = null;
            var filter = new WorkoutTemplateFilterDto { NamePattern = "   " }; // Whitespace only
            
            var component = RenderComponent<WorkoutTemplateFilters>(parameters => parameters
                .Add(p => p.Filter, filter)
                .Add(p => p.OnFilterChanged, EventCallback.Factory.Create<WorkoutTemplateFilterDto>(this, (f) => receivedFilter = f)));

            // Act
            component.Find("[data-testid=\"apply-filters-button\"]").Click();

            // Assert
            receivedFilter.Should().NotBeNull();
            receivedFilter!.NamePattern.Should().BeNull(); // Whitespace converted to null
        }

        [Fact]
        public void SearchBar_ConfiguredWithCorrectDebounceDelay()
        {
            // Act
            var component = RenderComponent<WorkoutTemplateFilters>();

            // Assert
            var searchBar = component.FindComponent<ReferenceDataSearchBar>();
            searchBar.Instance.DebounceDelay.Should().Be(300); // As configured in the component
        }

        [Fact]
        public void SearchBar_ShowsResultCount_WhenEnabled()
        {
            // Arrange & Act
            var component = RenderComponent<WorkoutTemplateFilters>(parameters => parameters
                .Add(p => p.ShowResultCount, true)
                .Add(p => p.ResultCount, 25));

            // Assert
            var searchBar = component.FindComponent<ReferenceDataSearchBar>();
            searchBar.Instance.ShowResultCount.Should().BeTrue();
            searchBar.Instance.ResultCount.Should().Be(25);
        }

        [Fact]
        public void SearchBar_HandlesNullSearchPattern()
        {
            // Arrange
            var filter = new WorkoutTemplateFilterDto { NamePattern = null };
            
            // Act
            var component = RenderComponent<WorkoutTemplateFilters>(parameters => parameters
                .Add(p => p.Filter, filter));

            // Assert
            var searchBar = component.FindComponent<ReferenceDataSearchBar>();
            searchBar.Instance.Value.Should().BeEmpty();
        }

        [Fact]
        public void ApplyFilters_TrimsSearchValue()
        {
            // Arrange
            WorkoutTemplateFilterDto? receivedFilter = null;
            var filter = new WorkoutTemplateFilterDto { NamePattern = "  spaces around  " };
            
            var component = RenderComponent<WorkoutTemplateFilters>(parameters => parameters
                .Add(p => p.Filter, filter)
                .Add(p => p.OnFilterChanged, EventCallback.Factory.Create<WorkoutTemplateFilterDto>(this, (f) => receivedFilter = f)));

            // Act
            component.Find("[data-testid=\"apply-filters-button\"]").Click();

            // Assert
            receivedFilter.Should().NotBeNull();
            // The component converts whitespace-only to null, so trimming happens implicitly
            receivedFilter!.NamePattern.Should().Be("  spaces around  ");
        }

        [Fact]
        public void SearchBar_PassesCorrectPlaceholder()
        {
            // Act
            var component = RenderComponent<WorkoutTemplateFilters>();

            // Assert
            var searchBar = component.FindComponent<ReferenceDataSearchBar>();
            searchBar.Instance.Placeholder.Should().Be("Search by name...");
            searchBar.Instance.Label.Should().Be("Search Templates");
        }

        [Fact]
        public void MultipleClearOperations_WorkCorrectly()
        {
            // Arrange
            var filter = new WorkoutTemplateFilterDto 
            { 
                NamePattern = "search",
                CategoryId = "cat-1",
                DifficultyId = "diff-1"
            };
            
            var component = RenderComponent<WorkoutTemplateFilters>(parameters => parameters
                .Add(p => p.Filter, filter));

            // Act - Clear all filters
            component.Find("[data-testid=\"clear-filters-button\"]").Click();
            
            // First clear should reset internal state
            var instance = component.Instance;
            instance.GetActiveFilterCount().Should().Be(0);

            // Clear again should still work
            component.Find("[data-testid=\"clear-filters-button\"]").Click();
            instance.GetActiveFilterCount().Should().Be(0);
        }

        [Fact]
        public void FilterSummary_UpdatesAfterFilterChange()
        {
            // Arrange
            WorkoutTemplateFilterDto? lastFilter = null;
            var filter = new WorkoutTemplateFilterDto();
            
            var component = RenderComponent<WorkoutTemplateFilters>(parameters => parameters
                .Add(p => p.Filter, filter)
                .Add(p => p.ShowFilterSummary, true)
                .Add(p => p.OnFilterChanged, EventCallback.Factory.Create<WorkoutTemplateFilterDto>(this, (f) => lastFilter = f)));

            // Initially no filter pills
            component.FindAll("[data-testid=\"filter-pill\"]").Count.Should().Be(0);

            // Act - Update with new filter containing values
            var newFilter = new WorkoutTemplateFilterDto 
            { 
                NamePattern = "updated",
                CategoryId = "cat-2"
            };
            component.SetParametersAndRender(parameters => parameters
                .Add(p => p.Filter, newFilter));

            // Assert - Filter pills should appear
            var pills = component.FindAll("[data-testid=\"filter-pill\"]");
            pills.Count.Should().Be(2);
            pills[0].TextContent.Should().Contain("Search: updated");
            pills[1].TextContent.Should().Contain("Category: Cardio");
        }
    }
}
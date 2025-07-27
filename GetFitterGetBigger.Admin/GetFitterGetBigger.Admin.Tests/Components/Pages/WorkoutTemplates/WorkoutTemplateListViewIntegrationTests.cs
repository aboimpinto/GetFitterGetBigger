using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Pages.WorkoutTemplates;
using GetFitterGetBigger.Admin.Components.WorkoutTemplates;
using GetFitterGetBigger.Admin.Components.Shared;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Builders;
using GetFitterGetBigger.Admin.Tests.TestHelpers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Bunit.TestDoubles;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using AngleSharp.Dom;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.WorkoutTemplates
{
    public class WorkoutTemplateListViewIntegrationTests : WorkoutTemplateTestBase
    {
        private readonly TestAuthenticationStateProvider _authStateProvider;
        private FakeNavigationManager _navManager;
        private WorkoutTemplatePagedResultDto _testPageData;

        public WorkoutTemplateListViewIntegrationTests()
        {
            _authStateProvider = new TestAuthenticationStateProvider("testuser");
            Services.AddSingleton<AuthenticationStateProvider>(_authStateProvider);
            Services.AddAuthorizationCore();
            
            _navManager = Services.GetRequiredService<FakeNavigationManager>();
            _navManager.NavigateTo("http://localhost/workout-templates");

            // Setup default test data
            _testPageData = new WorkoutTemplatePagedResultDto
            {
                Items = new List<WorkoutTemplateDto>
                {
                    new WorkoutTemplateDtoBuilder()
                        .WithId("template-1")
                        .WithName("Beginner Full Body")
                        .WithCategory("strength", "Strength")
                        .WithDifficulty("beginner", "Beginner")
                        .WithWorkoutState("published", "PUBLISHED")
                        .Build(),
                    new WorkoutTemplateDtoBuilder()
                        .WithId("template-2")
                        .WithName("Intermediate Cardio")
                        .WithCategory("cardio", "Cardio")
                        .WithDifficulty("intermediate", "Intermediate")
                        .WithWorkoutState("draft", "DRAFT")
                        .Build(),
                    new WorkoutTemplateDtoBuilder()
                        .WithId("template-3")
                        .WithName("Advanced Strength")
                        .WithCategory("strength", "Strength")
                        .WithDifficulty("advanced", "Advanced")
                        .WithWorkoutState("published", "PUBLISHED")
                        .Build()
                },
                TotalCount = 3,
                TotalPages = 1
            };
        }

        [Fact]
        public async Task CompleteWorkflow_LoadsTemplates_AppliesFilters_NavigatesToTemplate()
        {
            // Arrange - Setup initial data
            MockWorkoutTemplateListStore.Setup(x => x.IsLoading).Returns(false);
            MockWorkoutTemplateListStore.Setup(x => x.CurrentPage).Returns(_testPageData);
            MockWorkoutTemplateListStore.Setup(x => x.CurrentFilter).Returns(new WorkoutTemplateFilterDto());
            MockWorkoutTemplateListStore.Setup(x => x.ErrorMessage).Returns((string?)null);
            MockWorkoutReferenceDataStore.Setup(x => x.WorkoutCategories).Returns(new List<ReferenceDataDto>());
            MockWorkoutReferenceDataStore.Setup(x => x.DifficultyLevels).Returns(new List<ReferenceDataDto>());
            MockWorkoutReferenceDataStore.Setup(x => x.WorkoutStates).Returns(new List<ReferenceDataDto>());

            var component = RenderComponent<GetFitterGetBigger.Admin.Components.Pages.WorkoutTemplates.WorkoutTemplates>();
            
            // Wait for initial load
            component.WaitForElement("[data-testid='template-grid']", TimeSpan.FromSeconds(2));
            
            // Assert - Initial state loaded correctly
            var templateCards = component.FindAll("[data-testid='workout-template-card']");
            templateCards.Should().HaveCount(3);
            
            // Act - Apply name filter
            var searchInput = component.Find("[data-testid='search-input']");
            searchInput.Input("Advanced");
            
            // Mock the filtered result
            var filteredData = new WorkoutTemplatePagedResultDto
            {
                Items = _testPageData.Items.Where(t => t.Name.Contains("Advanced")).ToList(),
                TotalCount = 1,
                TotalPages = 1
            };
            MockWorkoutTemplateListStore.Setup(x => x.CurrentPage).Returns(filteredData);
            
            var applyButton = component.Find("[data-testid='apply-filter-button']");
            await component.InvokeAsync(() => applyButton.Click());
            
            // Assert - Filter applied
            component.WaitForAssertion(() =>
            {
                var filteredCards = component.FindAll("[data-testid='workout-template-card']");
                filteredCards.Should().HaveCount(1);
                filteredCards[0].QuerySelector("[data-testid='template-title']")!.TextContent
                    .Should().Contain("Advanced");
            });
            
            // Act - Clear filters
            var clearButton = component.Find("[data-testid='clear-filter-button']");
            MockWorkoutTemplateListStore.Setup(x => x.CurrentPage).Returns(_testPageData); // Reset data after filter
            await component.InvokeAsync(() => clearButton.Click());
            
            // Assert - All templates shown again
            component.WaitForAssertion(() =>
            {
                component.FindAll("[data-testid='workout-template-card']").Should().HaveCount(3);
            });
            
            // Act - Click on a template title
            var firstTemplateTitle = component.FindAll("[data-testid='template-title']")[0];
            await component.InvokeAsync(() => firstTemplateTitle.Click());
            
            // Assert - Navigation occurred to a template detail page
            _navManager.Uri.Should().Match("http://localhost/workout-templates/*");
        }

        [Fact]
        public async Task FilteringWorkflow_CombinesMultipleFilters()
        {
            // Arrange
            MockWorkoutTemplateListStore.Setup(x => x.IsLoading).Returns(false);
            MockWorkoutTemplateListStore.Setup(x => x.CurrentPage).Returns(_testPageData);
            MockWorkoutTemplateListStore.Setup(x => x.CurrentFilter).Returns(new WorkoutTemplateFilterDto());
            MockWorkoutTemplateListStore.Setup(x => x.ErrorMessage).Returns((string?)null);
            MockWorkoutReferenceDataStore.Setup(x => x.WorkoutCategories).Returns(new List<ReferenceDataDto>());
            MockWorkoutReferenceDataStore.Setup(x => x.DifficultyLevels).Returns(new List<ReferenceDataDto>());
            MockWorkoutReferenceDataStore.Setup(x => x.WorkoutStates).Returns(new List<ReferenceDataDto>());

            var component = RenderComponent<GetFitterGetBigger.Admin.Components.Pages.WorkoutTemplates.WorkoutTemplates>();
            component.WaitForElement("[data-testid='template-grid']");
            
            // Act - Apply multiple filters
            component.Find("[data-testid='category-select']").Change("strength");
            component.Find("[data-testid='difficulty-select']").Change("intermediate");
            component.Find("[data-testid='state-select']").Change("published");
            
            var applyButton = component.Find("[data-testid='apply-filter-button']");
            await component.InvokeAsync(() => applyButton.Click());
            
            // Assert - Filters combined correctly in store
            MockWorkoutTemplateListStore.Verify(x => x.LoadTemplatesAsync(
                It.Is<WorkoutTemplateFilterDto>(f => 
                    f != null &&
                    f.CategoryId == "strength" && 
                    f.DifficultyId == "intermediate" && 
                    f.StateId == "published")), Times.Once);
        }

        [Fact]
        public async Task SortingWorkflow_ChangesSortOrder()
        {
            // Arrange
            MockWorkoutTemplateListStore.Setup(x => x.IsLoading).Returns(false);
            MockWorkoutTemplateListStore.Setup(x => x.CurrentPage).Returns(_testPageData);
            MockWorkoutTemplateListStore.Setup(x => x.CurrentFilter).Returns(new WorkoutTemplateFilterDto());
            MockWorkoutTemplateListStore.Setup(x => x.ErrorMessage).Returns((string?)null);
            MockWorkoutReferenceDataStore.Setup(x => x.WorkoutCategories).Returns(new List<ReferenceDataDto>());
            MockWorkoutReferenceDataStore.Setup(x => x.DifficultyLevels).Returns(new List<ReferenceDataDto>());
            MockWorkoutReferenceDataStore.Setup(x => x.WorkoutStates).Returns(new List<ReferenceDataDto>());

            var component = RenderComponent<GetFitterGetBigger.Admin.Components.Pages.WorkoutTemplates.WorkoutTemplates>();
            component.WaitForElement("[data-testid='template-grid']");
            
            // Act - Change sort to updated date
            var sortSelect = component.Find("[data-testid='sort-select']");
            sortSelect.Change("updated");
            
            // Toggle sort direction
            var sortButton = component.Find("[data-testid='sort-direction-button']");
            await component.InvokeAsync(() => sortButton.Click());
            
            // Assert - Templates are sorted by updated date ascending
            var list = component.FindComponent<WorkoutTemplateList>();
            var sortedTemplates = list.Instance.GetSortedTemplates().ToList();
            sortedTemplates.Should().BeInAscendingOrder(t => t.UpdatedAt);
        }

        [Fact]
        public async Task PaginationWorkflow_NavigatesThroughPages()
        {
            // Arrange - Setup data with multiple pages
            var paginatedData = new WorkoutTemplatePagedResultDto
            {
                Items = Enumerable.Range(1, 10).Select(i => 
                    new WorkoutTemplateDtoBuilder()
                        .WithId($"template-{i}")
                        .WithName($"Template {i}")
                        .Build()
                ).ToList(),
                TotalCount = 25,
                TotalPages = 3
            };
            
            MockWorkoutTemplateListStore.Setup(x => x.IsLoading).Returns(false);
            MockWorkoutTemplateListStore.Setup(x => x.CurrentPage).Returns(paginatedData);
            MockWorkoutTemplateListStore.Setup(x => x.CurrentFilter).Returns(new WorkoutTemplateFilterDto { Page = 1 });
            MockWorkoutTemplateListStore.Setup(x => x.ErrorMessage).Returns((string?)null);
            MockWorkoutReferenceDataStore.Setup(x => x.WorkoutCategories).Returns(new List<ReferenceDataDto>());
            MockWorkoutReferenceDataStore.Setup(x => x.DifficultyLevels).Returns(new List<ReferenceDataDto>());
            MockWorkoutReferenceDataStore.Setup(x => x.WorkoutStates).Returns(new List<ReferenceDataDto>());

            var component = RenderComponent<GetFitterGetBigger.Admin.Components.Pages.WorkoutTemplates.WorkoutTemplates>();
            component.WaitForElement("[data-testid='template-grid']");
            
            // Assert - First page loaded
            component.FindAll("[data-testid='workout-template-card']").Should().HaveCount(10);
            
            // Act - Navigate to page 2
            var nextButton = component.Find("[data-testid='next-page-button']");
            await component.InvokeAsync(() => nextButton.Click());
            
            // Assert - Page 2 loaded
            MockWorkoutTemplateListStore.Verify(x => x.LoadTemplatesAsync(
                It.Is<WorkoutTemplateFilterDto>(f => f != null && f.Page == 2)), Times.Once);
        }

        [Fact]
        public void LoadingState_DisplaysSkeletonLoaders()
        {
            // Arrange
            MockWorkoutTemplateListStore.Setup(x => x.IsLoading).Returns(true);
            MockWorkoutTemplateListStore.Setup(x => x.CurrentPage).Returns((WorkoutTemplatePagedResultDto?)null);
            MockWorkoutTemplateListStore.Setup(x => x.CurrentFilter).Returns(new WorkoutTemplateFilterDto());
            MockWorkoutTemplateListStore.Setup(x => x.ErrorMessage).Returns((string?)null);
            MockWorkoutReferenceDataStore.Setup(x => x.WorkoutCategories).Returns(new List<ReferenceDataDto>());
            MockWorkoutReferenceDataStore.Setup(x => x.DifficultyLevels).Returns(new List<ReferenceDataDto>());
            MockWorkoutReferenceDataStore.Setup(x => x.WorkoutStates).Returns(new List<ReferenceDataDto>());
            
            // Act
            var component = RenderComponent<GetFitterGetBigger.Admin.Components.Pages.WorkoutTemplates.WorkoutTemplates>();
            
            // Assert
            component.FindComponent<WorkoutTemplateListSkeleton>().Should().NotBeNull();
        }

        [Fact]
        public void EmptyState_DisplaysAppropriateMessage()
        {
            // Arrange
            var emptyData = new WorkoutTemplatePagedResultDto
            {
                Items = new List<WorkoutTemplateDto>(),
                TotalCount = 0,
                TotalPages = 0
            };
            
            MockWorkoutTemplateListStore.Setup(x => x.IsLoading).Returns(false);
            MockWorkoutTemplateListStore.Setup(x => x.CurrentPage).Returns(emptyData);
            MockWorkoutTemplateListStore.Setup(x => x.CurrentFilter).Returns(new WorkoutTemplateFilterDto());
            MockWorkoutTemplateListStore.Setup(x => x.ErrorMessage).Returns((string?)null);
            MockWorkoutReferenceDataStore.Setup(x => x.WorkoutCategories).Returns(new List<ReferenceDataDto>());
            MockWorkoutReferenceDataStore.Setup(x => x.DifficultyLevels).Returns(new List<ReferenceDataDto>());
            MockWorkoutReferenceDataStore.Setup(x => x.WorkoutStates).Returns(new List<ReferenceDataDto>());
            
            // Act
            var component = RenderComponent<GetFitterGetBigger.Admin.Components.Pages.WorkoutTemplates.WorkoutTemplates>();
            
            // Assert
            component.WaitForAssertion(() =>
            {
                var emptyState = component.Find("[data-testid='empty-state']");
                emptyState.TextContent.Should().Contain("No workout templates");
            });
        }

        [Fact]
        public async Task EmptyStateWithFilters_ShowsDifferentMessage()
        {
            // Arrange
            var emptyData = new WorkoutTemplatePagedResultDto
            {
                Items = new List<WorkoutTemplateDto>(),
                TotalCount = 0,
                TotalPages = 0
            };
            
            MockWorkoutTemplateListStore.Setup(x => x.IsLoading).Returns(false);
            MockWorkoutTemplateListStore.Setup(x => x.CurrentPage).Returns(emptyData);
            MockWorkoutTemplateListStore.Setup(x => x.CurrentFilter).Returns(new WorkoutTemplateFilterDto { NamePattern = "NonExistent" });
            MockWorkoutTemplateListStore.Setup(x => x.ErrorMessage).Returns((string?)null);
            MockWorkoutReferenceDataStore.Setup(x => x.WorkoutCategories).Returns(new List<ReferenceDataDto>());
            MockWorkoutReferenceDataStore.Setup(x => x.DifficultyLevels).Returns(new List<ReferenceDataDto>());
            MockWorkoutReferenceDataStore.Setup(x => x.WorkoutStates).Returns(new List<ReferenceDataDto>());

            var component = RenderComponent<GetFitterGetBigger.Admin.Components.Pages.WorkoutTemplates.WorkoutTemplates>();
            
            // Act - Apply filter
            component.Find("[data-testid='search-input']").Input("NonExistent");
            var applyButton = component.Find("[data-testid='apply-filter-button']");
            await component.InvokeAsync(() => applyButton.Click());
            
            // Assert
            component.WaitForAssertion(() =>
            {
                var emptyState = component.Find("[data-testid='empty-state']");
                emptyState.TextContent.Should().Contain("No templates match");
            });
        }

        [Fact]
        public async Task ErrorState_DisplaysErrorAndAllowsRetry()
        {
            // Arrange
            MockWorkoutTemplateListStore.Setup(x => x.IsLoading).Returns(false);
            MockWorkoutTemplateListStore.Setup(x => x.CurrentPage).Returns((WorkoutTemplatePagedResultDto?)null);
            MockWorkoutTemplateListStore.Setup(x => x.CurrentFilter).Returns(new WorkoutTemplateFilterDto());
            MockWorkoutTemplateListStore.Setup(x => x.ErrorMessage).Returns("Failed to load templates");
            MockWorkoutReferenceDataStore.Setup(x => x.WorkoutCategories).Returns(new List<ReferenceDataDto>());
            MockWorkoutReferenceDataStore.Setup(x => x.DifficultyLevels).Returns(new List<ReferenceDataDto>());
            MockWorkoutReferenceDataStore.Setup(x => x.WorkoutStates).Returns(new List<ReferenceDataDto>());
            
            // Act
            var component = RenderComponent<GetFitterGetBigger.Admin.Components.Pages.WorkoutTemplates.WorkoutTemplates>();
            
            // Assert - Error displayed
            component.WaitForAssertion(() =>
            {
                var errorState = component.Find("[data-testid='error-state']");
                errorState.TextContent.Should().Contain("Failed to load templates");
            });
            
            // Act - Click retry
            MockWorkoutTemplateListStore.Setup(x => x.ErrorMessage).Returns((string?)null);
            MockWorkoutTemplateListStore.Setup(x => x.CurrentPage).Returns(_testPageData); // Reset to success
            
            var retryButton = component.Find("[data-testid='retry-button']");
            await component.InvokeAsync(() => retryButton.Click());
            
            // Assert - Data loaded after retry
            MockWorkoutTemplateListStore.Verify(x => x.ClearError(), Times.Once);
            MockWorkoutTemplateListStore.Verify(x => x.RefreshAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateNewButton_NavigatesToCreatePage()
        {
            // Arrange
            MockWorkoutTemplateListStore.Setup(x => x.IsLoading).Returns(false);
            MockWorkoutTemplateListStore.Setup(x => x.CurrentPage).Returns(_testPageData);
            MockWorkoutTemplateListStore.Setup(x => x.CurrentFilter).Returns(new WorkoutTemplateFilterDto());
            MockWorkoutTemplateListStore.Setup(x => x.ErrorMessage).Returns((string?)null);
            MockWorkoutReferenceDataStore.Setup(x => x.WorkoutCategories).Returns(new List<ReferenceDataDto>());
            MockWorkoutReferenceDataStore.Setup(x => x.DifficultyLevels).Returns(new List<ReferenceDataDto>());
            MockWorkoutReferenceDataStore.Setup(x => x.WorkoutStates).Returns(new List<ReferenceDataDto>());

            var component = RenderComponent<GetFitterGetBigger.Admin.Components.Pages.WorkoutTemplates.WorkoutTemplates>();
            component.WaitForElement("[data-testid='template-grid']");
            
            // Act
            var createButton = component.Find("[data-testid='create-new-button']");
            await component.InvokeAsync(() => createButton.Click());
            
            // Assert
            _navManager.Uri.Should().EndWith("/workout-templates/new");
        }

        [Fact]
        public async Task TemplateCardActions_TriggersCorrectHandlers()
        {
            // Arrange
            MockWorkoutTemplateListStore.Setup(x => x.IsLoading).Returns(false);
            MockWorkoutTemplateListStore.Setup(x => x.CurrentPage).Returns(_testPageData);
            MockWorkoutTemplateListStore.Setup(x => x.CurrentFilter).Returns(new WorkoutTemplateFilterDto());
            MockWorkoutTemplateListStore.Setup(x => x.ErrorMessage).Returns((string?)null);
            
            // Setup reference data
            MockWorkoutReferenceDataStore.Setup(x => x.WorkoutCategories).Returns(new List<ReferenceDataDto>
            {
                new() { Id = "strength", Value = "Strength" },
                new() { Id = "cardio", Value = "Cardio" }
            });
            MockWorkoutReferenceDataStore.Setup(x => x.DifficultyLevels).Returns(new List<ReferenceDataDto>
            {
                new() { Id = "beginner", Value = "Beginner" },
                new() { Id = "intermediate", Value = "Intermediate" },
                new() { Id = "advanced", Value = "Advanced" }
            });
            MockWorkoutReferenceDataStore.Setup(x => x.WorkoutStates).Returns(new List<ReferenceDataDto>
            {
                new() { Id = "draft", Value = "DRAFT" },
                new() { Id = "published", Value = "PUBLISHED" }
            });

            var component = RenderComponent<GetFitterGetBigger.Admin.Components.Pages.WorkoutTemplates.WorkoutTemplates>();
            component.WaitForElement("[data-testid='template-grid']");
            
            // Debug - Check what cards are rendered
            var cards = component.FindAll("[data-testid='workout-template-card']");
            cards.Should().NotBeEmpty("There should be workout template cards");
            
            // Act - Click view action
            var viewButtons = component.FindAll("[data-testid='action-view']");
            viewButtons.Should().NotBeEmpty("There should be view buttons on the template cards");
            
            await component.InvokeAsync(() => viewButtons[0].Click());
            
            // Assert - Navigated to detail view
            _navManager.Uri.Should().Match("http://localhost/workout-templates/template-*");
            
            // Reset navigation
            _navManager.NavigateTo("http://localhost/workout-templates");
            
            // Re-render the component after navigation reset
            component.Render();
            
            // Act - Click edit action (only draft templates have edit buttons)
            // Find the edit button for the draft template (template-2)
            var editButtons = component.FindAll("[data-testid='edit-button']");
            editButtons.Should().NotBeEmpty("There should be at least one edit button for draft templates");
            
            await component.InvokeAsync(() => editButtons[0].Click());
            
            // Assert - Navigated to edit view
            _navManager.Uri.Should().Match("http://localhost/workout-templates/template-*/edit");
        }
    }
}
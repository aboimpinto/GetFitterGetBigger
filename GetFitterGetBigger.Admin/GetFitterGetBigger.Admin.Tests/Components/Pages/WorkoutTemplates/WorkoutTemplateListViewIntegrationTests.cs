using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Pages.WorkoutTemplates;
using GetFitterGetBigger.Admin.Components.WorkoutTemplates;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Builders;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Bunit.TestDoubles;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using AngleSharp.Dom;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.WorkoutTemplates
{
    public class WorkoutTemplateListViewIntegrationTests : TestContext
    {
        private readonly MockWorkoutTemplateStateService _mockStateService;
        private readonly TestAuthenticationStateProvider _authStateProvider;
        private FakeNavigationManager _navManager;

        public WorkoutTemplateListViewIntegrationTests()
        {
            _mockStateService = new MockWorkoutTemplateStateService();
            _authStateProvider = new TestAuthenticationStateProvider("testuser");
            
            Services.AddSingleton<IWorkoutTemplateStateService>(_mockStateService);
            Services.AddSingleton<AuthenticationStateProvider>(_authStateProvider);
            Services.AddAuthorizationCore();
            
            _navManager = Services.GetRequiredService<FakeNavigationManager>();
            _navManager.NavigateTo("http://localhost/workout-templates-demo");
        }

        [Fact]
        public async Task CompleteWorkflow_LoadsTemplates_AppliesFilters_NavigatesToTemplate()
        {
            // Arrange - Setup initial data
            _mockStateService.SetupInitialData();
            var component = RenderComponent<WorkoutTemplateListDemo>();
            
            // Wait for initial load
            component.WaitForElement("[data-testid='template-grid']", TimeSpan.FromSeconds(2));
            
            // Assert - Initial state loaded correctly
            var templateCards = component.FindAll("[data-testid='workout-template-card']");
            templateCards.Should().HaveCount(3);
            
            // Act - Apply name filter
            var searchInput = component.Find("[data-testid='search-input']");
            searchInput.Input("Advanced");
            
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
            _mockStateService.SetupInitialData(); // Reset data after filter
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
            _mockStateService.SetupInitialData();
            var component = RenderComponent<WorkoutTemplateListDemo>();
            component.WaitForElement("[data-testid='template-grid']");
            
            // Act - Apply multiple filters
            component.Find("[data-testid='category-select']").Change("strength");
            component.Find("[data-testid='difficulty-select']").Change("intermediate");
            component.Find("[data-testid='state-select']").Change("published");
            
            await component.InvokeAsync(() => 
                component.Find("[data-testid='apply-filter-button']").Click()
            );
            
            // Assert - Filters applied correctly
            _mockStateService.LastAppliedFilter.Should().NotBeNull();
            _mockStateService.LastAppliedFilter!.CategoryId.Should().Be("strength");
            _mockStateService.LastAppliedFilter.DifficultyId.Should().Be("intermediate");
            _mockStateService.LastAppliedFilter.StateId.Should().Be("published");
            
            // Verify UI updated
            component.WaitForAssertion(() =>
            {
                var templates = component.FindAll("[data-testid='workout-template-card']");
                templates.Should().HaveCount(1);
                templates[0].QuerySelector("[data-testid='template-title']")!.TextContent
                    .Should().Contain("Intermediate Strength");
            });
        }

        [Fact]
        public async Task SortingWorkflow_ChangesSortOrder()
        {
            // Arrange
            _mockStateService.SetupInitialData();
            var component = RenderComponent<WorkoutTemplateListDemo>();
            component.WaitForElement("[data-testid='template-grid']");
            
            // Get initial order
            var initialNames = component.FindAll("[data-testid='template-title']")
                .Select(e => e.TextContent.Trim())
                .ToList();
            
            // Act - Change sort to created date
            var sortSelect = component.Find("[data-testid='sort-select']");
            sortSelect.Change("created");
            
            // Assert - Templates reordered by created date
            component.WaitForAssertion(() =>
            {
                var sortedNames = component.FindAll("[data-testid='template-title']")
                    .Select(e => e.TextContent.Trim())
                    .ToList();
                sortedNames.Should().NotEqual(initialNames);
            });
            
            // Act - Toggle sort direction
            var sortDirectionButton = component.Find("[data-testid='sort-direction-button']");
            await component.InvokeAsync(() => sortDirectionButton.Click());
            
            // Assert - Order reversed
            component.WaitForAssertion(() =>
            {
                var reversedNames = component.FindAll("[data-testid='template-title']")
                    .Select(e => e.TextContent.Trim())
                    .ToList();
                reversedNames.Should().BeInDescendingOrder();
            });
        }

        [Fact]
        public async Task PaginationWorkflow_NavigatesThroughPages()
        {
            // Arrange - Setup data with multiple pages
            _mockStateService.SetupPaginatedData(totalItems: 25, pageSize: 10);
            var component = RenderComponent<WorkoutTemplateListDemo>();
            component.WaitForElement("[data-testid='pagination']");
            
            // Assert - Initial page 1
            var pagination = component.Find("[data-testid='pagination']");
            var paginationText = System.Text.RegularExpressions.Regex.Replace(pagination.TextContent, @"\s+", " ").Trim();
            paginationText.Should().Contain("Showing 1 to 10 of 25");
            
            // Act - Go to page 2
            var page2Button = component.Find("[data-testid='page-2-button']");
            await component.InvokeAsync(() => page2Button.Click());
            
            // Assert - Page 2 loaded
            component.WaitForAssertion(() =>
            {
                var updatedPagination = component.Find("[data-testid='pagination']");
                var updatedText = System.Text.RegularExpressions.Regex.Replace(updatedPagination.TextContent, @"\s+", " ").Trim();
                updatedText.Should().Contain("Showing 11 to 20 of 25");
                _mockStateService.LastAppliedFilter!.Page.Should().Be(2);
            });
            
            // Act - Go to next page
            var nextButton = component.Find("[data-testid='next-page-button']");
            await component.InvokeAsync(() => nextButton.Click());
            
            // Assert - Page 3 loaded
            component.WaitForAssertion(() =>
            {
                var finalPagination = component.Find("[data-testid='pagination']");
                var finalText = System.Text.RegularExpressions.Regex.Replace(finalPagination.TextContent, @"\s+", " ").Trim();
                finalText.Should().Contain("Showing 21 to 25 of 25");
                _mockStateService.LastAppliedFilter!.Page.Should().Be(3);
            });
            
            // Assert - Next button disabled on last page
            nextButton.GetAttribute("disabled").Should().NotBeNull();
        }

        [Fact]
        public async Task TemplateCardActions_TriggersCorrectHandlers()
        {
            // Arrange
            _mockStateService.SetupInitialData();
            var component = RenderComponent<WorkoutTemplateListDemo>();
            component.WaitForElement("[data-testid='template-grid']");
            
            // Act - Edit template (find first card's edit button)
            var firstCard = component.FindAll("[data-testid='workout-template-card']")[0];
            var editButton = firstCard.QuerySelector("[data-testid='edit-button']");
            await component.InvokeAsync(() => editButton!.Click());
            
            // Assert - Navigation to edit page (first card might be sorted differently)
            _navManager.Uri.Should().Match("http://localhost/workout-templates/*/edit");
            
            // Reset navigation
            _navManager.NavigateTo("http://localhost/workout-templates-demo");
            
            // Act - Duplicate template
            var duplicateButton = firstCard.QuerySelector("[data-testid='duplicate-button']");
            await component.InvokeAsync(() => duplicateButton!.Click());
            
            // Assert - Duplicate called with correct data
            _mockStateService.LastDuplicatedTemplateId.Should().NotBeNull();
            _mockStateService.LastDuplicateName.Should().EndWith(" (Copy)");
            
            // Act - Delete template (find draft template - third card)
            var cards = component.FindAll("[data-testid='workout-template-card']");
            IElement? deleteButton = null;
            
            // Find a card with a delete button (should be draft or archived)
            foreach (var card in cards)
            {
                var btn = card.QuerySelector("[data-testid='delete-button']");
                if (btn != null)
                {
                    deleteButton = btn;
                    break;
                }
            }
            
            if (deleteButton != null)
            {
                await component.InvokeAsync(() => deleteButton.Click());
            }
            
            // Assert - Delete called (draft template can be deleted)
            _mockStateService.LastDeletedTemplateId.Should().NotBeNull();
        }

        [Fact]
        public async Task LoadingState_DisplaysSkeletonLoaders()
        {
            // Arrange - Set loading state before rendering
            _mockStateService.IsLoading = true;
            _mockStateService.CurrentPage = null; // No data yet
            
            var component = RenderComponent<WorkoutTemplateListDemo>();
            
            // Assert - Loading skeletons displayed
            var loadingState = component.Find("[data-testid='loading-state']");
            loadingState.Should().NotBeNull();
            
            var skeletons = loadingState.QuerySelectorAll(".animate-pulse");
            skeletons.Should().HaveCount(8);
            
            // Act - Complete loading
            _mockStateService.IsLoading = false;
            _mockStateService.SetupInitialData();
            
            await component.InvokeAsync(() =>
            {
                _mockStateService.TriggerStateChange();
            });
            
            // Assert - Content displayed
            component.WaitForAssertion(() =>
            {
                component.FindAll("[data-testid='loading-state']").Should().BeEmpty();
                component.FindAll("[data-testid='workout-template-card']").Should().HaveCount(3);
            });
        }

        [Fact]
        public async Task ErrorState_DisplaysErrorAndAllowsRetry()
        {
            // Arrange - Set error state
            _mockStateService.ErrorMessage = "Failed to load workout templates";
            var component = RenderComponent<WorkoutTemplateListDemo>();
            
            // Assert - Error displayed
            var errorState = component.Find("[data-testid='error-state']");
            errorState.TextContent.Should().Contain("Failed to load workout templates");
            
            // Act - Click retry
            var retryButton = component.Find("[data-testid='retry-button']");
            await component.InvokeAsync(() => retryButton.Click());
            
            // Assert - Retry attempted (InitializeAsync is called, then LoadWorkoutTemplates)
            component.WaitForAssertion(() =>
            {
                _mockStateService.LoadWorkoutTemplatesCallCount.Should().BeGreaterThan(0);
            });
        }

        [Fact]
        public async Task EmptyState_DisplaysAppropriateMessage()
        {
            // Arrange - No templates
            _mockStateService.SetupEmptyData();
            var component = RenderComponent<WorkoutTemplateListDemo>();
            
            component.WaitForElement("[data-testid='empty-state']");
            
            // Assert - Empty state with create button
            var emptyState = component.Find("[data-testid='empty-state']");
            emptyState.TextContent.Should().Contain("No workout templates found");
            emptyState.TextContent.Should().Contain("Get started by creating your first workout template");
            
            // Act - Click create new
            var createButton = component.Find("[data-testid='create-new-button']");
            await component.InvokeAsync(() => createButton.Click());
            
            // Assert - Navigation to create page
            _navManager.Uri.Should().Be("http://localhost/workout-templates/new");
        }

        [Fact]
        public async Task EmptyStateWithFilters_ShowsDifferentMessage()
        {
            // Arrange - Apply filters that return no results
            _mockStateService.SetupInitialData();
            var component = RenderComponent<WorkoutTemplateListDemo>();
            component.WaitForElement("[data-testid='template-grid']");
            
            // Act - Apply filter that returns no results
            component.Find("[data-testid='search-input']").Input("NonExistentTemplate");
            await component.InvokeAsync(() => 
                component.Find("[data-testid='apply-filter-button']").Click()
            );
            
            _mockStateService.SetupEmptyData();
            await component.InvokeAsync(() =>
            {
                _mockStateService.TriggerStateChange();
            });
            
            // Assert - Empty state with filter message
            component.WaitForAssertion(() =>
            {
                var emptyState = component.Find("[data-testid='empty-state']");
                emptyState.TextContent.Should().Contain("Try adjusting your filters");
            });
        }

        [Fact]
        public async Task CreateNewButton_NavigatesToCreatePage()
        {
            // Arrange
            _mockStateService.SetupInitialData();
            var component = RenderComponent<WorkoutTemplateListDemo>();
            component.WaitForElement("[data-testid='template-grid']");
            
            // Act - Click create button in header
            var createButton = component.Find("[data-testid='create-template-button']");
            await component.InvokeAsync(() => createButton.Click());
            
            // Assert - Navigation and return page stored
            _navManager.Uri.Should().Be("http://localhost/workout-templates/new");
            _mockStateService.ReturnPageStored.Should().BeTrue();
        }

        // Helper class for mocking IWorkoutTemplateStateService
        private class MockWorkoutTemplateStateService : IWorkoutTemplateStateService
        {
            private Action? _onChange;
            public event Action? OnChange
            {
                add => _onChange = value;
                remove => _onChange = value;
            }

            public WorkoutTemplatePagedResultDto? CurrentPage { get; set; }
            public WorkoutTemplateFilterDto CurrentFilter { get; set; } = new WorkoutTemplateFilterDtoBuilder().Build();
            public bool IsLoading { get; set; }
            public string? ErrorMessage { get; set; }
            public string? SuccessMessage { get; set; }
            public WorkoutTemplateDto? SelectedTemplate { get; set; }
            public bool IsLoadingTemplate { get; set; }
            public IEnumerable<ReferenceDataDto> WorkoutCategories { get; set; } = new List<ReferenceDataDto>
            {
                new() { Id = "strength", Value = "Strength Training" },
                new() { Id = "cardio", Value = "Cardio" },
                new() { Id = "flexibility", Value = "Flexibility" }
            };
            public IEnumerable<ReferenceDataDto> DifficultyLevels { get; set; } = new List<ReferenceDataDto>
            {
                new() { Id = "beginner", Value = "Beginner" },
                new() { Id = "intermediate", Value = "Intermediate" },
                new() { Id = "advanced", Value = "Advanced" }
            };
            public IEnumerable<ReferenceDataDto> WorkoutStates { get; set; } = new List<ReferenceDataDto>
            {
                new() { Id = "draft", Value = "Draft" },
                new() { Id = "published", Value = "Published" },
                new() { Id = "archived", Value = "Archived" }
            };
            public IEnumerable<ReferenceDataDto> WorkoutObjectives { get; set; } = Enumerable.Empty<ReferenceDataDto>();
            public IEnumerable<ReferenceDataDto> WorkoutPlanTypes { get; set; } = Enumerable.Empty<ReferenceDataDto>();
            public IEnumerable<ExerciseDto> AvailableExercises { get; set; } = Enumerable.Empty<ExerciseDto>();
            public bool IsLoadingReferenceData { get; set; }
            public bool HasStoredPage => ReturnPageStored;
            
            // Test tracking properties
            public WorkoutTemplateFilterDto? LastAppliedFilter { get; private set; }
            public string? LastDuplicatedTemplateId { get; private set; }
            public string? LastDuplicateName { get; private set; }
            public string? LastDeletedTemplateId { get; private set; }
            public bool ReturnPageStored { get; private set; }
            public int LoadWorkoutTemplatesCallCount { get; private set; }

            public void SetupInitialData()
            {
                CurrentPage = new WorkoutTemplatePagedResultDto
                {
                    Items = new List<WorkoutTemplateDto>
                    {
                        new WorkoutTemplateDto
                        {
                            Id = "1",
                            Name = "Beginner Full Body",
                            Description = "A full body workout for beginners",
                            Category = new ReferenceDataDto { Id = "strength", Value = "Strength Training" },
                            Difficulty = new ReferenceDataDto { Id = "beginner", Value = "Beginner" },
                            WorkoutState = new ReferenceDataDto { Id = "published", Value = "Published" },
                            CreatedAt = DateTime.UtcNow.AddDays(-10),
                            UpdatedAt = DateTime.UtcNow.AddDays(-5)
                        },
                        new WorkoutTemplateDto
                        {
                            Id = "2",
                            Name = "Intermediate Strength",
                            Description = "Strength training for intermediate level",
                            Category = new ReferenceDataDto { Id = "strength", Value = "Strength Training" },
                            Difficulty = new ReferenceDataDto { Id = "intermediate", Value = "Intermediate" },
                            WorkoutState = new ReferenceDataDto { Id = "published", Value = "Published" },
                            CreatedAt = DateTime.UtcNow.AddDays(-20),
                            UpdatedAt = DateTime.UtcNow.AddDays(-3)
                        },
                        new WorkoutTemplateDto
                        {
                            Id = "3",
                            Name = "Advanced HIIT",
                            Description = "High intensity interval training",
                            Category = new ReferenceDataDto { Id = "cardio", Value = "Cardio" },
                            Difficulty = new ReferenceDataDto { Id = "advanced", Value = "Advanced" },
                            WorkoutState = new ReferenceDataDto { Id = "draft", Value = "Draft" },
                            CreatedAt = DateTime.UtcNow.AddDays(-5),
                            UpdatedAt = DateTime.UtcNow.AddDays(-1)
                        }
                    },
                    TotalCount = 3,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalPages = 1
                };
                IsLoading = false;
                ErrorMessage = null;
            }

            public void SetupPaginatedData(int totalItems, int pageSize)
            {
                var items = Enumerable.Range(1, pageSize).Select(i => new WorkoutTemplateDto
                {
                    Id = i.ToString(),
                    Name = $"Template {i}",
                    Description = $"Description {i}",
                    Category = WorkoutCategories.First(),
                    Difficulty = DifficultyLevels.First(),
                    WorkoutState = WorkoutStates.First(),
                    CreatedAt = DateTime.UtcNow.AddDays(-i),
                    UpdatedAt = DateTime.UtcNow
                }).ToList();

                CurrentPage = new WorkoutTemplatePagedResultDto
                {
                    Items = items,
                    TotalCount = totalItems,
                    PageNumber = 1,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
                };
                IsLoading = false;
                ErrorMessage = null;
            }

            public void SetupEmptyData()
            {
                CurrentPage = new WorkoutTemplatePagedResultDto
                {
                    Items = new List<WorkoutTemplateDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalPages = 0
                };
                IsLoading = false;
                ErrorMessage = null;
            }

            public void TriggerStateChange()
            {
                _onChange?.Invoke();
            }

            public Task InitializeAsync()
            {
                if (!string.IsNullOrEmpty(ErrorMessage))
                {
                    return Task.CompletedTask;
                }
                if (CurrentPage == null && !IsLoading)
                {
                    SetupInitialData();
                }
                return Task.CompletedTask;
            }

            public Task LoadWorkoutTemplatesAsync(WorkoutTemplateFilterDto? filter = null)
            {
                LoadWorkoutTemplatesCallCount++;
                LastAppliedFilter = filter ?? CurrentFilter;
                CurrentFilter = LastAppliedFilter;
                
                // Simulate filtering
                if (filter != null)
                {
                    var filteredItems = CurrentPage?.Items ?? new List<WorkoutTemplateDto>();
                    
                    if (!string.IsNullOrWhiteSpace(filter.NamePattern))
                    {
                        filteredItems = filteredItems.Where(t => t.Name.Contains(filter.NamePattern, StringComparison.OrdinalIgnoreCase)).ToList();
                    }
                    
                    if (!string.IsNullOrWhiteSpace(filter.CategoryId))
                    {
                        filteredItems = filteredItems.Where(t => t.Category?.Id == filter.CategoryId).ToList();
                    }
                    
                    if (!string.IsNullOrWhiteSpace(filter.DifficultyId))
                    {
                        filteredItems = filteredItems.Where(t => t.Difficulty?.Id == filter.DifficultyId).ToList();
                    }
                    
                    if (!string.IsNullOrWhiteSpace(filter.StateId))
                    {
                        filteredItems = filteredItems.Where(t => t.WorkoutState?.Id == filter.StateId).ToList();
                    }
                    
                    // Update page for pagination
                    if (filter.Page > 1)
                    {
                        var newItems = Enumerable.Range((filter.Page - 1) * filter.PageSize + 1, Math.Min(filter.PageSize, 25 - (filter.Page - 1) * filter.PageSize))
                            .Select(i => new WorkoutTemplateDto
                            {
                                Id = i.ToString(),
                                Name = $"Template {i}",
                                Description = $"Description {i}",
                                Category = WorkoutCategories.First(),
                                Difficulty = DifficultyLevels.First(),
                                WorkoutState = WorkoutStates.First(),
                                CreatedAt = DateTime.UtcNow.AddDays(-i),
                                UpdatedAt = DateTime.UtcNow
                            }).ToList();
                        
                        CurrentPage = new WorkoutTemplatePagedResultDto
                        {
                            Items = newItems,
                            TotalCount = CurrentPage?.TotalCount ?? 25,
                            PageNumber = filter.Page,
                            PageSize = filter.PageSize,
                            TotalPages = CurrentPage?.TotalPages ?? 3
                        };
                    }
                    else
                    {
                        CurrentPage = new WorkoutTemplatePagedResultDto
                        {
                            Items = filteredItems,
                            TotalCount = filteredItems.Count,
                            PageNumber = 1,
                            PageSize = filter.PageSize,
                            TotalPages = (int)Math.Ceiling((double)filteredItems.Count / filter.PageSize)
                        };
                    }
                }
                
                TriggerStateChange();
                return Task.CompletedTask;
            }

            public Task LoadWorkoutTemplateByIdAsync(string id)
            {
                SelectedTemplate = CurrentPage?.Items.FirstOrDefault(t => t.Id == id);
                return Task.CompletedTask;
            }

            public Task CreateWorkoutTemplateAsync(CreateWorkoutTemplateDto dto) => Task.CompletedTask;
            public Task UpdateWorkoutTemplateAsync(string id, UpdateWorkoutTemplateDto dto) => Task.CompletedTask;
            
            public Task DuplicateWorkoutTemplateAsync(string id, DuplicateWorkoutTemplateDto dto)
            {
                LastDuplicatedTemplateId = id;
                LastDuplicateName = dto.NewName;
                return Task.CompletedTask;
            }
            
            public Task DeleteWorkoutTemplateAsync(string id)
            {
                LastDeletedTemplateId = id;
                return Task.CompletedTask;
            }
            
            public Task ChangeWorkoutTemplateStateAsync(string id, ChangeWorkoutStateDto changeState) => Task.CompletedTask;
            public Task LoadReferenceDataAsync() => Task.CompletedTask;
            public Task RefreshCurrentPageAsync()
            {
                ClearError();
                return LoadWorkoutTemplatesAsync(CurrentFilter);
            }
            public Task LoadWorkoutTemplatesWithStoredPageAsync() => LoadWorkoutTemplatesAsync(CurrentFilter);
            public void ClearError() => ErrorMessage = null;
            public void ClearSuccess() => SuccessMessage = null;
            public void ClearSelectedTemplate() => SelectedTemplate = null;
            public void StoreReturnPage() => ReturnPageStored = true;
            public void ClearStoredPage() => ReturnPageStored = false;
        }

        // Test authentication state provider
        private class TestAuthenticationStateProvider : AuthenticationStateProvider
        {
            private readonly string _username;

            public TestAuthenticationStateProvider(string username)
            {
                _username = username;
            }

            public override Task<AuthenticationState> GetAuthenticationStateAsync()
            {
                var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, _username),
                    new Claim("PT-Tier", "true")
                }, "test");

                var user = new ClaimsPrincipal(identity);
                return Task.FromResult(new AuthenticationState(user));
            }
        }
    }
}
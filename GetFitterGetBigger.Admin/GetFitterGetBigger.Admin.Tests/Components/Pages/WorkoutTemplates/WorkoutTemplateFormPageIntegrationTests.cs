using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Pages.WorkoutTemplates;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Builders;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Bunit.TestDoubles;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Web;
using GetFitterGetBigger.Admin.Components.WorkoutTemplates;
using AngleSharp.Dom;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.WorkoutTemplates
{
    /// <summary>
    /// Integration tests for the WorkoutTemplateFormPage component.
    /// Tests the complete workflow of creating and editing workout templates.
    /// </summary>
    public class WorkoutTemplateFormPageIntegrationTests : TestContext
    {
        private readonly MockWorkoutTemplateService _mockTemplateService;
        private readonly MockWorkoutTemplateStateService _mockStateService;
        private readonly FakeNavigationManager _navigationManager;

        public WorkoutTemplateFormPageIntegrationTests()
        {
            _mockTemplateService = new MockWorkoutTemplateService();
            _mockStateService = new MockWorkoutTemplateStateService();

            // Setup services
            Services.AddSingleton<IWorkoutTemplateService>(_mockTemplateService);
            Services.AddSingleton<IWorkoutTemplateStateService>(_mockStateService);
            Services.AddSingleton<ILocalStorageService>(new MockLocalStorageService());
            
            // Setup authentication
            var authContext = this.AddTestAuthorization();
            authContext.SetAuthorized("TestUser");
            authContext.SetClaims(new System.Security.Claims.Claim("ClaimType", "PT-Tier"));

            // Setup JS interop for localStorage (used by form recovery)
            JSInterop.Mode = JSRuntimeMode.Loose;

            _navigationManager = Services.GetRequiredService<FakeNavigationManager>();
            _navigationManager.NavigateTo("http://localhost/workout-templates/new");

            // Setup reference data
            _mockStateService.SetupReferenceData();
        }

        [Fact]
        public async Task CreateWorkoutTemplate_CompleteWorkflow_Success()
        {
            // Arrange
            var expectedTemplate = new WorkoutTemplateDtoBuilder()
                .WithId("created-template-id")
                .WithName("Test Template")
                .WithDescription("Test Description")
                .Build();

            _mockTemplateService.SetupCreateSuccess(expectedTemplate);
            _navigationManager.NavigateTo("http://localhost/workout-templates/new");

            // Act - Render the page
            var cut = RenderComponent<WorkoutTemplateFormPage>();

            // Wait for form to be rendered
            var form = cut.WaitForElement("form", TimeSpan.FromSeconds(2));

            // Get a reference to the form component
            var formComponent = cut.FindComponent<WorkoutTemplateForm>();

            // Fill the model directly
            await cut.InvokeAsync(() =>
            {
                formComponent.Instance.Model.Name = "Test Template";
                formComponent.Instance.Model.Description = "Test Description";
                formComponent.Instance.Model.CategoryId = "category-1";
                formComponent.Instance.Model.DifficultyId = "difficulty-2";
                formComponent.Instance.Model.EstimatedDurationMinutes = 45;
                formComponent.Instance.Model.IsPublic = false;
                
                // Mark as dirty to enable save
                formComponent.Instance.MarkAsDirty();
            });

            // Render to update the UI
            cut.Render();

            // Submit the form
            await cut.InvokeAsync(async () =>
            {
                await formComponent.Instance.HandleValidSubmit();
            });

            // Assert
            _mockTemplateService.CreateCallCount.Should().Be(1);
            _mockTemplateService.LastCreateDto.Should().NotBeNull();
            _mockTemplateService.LastCreateDto!.Name.Should().Be("Test Template");
            _mockTemplateService.LastCreateDto.Description.Should().Be("Test Description");
            _mockTemplateService.LastCreateDto.CategoryId.Should().Be("category-1");
            _mockTemplateService.LastCreateDto.DifficultyId.Should().Be("difficulty-2");
            _mockTemplateService.LastCreateDto.EstimatedDurationMinutes.Should().Be(45);
            
            // Should navigate back to list
            _navigationManager.Uri.Should().Be("http://localhost/workout-templates");
        }

        [Fact]
        public async Task EditWorkoutTemplate_CompleteWorkflow_Success()
        {
            // Arrange
            var existingTemplate = new WorkoutTemplateDtoBuilder()
                .WithId("template-123")
                .WithName("Original Name")
                .WithDescription("Original Description")
                .WithCategory("category-1", "Strength")
                .WithDifficulty("difficulty-1", "Beginner")
                .WithEstimatedDuration(30)
                .WithWorkoutState("DRAFT", "Draft")
                .Build();

            _mockTemplateService.SetupGetByIdSuccess(existingTemplate);
            _mockTemplateService.SetupUpdateSuccess(existingTemplate);

            _navigationManager.NavigateTo("http://localhost/workout-templates/template-123/edit");

            // Act - Render the page
            var cut = RenderComponent<WorkoutTemplateFormPage>(parameters => parameters
                .Add(p => p.Id, "template-123"));

            // Wait for form to be rendered with data
            cut.WaitForElement("form", TimeSpan.FromSeconds(2));

            // Get a reference to the form component
            var formComponent = cut.FindComponent<WorkoutTemplateForm>();

            // Update the model directly
            await cut.InvokeAsync(() =>
            {
                formComponent.Instance.Model.Name = "Updated Name";
                formComponent.Instance.Model.Description = "Updated Description";
                formComponent.Instance.Model.DifficultyId = "difficulty-3";
                
                // Mark as dirty to enable save
                formComponent.Instance.MarkAsDirty();
            });

            // Render to update the UI
            cut.Render();

            // Submit the form
            await cut.InvokeAsync(async () =>
            {
                await formComponent.Instance.HandleValidSubmit();
            });

            // Assert
            _mockTemplateService.UpdateCallCount.Should().Be(1);
            _mockTemplateService.LastUpdateDto.Should().NotBeNull();
            _mockTemplateService.LastUpdateId.Should().Be("template-123");
            _mockTemplateService.LastUpdateDto!.Name.Should().Be("Updated Name");
            _mockTemplateService.LastUpdateDto.Description.Should().Be("Updated Description");
            _mockTemplateService.LastUpdateDto.DifficultyId.Should().Be("difficulty-3");
            
            // Should navigate back to list
            _navigationManager.Uri.Should().Be("http://localhost/workout-templates");
        }

        [Fact]
        public void CreateWorkoutTemplate_ValidationErrors_ShowsErrors()
        {
            // Arrange
            _navigationManager.NavigateTo("http://localhost/workout-templates/new");

            // Act
            var cut = RenderComponent<WorkoutTemplateFormPage>();
            cut.WaitForElement("form", TimeSpan.FromSeconds(2));

            // Get a reference to the form component
            var formComponent = cut.FindComponent<WorkoutTemplateForm>();

            // Check that the form is invalid when required fields are missing
            var isValid = formComponent.Instance.IsFormValid();
            isValid.Should().BeFalse("form should be invalid when required fields are empty");

            // Find the save button - it should be disabled
            var saveButton = cut.Find("button[data-testid='floating-save-button']");
            var isDisabled = saveButton.GetAttribute("disabled");
            isDisabled.Should().NotBeNull("button should be disabled when form is invalid");

            // Should not have called the service
            _mockTemplateService.CreateCallCount.Should().Be(0);
            
            // Should remain on the same page
            _navigationManager.Uri.Should().Contain("/workout-templates/new");
        }

        // Mock implementations (same as original)
        private class MockWorkoutTemplateService : IWorkoutTemplateService
        {
            private readonly List<WorkoutTemplateDto> _templates = new();
            private WorkoutTemplateDto? _templateToReturn;
            private bool _shouldThrowOnCreate;
            private string? _errorMessage;

            public int CreateCallCount { get; private set; }
            public int UpdateCallCount { get; private set; }
            public CreateWorkoutTemplateDto? LastCreateDto { get; private set; }
            public UpdateWorkoutTemplateDto? LastUpdateDto { get; private set; }
            public string? LastUpdateId { get; private set; }

            public void SetupCreateSuccess(WorkoutTemplateDto template)
            {
                _templateToReturn = template;
                _shouldThrowOnCreate = false;
            }
            
            public void SetupCreateFailure(string errorMessage)
            {
                _shouldThrowOnCreate = true;
                _errorMessage = errorMessage;
            }

            public void SetupUpdateSuccess(WorkoutTemplateDto template)
            {
                _templateToReturn = template;
            }

            public void SetupGetByIdSuccess(WorkoutTemplateDto template)
            {
                _templates.Clear();
                _templates.Add(template);
            }

            public Task<WorkoutTemplateDto> CreateWorkoutTemplateAsync(CreateWorkoutTemplateDto createDto)
            {
                CreateCallCount++;
                LastCreateDto = createDto;

                if (_shouldThrowOnCreate)
                {
                    throw new InvalidOperationException(_errorMessage ?? "Create failed");
                }

                return Task.FromResult(_templateToReturn ?? new WorkoutTemplateDtoBuilder()
                    .WithName(createDto.Name)
                    .Build());
            }

            public Task<WorkoutTemplateDto> UpdateWorkoutTemplateAsync(string id, UpdateWorkoutTemplateDto updateDto)
            {
                UpdateCallCount++;
                LastUpdateId = id;
                LastUpdateDto = updateDto;

                return Task.FromResult(_templateToReturn ?? new WorkoutTemplateDtoBuilder()
                    .WithId(id)
                    .WithName(updateDto.Name)
                    .Build());
            }

            public Task<WorkoutTemplateDto?> GetWorkoutTemplateByIdAsync(string id)
            {
                var template = _templates.FirstOrDefault(t => t.Id == id);
                return Task.FromResult(template);
            }

            public Task<WorkoutTemplatePagedResultDto> GetWorkoutTemplatesAsync(WorkoutTemplateFilterDto filter)
            {
                return Task.FromResult(new WorkoutTemplatePagedResultDto
                {
                    Items = _templates,
                    TotalCount = _templates.Count,
                    PageNumber = filter.Page,
                    PageSize = filter.PageSize,
                    TotalPages = _templates.Count > 0 ? (int)Math.Ceiling(_templates.Count / (double)filter.PageSize) : 0,
                    HasPreviousPage = filter.Page > 1,
                    HasNextPage = filter.Page < (int)Math.Ceiling(_templates.Count / (double)filter.PageSize)
                });
            }

            public Task DeleteWorkoutTemplateAsync(string id)
            {
                _templates.RemoveAll(t => t.Id == id);
                return Task.CompletedTask;
            }

            public Task<WorkoutTemplateDto> ChangeWorkoutTemplateStateAsync(string id, ChangeWorkoutStateDto changeState)
            {
                var template = _templates.FirstOrDefault(t => t.Id == id);
                if (template == null)
                {
                    throw new InvalidOperationException("Template not found");
                }
                return Task.FromResult(template);
            }

            public Task<WorkoutTemplateDto> DuplicateWorkoutTemplateAsync(string id, DuplicateWorkoutTemplateDto duplicate)
            {
                var template = _templates.FirstOrDefault(t => t.Id == id);
                if (template == null)
                {
                    throw new InvalidOperationException("Template not found");
                }

                var duplicatedTemplate = new WorkoutTemplateDtoBuilder()
                    .WithName(duplicate.NewName ?? $"{template.Name} (Copy)")
                    .Build();

                _templates.Add(duplicatedTemplate);
                return Task.FromResult(duplicatedTemplate);
            }

            public Task<List<WorkoutTemplateDto>> SearchTemplatesByNameAsync(string namePattern)
            {
                var results = _templates.Where(t => t.Name.Contains(namePattern, StringComparison.OrdinalIgnoreCase)).ToList();
                return Task.FromResult(results);
            }

            public Task<List<WorkoutTemplateDto>> GetTemplatesByCategoryAsync(string categoryId)
            {
                var results = _templates.Where(t => t.Category?.Id == categoryId).ToList();
                return Task.FromResult(results);
            }

            public Task<List<WorkoutTemplateDto>> GetTemplatesByDifficultyAsync(string difficultyId)
            {
                var results = _templates.Where(t => t.Difficulty?.Id == difficultyId).ToList();
                return Task.FromResult(results);
            }

            public Task<List<WorkoutTemplateDto>> GetTemplatesByStateAsync(string stateId)
            {
                var results = _templates.Where(t => t.WorkoutState?.Id == stateId).ToList();
                return Task.FromResult(results);
            }

            public Task<List<WorkoutTemplateExerciseDto>> GetTemplateExercisesAsync(string templateId)
            {
                return Task.FromResult(new List<WorkoutTemplateExerciseDto>());
            }

            public Task<bool> CheckTemplateNameExistsAsync(string name)
            {
                var exists = _templates.Any(t => t.Name == name);
                return Task.FromResult(exists);
            }

            public Task<List<ReferenceDataDto>> GetWorkoutCategoriesAsync()
            {
                return Task.FromResult(new List<ReferenceDataDto>());
            }

            public Task<List<ReferenceDataDto>> GetDifficultyLevelsAsync()
            {
                return Task.FromResult(new List<ReferenceDataDto>());
            }

            public Task<List<ReferenceDataDto>> GetWorkoutStatesAsync()
            {
                return Task.FromResult(new List<ReferenceDataDto>());
            }

            public Task<List<ReferenceDataDto>> GetWorkoutObjectivesAsync()
            {
                return Task.FromResult(new List<ReferenceDataDto>());
            }
        }

        private class MockWorkoutTemplateStateService : IWorkoutTemplateStateService
        {
            public event Action? OnChange { add { } remove { } }

            public WorkoutTemplatePagedResultDto? CurrentPage { get; private set; }
            public WorkoutTemplateFilterDto CurrentFilter { get; } = new();
            public bool IsLoading { get; private set; }
            public string? ErrorMessage { get; private set; }
            public bool HasStoredPage { get; } = false;
            public WorkoutTemplateDto? SelectedTemplate { get; private set; }
            public bool IsLoadingTemplate { get; private set; }
            public IEnumerable<ReferenceDataDto> WorkoutCategories { get; private set; } = new List<ReferenceDataDto>();
            public IEnumerable<ReferenceDataDto> DifficultyLevels { get; private set; } = new List<ReferenceDataDto>();
            public IEnumerable<ReferenceDataDto> WorkoutStates { get; private set; } = new List<ReferenceDataDto>();
            public IEnumerable<ReferenceDataDto> WorkoutObjectives { get; private set; } = new List<ReferenceDataDto>();
            public bool IsLoadingReferenceData { get; private set; }

            public void SetupReferenceData()
            {
                WorkoutCategories = new List<ReferenceDataDto>
                {
                    new() { Id = "category-1", Value = "Strength", Description = "Strength training" },
                    new() { Id = "category-2", Value = "Cardio", Description = "Cardiovascular training" }
                };

                DifficultyLevels = new List<ReferenceDataDto>
                {
                    new() { Id = "difficulty-1", Value = "Beginner", Description = "For beginners" },
                    new() { Id = "difficulty-2", Value = "Intermediate", Description = "For intermediate users" },
                    new() { Id = "difficulty-3", Value = "Advanced", Description = "For advanced users" }
                };

                WorkoutStates = new List<ReferenceDataDto>
                {
                    new() { Id = "DRAFT", Value = "Draft", Description = "Draft state" },
                    new() { Id = "PRODUCTION", Value = "Production", Description = "Production state" },
                    new() { Id = "ARCHIVED", Value = "Archived", Description = "Archived state" }
                };

                WorkoutObjectives = new List<ReferenceDataDto>
                {
                    new() { Id = "objective-1", Value = "Build Muscle", Description = "Muscle building" },
                    new() { Id = "objective-2", Value = "Lose Fat", Description = "Fat loss" }
                };
            }

            public Task InitializeAsync() => Task.CompletedTask;
            public Task LoadWorkoutTemplatesAsync(WorkoutTemplateFilterDto? filter = null) => Task.CompletedTask;
            public Task LoadWorkoutTemplatesWithStoredPageAsync() => Task.CompletedTask;
            public Task LoadWorkoutTemplateByIdAsync(string id) => Task.CompletedTask;
            public Task CreateWorkoutTemplateAsync(CreateWorkoutTemplateDto template) => Task.CompletedTask;
            public Task UpdateWorkoutTemplateAsync(string id, UpdateWorkoutTemplateDto template) => Task.CompletedTask;
            public Task DeleteWorkoutTemplateAsync(string id) => Task.CompletedTask;
            public Task ChangeWorkoutTemplateStateAsync(string id, ChangeWorkoutStateDto changeState) => Task.CompletedTask;
            public Task DuplicateWorkoutTemplateAsync(string id, DuplicateWorkoutTemplateDto duplicate) => Task.CompletedTask;
            public Task RefreshCurrentPageAsync() => Task.CompletedTask;
            public void ClearSelectedTemplate() => SelectedTemplate = null;
            public void ClearError() => ErrorMessage = null;
            public void SetError(string message) => ErrorMessage = message;
            public void StoreReturnPage() { }
            public void ClearStoredPage() { }
        }

        private class MockLocalStorageService : ILocalStorageService
        {
            private readonly Dictionary<string, string> _storage = new();

            public Task SetItemAsync(string key, string value)
            {
                _storage[key] = value;
                return Task.CompletedTask;
            }

            public Task<string?> GetItemAsync(string key)
            {
                return Task.FromResult(_storage.TryGetValue(key, out var value) ? value : null);
            }

            public Task RemoveItemAsync(string key)
            {
                _storage.Remove(key);
                return Task.CompletedTask;
            }
        }
    }
}
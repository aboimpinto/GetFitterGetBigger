using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Pages.WorkoutTemplates;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.Results;
using GetFitterGetBigger.Admin.Models.Errors;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Services.Stores;
using GetFitterGetBigger.Admin.Builders;
using Microsoft.AspNetCore.Components;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Bunit.TestDoubles;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authorization;
using GetFitterGetBigger.Admin.Tests.TestHelpers;
using Moq;
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
        private readonly Mock<IWorkoutTemplateFormStore> _mockFormStore;
        private readonly Mock<IWorkoutReferenceDataStore> _mockReferenceDataStore;
        private readonly Mock<IToastService> _mockToastService;
        private readonly FakeNavigationManager _navigationManager;

        public WorkoutTemplateFormPageIntegrationTests()
        {
            _mockTemplateService = new MockWorkoutTemplateService();
            _mockFormStore = new Mock<IWorkoutTemplateFormStore>();
            _mockReferenceDataStore = new Mock<IWorkoutReferenceDataStore>();
            _mockToastService = new Mock<IToastService>();

            // Setup reference data
            var categories = new List<ReferenceDataDto>
            {
                new() { Id = "category-1", Value = "Strength", Description = "Strength training" },
                new() { Id = "category-2", Value = "Cardio", Description = "Cardiovascular training" }
            };

            var difficulties = new List<ReferenceDataDto>
            {
                new() { Id = "difficulty-1", Value = "Beginner", Description = "For beginners" },
                new() { Id = "difficulty-2", Value = "Intermediate", Description = "For intermediate users" },
                new() { Id = "difficulty-3", Value = "Advanced", Description = "For advanced users" }
            };

            var objectives = new List<ReferenceDataDto>
            {
                new() { Id = "objective-1", Value = "Build Muscle", Description = "Muscle building" },
                new() { Id = "objective-2", Value = "Lose Fat", Description = "Fat loss" }
            };

            _mockReferenceDataStore.Setup(x => x.WorkoutCategories).Returns(categories);
            _mockReferenceDataStore.Setup(x => x.DifficultyLevels).Returns(difficulties);
            _mockReferenceDataStore.Setup(x => x.WorkoutObjectives).Returns(objectives);
            _mockReferenceDataStore.Setup(x => x.LoadReferenceDataAsync()).Returns(Task.CompletedTask);

            // Setup services
            Services.AddSingleton<IWorkoutTemplateService>(_mockTemplateService);
            Services.AddSingleton(_mockFormStore.Object);
            Services.AddSingleton(_mockReferenceDataStore.Object);
            Services.AddSingleton<IToastService>(_mockToastService.Object);
            Services.AddSingleton<ILocalStorageService>(new MockLocalStorageService());
            
            // Setup authentication
            var authContext = this.AddTestAuthorization();
            authContext.SetAuthorized("TestUser");
            authContext.SetClaims(new System.Security.Claims.Claim("ClaimType", "PT-Tier"));

            // Setup JS interop for localStorage (used by form recovery)
            JSInterop.Mode = JSRuntimeMode.Loose;

            _navigationManager = Services.GetRequiredService<FakeNavigationManager>();
            _navigationManager.NavigateTo("http://localhost/workout-templates/new");
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
            
            // Setup form store to call the service
            _mockFormStore.Setup(x => x.CreateTemplateAsync(It.IsAny<CreateWorkoutTemplateDto>()))
                .Returns(async (CreateWorkoutTemplateDto dto) => 
                {
                    var result = await _mockTemplateService.CreateWorkoutTemplateAsync(dto);
                    if (result.IsSuccess && result.Data != null)
                        return result.Data;
                    throw new Exception(result.Errors.FirstOrDefault()?.Message ?? "Create failed");
                });
            
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

            var updatedTemplate = new WorkoutTemplateDtoBuilder()
                .WithId("template-123")
                .WithName("Updated Name")
                .WithDescription("Updated Description")
                .WithCategory("category-1", "Strength")
                .WithDifficulty("difficulty-3", "Advanced")
                .WithEstimatedDuration(45)
                .WithWorkoutState("DRAFT", "Draft")
                .Build();

            _mockTemplateService.SetupGetByIdSuccess(existingTemplate);
            _mockTemplateService.SetupUpdateSuccess(updatedTemplate);
            
            // Setup form store to call the service
            _mockFormStore.Setup(x => x.UpdateTemplateAsync(It.IsAny<string>(), It.IsAny<UpdateWorkoutTemplateDto>()))
                .Returns(async (string id, UpdateWorkoutTemplateDto dto) => 
                {
                    var result = await _mockTemplateService.UpdateWorkoutTemplateAsync(id, dto);
                    return result.Data!;
                });

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

            public Task<ServiceResult<WorkoutTemplateDto>> CreateWorkoutTemplateAsync(CreateWorkoutTemplateDto createDto)
            {
                CreateCallCount++;
                LastCreateDto = createDto;

                if (_shouldThrowOnCreate)
                {
                    return Task.FromResult(ServiceResult<WorkoutTemplateDto>.Failure(
                        new ServiceError(ServiceErrorCode.DependencyFailure, _errorMessage ?? "Create failed")));
                }

                var template = _templateToReturn ?? new WorkoutTemplateDtoBuilder()
                    .WithName(createDto.Name)
                    .Build();
                    
                return Task.FromResult(ServiceResult<WorkoutTemplateDto>.Success(template));
            }

            public Task<ServiceResult<WorkoutTemplateDto>> UpdateWorkoutTemplateAsync(string id, UpdateWorkoutTemplateDto updateDto)
            {
                UpdateCallCount++;
                LastUpdateId = id;
                LastUpdateDto = updateDto;

                var template = _templateToReturn ?? new WorkoutTemplateDtoBuilder()
                    .WithId(id)
                    .WithName(updateDto.Name)
                    .Build();
                    
                return Task.FromResult(ServiceResult<WorkoutTemplateDto>.Success(template));
            }

            public Task<ServiceResult<WorkoutTemplateDto>> GetWorkoutTemplateByIdAsync(string id)
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return Task.FromResult(ServiceResult<WorkoutTemplateDto>.Failure(
                        ServiceError.ValidationRequired("Template ID")));
                }

                var template = _templates.FirstOrDefault(t => t.Id == id);
                
                if (template == null)
                {
                    return Task.FromResult(ServiceResult<WorkoutTemplateDto>.Success(WorkoutTemplateDto.Empty));
                }

                return Task.FromResult(ServiceResult<WorkoutTemplateDto>.Success(template));
            }

            public Task<ServiceResult<WorkoutTemplatePagedResultDto>> GetWorkoutTemplatesAsync(WorkoutTemplateFilterDto filter)
            {
                var result = new WorkoutTemplatePagedResultDto
                {
                    Items = _templates,
                    TotalCount = _templates.Count,
                    PageNumber = filter.Page,
                    PageSize = filter.PageSize,
                    TotalPages = _templates.Count > 0 ? (int)Math.Ceiling(_templates.Count / (double)filter.PageSize) : 0,
                    HasPreviousPage = filter.Page > 1,
                    HasNextPage = filter.Page < (int)Math.Ceiling(_templates.Count / (double)filter.PageSize)
                };
                return Task.FromResult(ServiceResult<WorkoutTemplatePagedResultDto>.Success(result));
            }

            public Task<ServiceResult<bool>> DeleteWorkoutTemplateAsync(string id)
            {
                _templates.RemoveAll(t => t.Id == id);
                return Task.FromResult(ServiceResult<bool>.Success(true));
            }

            public Task<ServiceResult<WorkoutTemplateDto>> ChangeWorkoutTemplateStateAsync(string id, ChangeWorkoutStateDto changeState)
            {
                var template = _templates.FirstOrDefault(t => t.Id == id);
                if (template == null)
                {
                    return Task.FromResult(ServiceResult<WorkoutTemplateDto>.Failure(
                        ServiceError.TemplateNotFound(id)));
                }
                return Task.FromResult(ServiceResult<WorkoutTemplateDto>.Success(template));
            }

            public Task<ServiceResult<WorkoutTemplateDto>> DuplicateWorkoutTemplateAsync(string id, DuplicateWorkoutTemplateDto duplicate)
            {
                var template = _templates.FirstOrDefault(t => t.Id == id);
                if (template == null)
                {
                    return Task.FromResult(ServiceResult<WorkoutTemplateDto>.Failure(
                        ServiceError.TemplateNotFound(id)));
                }

                var duplicatedTemplate = new WorkoutTemplateDtoBuilder()
                    .WithName(duplicate.NewName ?? $"{template.Name} (Copy)")
                    .Build();

                _templates.Add(duplicatedTemplate);
                return Task.FromResult(ServiceResult<WorkoutTemplateDto>.Success(duplicatedTemplate));
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


            public Task<ServiceResult<bool>> CheckTemplateNameExistsAsync(string name)
            {
                var exists = _templates.Any(t => t.Name == name);
                return Task.FromResult(ServiceResult<bool>.Success(exists));
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

        // Removed MockWorkoutTemplateStateService class as it's no longer needed

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
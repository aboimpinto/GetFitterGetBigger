using AngleSharp.Dom;
using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.WorkoutTemplates;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.ComponentModel.DataAnnotations;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;
using static GetFitterGetBigger.Admin.Components.WorkoutTemplates.WorkoutTemplateForm;

namespace GetFitterGetBigger.Admin.Tests.Components.WorkoutTemplates;

public class WorkoutTemplateFormTests : TestContext
{
    private readonly Mock<IWorkoutTemplateService> _mockWorkoutTemplateService;
    private readonly Mock<IWorkoutTemplateStateService> _mockStateService;
    private readonly List<ReferenceDataDto> _mockCategories;
    private readonly List<ReferenceDataDto> _mockDifficulties;
    private readonly List<ReferenceDataDto> _mockObjectives;

    public WorkoutTemplateFormTests()
    {
        _mockWorkoutTemplateService = new Mock<IWorkoutTemplateService>();
        _mockStateService = new Mock<IWorkoutTemplateStateService>();
        
        _mockCategories = new List<ReferenceDataDto>
        {
            new() { Id = "cat1", Value = "Strength", Description = "Strength training" },
            new() { Id = "cat2", Value = "Cardio", Description = "Cardiovascular training" }
        };
        
        _mockDifficulties = new List<ReferenceDataDto>
        {
            new() { Id = "diff1", Value = "Beginner", Description = "For beginners" },
            new() { Id = "diff2", Value = "Intermediate", Description = "For intermediate users" },
            new() { Id = "diff3", Value = "Advanced", Description = "For advanced users" }
        };
        
        _mockObjectives = new List<ReferenceDataDto>
        {
            new() { Id = "obj1", Value = "Muscle Gain", Description = "Build muscle mass" },
            new() { Id = "obj2", Value = "Fat Loss", Description = "Lose body fat" }
        };
        
        SetupMockServices();
        Services.AddSingleton(_mockWorkoutTemplateService.Object);
        Services.AddSingleton(_mockStateService.Object);
    }
    
    private void SetupMockServices()
    {
        _mockWorkoutTemplateService.Setup(x => x.GetWorkoutCategoriesAsync())
            .ReturnsAsync(_mockCategories);
        _mockWorkoutTemplateService.Setup(x => x.GetDifficultyLevelsAsync())
            .ReturnsAsync(_mockDifficulties);
        _mockWorkoutTemplateService.Setup(x => x.GetWorkoutObjectivesAsync())
            .ReturnsAsync(_mockObjectives);
        _mockWorkoutTemplateService.Setup(x => x.CheckTemplateNameExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
    }
    
    [Fact]
    public void Should_RenderAllFormFields()
    {
        // Arrange & Act
        var cut = RenderComponent<WorkoutTemplateForm>();
        
        // Assert
        cut.Find("[data-testid='name-input']").Should().NotBeNull();
        cut.Find("[data-testid='description-input']").Should().NotBeNull();
        cut.Find("[data-testid='category-select']").Should().NotBeNull();
        cut.Find("[data-testid='difficulty-select']").Should().NotBeNull();
        cut.Find("[data-testid='objective-select']").Should().NotBeNull();
        cut.Find("[data-testid='duration-input']").Should().NotBeNull();
        cut.Find("[data-testid='public-checkbox']").Should().NotBeNull();
        cut.Find("[data-testid='tags-input']").Should().NotBeNull();
        cut.Find("[data-testid='submit-button']").Should().NotBeNull();
        cut.Find("[data-testid='cancel-button']").Should().NotBeNull();
    }
    
    [Fact]
    public void Should_LoadReferenceDataOnInitialization()
    {
        // Arrange & Act
        var cut = RenderComponent<WorkoutTemplateForm>();
        
        // Assert
        _mockWorkoutTemplateService.Verify(x => x.GetWorkoutCategoriesAsync(), Times.Once);
        _mockWorkoutTemplateService.Verify(x => x.GetDifficultyLevelsAsync(), Times.Once);
        _mockWorkoutTemplateService.Verify(x => x.GetWorkoutObjectivesAsync(), Times.Once);
        
        // Verify dropdowns are populated
        var categoryOptions = cut.FindAll("#category option");
        categoryOptions.Count.Should().Be(3); // Empty option + 2 categories
        
        var difficultyOptions = cut.FindAll("#difficulty option");
        difficultyOptions.Count.Should().Be(4); // Empty option + 3 difficulties
        
        var objectiveOptions = cut.FindAll("#objective option");
        objectiveOptions.Count.Should().Be(3); // Empty option + 2 objectives
    }
    
    [Fact]
    public void Should_ShowErrorMessage_WhenReferenceDataLoadFails()
    {
        // Arrange
        _mockWorkoutTemplateService.Setup(x => x.GetWorkoutCategoriesAsync())
            .ThrowsAsync(new HttpRequestException("Network error"));
            
        // Act
        var cut = RenderComponent<WorkoutTemplateForm>();
        
        // Assert
        var errorMessage = cut.Find("[data-testid='error-message']");
        errorMessage.TextContent.Should().Contain("Failed to load reference data");
    }
    
    [Fact]
    public void Should_DisplayValidationMessages_WhenFieldsAreInvalid()
    {
        // Arrange
        var cut = RenderComponent<WorkoutTemplateForm>();
        
        // Act - Submit form without filling required fields
        var form = cut.Find("form");
        form.Submit();
        
        // Assert - Check for validation messages
        var validationMessages = cut.FindAll(".validation-message");
        validationMessages.Should().NotBeEmpty();
        
        // Submit button should be disabled
        var submitButton = cut.Find("[data-testid='submit-button']");
        submitButton.GetAttribute("disabled").Should().NotBeNull();
    }
    
    [Fact]
    public async Task Should_CallOnValidSubmit_WhenFormIsValid()
    {
        // Arrange
        var submittedModel = null as WorkoutTemplateFormModel;
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.OnValidSubmit, EventCallback.Factory.Create<WorkoutTemplateFormModel>(
                this, model => submittedModel = model)));
        
        // Act - Fill form with valid data
        await cut.Find("[data-testid='name-input']").ChangeAsync(new ChangeEventArgs 
        { 
            Value = "Test Template" 
        });
        
        var categorySelect = cut.Find("[data-testid='category-select']");
        await categorySelect.ChangeAsync(new ChangeEventArgs { Value = "cat1" });
        
        var difficultySelect = cut.Find("[data-testid='difficulty-select']");
        await difficultySelect.ChangeAsync(new ChangeEventArgs { Value = "diff1" });
        
        await cut.Find("[data-testid='duration-input']").ChangeAsync(new ChangeEventArgs 
        { 
            Value = "45" 
        });
        
        // Submit form
        var form = cut.Find("form");
        await form.SubmitAsync();
        
        // Assert
        submittedModel.Should().NotBeNull();
        submittedModel!.Name.Should().Be("Test Template");
        submittedModel.CategoryId.Should().Be("cat1");
        submittedModel.DifficultyId.Should().Be("diff1");
        submittedModel.EstimatedDurationMinutes.Should().Be(45);
    }
    
    [Fact]
    public async Task Should_CallOnCancel_WhenCancelButtonClicked()
    {
        // Arrange
        var cancelCalled = false;
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => cancelCalled = true)));
        
        // Act
        var cancelButton = cut.Find("[data-testid='cancel-button']");
        await cancelButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
        
        // Assert
        cancelCalled.Should().BeTrue();
    }
    
    [Fact]
    public void Should_ShowStateInfo_ForNewTemplates()
    {
        // Arrange & Act
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.IsEditMode, false));
        
        // Assert
        var stateInfo = cut.Find("[data-testid='state-info']");
        stateInfo.TextContent.Should().Contain("New templates will be created in Draft state");
    }
    
    [Fact]
    public void Should_ShowStateInfo_ForProductionTemplates()
    {
        // Arrange
        var existingTemplate = new WorkoutTemplateDto
        {
            WorkoutState = new ReferenceDataDto { Id = "PRODUCTION", Value = "Production" }
        };
        
        // Act
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.IsEditMode, true)
            .Add(p => p.ExistingTemplate, existingTemplate));
        
        // Assert
        var stateInfo = cut.Find("[data-testid='state-info']");
        stateInfo.TextContent.Should().Contain("This template is in Production state");
    }
    
    [Fact]
    public void Should_ShowStateInfo_ForArchivedTemplates()
    {
        // Arrange
        var existingTemplate = new WorkoutTemplateDto
        {
            WorkoutState = new ReferenceDataDto { Id = "ARCHIVED", Value = "Archived" }
        };
        
        // Act
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.IsEditMode, true)
            .Add(p => p.ExistingTemplate, existingTemplate));
        
        // Assert
        var stateInfo = cut.Find("[data-testid='state-info']");
        stateInfo.TextContent.Should().Contain("This template is Archived");
    }
    
    [Fact]
    public void Should_DisableFields_WhenFieldDisabledPredicateReturnsTrue()
    {
        // Arrange
        Func<string, bool> predicate = fieldName => fieldName == nameof(WorkoutTemplateFormModel.Name);
        
        // Act
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.FieldDisabledPredicate, predicate));
        
        // Assert
        var nameInput = cut.Find("[data-testid='name-input']");
        nameInput.GetAttribute("disabled").Should().NotBeNull();
        
        var descriptionInput = cut.Find("[data-testid='description-input']");
        descriptionInput.GetAttribute("disabled").Should().BeNull();
    }
    
    [Fact]
    public async Task Should_ProcessTags_WhenTagsInputChanges()
    {
        // Arrange
        var model = new WorkoutTemplateFormModel();
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.Model, model));
        
        // Act
        var tagsInput = cut.Find("[data-testid='tags-input']");
        await tagsInput.ChangeAsync(new ChangeEventArgs { Value = "tag1, tag2, tag3" });
        
        // Trigger the form component's ProcessTags method
        cut.Instance.ProcessTags();
        
        // Wait for processing
        cut.WaitForState(() => model.Tags.Count > 0);
        
        // Assert
        model.Tags.Should().HaveCount(3);
        model.Tags.Should().Contain(new[] { "tag1", "tag2", "tag3" });
        
        // Verify tags are displayed
        var tagsDisplay = cut.Find("[data-testid='tags-display']");
        tagsDisplay.TextContent.Should().Contain("tag1");
        tagsDisplay.TextContent.Should().Contain("tag2");
        tagsDisplay.TextContent.Should().Contain("tag3");
    }
    
    [Fact]
    public void Should_ShowAutoSaveIndicator_WhenEnabledInEditMode()
    {
        // Arrange & Act
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.IsEditMode, true)
            .Add(p => p.EnableAutoSave, true));
        
        // Assert
        cut.FindAll("[data-testid='autosave-indicator']").Should().HaveCount(1);
    }
    
    [Fact]
    public void Should_NotShowAutoSaveIndicator_WhenNotInEditMode()
    {
        // Arrange & Act
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.IsEditMode, false)
            .Add(p => p.EnableAutoSave, true));
        
        // Assert
        cut.FindAll("[data-testid='autosave-indicator']").Should().BeEmpty();
    }
    
    [Fact]
    public void Should_CustomizeSubmitButtonText()
    {
        // Arrange & Act
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.SubmitButtonText, "Create Template"));
        
        // Assert
        var submitButton = cut.Find("[data-testid='submit-button']");
        submitButton.TextContent.Should().Contain("Create Template");
    }
    
    [Fact]
    public async Task Should_ShowNameExistsWarning_WhenNameAlreadyExists()
    {
        // Arrange
        _mockWorkoutTemplateService.Setup(x => x.CheckTemplateNameExistsAsync("Existing Template"))
            .ReturnsAsync(true);
            
        var cut = RenderComponent<WorkoutTemplateForm>();
        
        // Act
        var nameInput = cut.Find("[data-testid='name-input']");
        await nameInput.ChangeAsync(new ChangeEventArgs { Value = "Existing Template" });
        
        // Wait for validation
        await Task.Delay(600); // Wait for debounce
        cut.Render();
        
        // Assert
        var warning = cut.Find("[data-testid='name-exists-warning']");
        warning.TextContent.Should().Contain("A template with this name already exists");
    }
    
    [Fact]
    public void Should_InitializeFormWithExistingData()
    {
        // Arrange
        var model = new WorkoutTemplateFormModel
        {
            Name = "Existing Template",
            Description = "Existing Description",
            CategoryId = "cat1",
            DifficultyId = "diff2",
            ObjectiveId = "obj1",
            EstimatedDurationMinutes = 60,
            IsPublic = true,
            Tags = new List<string> { "tag1", "tag2" }
        };
        
        // Act
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.Model, model));
        
        // Assert
        cut.Find("[data-testid='name-input']").GetAttribute("value").Should().Be("Existing Template");
        cut.Find("[data-testid='description-input']").GetAttribute("value").Should().Be("Existing Description");
        cut.Find("[data-testid='category-select']").GetAttribute("value").Should().Be("cat1");
        cut.Find("[data-testid='difficulty-select']").GetAttribute("value").Should().Be("diff2");
        cut.Find("[data-testid='objective-select']").GetAttribute("value").Should().Be("obj1");
        cut.Find("[data-testid='duration-input']").GetAttribute("value").Should().Be("60");
        cut.Find("[data-testid='public-checkbox']").GetAttribute("checked").Should().NotBeNull();
        cut.Find("[data-testid='tags-input']").GetAttribute("value").Should().Be("tag1, tag2");
    }
    
    [Fact]
    public void Should_ValidateFormModel()
    {
        // Arrange
        var model = new WorkoutTemplateFormModel();
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        
        // Act
        var isValid = Validator.TryValidateObject(model, context, results, true);
        
        // Assert
        isValid.Should().BeFalse();
        results.Should().Contain(r => r.MemberNames.Contains(nameof(WorkoutTemplateFormModel.Name)));
        results.Should().Contain(r => r.MemberNames.Contains(nameof(WorkoutTemplateFormModel.CategoryId)));
        results.Should().Contain(r => r.MemberNames.Contains(nameof(WorkoutTemplateFormModel.DifficultyId)));
    }
    
    [Fact]
    public void Should_ValidateNameLength()
    {
        // Arrange
        var model = new WorkoutTemplateFormModel
        {
            Name = new string('a', 101) // Exceeds 100 character limit
        };
        var context = new ValidationContext(model) { MemberName = nameof(WorkoutTemplateFormModel.Name) };
        var results = new List<ValidationResult>();
        
        // Act
        var isValid = Validator.TryValidateProperty(model.Name, context, results);
        
        // Assert
        isValid.Should().BeFalse();
        results.Should().Contain(r => r.ErrorMessage!.Contains("100 characters"));
    }
    
    [Fact]
    public void Should_ValidateDurationRange()
    {
        // Arrange
        var model = new WorkoutTemplateFormModel
        {
            EstimatedDurationMinutes = 301 // Exceeds 300 minute limit
        };
        var context = new ValidationContext(model) { MemberName = nameof(WorkoutTemplateFormModel.EstimatedDurationMinutes) };
        var results = new List<ValidationResult>();
        
        // Act
        var isValid = Validator.TryValidateProperty(model.EstimatedDurationMinutes, context, results);
        
        // Assert
        isValid.Should().BeFalse();
        results.Should().Contain(r => r.ErrorMessage!.Contains("between 5 and 300"));
    }
}
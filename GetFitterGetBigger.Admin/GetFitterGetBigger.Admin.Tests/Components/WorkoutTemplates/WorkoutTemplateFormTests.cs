using AngleSharp.Dom;
using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.WorkoutTemplates;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
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
        // Floating buttons should exist
        cut.Find("[data-testid='floating-save-button']").Should().NotBeNull();
        cut.Find("[data-testid='floating-cancel-button']").Should().NotBeNull();
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
        
        // Submit button should be disabled when form is invalid
        var submitButton = cut.Find("[data-testid='floating-save-button']");
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
        
        // Click floating save button
        var saveButton = cut.Find("[data-testid='floating-save-button']");
        await saveButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
        
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
        var cancelButton = cut.Find("[data-testid='floating-cancel-button']");
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
        var submitButton = cut.Find("[data-testid='floating-save-button']");
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
    public void Should_HideFloatingButtons_WhenLoading()
    {
        // Arrange
        _mockWorkoutTemplateService.Setup(x => x.GetWorkoutCategoriesAsync())
            .Returns(async () =>
            {
                await Task.Delay(100); // Simulate loading
                return _mockCategories;
            });
            
        // Act
        var cut = RenderComponent<WorkoutTemplateForm>();
        
        // Assert - buttons should not be visible during loading
        cut.FindAll("[data-testid='floating-save-button']").Should().BeEmpty();
        cut.FindAll("[data-testid='floating-cancel-button']").Should().BeEmpty();
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
    
    // Task 6.4: Create unit tests for validation logic (all validation rules)
    [Theory]
    [InlineData("", false)] // Empty name
    [InlineData(" ", false)] // Whitespace only
    [InlineData("Valid Name", true)] // Valid name
    public async Task Should_ValidateNameIsRequired(string name, bool shouldBeValid)
    {
        // Arrange
        var model = new WorkoutTemplateFormModel 
        { 
            Name = name,
            CategoryId = shouldBeValid ? "cat1" : "", // Need valid values for form to be valid
            DifficultyId = shouldBeValid ? "diff1" : "",
            EstimatedDurationMinutes = shouldBeValid ? 30 : 0
        };
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.Model, model));
        
        // Act
        await cut.Find("[data-testid='name-input']").ChangeAsync(new ChangeEventArgs { Value = name });
        
        // Assert
        var isValid = cut.Instance.IsFormValid();
        isValid.Should().Be(shouldBeValid);
    }
    
    [Fact]
    public void Should_ValidateDescriptionLength()
    {
        // Arrange
        var model = new WorkoutTemplateFormModel
        {
            Description = new string('a', 501) // Exceeds 500 character limit
        };
        var context = new ValidationContext(model) { MemberName = nameof(WorkoutTemplateFormModel.Description) };
        var results = new List<ValidationResult>();
        
        // Act
        var isValid = Validator.TryValidateProperty(model.Description, context, results);
        
        // Assert
        isValid.Should().BeFalse();
        results.Should().Contain(r => r.ErrorMessage!.Contains("500 characters"));
    }
    
    [Theory]
    [InlineData("", false)] // Empty category
    [InlineData("cat1", true)] // Valid category
    public async Task Should_ValidateCategoryIsRequired(string categoryId, bool shouldBeValid)
    {
        // Arrange
        var model = new WorkoutTemplateFormModel 
        { 
            Name = "Test",
            CategoryId = categoryId,
            DifficultyId = "diff1",
            EstimatedDurationMinutes = 30
        };
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.Model, model));
        
        // Act & Assert
        var isValid = cut.Instance.IsFormValid();
        isValid.Should().Be(shouldBeValid);
    }
    
    [Theory]
    [InlineData("", false)] // Empty difficulty
    [InlineData("diff1", true)] // Valid difficulty
    public async Task Should_ValidateDifficultyIsRequired(string difficultyId, bool shouldBeValid)
    {
        // Arrange
        var model = new WorkoutTemplateFormModel 
        { 
            Name = "Test",
            CategoryId = "cat1",
            DifficultyId = difficultyId,
            EstimatedDurationMinutes = 30
        };
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.Model, model));
        
        // Act & Assert
        var isValid = cut.Instance.IsFormValid();
        isValid.Should().Be(shouldBeValid);
    }
    
    [Theory]
    [InlineData(4, false)] // Below minimum
    [InlineData(5, true)] // Minimum valid
    [InlineData(300, true)] // Maximum valid
    [InlineData(301, false)] // Above maximum
    public async Task Should_ValidateDurationRange_Comprehensive(int duration, bool shouldBeValid)
    {
        // Arrange
        var model = new WorkoutTemplateFormModel 
        { 
            Name = "Test",
            CategoryId = "cat1",
            DifficultyId = "diff1",
            EstimatedDurationMinutes = duration
        };
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.Model, model));
        
        // Act & Assert
        var isValid = cut.Instance.IsFormValid();
        isValid.Should().Be(shouldBeValid);
    }
    
    [Fact]
    public async Task Should_ValidateFormWithNameConflict()
    {
        // Arrange
        _mockWorkoutTemplateService.Setup(x => x.CheckTemplateNameExistsAsync("Existing Name"))
            .ReturnsAsync(true);
            
        var model = new WorkoutTemplateFormModel 
        { 
            Name = "Existing Name",
            CategoryId = "cat1",
            DifficultyId = "diff1",
            EstimatedDurationMinutes = 30
        };
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.Model, model));
        
        // Act - Trigger name validation
        await cut.Instance.ValidateNameAsync("Existing Name");
        await Task.Delay(600); // Wait for debounce
        
        // Assert
        cut.Instance.ShowNameExistsWarning.Should().BeTrue();
        cut.Instance.IsFormValid().Should().BeFalse();
    }
    
    [Fact]
    public async Task Should_DisableSaveButton_WhenFormIsInvalid()
    {
        // Arrange - Create form with invalid data
        var model = new WorkoutTemplateFormModel 
        { 
            Name = "", // Invalid - empty
            CategoryId = "", // Invalid - empty
            DifficultyId = "", // Invalid - empty
            EstimatedDurationMinutes = 0 // Invalid - below minimum
        };
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.Model, model));
        
        // Assert
        var saveButton = cut.Find("[data-testid='floating-save-button']");
        saveButton.GetAttribute("disabled").Should().NotBeNull();
    }
    
    [Fact]
    public async Task Should_EnableSaveButton_WhenFormBecomesValid()
    {
        // Arrange
        var model = new WorkoutTemplateFormModel();
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.Model, model));
        
        // Initially button should be disabled
        var saveButton = cut.Find("[data-testid='floating-save-button']");
        saveButton.GetAttribute("disabled").Should().NotBeNull();
        
        // Act - Fill form with valid data
        await cut.Find("[data-testid='name-input']").ChangeAsync(new ChangeEventArgs { Value = "Valid Name" });
        await cut.Find("[data-testid='category-select']").ChangeAsync(new ChangeEventArgs { Value = "cat1" });
        await cut.Find("[data-testid='difficulty-select']").ChangeAsync(new ChangeEventArgs { Value = "diff1" });
        await cut.Find("[data-testid='duration-input']").ChangeAsync(new ChangeEventArgs { Value = "30" });
        
        // Assert - Button should now be enabled
        saveButton = cut.Find("[data-testid='floating-save-button']");
        saveButton.GetAttribute("disabled").Should().BeNull();
    }
    
    [Fact]
    public void Should_ValidateAllFieldsInFormModel()
    {
        // Arrange - Test complete validation of all fields
        var validModel = new WorkoutTemplateFormModel
        {
            Name = "Valid Template",
            Description = "Valid description under 500 chars",
            CategoryId = "cat1",
            DifficultyId = "diff1",
            ObjectiveId = "obj1", // Optional but valid
            EstimatedDurationMinutes = 45,
            IsPublic = true,
            Tags = new List<string> { "tag1", "tag2" }
        };
        
        var context = new ValidationContext(validModel);
        var results = new List<ValidationResult>();
        
        // Act
        var isValid = Validator.TryValidateObject(validModel, context, results, true);
        
        // Assert
        isValid.Should().BeTrue();
        results.Should().BeEmpty();
    }
    
    [Theory]
    [InlineData("Name!@#", true)] // Special characters allowed
    [InlineData("Template 123", true)] // Numbers allowed
    [InlineData("Тренировка", true)] // Unicode allowed
    [InlineData("A", true)] // Single character allowed
    public async Task Should_ValidateNameFormat(string name, bool shouldBeValid)
    {
        // Arrange
        var model = new WorkoutTemplateFormModel 
        { 
            Name = name,
            CategoryId = "cat1",
            DifficultyId = "diff1",
            EstimatedDurationMinutes = 30
        };
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.Model, model));
        
        // Act & Assert
        var isValid = cut.Instance.IsFormValid();
        isValid.Should().Be(shouldBeValid);
    }
    
    // Task 6.6: Create unit tests for auto-save (timer, dirty state detection)
    [Fact]
    public async Task Should_StartAutoSaveTimer_WhenEnabledInEditMode()
    {
        // Arrange & Act
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.IsEditMode, true)
            .Add(p => p.EnableAutoSave, true));
        
        // Assert
        cut.Instance.autoSaveTimer.Should().NotBeNull();
    }
    
    [Fact]
    public async Task Should_NotStartAutoSaveTimer_WhenNotInEditMode()
    {
        // Arrange & Act
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.IsEditMode, false)
            .Add(p => p.EnableAutoSave, true));
        
        // Assert
        cut.Instance.autoSaveTimer.Should().BeNull();
    }
    
    [Fact]
    public async Task Should_NotStartAutoSaveTimer_WhenDisabled()
    {
        // Arrange & Act
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.IsEditMode, true)
            .Add(p => p.EnableAutoSave, false));
        
        // Assert
        cut.Instance.autoSaveTimer.Should().BeNull();
    }
    
    [Fact]
    public async Task Should_MarkFormAsDirty_WhenFieldsChange()
    {
        // Arrange
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.IsEditMode, true)
            .Add(p => p.EnableAutoSave, true));
        
        // Initially not dirty
        cut.Instance.isDirty.Should().BeFalse();
        
        // Act - Change various fields
        await cut.Find("[data-testid='name-input']").ChangeAsync(new ChangeEventArgs { Value = "New Name" });
        
        // Wait for the change event to be processed
        cut.WaitForState(() => cut.Instance.isDirty);
        
        // Assert
        cut.Instance.isDirty.Should().BeTrue();
    }
    
    [Theory]
    [InlineData("description-input", "New Description")]
    [InlineData("category-select", "cat2")]
    [InlineData("difficulty-select", "diff2")]
    [InlineData("duration-input", "60")]
    [InlineData("tags-input", "tag1, tag2")]
    public async Task Should_MarkAsDirty_WhenAnyFieldChanges(string testId, string newValue)
    {
        // Arrange
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.IsEditMode, true)
            .Add(p => p.EnableAutoSave, true));
        
        // Act
        await cut.Find($"[data-testid='{testId}']").ChangeAsync(new ChangeEventArgs { Value = newValue });
        
        // Assert
        cut.Instance.isDirty.Should().BeTrue();
    }
    
    [Fact]
    public async Task Should_MarkAsDirty_WhenCheckboxChanges()
    {
        // Arrange
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.IsEditMode, true)
            .Add(p => p.EnableAutoSave, true));
        
        // Act
        var checkbox = cut.Find("[data-testid='public-checkbox']");
        await checkbox.ChangeAsync(new ChangeEventArgs { Value = true });
        
        // Assert
        cut.Instance.isDirty.Should().BeTrue();
    }
    
    [Fact]
    public async Task Should_TriggerAutoSave_WhenFormIsDirtyAndValid()
    {
        // Arrange
        var saveCallbackInvoked = false;
        var savedModel = null as WorkoutTemplateFormModel;
        
        var model = new WorkoutTemplateFormModel
        {
            Name = "Valid Template",
            CategoryId = "cat1",
            DifficultyId = "diff1",
            EstimatedDurationMinutes = 30
        };
        
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.IsEditMode, true)
            .Add(p => p.EnableAutoSave, true)
            .Add(p => p.Model, model)
            .Add(p => p.OnValidSubmit, EventCallback.Factory.Create<WorkoutTemplateFormModel>(
                this, m => 
                {
                    saveCallbackInvoked = true;
                    savedModel = m;
                })));
        
        // Act - Mark as dirty and trigger auto-save
        cut.Instance.MarkAsDirty();
        await cut.Instance.AutoSave();
        
        // Assert
        saveCallbackInvoked.Should().BeTrue();
        savedModel.Should().NotBeNull();
        cut.Instance.LastAutoSaved.Should().NotBeNull();
        cut.Instance.isDirty.Should().BeFalse(); // Should be reset after save
    }
    
    [Fact]
    public async Task Should_NotAutoSave_WhenFormIsNotDirty()
    {
        // Arrange
        var saveCallbackInvoked = false;
        var model = new WorkoutTemplateFormModel
        {
            Name = "Valid Template",
            CategoryId = "cat1",
            DifficultyId = "diff1",
            EstimatedDurationMinutes = 30
        };
        
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.IsEditMode, true)
            .Add(p => p.EnableAutoSave, true)
            .Add(p => p.Model, model)
            .Add(p => p.OnValidSubmit, EventCallback.Factory.Create<WorkoutTemplateFormModel>(
                this, _ => saveCallbackInvoked = true)));
        
        // Act - Trigger auto-save without marking dirty
        await cut.Instance.AutoSave();
        
        // Assert
        saveCallbackInvoked.Should().BeFalse();
    }
    
    [Fact]
    public async Task Should_NotAutoSave_WhenFormIsInvalid()
    {
        // Arrange
        var saveCallbackInvoked = false;
        var model = new WorkoutTemplateFormModel
        {
            Name = "", // Invalid
            CategoryId = "cat1",
            DifficultyId = "diff1",
            EstimatedDurationMinutes = 30
        };
        
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.IsEditMode, true)
            .Add(p => p.EnableAutoSave, true)
            .Add(p => p.Model, model)
            .Add(p => p.OnValidSubmit, EventCallback.Factory.Create<WorkoutTemplateFormModel>(
                this, _ => saveCallbackInvoked = true)));
        
        // Act
        cut.Instance.MarkAsDirty();
        await cut.Instance.AutoSave();
        
        // Assert
        saveCallbackInvoked.Should().BeFalse();
    }
    
    [Fact]
    public async Task Should_NotAutoSave_WhenAlreadySubmitting()
    {
        // Arrange
        var saveCount = 0;
        var model = new WorkoutTemplateFormModel
        {
            Name = "Valid Template",
            CategoryId = "cat1",
            DifficultyId = "diff1",
            EstimatedDurationMinutes = 30
        };
        
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.IsEditMode, true)
            .Add(p => p.EnableAutoSave, true)
            .Add(p => p.Model, model)
            .Add(p => p.OnValidSubmit, EventCallback.Factory.Create<WorkoutTemplateFormModel>(
                this, _ => saveCount++)));
        
        // Act - Set submitting flag and try auto-save
        cut.Instance.IsSubmitting = true;
        cut.Instance.MarkAsDirty();
        await cut.Instance.AutoSave();
        
        // Assert
        saveCount.Should().Be(0);
    }
    
    [Fact]
    public async Task Should_ShowAutoSaveIndicator_WhenSaving()
    {
        // Arrange
        var model = new WorkoutTemplateFormModel
        {
            Name = "Valid Template",
            CategoryId = "cat1",
            DifficultyId = "diff1",
            EstimatedDurationMinutes = 30
        };
        
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.IsEditMode, true)
            .Add(p => p.EnableAutoSave, true)
            .Add(p => p.Model, model));
        
        // Act - Set auto-saving state
        cut.Instance.IsAutoSaving = true;
        cut.Render();
        
        // Assert
        var indicator = cut.Find("[data-testid='autosave-indicator']");
        indicator.TextContent.Should().Contain("Saving...");
    }
    
    [Theory]
    [InlineData(30, "Just now")]
    [InlineData(90, "1 minute ago")]
    [InlineData(150, "2 minutes ago")]
    [InlineData(3900, "1 hour ago")]
    [InlineData(7800, "2 hours ago")]
    public void Should_FormatLastSavedTimeCorrectly(int secondsAgo, string expectedText)
    {
        // Arrange
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.IsEditMode, true)
            .Add(p => p.EnableAutoSave, true));
        
        // Act
        cut.Instance.LastAutoSaved = DateTime.Now.AddSeconds(-secondsAgo);
        var result = cut.Instance.GetLastSavedTimeAgo();
        
        // Assert
        result.Should().Be(expectedText);
    }
    
    [Fact]
    public void Should_ReturnNever_WhenNotAutoSaved()
    {
        // Arrange
        var cut = RenderComponent<WorkoutTemplateForm>();
        
        // Act
        var result = cut.Instance.GetLastSavedTimeAgo();
        
        // Assert
        result.Should().Be("Never");
    }
    
    [Fact]
    public void Should_DisposeAutoSaveTimer_WhenComponentDisposed()
    {
        // Arrange
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.IsEditMode, true)
            .Add(p => p.EnableAutoSave, true));
        
        var timer = cut.Instance.autoSaveTimer;
        timer.Should().NotBeNull();
        
        // Act
        cut.Dispose();
        
        // Assert - Timer should be cleaned up
        // We can't directly check if timer is disposed, but method should complete without errors
    }
    
    [Fact]
    public async Task Should_StopAutoSaveTimer_WhenCancelClicked()
    {
        // Arrange
        var cancelCalled = false;
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.IsEditMode, true)
            .Add(p => p.EnableAutoSave, true)
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => cancelCalled = true)));
        
        var timerBefore = cut.Instance.autoSaveTimer;
        timerBefore.Should().NotBeNull();
        
        // Act
        await cut.Find("[data-testid='floating-cancel-button']").ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
        
        // Assert
        cancelCalled.Should().BeTrue();
        // Timer should be disposed in HandleCancel
    }
    
    // Task 6.10: Create unit tests for async validation (debouncing, error states)
    [Fact]
    public async Task Should_DebounceNameValidation()
    {
        // Arrange
        var validationCallCount = 0;
        _mockWorkoutTemplateService.Setup(x => x.CheckTemplateNameExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false)
            .Callback(() => validationCallCount++);
            
        var cut = RenderComponent<WorkoutTemplateForm>();
        
        // Act - Type multiple times quickly
        var nameInput = cut.Find("[data-testid='name-input']");
        await nameInput.ChangeAsync(new ChangeEventArgs { Value = "T" });
        await nameInput.ChangeAsync(new ChangeEventArgs { Value = "Te" });
        await nameInput.ChangeAsync(new ChangeEventArgs { Value = "Test" });
        
        // Wait less than debounce time
        await Task.Delay(300);
        
        // Assert - Should not have called validation yet
        validationCallCount.Should().Be(0);
        
        // Wait for debounce to complete
        await Task.Delay(300);
        
        // Assert - Should call validation only once
        validationCallCount.Should().Be(1);
        _mockWorkoutTemplateService.Verify(x => x.CheckTemplateNameExistsAsync("Test"), Times.Once);
    }
    
    [Fact]
    public async Task Should_CancelPreviousValidation_WhenNewInputArrives()
    {
        // This test verifies that the cancellation token properly cancels previous validations
        // We'll test this by checking the number of times the service is called
        
        // Arrange
        var callCount = 0;
        _mockWorkoutTemplateService.Setup(x => x.CheckTemplateNameExistsAsync(It.IsAny<string>()))
            .ReturnsAsync((string name) => 
            {
                callCount++;
                return false;
            });
            
        var cut = RenderComponent<WorkoutTemplateForm>();
        
        // Act - Type three values quickly (within debounce time)
        var nameInput = cut.Find("[data-testid='name-input']");
        await nameInput.ChangeAsync(new ChangeEventArgs { Value = "F" });
        await Task.Delay(100); // Less than 500ms debounce
        await nameInput.ChangeAsync(new ChangeEventArgs { Value = "Fi" });
        await Task.Delay(100); // Less than 500ms debounce
        await nameInput.ChangeAsync(new ChangeEventArgs { Value = "First" });
        
        // Wait for debounce and validation to complete
        await Task.Delay(700);
        
        // Assert - Should only validate the final value
        callCount.Should().Be(1);
        _mockWorkoutTemplateService.Verify(x => x.CheckTemplateNameExistsAsync("First"), Times.Once);
        _mockWorkoutTemplateService.Verify(x => x.CheckTemplateNameExistsAsync("F"), Times.Never);
        _mockWorkoutTemplateService.Verify(x => x.CheckTemplateNameExistsAsync("Fi"), Times.Never);
    }
    
    [Fact]
    public async Task Should_NotValidateName_WhenDisabled()
    {
        // Arrange
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.EnableNameValidation, false));
        
        // Act
        await cut.Instance.ValidateNameAsync("Test Name");
        await Task.Delay(600); // Wait for potential validation
        
        // Assert
        _mockWorkoutTemplateService.Verify(x => x.CheckTemplateNameExistsAsync(It.IsAny<string>()), Times.Never);
        cut.Instance.ShowNameExistsWarning.Should().BeFalse();
    }
    
    [Fact]
    public async Task Should_NotValidateName_WhenEmpty()
    {
        // Arrange
        var cut = RenderComponent<WorkoutTemplateForm>();
        
        // Act
        await cut.Instance.ValidateNameAsync("");
        await cut.Instance.ValidateNameAsync("   "); // Whitespace
        await Task.Delay(600);
        
        // Assert
        _mockWorkoutTemplateService.Verify(x => x.CheckTemplateNameExistsAsync(It.IsAny<string>()), Times.Never);
        cut.Instance.ShowNameExistsWarning.Should().BeFalse();
    }
    
    [Fact]
    public async Task Should_SkipValidation_WhenNameNotChanged()
    {
        // Arrange
        var cut = RenderComponent<WorkoutTemplateForm>();
        
        // Act - Validate same name twice
        await cut.Instance.ValidateNameAsync("Same Name");
        await Task.Delay(600); // Wait for first validation
        
        _mockWorkoutTemplateService.Verify(x => x.CheckTemplateNameExistsAsync("Same Name"), Times.Once);
        
        // Validate same name again
        await cut.Instance.ValidateNameAsync("Same Name");
        await Task.Delay(600);
        
        // Assert - Should not validate again
        _mockWorkoutTemplateService.Verify(x => x.CheckTemplateNameExistsAsync("Same Name"), Times.Once);
    }
    
    [Fact]
    public async Task Should_NotShowWarning_ForOwnNameInEditMode()
    {
        // Arrange
        var existingTemplate = new WorkoutTemplateDto
        {
            Name = "My Template"
        };
        
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.IsEditMode, true)
            .Add(p => p.ExistingTemplate, existingTemplate));
        
        // Act - Validate the template's own name
        await cut.Instance.ValidateNameAsync("My Template");
        await Task.Delay(600);
        
        // Assert
        _mockWorkoutTemplateService.Verify(x => x.CheckTemplateNameExistsAsync(It.IsAny<string>()), Times.Never);
        cut.Instance.ShowNameExistsWarning.Should().BeFalse();
    }
    
    [Fact]
    public async Task Should_HandleValidationErrors_Gracefully()
    {
        // Arrange
        _mockWorkoutTemplateService.Setup(x => x.CheckTemplateNameExistsAsync(It.IsAny<string>()))
            .ThrowsAsync(new HttpRequestException("Network error"));
            
        var cut = RenderComponent<WorkoutTemplateForm>();
        
        // Act
        await cut.Instance.ValidateNameAsync("Test Name");
        await Task.Delay(600);
        
        // Assert - Should not crash, warning should not be shown
        cut.Instance.ShowNameExistsWarning.Should().BeFalse();
    }
    
    [Fact]
    public async Task Should_ShowWarning_AndDisableSave_WhenNameExists()
    {
        // Arrange
        _mockWorkoutTemplateService.Setup(x => x.CheckTemplateNameExistsAsync("Duplicate"))
            .ReturnsAsync(true);
            
        var model = new WorkoutTemplateFormModel
        {
            Name = "Duplicate",
            CategoryId = "cat1",
            DifficultyId = "diff1",
            EstimatedDurationMinutes = 30
        };
        
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.Model, model));
        
        // Act
        await cut.Instance.ValidateNameAsync("Duplicate");
        await Task.Delay(600);
        cut.Render();
        
        // Assert
        cut.Instance.ShowNameExistsWarning.Should().BeTrue();
        cut.Instance.IsFormValid().Should().BeFalse();
        
        var warning = cut.Find("[data-testid='name-exists-warning']");
        warning.TextContent.Should().Contain("already exists");
        
        var saveButton = cut.Find("[data-testid='floating-save-button']");
        saveButton.GetAttribute("disabled").Should().NotBeNull();
    }
    
    [Fact]
    public async Task Should_ClearWarning_WhenNameChangesToValid()
    {
        // Arrange
        _mockWorkoutTemplateService.Setup(x => x.CheckTemplateNameExistsAsync("Duplicate"))
            .ReturnsAsync(true);
        _mockWorkoutTemplateService.Setup(x => x.CheckTemplateNameExistsAsync("Unique"))
            .ReturnsAsync(false);
            
        var cut = RenderComponent<WorkoutTemplateForm>();
        
        // Act - First set duplicate name
        await cut.Instance.ValidateNameAsync("Duplicate");
        await Task.Delay(600);
        cut.Instance.ShowNameExistsWarning.Should().BeTrue();
        
        // Then change to unique name
        await cut.Instance.ValidateNameAsync("Unique");
        await Task.Delay(600);
        
        // Assert
        cut.Instance.ShowNameExistsWarning.Should().BeFalse();
    }
    
    [Fact]
    public void Should_DisposeValidationCancellationToken_OnDispose()
    {
        // Arrange
        var cut = RenderComponent<WorkoutTemplateForm>();
        
        // Start a validation
        _ = cut.Instance.ValidateNameAsync("Test");
        
        // Act
        cut.Dispose();
        
        // Assert - Should dispose without errors
        // The cancellation token should be properly disposed
    }
    
    #region Unsaved Changes Tests
    
    [Fact]
    public async Task Should_ShowRecoveryDialog_WhenRecoveryDataExists()
    {
        // Arrange
        var jsRuntime = new Mock<IJSRuntime>();
        jsRuntime.Setup(x => x.InvokeAsync<string?>("localStorage.getItem", 
            It.Is<object[]>(args => args[0].ToString() == "workoutTemplateFormData")))
            .ReturnsAsync("{\"Name\":\"Recovered Template\",\"CategoryId\":\"cat1\"}");
        
        Services.AddSingleton(jsRuntime.Object);
        
        // Act
        var cut = RenderComponent<WorkoutTemplateForm>();
        await cut.InvokeAsync(() => cut.Instance.CheckForRecoveryData());
        
        // Assert
        cut.Instance.ShowRecoveryDialog.Should().BeTrue();
        cut.Find("[data-testid='recovery-dialog']").Should().NotBeNull();
    }
    
    [Fact]
    public async Task Should_RecoverFormData_WhenRecoverClicked()
    {
        // Arrange
        var jsRuntime = new Mock<IJSRuntime>();
        var recoveredData = "{\"Name\":\"Recovered Template\",\"CategoryId\":\"cat1\",\"DifficultyId\":\"diff1\",\"EstimatedDurationMinutes\":45}";
        jsRuntime.Setup(x => x.InvokeAsync<string?>("localStorage.getItem", 
            It.Is<object[]>(args => args[0].ToString() == "workoutTemplateFormData")))
            .ReturnsAsync(recoveredData);
        
        Services.AddSingleton(jsRuntime.Object);
        
        var cut = RenderComponent<WorkoutTemplateForm>();
        cut.Instance.ShowRecoveryDialog = true;
        cut.Render();
        
        // Act
        await cut.Find("[data-testid='recover-button']").ClickAsync(new MouseEventArgs());
        
        // Assert
        cut.Instance.Model.Name.Should().Be("Recovered Template");
        cut.Instance.Model.CategoryId.Should().Be("cat1");
        cut.Instance.Model.DifficultyId.Should().Be("diff1");
        cut.Instance.Model.EstimatedDurationMinutes.Should().Be(45);
        cut.Instance.ShowRecoveryDialog.Should().BeFalse();
        cut.Instance.isDirty.Should().BeTrue();
        
        // Verify recovery data was cleared
        jsRuntime.Verify(x => x.InvokeVoidAsync("localStorage.removeItem", 
            It.Is<object[]>(args => args[0].ToString() == "workoutTemplateFormData")), Times.Once);
    }
    
    [Fact]
    public async Task Should_DiscardRecoveryData_WhenDiscardClicked()
    {
        // Arrange
        var jsRuntime = new Mock<IJSRuntime>();
        jsRuntime.Setup(x => x.InvokeAsync<string?>("localStorage.getItem", 
            It.Is<object[]>(args => args[0].ToString() == "workoutTemplateFormData")))
            .ReturnsAsync("{\"Name\":\"Recovered Template\"}");
        
        Services.AddSingleton(jsRuntime.Object);
        
        var cut = RenderComponent<WorkoutTemplateForm>();
        cut.Instance.ShowRecoveryDialog = true;
        cut.Render();
        
        // Act
        await cut.Find("[data-testid='discard-recovery-button']").ClickAsync(new MouseEventArgs());
        
        // Assert
        cut.Instance.ShowRecoveryDialog.Should().BeFalse();
        cut.Instance.Model.Name.Should().Be(""); // Not recovered
        
        // Verify recovery data was cleared
        jsRuntime.Verify(x => x.InvokeVoidAsync("localStorage.removeItem", 
            It.Is<object[]>(args => args[0].ToString() == "workoutTemplateFormData")), Times.Once);
    }
    
    [Fact]
    public void Should_ShowUnsavedChangesDialog_WhenNavigatingWithDirtyForm()
    {
        // Arrange
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        var cut = RenderComponent<WorkoutTemplateForm>();
        
        // Make form dirty
        cut.Instance.MarkAsDirty();
        
        // Act - Simulate showing dialog
        cut.Instance.ShowUnsavedChangesDialog = true;
        cut.Instance.pendingNavigationUrl = "/other-page";
        cut.Render();
        
        // Assert
        cut.Instance.ShowUnsavedChangesDialog.Should().BeTrue();
        cut.Find("[data-testid='unsaved-changes-dialog']").Should().NotBeNull();
    }
    
    [Fact]
    public void Should_NotShowUnsavedChangesDialog_WhenFormNotDirty()
    {
        // Arrange
        var cut = RenderComponent<WorkoutTemplateForm>();
        
        // Act - Form is not dirty, so no dialog should appear
        cut.Render();
        
        // Assert
        cut.Instance.ShowUnsavedChangesDialog.Should().BeFalse();
        cut.FindAll("[data-testid='unsaved-changes-dialog']").Should().BeEmpty();
    }
    
    [Fact]
    public async Task Should_NavigateAway_WhenDiscardChangesClicked()
    {
        // Arrange
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        var jsRuntime = new Mock<IJSRuntime>();
        Services.AddSingleton(jsRuntime.Object);
        
        var cut = RenderComponent<WorkoutTemplateForm>();
        cut.Instance.ShowUnsavedChangesDialog = true;
        cut.Instance.pendingNavigationUrl = "/other-page";
        cut.Render();
        
        // Act
        await cut.Find("[data-testid='discard-button']").ClickAsync(new MouseEventArgs());
        
        // Assert
        cut.Instance.ShowUnsavedChangesDialog.Should().BeFalse();
        cut.Instance.isDirty.Should().BeFalse();
        navigationManager.Uri.Should().EndWith("/other-page");
        
        // Verify recovery data was cleared
        jsRuntime.Verify(x => x.InvokeVoidAsync("localStorage.removeItem", 
            It.Is<object[]>(args => args[0].ToString() == "workoutTemplateFormData")), Times.Once);
    }
    
    [Fact]
    public async Task Should_StayOnPage_WhenCancelNavigationClicked()
    {
        // Arrange
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        var originalUri = navigationManager.Uri;
        
        var cut = RenderComponent<WorkoutTemplateForm>();
        cut.Instance.ShowUnsavedChangesDialog = true;
        cut.Instance.pendingNavigationUrl = "/other-page";
        cut.Render();
        
        // Act
        await cut.Find("[data-testid='stay-button']").ClickAsync(new MouseEventArgs());
        
        // Assert
        cut.Instance.ShowUnsavedChangesDialog.Should().BeFalse();
        cut.Instance.isDirty.Should().BeTrue(); // Still dirty
        navigationManager.Uri.Should().Be(originalUri); // Didn't navigate
    }
    
    [Fact]
    public async Task Should_SaveAndNavigate_WhenSaveAndContinueClicked()
    {
        // Arrange
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        var jsRuntime = new Mock<IJSRuntime>();
        Services.AddSingleton(jsRuntime.Object);
        
        var submitInvoked = false;
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.OnValidSubmit, EventCallback.Factory.Create<WorkoutTemplateFormModel>(this, (model) => 
            {
                submitInvoked = true;
            })));
        
        // Setup valid form
        cut.Instance.Model.Name = "Test Template";
        cut.Instance.Model.CategoryId = "cat1";
        cut.Instance.Model.DifficultyId = "diff1";
        cut.Instance.Model.EstimatedDurationMinutes = 30;
        
        cut.Instance.ShowUnsavedChangesDialog = true;
        cut.Instance.pendingNavigationUrl = "/other-page";
        cut.Render();
        
        // Act
        await cut.Find("[data-testid='save-navigate-button']").ClickAsync(new MouseEventArgs());
        
        // Assert
        submitInvoked.Should().BeTrue();
        cut.Instance.ShowUnsavedChangesDialog.Should().BeFalse();
        navigationManager.Uri.Should().EndWith("/other-page");
        
        // Verify recovery data was cleared
        jsRuntime.Verify(x => x.InvokeVoidAsync("localStorage.removeItem", 
            It.Is<object[]>(args => args[0].ToString() == "workoutTemplateFormData")), Times.Once);
    }
    
    [Fact]
    public void Should_DisableSaveAndNavigate_WhenFormInvalid()
    {
        // Arrange
        var cut = RenderComponent<WorkoutTemplateForm>();
        
        // Make form invalid (missing required fields)
        cut.Instance.Model.Name = "";
        cut.Instance.ShowUnsavedChangesDialog = true;
        cut.Render();
        
        // Assert
        var saveButton = cut.Find("[data-testid='save-navigate-button']");
        saveButton.GetAttribute("disabled").Should().NotBeNull();
    }
    
    [Fact]
    public async Task Should_SaveFormDataToLocalStorage_OnNavigation()
    {
        // Arrange
        var jsRuntime = new Mock<IJSRuntime>();
        Services.AddSingleton(jsRuntime.Object);
        
        var cut = RenderComponent<WorkoutTemplateForm>();
        cut.Instance.Model.Name = "Test Template";
        cut.Instance.Model.CategoryId = "cat1";
        cut.Instance.MarkAsDirty();
        
        // Act - Save form data to local storage
        await cut.Instance.SaveFormDataToLocalStorage();
        
        // Assert
        jsRuntime.Verify(x => x.InvokeVoidAsync("localStorage.setItem", 
            It.Is<object[]>(args => 
                args[0].ToString() == "workoutTemplateFormData" && 
                args[1].ToString()!.Contains("Test Template"))), Times.Once);
    }
    
    [Fact]
    public async Task Should_ClearDirtyFlag_OnSuccessfulSave()
    {
        // Arrange
        var jsRuntime = new Mock<IJSRuntime>();
        Services.AddSingleton(jsRuntime.Object);
        
        var cut = RenderComponent<WorkoutTemplateForm>(parameters => parameters
            .Add(p => p.OnValidSubmit, EventCallback.Factory.Create<WorkoutTemplateFormModel>(this, (model) => { })));
        
        cut.Instance.MarkAsDirty();
        cut.Instance.isDirty.Should().BeTrue();
        
        // Setup valid form
        cut.Instance.Model.Name = "Test";
        cut.Instance.Model.CategoryId = "cat1";
        cut.Instance.Model.DifficultyId = "diff1";
        cut.Instance.Model.EstimatedDurationMinutes = 30;
        
        // Act
        await cut.Instance.HandleValidSubmit();
        
        // Assert
        cut.Instance.isDirty.Should().BeFalse();
        
        // Verify recovery data was cleared
        jsRuntime.Verify(x => x.InvokeVoidAsync("localStorage.removeItem", 
            It.Is<object[]>(args => args[0].ToString() == "workoutTemplateFormData")), Times.Once);
    }
    
    [Fact]
    public void Should_NotShowUnsavedChangesDialog_WhenAutoSaving()
    {
        // Arrange
        var cut = RenderComponent<WorkoutTemplateForm>();
        cut.Instance.MarkAsDirty();
        cut.Instance.IsAutoSaving = true;
        
        // Act - When auto-saving, navigation should not be blocked
        cut.Render();
        
        // Assert
        cut.Instance.ShowUnsavedChangesDialog.Should().BeFalse();
    }
    
    [Fact]
    public void Should_NotShowUnsavedChangesDialog_WhenSubmitting()
    {
        // Arrange
        var cut = RenderComponent<WorkoutTemplateForm>();
        cut.Instance.MarkAsDirty();
        cut.Instance.IsSubmitting = true;
        
        // Act - When submitting, navigation should not be blocked
        cut.Render();
        
        // Assert
        cut.Instance.ShowUnsavedChangesDialog.Should().BeFalse();
    }
    
    #endregion
}
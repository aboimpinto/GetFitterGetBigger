using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using GetFitterGetBigger.Admin.Components.WorkoutTemplates;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http;
using Bunit.TestDoubles;

namespace GetFitterGetBigger.Admin.Tests.Components.WorkoutTemplates
{
    public class WorkoutTemplateCreateFormTests : TestContext
    {
        private readonly Mock<IWorkoutTemplateService> _mockWorkoutTemplateService;
        private readonly List<ReferenceDataDto> _categories;
        private readonly List<ReferenceDataDto> _difficulties;

        public WorkoutTemplateCreateFormTests()
        {
            _mockWorkoutTemplateService = new Mock<IWorkoutTemplateService>();
            
            _categories = new List<ReferenceDataDto>
            {
                new() { Id = "cat1", Value = "Strength" },
                new() { Id = "cat2", Value = "Cardio" }
            };
            
            _difficulties = new List<ReferenceDataDto>
            {
                new() { Id = "diff1", Value = "Beginner" },
                new() { Id = "diff2", Value = "Intermediate" }
            };

            Services.AddSingleton(_mockWorkoutTemplateService.Object);
        }

        [Fact]
        public void Should_RenderLoadingState_WhenInitializing()
        {
            // Arrange
            var loadingTcs = new TaskCompletionSource<List<ReferenceDataDto>>();
            _mockWorkoutTemplateService
                .Setup(x => x.GetWorkoutCategoriesAsync())
                .Returns(loadingTcs.Task);
            _mockWorkoutTemplateService
                .Setup(x => x.GetDifficultyLevelsAsync())
                .Returns(Task.FromResult(new List<ReferenceDataDto>()));

            // Act
            var cut = RenderComponent<WorkoutTemplateCreateForm>();

            // Assert
            Assert.NotNull(cut.Find("[data-testid='loading-indicator']"));
            Assert.Throws<Bunit.ElementNotFoundException>(() => cut.Find("form"));
        }

        [Fact]
        public async Task Should_LoadReferenceData_OnInitialization()
        {
            // Arrange
            _mockWorkoutTemplateService
                .Setup(x => x.GetWorkoutCategoriesAsync())
                .ReturnsAsync(_categories);
            _mockWorkoutTemplateService
                .Setup(x => x.GetDifficultyLevelsAsync())
                .ReturnsAsync(_difficulties);

            // Act
            var cut = RenderComponent<WorkoutTemplateCreateForm>();
            await Task.Delay(50); // Allow async initialization to complete

            // Assert
            var categorySelect = cut.Find("[data-testid='category-select']");
            var categoryOptions = categorySelect.QuerySelectorAll("option");
            Assert.Equal(3, categoryOptions.Length); // Empty option + 2 categories
            
            var difficultySelect = cut.Find("[data-testid='difficulty-select']");
            var difficultyOptions = difficultySelect.QuerySelectorAll("option");
            Assert.Equal(3, difficultyOptions.Length); // Empty option + 2 difficulties
        }

        [Fact]
        public async Task Should_ShowErrorMessage_WhenLoadingReferenceDataFails()
        {
            // Arrange
            _mockWorkoutTemplateService
                .Setup(x => x.GetWorkoutCategoriesAsync())
                .ThrowsAsync(new Exception("Service error"));
            _mockWorkoutTemplateService
                .Setup(x => x.GetDifficultyLevelsAsync())
                .ReturnsAsync(_difficulties);

            // Act
            var cut = RenderComponent<WorkoutTemplateCreateForm>();
            await Task.Delay(50); // Allow async initialization to complete

            // Assert
            var errorMessage = cut.Find("[data-testid='error-message']");
            Assert.Contains("Failed to load reference data", errorMessage.TextContent);
        }

        [Fact]
        public async Task Should_ValidateRequiredFields()
        {
            // Arrange
            SetupSuccessfulReferenceDataLoad();
            var cut = RenderComponent<WorkoutTemplateCreateForm>();
            await Task.Delay(50);

            // Act
            var form = cut.Find("form");
            await cut.InvokeAsync(() => form.Submit());

            // Assert
            var validationMessages = cut.FindAll(".validation-message");
            Assert.NotEmpty(validationMessages);
        }

        [Fact]
        public async Task Should_ProcessTagsCorrectly()
        {
            // Arrange
            SetupSuccessfulReferenceDataLoad();
            var cut = RenderComponent<WorkoutTemplateCreateForm>();
            await Task.Delay(50);

            // Act
            var tagsInput = cut.Find("[data-testid='tags-input']");
            await cut.InvokeAsync(() => tagsInput.Change("tag1, tag2, tag3"));
            await cut.InvokeAsync(() => tagsInput.Blur());

            // Assert
            var tagBadges = cut.FindAll("[data-testid='tags-display'] .badge");
            Assert.Equal(3, tagBadges.Count);
        }

        [Fact]
        public async Task Should_CreateTemplate_WhenFormIsValid()
        {
            // Arrange
            SetupSuccessfulReferenceDataLoad();
            var createdTemplate = new WorkoutTemplateDto 
            { 
                Id = "123", 
                Name = "Test Template",
                Category = new ReferenceDataDto { Id = "cat1", Value = "Strength" },
                Difficulty = new ReferenceDataDto { Id = "diff1", Value = "Beginner" },
                WorkoutState = new ReferenceDataDto { Id = "draft", Value = "Draft" },
                EstimatedDurationMinutes = 60 
            };
            _mockWorkoutTemplateService
                .Setup(x => x.CreateWorkoutTemplateAsync(It.IsAny<CreateWorkoutTemplateDto>()))
                .ReturnsAsync(createdTemplate);

            var templateCreatedCalled = false;
            var cut = RenderComponent<WorkoutTemplateCreateForm>(parameters => parameters
                .Add(p => p.OnTemplateCreated, EventCallback.Factory.Create<WorkoutTemplateDto>(this, _ => templateCreatedCalled = true)));
            await Task.Delay(50);

            // Act
            await FillAndSubmitForm(cut);

            // Assert
            Assert.True(templateCreatedCalled);
            _mockWorkoutTemplateService.Verify(x => x.CreateWorkoutTemplateAsync(It.Is<CreateWorkoutTemplateDto>(dto =>
                dto.Name == "Test Template" &&
                dto.CategoryId == "cat1" &&
                dto.DifficultyId == "diff1" &&
                dto.EstimatedDurationMinutes == 60
            )), Times.Once);
        }

        [Fact]
        public async Task Should_ShowSubmittingState_DuringSubmission()
        {
            // Arrange
            SetupSuccessfulReferenceDataLoad();
            var submissionTcs = new TaskCompletionSource<WorkoutTemplateDto>();
            _mockWorkoutTemplateService
                .Setup(x => x.CreateWorkoutTemplateAsync(It.IsAny<CreateWorkoutTemplateDto>()))
                .Returns(submissionTcs.Task);

            var cut = RenderComponent<WorkoutTemplateCreateForm>();
            await Task.Delay(50);

            // Act
            await FillAndSubmitForm(cut);

            // Assert
            var submitButton = cut.Find("[data-testid='submit-button']");
            Assert.True(submitButton.HasAttribute("disabled"));
            Assert.Contains("spinner-border", submitButton.InnerHtml);
        }

        [Fact]
        public async Task Should_ShowErrorMessage_WhenCreateFails()
        {
            // Arrange
            SetupSuccessfulReferenceDataLoad();
            _mockWorkoutTemplateService
                .Setup(x => x.CreateWorkoutTemplateAsync(It.IsAny<CreateWorkoutTemplateDto>()))
                .ThrowsAsync(new Exception("Create failed"));

            var cut = RenderComponent<WorkoutTemplateCreateForm>();
            await Task.Delay(50);

            // Act
            await FillAndSubmitForm(cut);

            // Assert
            var errorMessage = cut.Find("[data-testid='error-message']");
            Assert.Contains("Failed to create workout template", errorMessage.TextContent);
        }

        [Fact]
        public async Task Should_ShowNetworkError_WhenHttpRequestFails()
        {
            // Arrange
            SetupSuccessfulReferenceDataLoad();
            _mockWorkoutTemplateService
                .Setup(x => x.CreateWorkoutTemplateAsync(It.IsAny<CreateWorkoutTemplateDto>()))
                .ThrowsAsync(new HttpRequestException("Network error"));

            var cut = RenderComponent<WorkoutTemplateCreateForm>();
            await Task.Delay(50);

            // Act
            await FillAndSubmitForm(cut);

            // Assert
            var errorMessage = cut.Find("[data-testid='error-message']");
            Assert.Contains("Network error occurred", errorMessage.TextContent);
        }

        [Fact]
        public async Task Should_NavigateToList_WhenCancelClicked()
        {
            // Arrange
            SetupSuccessfulReferenceDataLoad();
            var cut = RenderComponent<WorkoutTemplateCreateForm>();
            await Task.Delay(50);
            var navMan = Services.GetRequiredService<FakeNavigationManager>();

            // Act
            var cancelButton = cut.Find("[data-testid='cancel-button']");
            await cut.InvokeAsync(() => cancelButton.Click());

            // Assert
            Assert.Equal("http://localhost/workout-templates", navMan.Uri);
        }

        [Fact]
        public async Task Should_InvokeOnCancel_WhenCancelClickedAndCallbackProvided()
        {
            // Arrange
            SetupSuccessfulReferenceDataLoad();
            var cancelCalled = false;
            var cut = RenderComponent<WorkoutTemplateCreateForm>(parameters => parameters
                .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => cancelCalled = true)));
            await Task.Delay(50);
            var navMan = Services.GetRequiredService<FakeNavigationManager>();
            var originalUri = navMan.Uri;

            // Act
            var cancelButton = cut.Find("[data-testid='cancel-button']");
            await cut.InvokeAsync(() => cancelButton.Click());

            // Assert
            Assert.True(cancelCalled);
            Assert.Equal(originalUri, navMan.Uri); // Navigation should not happen when callback is provided
        }

        [Fact]
        public async Task Should_HandleEmptyTags()
        {
            // Arrange
            SetupSuccessfulReferenceDataLoad();
            var cut = RenderComponent<WorkoutTemplateCreateForm>();
            await Task.Delay(50);

            // Act
            var tagsInput = cut.Find("[data-testid='tags-input']");
            await cut.InvokeAsync(() => tagsInput.Change("   "));
            await cut.InvokeAsync(() => tagsInput.Blur());

            // Assert
            Assert.Throws<Bunit.ElementNotFoundException>(() => cut.Find("[data-testid='tags-display']"));
        }

        [Fact]
        public async Task Should_RemoveDuplicateTags()
        {
            // Arrange
            SetupSuccessfulReferenceDataLoad();
            var cut = RenderComponent<WorkoutTemplateCreateForm>();
            await Task.Delay(50);

            // Act
            var tagsInput = cut.Find("[data-testid='tags-input']");
            await cut.InvokeAsync(() => tagsInput.Change("tag1, tag2, tag1, tag3, tag2"));
            await cut.InvokeAsync(() => tagsInput.Blur());

            // Assert
            var tagBadges = cut.FindAll("[data-testid='tags-display'] .badge");
            Assert.Equal(3, tagBadges.Count); // Only unique tags
        }

        private void SetupSuccessfulReferenceDataLoad()
        {
            _mockWorkoutTemplateService
                .Setup(x => x.GetWorkoutCategoriesAsync())
                .ReturnsAsync(_categories);
            _mockWorkoutTemplateService
                .Setup(x => x.GetDifficultyLevelsAsync())
                .ReturnsAsync(_difficulties);
        }

        private async Task FillAndSubmitForm(IRenderedComponent<WorkoutTemplateCreateForm> cut)
        {
            // Fill required fields
            var nameInput = cut.Find("[data-testid='name-input']");
            await cut.InvokeAsync(() => nameInput.Change("Test Template"));

            var categorySelect = cut.Find("[data-testid='category-select']");
            await cut.InvokeAsync(() => categorySelect.Change("cat1"));

            var difficultySelect = cut.Find("[data-testid='difficulty-select']");
            await cut.InvokeAsync(() => difficultySelect.Change("diff1"));

            var durationInput = cut.Find("[data-testid='duration-input']");
            await cut.InvokeAsync(() => durationInput.Change(60));

            // Submit form
            var form = cut.Find("form");
            await cut.InvokeAsync(() => form.Submit());
        }
    }
}
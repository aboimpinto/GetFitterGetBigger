using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Pages.WorkoutTemplates;
using GetFitterGetBigger.Admin.Components.WorkoutTemplates;
using GetFitterGetBigger.Admin.Components.Shared;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Builders;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.WorkoutTemplates;

public class WorkoutTemplateDetailPageIntegrationTests : TestContext
{
    private readonly Mock<IWorkoutTemplateService> _mockService;
    private readonly Mock<IWorkoutTemplateStateService> _mockStateService;
    private readonly WorkoutTemplateDto _defaultTemplate;

    public WorkoutTemplateDetailPageIntegrationTests()
    {
        _mockService = new Mock<IWorkoutTemplateService>();
        _mockStateService = new Mock<IWorkoutTemplateStateService>();

        _defaultTemplate = new WorkoutTemplateDtoBuilder()
            .WithId("template-1")
            .WithName("Test Workout Template")
            .WithDescription("A test workout template for integration testing")
            .WithCategory("cat-1", "Strength Training")
            .WithDifficulty("diff-2", "Intermediate")
            .WithEstimatedDuration(60)
            .WithTags("test", "integration")
            .WithObjectives(
                new ReferenceDataDto { Id = "obj-1", Value = "Build Strength", Description = "Increase strength" }
            )
            .WithExercises(
                new WorkoutTemplateExerciseDto
                {
                    ExerciseId = "ex-1",
                    ExerciseName = "Bench Press",
                    OrderIndex = 1,
                    Sets = 3,
                    TargetReps = "8-10",
                    RestSeconds = 90
                }
            )
            .WithWorkoutState("state-draft", "DRAFT")
            .WithIsPublic(false)
            .Build();

        // Register services
        Services.AddSingleton(_mockService.Object);
        Services.AddSingleton(_mockStateService.Object);
    }

    [Fact]
    public void Should_DisplayTemplateDetails_WhenLoadedSuccessfully()
    {
        // Arrange
        _mockService.Setup(x => x.GetWorkoutTemplateByIdAsync("template-1"))
            .ReturnsAsync(_defaultTemplate);

        // Act
        var cut = RenderComponent<WorkoutTemplateDetailPage>(parameters => parameters
            .Add(p => p.Id, "template-1"));

        // Assert - Verify template details are displayed
        cut.WaitForAssertion(() =>
        {
            var detail = cut.FindComponent<WorkoutTemplateDetail>();
            detail.Instance.Template.Should().NotBeNull();
            detail.Instance.Template!.Name.Should().Be("Test Workout Template");
        });

        // Verify all main sections are rendered
        cut.Find("[data-testid='template-name']").TextContent.Should().Be("Test Workout Template");
        cut.Find("[data-testid='template-description']").TextContent.Should().Be("A test workout template for integration testing");
        cut.Find("[data-testid='detail-category']").TextContent.Should().Contain("Strength Training");
        cut.Find("[data-testid='detail-difficulty']").TextContent.Should().Contain("Intermediate");
        cut.Find("[data-testid='detail-duration']").TextContent.Should().Contain("1 hour");
    }

    [Fact]
    public void Should_NavigateToEdit_WhenEditButtonClicked()
    {
        // Arrange
        _mockService.Setup(x => x.GetWorkoutTemplateByIdAsync("template-1"))
            .ReturnsAsync(_defaultTemplate);

        var navMan = Services.GetRequiredService<NavigationManager>();

        // Act
        var cut = RenderComponent<WorkoutTemplateDetailPage>(parameters => parameters
            .Add(p => p.Id, "template-1"));

        cut.WaitForAssertion(() =>
        {
            var editButton = cut.Find("[data-testid='action-edit']");
            editButton.Click();
        });

        // Assert - bUnit's FakeNavigationManager tracks the navigation
        navMan.Uri.Should().EndWith("/workout-templates/template-1/edit");
    }

    [Fact]
    public async Task Should_DuplicateTemplate_WhenDuplicateButtonClicked()
    {
        // Arrange
        _mockService.Setup(x => x.GetWorkoutTemplateByIdAsync("template-1"))
            .ReturnsAsync(_defaultTemplate);

        var duplicatedTemplate = new WorkoutTemplateDtoBuilder()
            .WithId("template-2")
            .WithName("Test Workout Template (Copy)")
            .WithWorkoutState("state-draft", "DRAFT")
            .Build();

        _mockService.Setup(x => x.DuplicateWorkoutTemplateAsync("template-1", It.IsAny<DuplicateWorkoutTemplateDto>()))
            .ReturnsAsync(duplicatedTemplate);

        var navMan = Services.GetRequiredService<NavigationManager>();

        // Act
        var cut = RenderComponent<WorkoutTemplateDetailPage>(parameters => parameters
            .Add(p => p.Id, "template-1"));

        await cut.InvokeAsync(async () =>
        {
            var duplicateButton = cut.Find("[data-testid='action-duplicate']");
            await duplicateButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
        });

        // Assert
        _mockService.Verify(x => x.DuplicateWorkoutTemplateAsync("template-1", It.IsAny<DuplicateWorkoutTemplateDto>()), Times.Once);
        navMan.Uri.Should().EndWith("/workout-templates/template-2/edit");
    }

    [Fact]
    public async Task Should_DeleteTemplate_WhenDeleteButtonClickedAndConfirmed()
    {
        // Arrange
        _mockService.Setup(x => x.GetWorkoutTemplateByIdAsync("template-1"))
            .ReturnsAsync(_defaultTemplate);

        _mockService.Setup(x => x.DeleteWorkoutTemplateAsync("template-1"))
            .Returns(Task.CompletedTask);

        var navMan = Services.GetRequiredService<NavigationManager>();

        // Act
        var cut = RenderComponent<WorkoutTemplateDetailPage>(parameters => parameters
            .Add(p => p.Id, "template-1"));

        await cut.InvokeAsync(async () =>
        {
            var deleteButton = cut.Find("[data-testid='action-delete']");
            await deleteButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());
        });

        // Assert - In a real test, we'd need to handle confirmation dialog
        // For now, verify delete was attempted
        _mockService.Verify(x => x.DeleteWorkoutTemplateAsync("template-1"), Times.Once);
        navMan.Uri.Should().EndWith("/workout-templates");
    }

    [Fact]
    public async Task Should_TransitionState_WhenStateButtonClicked()
    {
        // Arrange
        _mockService.Setup(x => x.GetWorkoutTemplateByIdAsync("template-1"))
            .ReturnsAsync(_defaultTemplate);

        var updatedTemplate = new WorkoutTemplateDtoBuilder()
            .WithId(_defaultTemplate.Id)
            .WithName(_defaultTemplate.Name)
            .WithDescription(_defaultTemplate.Description)
            .WithCategory(_defaultTemplate.Category.Id, _defaultTemplate.Category.Value)
            .WithDifficulty(_defaultTemplate.Difficulty.Id, _defaultTemplate.Difficulty.Value)
            .WithEstimatedDuration(_defaultTemplate.EstimatedDurationMinutes)
            .WithTags(_defaultTemplate.Tags.ToArray())
            .WithObjectives(_defaultTemplate.Objectives.ToArray())
            .WithExercises(_defaultTemplate.Exercises.ToArray())
            .WithWorkoutState("state-production", "PRODUCTION")
            .WithIsPublic(_defaultTemplate.IsPublic)
            .Build();

        _mockService.Setup(x => x.ChangeWorkoutTemplateStateAsync("template-1", It.IsAny<ChangeWorkoutStateDto>()))
            .ReturnsAsync(updatedTemplate);

        // Act
        var cut = RenderComponent<WorkoutTemplateDetailPage>(parameters => parameters
            .Add(p => p.Id, "template-1"));

        // Find the state transition button component
        var stateButton = cut.FindComponent<StateTransitionButton>();
        
        // Trigger state change
        await cut.InvokeAsync(async () =>
        {
            await stateButton.Instance.OnStateChanged.InvokeAsync(
                new ReferenceDataDto { Id = "state-production", Value = "PRODUCTION" });
        });

        // Assert
        _mockService.Verify(x => x.ChangeWorkoutTemplateStateAsync("template-1", It.IsAny<ChangeWorkoutStateDto>()), Times.Once);
    }

    [Fact]
    public void Should_NotShowEditButton_ForProductionTemplates()
    {
        // Arrange
        var productionTemplate = new WorkoutTemplateDtoBuilder()
            .WithId("template-prod")
            .WithName("Production Template")
            .WithWorkoutState("state-production", "PRODUCTION")
            .Build();

        _mockService.Setup(x => x.GetWorkoutTemplateByIdAsync("template-prod"))
            .ReturnsAsync(productionTemplate);

        // Act
        var cut = RenderComponent<WorkoutTemplateDetailPage>(parameters => parameters
            .Add(p => p.Id, "template-prod"));

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.FindAll("[data-testid='action-edit']").Should().BeEmpty();
        });
    }

    [Fact]
    public void Should_ShowArchiveButton_ForProductionTemplates()
    {
        // Arrange
        var productionTemplate = new WorkoutTemplateDtoBuilder()
            .WithId("template-prod")
            .WithName("Production Template")
            .WithWorkoutState("state-production", "PRODUCTION")
            .Build();

        _mockService.Setup(x => x.GetWorkoutTemplateByIdAsync("template-prod"))
            .ReturnsAsync(productionTemplate);

        // Act
        var cut = RenderComponent<WorkoutTemplateDetailPage>(parameters => parameters
            .Add(p => p.Id, "template-prod"));

        // Assert
        cut.WaitForAssertion(() =>
        {
            var stateButton = cut.FindComponent<StateTransitionButton>();
            stateButton.Should().NotBeNull();
            stateButton.Instance.ButtonText.Should().Be("Archive Template");
            stateButton.Instance.TargetState.Value.Should().Be("ARCHIVED");
        });
    }

    [Fact]
    public void Should_DisplayLoadingState_WhileLoadingTemplate()
    {
        // Arrange
        var tcs = new TaskCompletionSource<WorkoutTemplateDto?>();
        _mockService.Setup(x => x.GetWorkoutTemplateByIdAsync("template-1"))
            .Returns(tcs.Task);

        // Act
        var cut = RenderComponent<WorkoutTemplateDetailPage>(parameters => parameters
            .Add(p => p.Id, "template-1"));

        // Assert - Should show loading skeleton initially
        cut.FindComponents<WorkoutTemplateDetailSkeleton>().Should().NotBeEmpty();

        // Complete loading
        tcs.SetResult(_defaultTemplate);

        // Assert - Loading should be gone and content displayed
        cut.WaitForAssertion(() =>
        {
            cut.FindComponents<WorkoutTemplateDetailSkeleton>().Should().BeEmpty();
            cut.Find("[data-testid='workout-template-detail']").Should().NotBeNull();
        });
    }

    [Fact]
    public void Should_DisplayError_WhenLoadingFails()
    {
        // Arrange
        _mockService.Setup(x => x.GetWorkoutTemplateByIdAsync("template-1"))
            .ThrowsAsync(new HttpRequestException("Failed to load template"));

        // Act
        var cut = RenderComponent<WorkoutTemplateDetailPage>(parameters => parameters
            .Add(p => p.Id, "template-1"));

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.FindAll("[data-testid='error-message']").Should().NotBeEmpty();
            cut.Find("[data-testid='error-message']").TextContent
                .Should().Contain("Failed to load template");
        });
    }

    [Fact]
    public void Should_DisplayNotFound_WhenTemplateDoesNotExist()
    {
        // Arrange
        _mockService.Setup(x => x.GetWorkoutTemplateByIdAsync("non-existent"))
            .ReturnsAsync((WorkoutTemplateDto?)null);

        // Act
        var cut = RenderComponent<WorkoutTemplateDetailPage>(parameters => parameters
            .Add(p => p.Id, "non-existent"));

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find("[data-testid='no-template']").TextContent
                .Should().Contain("No workout template to display");
        });
    }
}
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
using GetFitterGetBigger.Admin.Tests.TestHelpers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.WorkoutTemplates;

public class WorkoutTemplateDetailPageIntegrationTests : WorkoutTemplateTestBase
{
    private readonly WorkoutTemplateDto _defaultTemplate;

    public WorkoutTemplateDetailPageIntegrationTests()
    {
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
    }

    [Fact]
    public void Should_DisplayTemplateDetails_WhenLoadedSuccessfully()
    {
        // Arrange
        MockWorkoutTemplateService.Setup(x => x.GetWorkoutTemplateByIdAsync("template-1"))
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
        MockWorkoutTemplateService.Setup(x => x.GetWorkoutTemplateByIdAsync("template-1"))
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
        MockWorkoutTemplateService.Setup(x => x.GetWorkoutTemplateByIdAsync("template-1"))
            .ReturnsAsync(_defaultTemplate);

        var duplicatedTemplate = new WorkoutTemplateDtoBuilder()
            .WithId("template-2")
            .WithName("Test Workout Template (Copy)")
            .WithWorkoutState("state-draft", "DRAFT")
            .Build();

        MockWorkoutTemplateService.Setup(x => x.DuplicateWorkoutTemplateAsync("template-1", It.IsAny<DuplicateWorkoutTemplateDto>()))
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
        MockWorkoutTemplateService.Verify(x => x.DuplicateWorkoutTemplateAsync("template-1", It.IsAny<DuplicateWorkoutTemplateDto>()), Times.Once);
        navMan.Uri.Should().EndWith("/workout-templates/template-2/edit");
    }

    [Fact]
    public async Task Should_DeleteTemplate_WhenDeleteButtonClickedAndConfirmed()
    {
        // Arrange
        MockWorkoutTemplateService.Setup(x => x.GetWorkoutTemplateByIdAsync("template-1"))
            .ReturnsAsync(_defaultTemplate);

        MockWorkoutTemplateService.Setup(x => x.DeleteWorkoutTemplateAsync("template-1"))
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
        MockWorkoutTemplateService.Verify(x => x.DeleteWorkoutTemplateAsync("template-1"), Times.Once);
        navMan.Uri.Should().EndWith("/workout-templates");
    }

    [Fact]
    public async Task Should_TransitionState_WhenStateButtonClicked()
    {
        // Arrange
        MockWorkoutTemplateService.Setup(x => x.GetWorkoutTemplateByIdAsync("template-1"))
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

        MockWorkoutTemplateService.Setup(x => x.ChangeWorkoutTemplateStateAsync("template-1", It.IsAny<ChangeWorkoutStateDto>()))
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
        MockWorkoutTemplateService.Verify(x => x.ChangeWorkoutTemplateStateAsync("template-1", 
            It.Is<ChangeWorkoutStateDto>(dto => dto.NewStateId == "state-production")), Times.Once);
    }

    [Fact]
    public void Should_NotShowEditButton_ForProductionTemplates()
    {
        // Arrange
        var productionTemplate = new WorkoutTemplateDtoBuilder()
            .WithId("template-1")
            .WithName("Production Template")
            .WithWorkoutState("state-production", "PRODUCTION")
            .Build();

        MockWorkoutTemplateService.Setup(x => x.GetWorkoutTemplateByIdAsync("template-1"))
            .ReturnsAsync(productionTemplate);

        // Act
        var cut = RenderComponent<WorkoutTemplateDetailPage>(parameters => parameters
            .Add(p => p.Id, "template-1"));

        // Assert
        cut.WaitForAssertion(() =>
        {
            Assert.Throws<Bunit.ElementNotFoundException>(() => cut.Find("[data-testid='action-edit']"));
        });
    }

    [Fact]
    public void Should_ShowArchiveButton_ForProductionTemplates()
    {
        // Arrange
        var productionTemplate = new WorkoutTemplateDtoBuilder()
            .WithId("template-1")
            .WithName("Production Template")
            .WithWorkoutState("state-production", "PRODUCTION")
            .Build();

        MockWorkoutTemplateService.Setup(x => x.GetWorkoutTemplateByIdAsync("template-1"))
            .ReturnsAsync(productionTemplate);

        // Act
        var cut = RenderComponent<WorkoutTemplateDetailPage>(parameters => parameters
            .Add(p => p.Id, "template-1"));

        // Assert
        cut.WaitForAssertion(() =>
        {
            // Production templates should show Archive button
            var detail = cut.FindComponent<WorkoutTemplateDetail>();
            detail.Should().NotBeNull();
            
            // Find the archive state transition button
            var buttons = cut.FindComponents<StateTransitionButton>();
            buttons.Should().NotBeEmpty();
            buttons.Should().Contain(btn => btn.Instance.TargetState != null && btn.Instance.TargetState.Value == "ARCHIVED");
        });
    }

    [Fact]
    public void Should_DisplayError_WhenLoadingFails()
    {
        // Arrange
        MockWorkoutTemplateService.Setup(x => x.GetWorkoutTemplateByIdAsync("template-1"))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var cut = RenderComponent<WorkoutTemplateDetailPage>(parameters => parameters
            .Add(p => p.Id, "template-1"));

        // Assert
        cut.WaitForAssertion(() =>
        {
            var errorDisplay = cut.FindComponent<ErrorDisplay>();
            errorDisplay.Instance.ErrorMessage.Should().NotBeEmpty();
            errorDisplay.Instance.ShowRetry.Should().BeTrue();
        });
    }

    [Fact]
    public void Should_DisplayNotFound_WhenTemplateDoesNotExist()
    {
        // Arrange
        MockWorkoutTemplateService.Setup(x => x.GetWorkoutTemplateByIdAsync("non-existent"))
            .ReturnsAsync((WorkoutTemplateDto?)null);

        // Act
        var cut = RenderComponent<WorkoutTemplateDetailPage>(parameters => parameters
            .Add(p => p.Id, "non-existent"));

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find("[data-testid='not-found-message']").TextContent
                .Should().Contain("Workout template not found");
        });
    }

    [Fact]
    public void Should_DisplayLoadingState_WhileLoadingTemplate()
    {
        // Arrange
        var tcs = new TaskCompletionSource<WorkoutTemplateDto?>();
        MockWorkoutTemplateService.Setup(x => x.GetWorkoutTemplateByIdAsync("template-1"))
            .Returns(tcs.Task);

        // Act
        var cut = RenderComponent<WorkoutTemplateDetailPage>(parameters => parameters
            .Add(p => p.Id, "template-1"));

        // Assert - Should show skeleton loader
        var skeleton = cut.FindComponent<WorkoutTemplateDetailSkeleton>();
        skeleton.Should().NotBeNull();

        // Complete the loading
        tcs.SetResult(_defaultTemplate);

        // Verify skeleton is replaced with actual content
        cut.WaitForAssertion(() =>
        {
            Assert.Throws<Bunit.Rendering.ComponentNotFoundException>(() => cut.FindComponent<WorkoutTemplateDetailSkeleton>());
            cut.FindComponent<WorkoutTemplateDetail>().Should().NotBeNull();
        });
    }
}
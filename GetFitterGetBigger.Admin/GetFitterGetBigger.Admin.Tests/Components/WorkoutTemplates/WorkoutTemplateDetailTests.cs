using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.WorkoutTemplates;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Builders;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Components.WorkoutTemplates;

public class WorkoutTemplateDetailTests : TestContext
{
    private WorkoutTemplateDto CreateDefaultTemplate()
    {
        return new WorkoutTemplateDtoBuilder()
            .WithId("template-1")
            .WithName("Full Body Workout")
            .WithDescription("A comprehensive full body workout routine")
            .WithCategory("cat-1", "Strength Training")
            .WithDifficulty("diff-2", "Intermediate")
            .WithEstimatedDuration(60)
            .WithTags("strength", "full-body", "intermediate")
            .WithObjectives(
                new ReferenceDataDto { Id = "obj-1", Value = "Build Strength", Description = "Increase overall strength" },
                new ReferenceDataDto { Id = "obj-2", Value = "Muscle Endurance", Description = "Improve muscular endurance" }
            )
            .WithExercises(
                new WorkoutTemplateExerciseDto
                {
                    ExerciseId = "exercise-1",
                    ExerciseName = "Barbell Squat",
                    OrderIndex = 1,
                    Sets = 3,
                    TargetReps = "8-12",
                    RestSeconds = 90,
                    Notes = "Focus on form"
                }
            )
            .WithWorkoutState("state-draft", "DRAFT")
            .WithIsPublic(false)
            .Build();
    }

    [Fact]
    public void Should_ShowNoTemplateMessage_WhenTemplateIsNull()
    {
        // Act
        var cut = RenderComponent<WorkoutTemplateDetail>(parameters => parameters
            .Add(p => p.Template, null));

        // Assert
        cut.Find("[data-testid='no-template']").TextContent.Should().Contain("No workout template to display");
        cut.FindAll("[data-testid='workout-template-detail']").Should().BeEmpty();
    }

    [Fact]
    public void Should_DisplayAllBasicTemplateInformation()
    {
        // Arrange
        var template = CreateDefaultTemplate();

        // Act
        var cut = RenderComponent<WorkoutTemplateDetail>(parameters => parameters
            .Add(p => p.Template, template));

        // Assert
        cut.Find("[data-testid='template-name']").TextContent.Should().Be("Full Body Workout");
        cut.Find("[data-testid='template-description']").TextContent.Should().Be("A comprehensive full body workout routine");
        cut.Find("[data-testid='detail-category']").TextContent.Should().Contain("Strength Training");
        cut.Find("[data-testid='detail-difficulty']").TextContent.Should().Contain("Intermediate");
        cut.Find("[data-testid='detail-duration']").TextContent.Should().Contain("1 hour");
        cut.Find("[data-testid='detail-visibility']").TextContent.Should().Contain("Private");
    }

    [Fact]
    public void Should_DisplayPublicIndicator_WhenTemplateIsPublic()
    {
        // Arrange
        var publicTemplate = new WorkoutTemplateDtoBuilder()
            .WithName("Public Workout")
            .WithIsPublic(true)
            .Build();

        // Act
        var cut = RenderComponent<WorkoutTemplateDetail>(parameters => parameters
            .Add(p => p.Template, publicTemplate));

        // Assert
        cut.Find("[data-testid='detail-visibility']").TextContent.Should().Contain("Public");
    }

    [Fact]
    public void Should_DisplayTags_WhenTagsExist()
    {
        // Arrange
        var template = CreateDefaultTemplate();

        // Act
        var cut = RenderComponent<WorkoutTemplateDetail>(parameters => parameters
            .Add(p => p.Template, template));

        // Assert
        var tags = cut.Find("[data-testid='detail-tags']").QuerySelectorAll("span");
        tags.Should().HaveCount(3);
        tags[0].TextContent.Should().Be("strength");
        tags[1].TextContent.Should().Be("full-body");
        tags[2].TextContent.Should().Be("intermediate");
    }

    [Fact]
    public void Should_NotDisplayTagsSection_WhenNoTags()
    {
        // Arrange
        var templateWithoutTags = new WorkoutTemplateDtoBuilder()
            .WithName("No Tags Workout")
            .WithTags()
            .Build();

        // Act
        var cut = RenderComponent<WorkoutTemplateDetail>(parameters => parameters
            .Add(p => p.Template, templateWithoutTags));

        // Assert
        cut.FindAll("[data-testid='detail-tags']").Should().BeEmpty();
    }

    [Fact]
    public void Should_DisplayObjectives_WhenObjectivesExist()
    {
        // Arrange
        var template = CreateDefaultTemplate();

        // Act
        var cut = RenderComponent<WorkoutTemplateDetail>(parameters => parameters
            .Add(p => p.Template, template));

        // Assert
        var objectives = cut.Find("[data-testid='detail-objectives']").QuerySelectorAll("div.inline-flex");
        objectives.Should().HaveCount(2);
        objectives[0].TextContent.Should().Contain("Build Strength");
        objectives[1].TextContent.Should().Contain("Muscle Endurance");
    }

    [Fact]
    public void Should_DisplayExercises_WhenExercisesExist()
    {
        // Arrange
        var template = CreateDefaultTemplate();

        // Act
        var cut = RenderComponent<WorkoutTemplateDetail>(parameters => parameters
            .Add(p => p.Template, template));

        // Assert
        var exercisesSection = cut.Find("[data-testid='detail-exercises']");
        exercisesSection.Should().NotBeNull();
        
        // Verify WorkoutTemplateExerciseView component is rendered
        var exerciseView = cut.FindComponent<WorkoutTemplateExerciseView>();
        exerciseView.Instance.Exercises.Should().HaveCount(1);
        exerciseView.Instance.ExpandAllByDefault.Should().BeTrue();
    }

    [Fact]
    public void Should_DisplayNoExercisesMessage_WhenNoExercises()
    {
        // Arrange
        var templateWithoutExercises = new WorkoutTemplateDtoBuilder()
            .WithName("No Exercises Workout")
            .WithExercises()
            .Build();

        // Act
        var cut = RenderComponent<WorkoutTemplateDetail>(parameters => parameters
            .Add(p => p.Template, templateWithoutExercises));

        // Assert
        cut.Find("[data-testid='no-exercises']").TextContent
            .Should().Contain("No exercises added to this template yet");
    }

    [Fact]
    public void Should_DisplayEquipmentPlaceholder()
    {
        // Arrange
        var template = CreateDefaultTemplate();

        // Act
        var cut = RenderComponent<WorkoutTemplateDetail>(parameters => parameters
            .Add(p => p.Template, template));

        // Assert
        cut.Find("[data-testid='equipment-placeholder']").TextContent
            .Should().Contain("Equipment information will be displayed here once the feature is available");
    }

    [Fact]
    public void Should_ShowEditButton_ForDraftTemplates()
    {
        // Arrange
        var template = CreateDefaultTemplate();
        var editCalled = false;
        EventCallback<WorkoutTemplateDto> onEdit = EventCallback.Factory.Create<WorkoutTemplateDto>(
            this, _ => editCalled = true);

        // Act
        var cut = RenderComponent<WorkoutTemplateDetail>(parameters => parameters
            .Add(p => p.Template, template)
            .Add(p => p.OnEdit, onEdit));

        // Assert
        var editButton = cut.Find("[data-testid='action-edit']");
        editButton.Should().NotBeNull();
        
        // Act - Click edit
        editButton.Click();

        // Assert
        editCalled.Should().BeTrue();
    }

    [Fact]
    public void Should_NotShowEditButton_ForProductionTemplates()
    {
        // Arrange
        var productionTemplate = new WorkoutTemplateDtoBuilder()
            .WithName("Production Workout")
            .WithWorkoutState("state-production", "PRODUCTION")
            .Build();

        // Act
        var cut = RenderComponent<WorkoutTemplateDetail>(parameters => parameters
            .Add(p => p.Template, productionTemplate)
            .Add(p => p.OnEdit, EventCallback.Factory.Create<WorkoutTemplateDto>(this, _ => { })));

        // Assert
        cut.FindAll("[data-testid='action-edit']").Should().BeEmpty();
    }

    [Fact]
    public void Should_ShowDuplicateButton_Always()
    {
        // Arrange
        var template = CreateDefaultTemplate();
        var duplicateCalled = false;
        EventCallback<WorkoutTemplateDto> onDuplicate = EventCallback.Factory.Create<WorkoutTemplateDto>(
            this, _ => duplicateCalled = true);

        // Act
        var cut = RenderComponent<WorkoutTemplateDetail>(parameters => parameters
            .Add(p => p.Template, template)
            .Add(p => p.OnDuplicate, onDuplicate));

        // Assert
        var duplicateButton = cut.Find("[data-testid='action-duplicate']");
        duplicateButton.Should().NotBeNull();
        
        // Act - Click duplicate
        duplicateButton.Click();

        // Assert
        duplicateCalled.Should().BeTrue();
    }

    [Fact]
    public void Should_ShowDeleteButton_ForDraftTemplates()
    {
        // Arrange
        var template = CreateDefaultTemplate();
        var deleteCalled = false;
        EventCallback<WorkoutTemplateDto> onDelete = EventCallback.Factory.Create<WorkoutTemplateDto>(
            this, _ => deleteCalled = true);

        // Act
        var cut = RenderComponent<WorkoutTemplateDetail>(parameters => parameters
            .Add(p => p.Template, template)
            .Add(p => p.OnDelete, onDelete));

        // Assert
        var deleteButton = cut.Find("[data-testid='action-delete']");
        deleteButton.Should().NotBeNull();
        
        // Act - Click delete
        deleteButton.Click();

        // Assert
        deleteCalled.Should().BeTrue();
    }

    [Fact]
    public void Should_ShowDeleteButton_ForArchivedTemplates()
    {
        // Arrange
        var archivedTemplate = new WorkoutTemplateDtoBuilder()
            .WithName("Archived Workout")
            .WithWorkoutState("state-archived", "ARCHIVED")
            .Build();

        // Act
        var cut = RenderComponent<WorkoutTemplateDetail>(parameters => parameters
            .Add(p => p.Template, archivedTemplate)
            .Add(p => p.OnDelete, EventCallback.Factory.Create<WorkoutTemplateDto>(this, _ => { })));

        // Assert
        cut.Find("[data-testid='action-delete']").Should().NotBeNull();
    }

    [Fact]
    public void Should_NotShowDeleteButton_ForProductionTemplates()
    {
        // Arrange
        var productionTemplate = new WorkoutTemplateDtoBuilder()
            .WithName("Production Workout")
            .WithWorkoutState("state-production", "PRODUCTION")
            .Build();

        // Act
        var cut = RenderComponent<WorkoutTemplateDetail>(parameters => parameters
            .Add(p => p.Template, productionTemplate)
            .Add(p => p.OnDelete, EventCallback.Factory.Create<WorkoutTemplateDto>(this, _ => { })));

        // Assert
        cut.FindAll("[data-testid='action-delete']").Should().BeEmpty();
    }

    [Fact]
    public void Should_ShowPublishButton_ForDraftTemplates()
    {
        // Arrange
        var template = CreateDefaultTemplate();

        // Act
        var cut = RenderComponent<WorkoutTemplateDetail>(parameters => parameters
            .Add(p => p.Template, template)
            .Add(p => p.OnStateChange, EventCallback.Factory.Create<ReferenceDataDto>(this, _ => { })));

        // Assert
        var stateButton = cut.FindComponent<StateTransitionButton>();
        stateButton.Should().NotBeNull();
        stateButton.Instance.ButtonText.Should().Be("Publish to Production");
        stateButton.Instance.TargetState.Value.Should().Be("PRODUCTION");
    }

    [Fact]
    public void Should_ShowArchiveButton_ForProductionTemplates()
    {
        // Arrange
        var productionTemplate = new WorkoutTemplateDtoBuilder()
            .WithName("Production Workout")
            .WithWorkoutState("state-production", "PRODUCTION")
            .Build();

        // Act
        var cut = RenderComponent<WorkoutTemplateDetail>(parameters => parameters
            .Add(p => p.Template, productionTemplate)
            .Add(p => p.OnStateChange, EventCallback.Factory.Create<ReferenceDataDto>(this, _ => { })));

        // Assert
        var stateButton = cut.FindComponent<StateTransitionButton>();
        stateButton.Should().NotBeNull();
        stateButton.Instance.ButtonText.Should().Be("Archive Template");
        stateButton.Instance.TargetState.Value.Should().Be("ARCHIVED");
    }

    [Fact]
    public void Should_HideAllActions_WhenShowActionsIsFalse()
    {
        // Arrange
        var template = CreateDefaultTemplate();

        // Act
        var cut = RenderComponent<WorkoutTemplateDetail>(parameters => parameters
            .Add(p => p.Template, template)
            .Add(p => p.ShowActions, false)
            .Add(p => p.OnEdit, EventCallback.Factory.Create<WorkoutTemplateDto>(this, _ => { }))
            .Add(p => p.OnDuplicate, EventCallback.Factory.Create<WorkoutTemplateDto>(this, _ => { }))
            .Add(p => p.OnDelete, EventCallback.Factory.Create<WorkoutTemplateDto>(this, _ => { })));

        // Assert
        cut.FindAll("[data-testid='action-edit']").Should().BeEmpty();
        cut.FindAll("[data-testid='action-duplicate']").Should().BeEmpty();
        cut.FindAll("[data-testid='action-delete']").Should().BeEmpty();
        // When ShowActions is false, the action buttons section should not be rendered
        cut.Find("[data-testid='equipment-placeholder']").Should().NotBeNull();
    }

    [Fact]
    public void Should_FormatDurationCorrectly()
    {
        // Arrange
        var testCases = new[]
        {
            (30, "30 minutes"),
            (60, "1 hour"),
            (90, "1 hour 30 minutes"),
            (120, "2 hours"),
            (150, "2 hours 30 minutes")
        };

        foreach (var (minutes, expected) in testCases)
        {
            // Arrange
            var template = new WorkoutTemplateDtoBuilder()
                .WithName($"Workout {minutes} min")
                .WithEstimatedDuration(minutes)
                .Build();

            // Act
            var cut = RenderComponent<WorkoutTemplateDetail>(parameters => parameters
                .Add(p => p.Template, template));

            // Assert
            cut.Find("[data-testid='detail-duration']").TextContent.Should().Contain(expected);
        }
    }

    [Fact]
    public void Should_DisplayCreatedAndUpdatedDates()
    {
        // Arrange
        var createdDate = new DateTime(2025, 1, 15, 10, 30, 0);
        var updatedDate = new DateTime(2025, 1, 20, 14, 45, 0);
        
        var template = new WorkoutTemplateDtoBuilder()
            .WithName("Date Test Workout")
            .WithCreatedAt(createdDate)
            .WithUpdatedAt(updatedDate)
            .Build();

        // Act
        var cut = RenderComponent<WorkoutTemplateDetail>(parameters => parameters
            .Add(p => p.Template, template));

        // Assert
        cut.Find("[data-testid='detail-created']").TextContent.Should().Contain("January 15, 2025 at 10:30 AM");
        cut.Find("[data-testid='detail-updated']").TextContent.Should().Contain("January 20, 2025 at 2:45 PM");
    }

    [Fact]
    public void Should_TriggerStateChangeCallback_WhenStateTransitionButtonClicked()
    {
        // Arrange
        var template = CreateDefaultTemplate();
        ReferenceDataDto? receivedState = null;
        EventCallback<ReferenceDataDto> onStateChange = EventCallback.Factory.Create<ReferenceDataDto>(
            this, state => receivedState = state);

        // Act
        var cut = RenderComponent<WorkoutTemplateDetail>(parameters => parameters
            .Add(p => p.Template, template)
            .Add(p => p.OnStateChange, onStateChange));

        // Find and trigger the state transition button
        var stateButton = cut.FindComponent<StateTransitionButton>();
        stateButton.InvokeAsync(() => stateButton.Instance.OnStateChanged.InvokeAsync(
            new ReferenceDataDto { Id = "state-production", Value = "PRODUCTION" }));

        // Assert
        receivedState.Should().NotBeNull();
        receivedState?.Value.Should().Be("PRODUCTION");
    }
}
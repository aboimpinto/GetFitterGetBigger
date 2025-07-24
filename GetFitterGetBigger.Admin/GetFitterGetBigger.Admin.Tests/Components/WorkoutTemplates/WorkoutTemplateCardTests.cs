using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.WorkoutTemplates;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Builders;
using Microsoft.AspNetCore.Components;
using AngleSharp.Dom;

namespace GetFitterGetBigger.Admin.Tests.Components.WorkoutTemplates
{
    public class WorkoutTemplateCardTests : TestContext
    {
        private WorkoutTemplateDto CreateTestTemplate()
        {
            return new WorkoutTemplateDtoBuilder()
                .WithId("template-1")
                .WithName("Full Body Workout")
                .WithDescription("A comprehensive full body workout routine")
                .WithCategory("category-1", "Strength Training")
                .WithDifficulty("difficulty-2", "Intermediate")
                .WithWorkoutState("state-1", "DRAFT")
                .WithEstimatedDuration(60)
                .WithIsPublic(false)
                .WithTags("strength", "full-body", "gym")
                .WithObjectives(
                    new ReferenceDataDto { Id = "obj-1", Value = "Build Muscle", Description = null },
                    new ReferenceDataDto { Id = "obj-2", Value = "Increase Strength", Description = null }
                )
                .WithExercises(
                    new WorkoutTemplateExerciseDto { ExerciseId = "ex-1", ExerciseName = "Bench Press", OrderIndex = 1, Sets = 3, TargetReps = "8-10", RestSeconds = 90 },
                    new WorkoutTemplateExerciseDto { ExerciseId = "ex-2", ExerciseName = "Squat", OrderIndex = 2, Sets = 3, TargetReps = "8-10", RestSeconds = 120 },
                    new WorkoutTemplateExerciseDto { ExerciseId = "ex-3", ExerciseName = "Deadlift", OrderIndex = 3, Sets = 3, TargetReps = "5-6", RestSeconds = 180 }
                )
                .WithCreatedAt(DateTime.UtcNow.AddDays(-7))
                .WithUpdatedAt(DateTime.UtcNow.AddHours(-2))
                .Build();
        }

        [Fact]
        public void Component_RendersBasicInformation()
        {
            // Arrange
            var template = CreateTestTemplate();

            // Act
            var component = RenderComponent<WorkoutTemplateCard>(parameters => parameters
                .Add(p => p.Template, template));

            // Assert
            var card = component.Find("[data-testid=\"workout-template-card\"]");
            card.Should().NotBeNull();

            var title = component.Find("[data-testid=\"template-title\"]");
            title.TextContent.Should().Be("Full Body Workout");

            var description = component.Find("[data-testid=\"template-description\"]");
            description.TextContent.Should().Be("A comprehensive full body workout routine");

            var category = component.Find("[data-testid=\"template-category\"]");
            category.TextContent.Should().Contain("Strength Training");

            var difficulty = component.Find("[data-testid=\"template-difficulty\"]");
            difficulty.TextContent.Should().Contain("Intermediate");

            var duration = component.Find("[data-testid=\"template-duration\"]");
            duration.TextContent.Should().Contain("1h");

            var exerciseCount = component.Find("[data-testid=\"template-exercise-count\"]");
            exerciseCount.TextContent.Should().Contain("3");
        }

        [Fact]
        public void Component_RendersStateIndicator()
        {
            // Arrange
            var template = CreateTestTemplate();

            // Act
            var component = RenderComponent<WorkoutTemplateCard>(parameters => parameters
                .Add(p => p.Template, template));

            // Assert
            var stateIndicator = component.FindComponent<WorkoutStateIndicator>();
            stateIndicator.Should().NotBeNull();
            stateIndicator.Instance.WorkoutState.Should().Be(template.WorkoutState);
            stateIndicator.Instance.Size.Should().Be(WorkoutStateIndicator.IndicatorSize.Small);
        }

        [Fact]
        public void Component_RendersTags_WhenPresent()
        {
            // Arrange
            var template = CreateTestTemplate();

            // Act
            var component = RenderComponent<WorkoutTemplateCard>(parameters => parameters
                .Add(p => p.Template, template));

            // Assert
            var tags = component.Find("[data-testid=\"template-tags\"]");
            tags.TextContent.Should().Contain("strength");
            tags.TextContent.Should().Contain("full-body");
            tags.TextContent.Should().Contain("gym");
        }

        [Fact]
        public void Component_DoesNotRenderTags_WhenEmpty()
        {
            // Arrange
            var template = CreateTestTemplate();
            template.Tags.Clear();

            // Act
            var component = RenderComponent<WorkoutTemplateCard>(parameters => parameters
                .Add(p => p.Template, template));

            // Assert
            component.FindAll("[data-testid=\"template-tags\"]").Should().BeEmpty();
        }

        [Fact]
        public void Component_RendersObjectives_WhenPresent()
        {
            // Arrange
            var template = CreateTestTemplate();

            // Act
            var component = RenderComponent<WorkoutTemplateCard>(parameters => parameters
                .Add(p => p.Template, template));

            // Assert
            var objectives = component.Find("[data-testid=\"template-objectives\"]");
            objectives.TextContent.Should().Contain("Build Muscle");
            objectives.TextContent.Should().Contain("Increase Strength");
        }

        [Fact]
        public void Component_ShowsPublicIndicator_WhenEnabled()
        {
            // Test Private
            var privateTemplate = CreateTestTemplate();
            var privateComponent = RenderComponent<WorkoutTemplateCard>(parameters => parameters
                .Add(p => p.Template, privateTemplate)
                .Add(p => p.ShowPublicIndicator, true));

            var privateIndicator = privateComponent.Find("[data-testid=\"public-indicator\"]");
            privateIndicator.TextContent.Should().Contain("🔒 Private");

            // Test Public
            var publicTemplate = CreateTestTemplate();
            publicTemplate.IsPublic = true;
            var publicComponent = RenderComponent<WorkoutTemplateCard>(parameters => parameters
                .Add(p => p.Template, publicTemplate)
                .Add(p => p.ShowPublicIndicator, true));

            var publicIndicator = publicComponent.Find("[data-testid=\"public-indicator\"]");
            publicIndicator.TextContent.Should().Contain("🌍 Public");
        }

        [Fact]
        public void Component_HidesPublicIndicator_WhenDisabled()
        {
            // Arrange
            var template = CreateTestTemplate();

            // Act
            var component = RenderComponent<WorkoutTemplateCard>(parameters => parameters
                .Add(p => p.Template, template)
                .Add(p => p.ShowPublicIndicator, false));

            // Assert
            component.FindAll("[data-testid=\"public-indicator\"]").Should().BeEmpty();
        }

        [Fact]
        public void Component_ShowsEditButton_ForDraftTemplates()
        {
            // Arrange
            var template = CreateTestTemplate();
            var editClicked = false;

            // Act
            var component = RenderComponent<WorkoutTemplateCard>(parameters => parameters
                .Add(p => p.Template, template)
                .Add(p => p.OnEdit, EventCallback.Factory.Create<WorkoutTemplateDto>(this, (_) => editClicked = true)));

            // Assert
            var editButton = component.Find("[data-testid=\"edit-button\"]");
            editButton.Should().NotBeNull();
            editButton.Click();
            editClicked.Should().BeTrue();
        }

        [Fact]
        public void Component_HidesEditButton_ForProductionTemplates()
        {
            // Arrange
            var template = CreateTestTemplate();
            template.WorkoutState = new ReferenceDataDto { Id = "state-2", Value = "PRODUCTION", Description = "Production state" };

            // Act
            var component = RenderComponent<WorkoutTemplateCard>(parameters => parameters
                .Add(p => p.Template, template)
                .Add(p => p.OnEdit, EventCallback.Factory.Create<WorkoutTemplateDto>(this, (_) => { })));

            // Assert
            component.FindAll("[data-testid=\"edit-button\"]").Should().BeEmpty();
        }

        [Fact]
        public void Component_ShowsDuplicateButton_WhenCallbackProvided()
        {
            // Arrange
            var template = CreateTestTemplate();
            WorkoutTemplateDto? duplicatedTemplate = null;

            // Act
            var component = RenderComponent<WorkoutTemplateCard>(parameters => parameters
                .Add(p => p.Template, template)
                .Add(p => p.OnDuplicate, EventCallback.Factory.Create<WorkoutTemplateDto>(this, (t) => duplicatedTemplate = t)));

            // Assert
            var duplicateButton = component.Find("[data-testid=\"duplicate-button\"]");
            duplicateButton.Should().NotBeNull();
            duplicateButton.Click();
            duplicatedTemplate.Should().Be(template);
        }

        [Fact]
        public void Component_ShowsDeleteButton_ForDraftAndArchivedTemplates()
        {
            // Test DRAFT
            var draftTemplate = CreateTestTemplate();
            var draftComponent = RenderComponent<WorkoutTemplateCard>(parameters => parameters
                .Add(p => p.Template, draftTemplate)
                .Add(p => p.OnDelete, EventCallback.Factory.Create<WorkoutTemplateDto>(this, (_) => { })));

            draftComponent.Find("[data-testid=\"delete-button\"]").Should().NotBeNull();

            // Test ARCHIVED
            var archivedTemplate = CreateTestTemplate();
            archivedTemplate.WorkoutState = new ReferenceDataDto { Id = "state-3", Value = "ARCHIVED", Description = "Archived state" };
            var archivedComponent = RenderComponent<WorkoutTemplateCard>(parameters => parameters
                .Add(p => p.Template, archivedTemplate)
                .Add(p => p.OnDelete, EventCallback.Factory.Create<WorkoutTemplateDto>(this, (_) => { })));

            archivedComponent.Find("[data-testid=\"delete-button\"]").Should().NotBeNull();

            // Test PRODUCTION (should not show)
            var productionTemplate = CreateTestTemplate();
            productionTemplate.WorkoutState = new ReferenceDataDto { Id = "state-2", Value = "PRODUCTION", Description = "Production state" };
            var productionComponent = RenderComponent<WorkoutTemplateCard>(parameters => parameters
                .Add(p => p.Template, productionTemplate)
                .Add(p => p.OnDelete, EventCallback.Factory.Create<WorkoutTemplateDto>(this, (_) => { })));

            productionComponent.FindAll("[data-testid=\"delete-button\"]").Should().BeEmpty();
        }

        [Fact]
        public void Component_ShowsStateTransitionButtons_WhenEnabled()
        {
            // Test DRAFT -> PRODUCTION
            var draftTemplate = CreateTestTemplate();
            var draftComponent = RenderComponent<WorkoutTemplateCard>(parameters => parameters
                .Add(p => p.Template, draftTemplate)
                .Add(p => p.ShowStateActions, true)
                .Add(p => p.OnStateChange, EventCallback.Factory.Create<ReferenceDataDto>(this, (_) => { })));

            var publishButton = draftComponent.FindComponent<StateTransitionButton>();
            publishButton.Instance.ButtonText.Should().Be("Publish");
            publishButton.Instance.TargetState?.Value.Should().Be("PRODUCTION");

            // Test PRODUCTION -> ARCHIVED
            var productionTemplate = CreateTestTemplate();
            productionTemplate.WorkoutState = new ReferenceDataDto { Id = "state-2", Value = "PRODUCTION", Description = "Production state" };
            var productionComponent = RenderComponent<WorkoutTemplateCard>(parameters => parameters
                .Add(p => p.Template, productionTemplate)
                .Add(p => p.ShowStateActions, true)
                .Add(p => p.OnStateChange, EventCallback.Factory.Create<ReferenceDataDto>(this, (_) => { })));

            var archiveButton = productionComponent.FindComponent<StateTransitionButton>();
            archiveButton.Instance.ButtonText.Should().Be("Archive");
            archiveButton.Instance.TargetState?.Value.Should().Be("ARCHIVED");
        }

        [Fact]
        public void Component_InvokesTitleClickCallback()
        {
            // Arrange
            var template = CreateTestTemplate();
            WorkoutTemplateDto? clickedTemplate = null;

            // Act
            var component = RenderComponent<WorkoutTemplateCard>(parameters => parameters
                .Add(p => p.Template, template)
                .Add(p => p.OnClick, EventCallback.Factory.Create<WorkoutTemplateDto>(this, (t) => clickedTemplate = t)));

            component.Find("[data-testid=\"template-title\"]").Click();

            // Assert
            clickedTemplate.Should().Be(template);
        }

        [Fact]
        public void Component_AppliesSelectedStyles_WhenSelected()
        {
            // Arrange
            var template = CreateTestTemplate();

            // Act
            var component = RenderComponent<WorkoutTemplateCard>(parameters => parameters
                .Add(p => p.Template, template)
                .Add(p => p.IsSelected, true));

            // Assert
            var card = component.Find("[data-testid=\"workout-template-card\"]");
            var classes = card.GetAttribute("class") ?? "";
            classes.Should().Contain("border-blue-500");
            classes.Should().Contain("ring-2");
            classes.Should().Contain("ring-blue-200");
        }

        [Fact]
        public void Component_AppliesHoverStyles_WhenHovered()
        {
            // Arrange
            var template = CreateTestTemplate();

            // Act
            var component = RenderComponent<WorkoutTemplateCard>(parameters => parameters
                .Add(p => p.Template, template)
                .Add(p => p.IsHovered, true));

            // Assert
            var card = component.Find("[data-testid=\"workout-template-card\"]");
            var classes = card.GetAttribute("class") ?? "";
            classes.Should().Contain("border-gray-300");
            classes.Should().Contain("shadow-md");
        }

        [Fact]
        public void Component_ShowsRelativeUpdateTime()
        {
            // Arrange
            var template = CreateTestTemplate();
            template.UpdatedAt = DateTime.UtcNow.AddMinutes(-30);

            // Act
            var component = RenderComponent<WorkoutTemplateCard>(parameters => parameters
                .Add(p => p.Template, template));

            // Assert
            var updated = component.Find("[data-testid=\"template-updated\"]");
            updated.TextContent.Should().Contain("30m ago");
        }

        [Theory]
        [InlineData(45, "45 min")]
        [InlineData(60, "1h")]
        [InlineData(90, "1h 30min")]
        [InlineData(120, "2h")]
        [InlineData(135, "2h 15min")]
        public void FormatDuration_ReturnsCorrectFormat(int minutes, string expected)
        {
            // Arrange
            var component = RenderComponent<WorkoutTemplateCard>(parameters => parameters
                .Add(p => p.Template, CreateTestTemplate()));

            // Act
            var result = component.Instance.FormatDuration(minutes);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void Component_HasProperAriaAttributes()
        {
            // Arrange
            var template = CreateTestTemplate();

            // Act
            var component = RenderComponent<WorkoutTemplateCard>(parameters => parameters
                .Add(p => p.Template, template));

            // Assert
            var card = component.Find("[data-testid=\"workout-template-card\"]");
            card.GetAttribute("role").Should().Be("listitem");
            card.GetAttribute("aria-label").Should().Be("Workout template: Full Body Workout, DRAFT state");
        }

        [Fact]
        public void Component_DoesNotShowDescription_WhenEmpty()
        {
            // Arrange
            var template = CreateTestTemplate();
            template.Description = null;

            // Act
            var component = RenderComponent<WorkoutTemplateCard>(parameters => parameters
                .Add(p => p.Template, template));

            // Assert
            component.FindAll("[data-testid=\"template-description\"]").Should().BeEmpty();
        }
    }
}
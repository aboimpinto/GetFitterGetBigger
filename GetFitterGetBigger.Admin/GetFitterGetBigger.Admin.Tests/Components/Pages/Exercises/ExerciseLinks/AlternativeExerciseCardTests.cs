using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Pages.Exercises.ExerciseLinks;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Builders;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Generic;
using System.Linq;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.Exercises.ExerciseLinks
{
    public class AlternativeExerciseCardTests : TestContext
    {
        [Fact]
        public void AlternativeExerciseCard_RendersBasicInformation()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder()
                .WithId("alt-link-1")
                .WithTargetExerciseId("exercise-1")
                .AsAlternative()
                .Build();

            // Act
            var component = RenderComponent<AlternativeExerciseCard>(parameters => parameters
                .Add(p => p.Link, link));

            // Assert
            component.Find("[data-testid='alternative-exercise-card']").Should().NotBeNull();
            component.Markup.Should().Contain($"Exercise ID: {link.TargetExerciseId}");
            component.Markup.Should().Contain("Alternative");
        }

        [Fact]
        public void AlternativeExerciseCard_HasPurpleTheme()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder()
                .AsAlternative()
                .Build();

            // Act
            var component = RenderComponent<AlternativeExerciseCard>(parameters => parameters
                .Add(p => p.Link, link));

            // Assert
            var card = component.Find("[data-testid='alternative-exercise-card']");
            card.GetAttribute("class").Should().Contain("from-purple-50");
            card.GetAttribute("class").Should().Contain("border-purple-200");
        }

        [Fact]
        public void AlternativeExerciseCard_RendersExerciseDetailsWhenProvided()
        {
            // Arrange
            var muscleGroups = new List<MuscleGroupListItemDto>
            {
                new MuscleGroupListItemDto
                {
                    MuscleGroup = new ReferenceDataDto { Id = "1", Value = "Chest" },
                    Role = new ReferenceDataDto { Id = "1", Value = "Primary" }
                },
                new MuscleGroupListItemDto
                {
                    MuscleGroup = new ReferenceDataDto { Id = "2", Value = "Shoulders" },
                    Role = new ReferenceDataDto { Id = "1", Value = "Primary" }
                }
            };

            var exerciseTypes = new List<ExerciseTypeDto>
            {
                new ExerciseTypeDto { Id = "1", Value = "Workout" },
                new ExerciseTypeDto { Id = "2", Value = "Warmup" }
            };

            var targetExercise = new ExerciseListDtoBuilder()
                .WithId("exercise-1")
                .WithName("Diamond Push-ups")
                .WithDifficulty("Intermediate", "2")
                .WithMuscleGroups(muscleGroups.ToArray())
                .WithEquipment(new ReferenceDataDto { Id = "1", Value = "Bodyweight" })
                .WithExerciseTypes(exerciseTypes)
                .Build();

            var link = new ExerciseLinkDtoBuilder()
                .WithTargetExercise(targetExercise)
                .AsAlternative()
                .Build();

            // Act
            var component = RenderComponent<AlternativeExerciseCard>(parameters => parameters
                .Add(p => p.Link, link));

            // Assert
            component.Markup.Should().Contain("Diamond Push-ups");
            component.Markup.Should().Contain("Difficulty:");
            component.Markup.Should().Contain("Intermediate");
            component.Markup.Should().Contain("Primary:");
            component.Markup.Should().Contain("Chest");
            component.Markup.Should().Contain("Shoulders");
            component.Markup.Should().Contain("Equipment:");
            component.Markup.Should().Contain("Bodyweight");
            component.Markup.Should().Contain("Types:");
            component.Markup.Should().Contain("Workout");
            component.Markup.Should().Contain("Warmup");
        }

        [Fact]
        public void AlternativeExerciseCard_DisplaysExerciseTypesWithPurpleBadges()
        {
            // Arrange
            var exerciseTypes = new List<ExerciseTypeDto>
            {
                new ExerciseTypeDto { Id = "1", Value = "Workout" },
                new ExerciseTypeDto { Id = "2", Value = "Cooldown" }
            };

            var targetExercise = new ExerciseListDtoBuilder()
                .WithName("Plank")
                .WithExerciseTypes(exerciseTypes)
                .Build();

            var link = new ExerciseLinkDtoBuilder()
                .WithTargetExercise(targetExercise)
                .AsAlternative()
                .Build();

            // Act
            var component = RenderComponent<AlternativeExerciseCard>(parameters => parameters
                .Add(p => p.Link, link));

            // Assert - Check that exercise types are displayed with purple badges
            component.Markup.Should().Contain("Types:");
            component.Markup.Should().Contain("Workout");
            component.Markup.Should().Contain("Cooldown");
            
            // Check that there are purple badges for exercise types (excluding the Alternative badge)
            var allPurpleBadges = component.FindAll("span.bg-purple-100.text-purple-700");
            allPurpleBadges.Count.Should().BeGreaterThanOrEqualTo(2, "Should have at least the Alternative badge plus exercise type badges");
        }

        [Fact]
        public void AlternativeExerciseCard_DoesNotShowMoveButtons()
        {
            // Arrange - Alternative exercises shouldn't have move buttons
            var link = new ExerciseLinkDtoBuilder()
                .AsAlternative()
                .Build();

            // Act
            var component = RenderComponent<AlternativeExerciseCard>(parameters => parameters
                .Add(p => p.Link, link)
                .Add(p => p.Disabled, false));

            // Assert
            component.FindAll("[data-testid='ordering-buttons']").Should().BeEmpty();
            component.FindAll("[data-testid='move-up-button']").Should().BeEmpty();
            component.FindAll("[data-testid='move-down-button']").Should().BeEmpty();
        }

        [Fact]
        public void AlternativeExerciseCard_DoesNotShowPositionInfo()
        {
            // Arrange - Alternative exercises don't have positions
            var link = new ExerciseLinkDtoBuilder()
                .AsAlternative()
                .WithDisplayOrder(0)
                .Build();

            // Act
            var component = RenderComponent<AlternativeExerciseCard>(parameters => parameters
                .Add(p => p.Link, link));

            // Assert
            component.Markup.Should().NotContain("Position:");
        }

        [Fact]
        public void AlternativeExerciseCard_ShowsRemoveButtonWhenEnabled()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder()
                .AsAlternative()
                .Build();

            // Act
            var component = RenderComponent<AlternativeExerciseCard>(parameters => parameters
                .Add(p => p.Link, link)
                .Add(p => p.Disabled, false)
                .Add(p => p.OnRemove, EventCallback.Factory.Create<ExerciseLinkDto>(this, _ => { })));

            // Assert
            component.Find("[data-testid='remove-button']").Should().NotBeNull();
        }

        [Fact]
        public void AlternativeExerciseCard_HidesRemoveButtonWhenDisabled()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder()
                .AsAlternative()
                .Build();

            // Act
            var component = RenderComponent<AlternativeExerciseCard>(parameters => parameters
                .Add(p => p.Link, link)
                .Add(p => p.Disabled, true)
                .Add(p => p.OnRemove, EventCallback.Factory.Create<ExerciseLinkDto>(this, _ => { })));

            // Assert
            component.FindAll("[data-testid='remove-button']").Should().BeEmpty();
        }

        [Fact]
        public void AlternativeExerciseCard_CallsOnRemoveWhenRemoveButtonClicked()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder()
                .WithId("alt-link-1")
                .AsAlternative()
                .Build();
            ExerciseLinkDto? removedLink = null;

            var component = RenderComponent<AlternativeExerciseCard>(parameters => parameters
                .Add(p => p.Link, link)
                .Add(p => p.OnRemove, EventCallback.Factory.Create<ExerciseLinkDto>(this, l => removedLink = l)));

            // Act
            component.Find("[data-testid='remove-button']").Click();

            // Assert
            removedLink.Should().NotBeNull();
            removedLink!.Id.Should().Be("alt-link-1");
        }

        [Fact]
        public void AlternativeExerciseCard_ShowsViewExerciseButtonWhenEnabled()
        {
            // Arrange
            var targetExercise = new ExerciseListDtoBuilder()
                .WithName("Push-up Variations")
                .Build();

            var link = new ExerciseLinkDtoBuilder()
                .WithTargetExercise(targetExercise)
                .AsAlternative()
                .Build();

            // Act
            var component = RenderComponent<AlternativeExerciseCard>(parameters => parameters
                .Add(p => p.Link, link)
                .Add(p => p.OnViewExercise, EventCallback.Factory.Create<ExerciseLinkDto>(this, _ => { })));

            // Assert
            var viewButton = component.Find("[data-testid='view-exercise-button']");
            viewButton.Should().NotBeNull();
            viewButton.TextContent.Should().Contain("View Details");
            viewButton.GetAttribute("class").Should().Contain("text-purple-600");
        }

        [Fact]
        public void AlternativeExerciseCard_HidesViewExerciseButtonWhenNoTargetExercise()
        {
            // Arrange - No target exercise provided
            var link = new ExerciseLinkDtoBuilder()
                .AsAlternative()
                .Build(); // This won't have a TargetExercise object

            // Act
            var component = RenderComponent<AlternativeExerciseCard>(parameters => parameters
                .Add(p => p.Link, link)
                .Add(p => p.OnViewExercise, EventCallback.Factory.Create<ExerciseLinkDto>(this, _ => { })));

            // Assert
            component.FindAll("[data-testid='view-exercise-button']").Should().BeEmpty();
        }

        [Fact]
        public void AlternativeExerciseCard_CallsOnViewExerciseWhenViewButtonClicked()
        {
            // Arrange
            var targetExercise = new ExerciseListDtoBuilder()
                .WithName("Incline Push-ups")
                .Build();

            var link = new ExerciseLinkDtoBuilder()
                .WithId("alt-link-1")
                .WithTargetExercise(targetExercise)
                .AsAlternative()
                .Build();
            ExerciseLinkDto? viewedLink = null;

            var component = RenderComponent<AlternativeExerciseCard>(parameters => parameters
                .Add(p => p.Link, link)
                .Add(p => p.OnViewExercise, EventCallback.Factory.Create<ExerciseLinkDto>(this, l => viewedLink = l)));

            // Act
            component.Find("[data-testid='view-exercise-button']").Click();

            // Assert
            viewedLink.Should().NotBeNull();
            viewedLink!.Id.Should().Be("alt-link-1");
        }

        [Fact]
        public void AlternativeExerciseCard_FormatsCreatedDateCorrectly()
        {
            // Arrange
            var createdDate = new DateTime(2024, 3, 15, 10, 30, 0, DateTimeKind.Utc);
            var link = new ExerciseLinkDtoBuilder()
                .WithCreatedAt(createdDate)
                .AsAlternative()
                .Build();

            // Act
            var component = RenderComponent<AlternativeExerciseCard>(parameters => parameters
                .Add(p => p.Link, link));

            // Assert
            component.Markup.Should().Contain("Added Mar 15, 2024");
        }

        [Fact]
        public void AlternativeExerciseCard_DisplaysAlternativeExerciseIconAndText()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder()
                .AsAlternative()
                .Build();

            // Act
            var component = RenderComponent<AlternativeExerciseCard>(parameters => parameters
                .Add(p => p.Link, link));

            // Assert
            // Check for heart icon (alternative exercise indicator)
            component.Markup.Should().Contain("Alternative");
            // Check for purple heart icon
            var iconElement = component.Find("svg.text-purple-400");
            iconElement.Should().NotBeNull();
        }

        [Fact]
        public void AlternativeExerciseCard_HasCorrectAriaLabel()
        {
            // Arrange
            var targetExercise = new ExerciseListDtoBuilder()
                .WithName("Wall Push-ups")
                .Build();

            var link = new ExerciseLinkDtoBuilder()
                .WithTargetExercise(targetExercise)
                .AsAlternative()
                .Build();

            // Act
            var component = RenderComponent<AlternativeExerciseCard>(parameters => parameters
                .Add(p => p.Link, link));

            // Assert
            var card = component.Find("[data-testid='alternative-exercise-card']");
            card.GetAttribute("aria-label").Should().Be("Alternative exercise: Wall Push-ups");
        }

        [Fact]
        public void AlternativeExerciseCard_HasCorrectAriaLabelWithoutTargetExercise()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder()
                .WithTargetExerciseId("exercise-123")
                .AsAlternative()
                .Build();

            // Act
            var component = RenderComponent<AlternativeExerciseCard>(parameters => parameters
                .Add(p => p.Link, link));

            // Assert
            var card = component.Find("[data-testid='alternative-exercise-card']");
            card.GetAttribute("aria-label").Should().Be("Alternative exercise: Exercise ID exercise-123");
        }

        [Fact]
        public void AlternativeExerciseCard_RemoveButtonHasCorrectAriaLabel()
        {
            // Arrange
            var targetExercise = new ExerciseListDtoBuilder()
                .WithName("Modified Push-ups")
                .Build();

            var link = new ExerciseLinkDtoBuilder()
                .WithTargetExercise(targetExercise)
                .AsAlternative()
                .Build();

            // Act
            var component = RenderComponent<AlternativeExerciseCard>(parameters => parameters
                .Add(p => p.Link, link)
                .Add(p => p.OnRemove, EventCallback.Factory.Create<ExerciseLinkDto>(this, _ => { })));

            // Assert
            var removeButton = component.Find("[data-testid='remove-button']");
            removeButton.GetAttribute("aria-label").Should().Be("Remove Modified Push-ups");
        }

        [Fact]
        public void AlternativeExerciseCard_ViewExerciseButtonHasCorrectAriaLabel()
        {
            // Arrange
            var targetExercise = new ExerciseListDtoBuilder()
                .WithName("Knee Push-ups")
                .Build();

            var link = new ExerciseLinkDtoBuilder()
                .WithTargetExercise(targetExercise)
                .AsAlternative()
                .Build();

            // Act
            var component = RenderComponent<AlternativeExerciseCard>(parameters => parameters
                .Add(p => p.Link, link)
                .Add(p => p.OnViewExercise, EventCallback.Factory.Create<ExerciseLinkDto>(this, _ => { })));

            // Assert
            var viewButton = component.Find("[data-testid='view-exercise-button']");
            viewButton.GetAttribute("aria-label").Should().Be("View details for Knee Push-ups");
        }
    }
}
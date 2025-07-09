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
    public class ExerciseLinkCardTests : TestContext
    {
        [Fact]
        public void ExerciseLinkCard_RendersBasicInformation()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder()
                .WithId("link-1")
                .WithTargetExerciseId("exercise-1")
                .WithDisplayOrder(1)
                .AsWarmup()
                .Build();

            // Act
            var component = RenderComponent<ExerciseLinkCard>(parameters => parameters
                .Add(p => p.Link, link));

            // Assert
            component.Find("[data-testid='exercise-link-card']").Should().NotBeNull();
            component.Markup.Should().Contain($"Exercise ID: {link.TargetExerciseId}");
            component.Markup.Should().Contain($"Position: {link.DisplayOrder + 1}");
        }

        [Fact]
        public void ExerciseLinkCard_RendersExerciseDetailsWhenProvided()
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
                    MuscleGroup = new ReferenceDataDto { Id = "2", Value = "Triceps" },
                    Role = new ReferenceDataDto { Id = "1", Value = "Primary" }
                }
            };

            var targetExercise = new ExerciseListDtoBuilder()
                .WithId("exercise-1")
                .WithName("Push-ups")
                .WithDifficulty("Beginner", "1")
                .WithMuscleGroups(muscleGroups.ToArray())
                .WithEquipment(new ReferenceDataDto { Id = "1", Value = "Bodyweight" })
                .Build();

            var link = new ExerciseLinkDtoBuilder()
                .WithTargetExercise(targetExercise)
                .AsWarmup()
                .Build();

            // Act
            var component = RenderComponent<ExerciseLinkCard>(parameters => parameters
                .Add(p => p.Link, link));

            // Assert
            component.Markup.Should().Contain("Push-ups");
            component.Markup.Should().Contain("Difficulty:");
            component.Markup.Should().Contain("Beginner");
            component.Markup.Should().Contain("Primary:");
            component.Markup.Should().Contain("Chest");
            component.Markup.Should().Contain("Triceps");
            component.Markup.Should().Contain("Equipment:");
            component.Markup.Should().Contain("Bodyweight");
        }

        [Fact]
        public void ExerciseLinkCard_ShowsMoveButtonsWhenEnabled()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder().Build();

            // Act
            var component = RenderComponent<ExerciseLinkCard>(parameters => parameters
                .Add(p => p.Link, link)
                .Add(p => p.Disabled, false)
                .Add(p => p.CanMoveUp, true)
                .Add(p => p.CanMoveDown, true)
                .Add(p => p.OnMoveUp, EventCallback.Factory.Create<ExerciseLinkDto>(this, _ => { }))
                .Add(p => p.OnMoveDown, EventCallback.Factory.Create<ExerciseLinkDto>(this, _ => { })));

            // Assert
            component.Find("[data-testid='ordering-buttons']").Should().NotBeNull();
            component.Find("[data-testid='move-up-button']").Should().NotBeNull();
            component.Find("[data-testid='move-down-button']").Should().NotBeNull();
        }

        [Fact]
        public void ExerciseLinkCard_HidesMoveButtonsWhenDisabled()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder().Build();

            // Act
            var component = RenderComponent<ExerciseLinkCard>(parameters => parameters
                .Add(p => p.Link, link)
                .Add(p => p.Disabled, true)
                .Add(p => p.OnMoveUp, EventCallback.Factory.Create<ExerciseLinkDto>(this, _ => { }))
                .Add(p => p.OnMoveDown, EventCallback.Factory.Create<ExerciseLinkDto>(this, _ => { })));

            // Assert
            component.FindAll("[data-testid='ordering-buttons']").Should().BeEmpty();
        }

        [Fact]
        public void ExerciseLinkCard_ShowsRemoveButtonWhenEnabled()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder().Build();

            // Act
            var component = RenderComponent<ExerciseLinkCard>(parameters => parameters
                .Add(p => p.Link, link)
                .Add(p => p.Disabled, false)
                .Add(p => p.OnRemove, EventCallback.Factory.Create<ExerciseLinkDto>(this, _ => { })));

            // Assert
            component.Find("[data-testid='remove-button']").Should().NotBeNull();
        }

        [Fact]
        public void ExerciseLinkCard_HidesRemoveButtonWhenDisabled()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder().Build();

            // Act
            var component = RenderComponent<ExerciseLinkCard>(parameters => parameters
                .Add(p => p.Link, link)
                .Add(p => p.Disabled, true)
                .Add(p => p.OnRemove, EventCallback.Factory.Create<ExerciseLinkDto>(this, _ => { })));

            // Assert
            component.FindAll("[data-testid='remove-button']").Should().BeEmpty();
        }

        [Fact]
        public void ExerciseLinkCard_CallsOnRemoveWhenRemoveButtonClicked()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder().WithId("link-1").Build();
            ExerciseLinkDto? removedLink = null;

            var component = RenderComponent<ExerciseLinkCard>(parameters => parameters
                .Add(p => p.Link, link)
                .Add(p => p.OnRemove, EventCallback.Factory.Create<ExerciseLinkDto>(this, l => removedLink = l)));

            // Act
            component.Find("[data-testid='remove-button']").Click();

            // Assert
            removedLink.Should().NotBeNull();
            removedLink!.Id.Should().Be("link-1");
        }

        [Fact]
        public void ExerciseLinkCard_CallsMoveUpWhenUpButtonClicked()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder().WithId("link-1").Build();
            ExerciseLinkDto? movedLink = null;

            var component = RenderComponent<ExerciseLinkCard>(parameters => parameters
                .Add(p => p.Link, link)
                .Add(p => p.CanMoveUp, true)
                .Add(p => p.OnMoveUp, EventCallback.Factory.Create<ExerciseLinkDto>(this, l => movedLink = l)));

            // Act
            component.Find("[data-testid='move-up-button']").Click();

            // Assert
            movedLink.Should().NotBeNull();
            movedLink!.Id.Should().Be("link-1");
        }

        [Fact]
        public void ExerciseLinkCard_CallsMoveDownWhenDownButtonClicked()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder().WithId("link-1").Build();
            ExerciseLinkDto? movedLink = null;

            var component = RenderComponent<ExerciseLinkCard>(parameters => parameters
                .Add(p => p.Link, link)
                .Add(p => p.CanMoveDown, true)
                .Add(p => p.OnMoveDown, EventCallback.Factory.Create<ExerciseLinkDto>(this, l => movedLink = l)));

            // Act
            component.Find("[data-testid='move-down-button']").Click();

            // Assert
            movedLink.Should().NotBeNull();
            movedLink!.Id.Should().Be("link-1");
        }

        [Fact]
        public void ExerciseLinkCard_DisablesMoveUpButtonWhenCannotMoveUp()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder().Build();

            // Act
            var component = RenderComponent<ExerciseLinkCard>(parameters => parameters
                .Add(p => p.Link, link)
                .Add(p => p.CanMoveUp, false)
                .Add(p => p.OnMoveUp, EventCallback.Factory.Create<ExerciseLinkDto>(this, _ => { })));

            // Assert
            var moveUpButton = component.Find("[data-testid='move-up-button']");
            moveUpButton.HasAttribute("disabled").Should().BeTrue();
        }

        [Fact]
        public void ExerciseLinkCard_DisablesMoveDownButtonWhenCannotMoveDown()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder().Build();

            // Act
            var component = RenderComponent<ExerciseLinkCard>(parameters => parameters
                .Add(p => p.Link, link)
                .Add(p => p.CanMoveDown, false)
                .Add(p => p.OnMoveDown, EventCallback.Factory.Create<ExerciseLinkDto>(this, _ => { })));

            // Assert
            var moveDownButton = component.Find("[data-testid='move-down-button']");
            moveDownButton.HasAttribute("disabled").Should().BeTrue();
        }

        [Fact]
        public void ExerciseLinkCard_FormatsCreatedDateCorrectly()
        {
            // Arrange
            var createdDate = new DateTime(2024, 3, 15, 10, 30, 0, DateTimeKind.Utc);
            var link = new ExerciseLinkDtoBuilder()
                .WithCreatedAt(createdDate)
                .Build();

            // Act
            var component = RenderComponent<ExerciseLinkCard>(parameters => parameters
                .Add(p => p.Link, link));

            // Assert
            component.Markup.Should().Contain("Added: Mar 15, 2024");
        }
    }
}
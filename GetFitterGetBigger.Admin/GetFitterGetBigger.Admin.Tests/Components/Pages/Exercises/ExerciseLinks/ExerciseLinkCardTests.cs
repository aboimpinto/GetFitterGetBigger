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
            component.Markup.Should().Contain($"Order: {link.DisplayOrder}");
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
        public void ExerciseLinkCard_ShowsDragHandleWhenEnabled()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder().Build();
            var dragStartCalled = false;

            // Act
            var component = RenderComponent<ExerciseLinkCard>(parameters => parameters
                .Add(p => p.Link, link)
                .Add(p => p.Disabled, false)
                .Add(p => p.OnDragStart, EventCallback.Factory.Create<ExerciseLinkDto>(this, _ => dragStartCalled = true)));

            // Assert
            component.Find("[data-testid='drag-handle']").Should().NotBeNull();
            var card = component.Find("[data-testid='exercise-link-card']");
            card.GetAttribute("draggable").Should().Be("true");
        }

        [Fact]
        public void ExerciseLinkCard_HidesDragHandleWhenDisabled()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder().Build();

            // Act
            var component = RenderComponent<ExerciseLinkCard>(parameters => parameters
                .Add(p => p.Link, link)
                .Add(p => p.Disabled, true)
                .Add(p => p.OnDragStart, EventCallback.Factory.Create<ExerciseLinkDto>(this, _ => { })));

            // Assert
            component.FindAll("[data-testid='drag-handle']").Should().BeEmpty();
            var card = component.Find("[data-testid='exercise-link-card']");
            card.GetAttribute("draggable").Should().Be("false");
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
        public void ExerciseLinkCard_ShowsInactiveIndicatorWhenLinkIsInactive()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder()
                .WithIsActive(false)
                .Build();

            // Act
            var component = RenderComponent<ExerciseLinkCard>(parameters => parameters
                .Add(p => p.Link, link));

            // Assert
            component.Markup.Should().Contain("Inactive");
            var card = component.Find("[data-testid='exercise-link-card']");
            card.GetAttribute("class").Should().Contain("border-red-200");
            card.GetAttribute("class").Should().Contain("bg-red-50");
        }

        [Fact]
        public void ExerciseLinkCard_AppliesDraggingStylesWhenIsDragging()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder().Build();

            // Act
            var component = RenderComponent<ExerciseLinkCard>(parameters => parameters
                .Add(p => p.Link, link)
                .Add(p => p.IsDragging, true));

            // Assert
            var card = component.Find("[data-testid='exercise-link-card']");
            card.GetAttribute("class").Should().Contain("opacity-50");
            card.GetAttribute("class").Should().Contain("scale-95");
        }

        [Fact]
        public void ExerciseLinkCard_CallsDragEventHandlers()
        {
            // Arrange
            var link = new ExerciseLinkDtoBuilder().WithId("link-1").Build();
            ExerciseLinkDto? draggedLink = null;
            var dragEndCalled = false;

            var component = RenderComponent<ExerciseLinkCard>(parameters => parameters
                .Add(p => p.Link, link)
                .Add(p => p.OnDragStart, EventCallback.Factory.Create<ExerciseLinkDto>(this, l => draggedLink = l))
                .Add(p => p.OnDragEnd, EventCallback.Factory.Create(this, () => dragEndCalled = true)));

            var card = component.Find("[data-testid='exercise-link-card']");

            // Act - Drag start
            card.TriggerEvent("ondragstart", new DragEventArgs());

            // Assert
            draggedLink.Should().NotBeNull();
            draggedLink!.Id.Should().Be("link-1");

            // Act - Drag end
            card.TriggerEvent("ondragend", new DragEventArgs());

            // Assert
            dragEndCalled.Should().BeTrue();
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
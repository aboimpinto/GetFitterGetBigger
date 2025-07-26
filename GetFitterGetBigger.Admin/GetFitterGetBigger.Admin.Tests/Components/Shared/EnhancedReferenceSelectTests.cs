using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Shared;
using GetFitterGetBigger.Admin.Models;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.ReferenceData;
using GetFitterGetBigger.Admin.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Components.Shared
{
    public class EnhancedReferenceSelectTests : TestContext
    {
        private readonly Mock<IEquipmentService> _mockEquipmentService;
        private readonly Mock<IMuscleGroupsService> _mockMuscleGroupsService;
        private readonly Mock<IGenericReferenceDataService> _mockReferenceDataService;

        public EnhancedReferenceSelectTests()
        {
            _mockEquipmentService = new Mock<IEquipmentService>();
            _mockMuscleGroupsService = new Mock<IMuscleGroupsService>();
            _mockReferenceDataService = new Mock<IGenericReferenceDataService>();

            Services.AddSingleton<IEquipmentService>(_mockEquipmentService.Object);
            Services.AddSingleton<IMuscleGroupsService>(_mockMuscleGroupsService.Object);
            Services.AddSingleton<IGenericReferenceDataService>(_mockReferenceDataService.Object);

            // Setup default body parts for muscle group creation modal
            _mockReferenceDataService.Setup(x => x.GetReferenceDataAsync<BodyParts>())
                .ReturnsAsync(new List<ReferenceDataDto>
                {
                    new() { Id = "1", Value = "Arms" }
                });
        }
        private readonly List<ReferenceDataDto> _testItems = new()
        {
            new() { Id = "1", Value = "Item 1", Description = "Description 1" },
            new() { Id = "2", Value = "Item 2", Description = "Description 2" },
            new() { Id = "3", Value = "Item 3", Description = "Description 3" }
        };

        [Fact]
        public void EnhancedReferenceSelect_RendersAsStandardSelectWhenInlineCreationDisabled()
        {
            // Arrange & Act
            var component = RenderComponent<EnhancedReferenceSelect<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Items, _testItems)
                .Add(p => p.Value, string.Empty)
                .Add(p => p.GetItemId, item => item.Id)
                .Add(p => p.GetItemName, item => item.Value)
                .Add(p => p.EnableInlineCreation, false));

            // Assert
            component.Find("select").Should().NotBeNull();
            component.Markup.Should().NotContain("Add new");
            component.Markup.Should().NotContain("Ctrl+N");
        }

        [Fact]
        public void EnhancedReferenceSelect_ShowsInlineCreationButtonWhenEnabled()
        {
            // Arrange & Act
            var component = RenderComponent<EnhancedReferenceSelect<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Items, _testItems)
                .Add(p => p.Value, string.Empty)
                .Add(p => p.GetItemId, item => item.Id)
                .Add(p => p.GetItemName, item => item.Value)
                .Add(p => p.EnableInlineCreation, true)
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Assert
            var addButton = component.Find("button.bg-blue-600");
            addButton.Should().NotBeNull();
            addButton.GetAttribute("title").Should().Contain("Add new equipment");
        }

        [Fact]
        public void EnhancedReferenceSelect_DisplaysKeyboardShortcutHintWhenEnabled()
        {
            // Arrange & Act
            var component = RenderComponent<EnhancedReferenceSelect<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Items, _testItems)
                .Add(p => p.Value, string.Empty)
                .Add(p => p.GetItemId, item => item.Id)
                .Add(p => p.GetItemName, item => item.Value)
                .Add(p => p.EnableInlineCreation, true)
                .Add(p => p.ShowInlineCreationHint, true)
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Assert
            component.Markup.Should().Contain("Press Ctrl+N");
        }

        [Fact]
        public void EnhancedReferenceSelect_OpensModalWhenInlineCreationButtonClicked()
        {
            // Arrange
            var component = RenderComponent<EnhancedReferenceSelect<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Items, _testItems)
                .Add(p => p.Value, string.Empty)
                .Add(p => p.GetItemId, item => item.Id)
                .Add(p => p.GetItemName, item => item.Value)
                .Add(p => p.EnableInlineCreation, true)
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Act
            var addButton = component.Find("button.bg-blue-600");
            addButton.Click();

            // Assert
            component.FindComponent<AddReferenceItemModal>().Should().NotBeNull();
        }

        [Fact]
        public void EnhancedReferenceSelect_OpensModalWithCtrlNKeyboardShortcut()
        {
            // Arrange
            var component = RenderComponent<EnhancedReferenceSelect<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Items, _testItems)
                .Add(p => p.Value, string.Empty)
                .Add(p => p.GetItemId, item => item.Id)
                .Add(p => p.GetItemName, item => item.Value)
                .Add(p => p.EnableInlineCreation, true)
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Act
            var select = component.Find("select");
            select.KeyDown(new KeyboardEventArgs { Key = "n", CtrlKey = true });

            // Assert
            component.FindComponent<AddReferenceItemModal>().Should().NotBeNull();
        }

        [Fact]
        public void EnhancedReferenceSelect_PassesCorrectEntityTypeToModal()
        {
            // Arrange
            var component = RenderComponent<EnhancedReferenceSelect<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Items, _testItems)
                .Add(p => p.Value, string.Empty)
                .Add(p => p.GetItemId, item => item.Id)
                .Add(p => p.GetItemName, item => item.Value)
                .Add(p => p.EnableInlineCreation, true)
                .Add(p => p.EntityType, ReferenceEntityType.MuscleGroup));

            // Act
            component.Find("button.bg-blue-600").Click();

            // Assert
            var modal = component.FindComponent<AddReferenceItemModal>();
            modal.Instance.EntityType.Should().Be(ReferenceEntityType.MuscleGroup);
        }

        [Fact]
        public void EnhancedReferenceSelect_ShowsOptimisticUpdateAfterCreation()
        {
            // Arrange
            var items = new List<ReferenceDataDto>(_testItems);

            var component = RenderComponent<EnhancedReferenceSelect<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Items, items)
                .Add(p => p.Value, string.Empty)
                .Add(p => p.GetItemId, item => item.Id)
                .Add(p => p.GetItemName, item => item.Value)
                .Add(p => p.EnableInlineCreation, true)
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Open modal
            component.Find("button.bg-blue-600").Click();
            var modal = component.FindComponent<AddReferenceItemModal>();

            // Act - Simulate item creation by invoking the callback
            var newItem = new ReferenceDataDto { Id = "4", Value = "New Item", Description = "New Description" };
            component.InvokeAsync(() => modal.Instance.OnItemCreated.InvokeAsync(newItem));

            // Assert
            var options = component.FindAll("option").Skip(1).ToList(); // Skip placeholder
            options.Should().HaveCount(4); // Original 3 + new 1
            options.Last().GetAttribute("value").Should().Be("4");
        }

        [Fact]
        public void EnhancedReferenceSelect_MaintainsSelectionStateDuringModalInteraction()
        {
            // Arrange
            var selectedValue = "2";
            var component = RenderComponent<EnhancedReferenceSelect<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Items, _testItems)
                .Add(p => p.Value, selectedValue)
                .Add(p => p.GetItemId, item => item.Id)
                .Add(p => p.GetItemName, item => item.Value)
                .Add(p => p.EnableInlineCreation, true)
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Act - Open and close modal without creating
            component.Find("button.bg-blue-600").Click();
            var modal = component.FindComponent<AddReferenceItemModal>();
            component.InvokeAsync(() => modal.Instance.OnCancel.InvokeAsync());

            // Assert
            component.Find("select").GetAttribute("value").Should().Be(selectedValue);
        }

        [Fact]
        public void EnhancedReferenceSelect_DisablesSelectWhenDisabled()
        {
            // Arrange & Act
            var component = RenderComponent<EnhancedReferenceSelect<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Items, _testItems)
                .Add(p => p.Value, string.Empty)
                .Add(p => p.GetItemId, item => item.Id)
                .Add(p => p.GetItemName, item => item.Value)
                .Add(p => p.Disabled, true));

            // Assert
            component.Find("select").GetAttribute("disabled").Should().NotBeNull();
        }

        [Fact]
        public void EnhancedReferenceSelect_ShowsPlaceholderOption()
        {
            // Arrange & Act
            var component = RenderComponent<EnhancedReferenceSelect<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Items, _testItems)
                .Add(p => p.Value, string.Empty)
                .Add(p => p.GetItemId, item => item.Id)
                .Add(p => p.GetItemName, item => item.Value)
                .Add(p => p.Placeholder, "Select an item..."));

            // Assert
            var placeholderOption = component.Find("option");
            placeholderOption.TextContent.Should().Be("Select an item...");
            placeholderOption.GetAttribute("value").Should().Be(string.Empty);
        }

        [Fact]
        public void EnhancedReferenceSelect_HandlesValueChange()
        {
            // Arrange
            var selectedValue = string.Empty;
            var component = RenderComponent<EnhancedReferenceSelect<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Items, _testItems)
                .Add(p => p.Value, selectedValue)
                .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => selectedValue = value))
                .Add(p => p.GetItemId, item => item.Id)
                .Add(p => p.GetItemName, item => item.Value));

            // Act
            component.Find("select").Change("2");

            // Assert
            selectedValue.Should().Be("2");
        }

        [Fact]
        public void EnhancedReferenceSelect_SelectsNewlyCreatedItem()
        {
            // Arrange
            var selectedValue = string.Empty;
            var component = RenderComponent<EnhancedReferenceSelect<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Items, new List<ReferenceDataDto>(_testItems))
                .Add(p => p.Value, selectedValue)
                .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => selectedValue = value))
                .Add(p => p.GetItemId, item => item.Id)
                .Add(p => p.GetItemName, item => item.Value)
                .Add(p => p.EnableInlineCreation, true)
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Open modal
            component.Find("button.bg-blue-600").Click();
            var modal = component.FindComponent<AddReferenceItemModal>();

            // Act - Simulate successful item creation
            var newItem = new ReferenceDataDto { Id = "4", Value = "New Item" };
            component.InvokeAsync(() => modal.Instance.OnItemCreated.InvokeAsync(newItem));

            // Assert
            selectedValue.Should().Be("4");
        }

        [Fact]
        public void EnhancedReferenceSelect_HandlesCancelledCreation()
        {
            // Arrange
            var component = RenderComponent<EnhancedReferenceSelect<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Items, _testItems)
                .Add(p => p.Value, string.Empty)
                .Add(p => p.GetItemId, item => item.Id)
                .Add(p => p.GetItemName, item => item.Value)
                .Add(p => p.EnableInlineCreation, true)
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Act - Open modal and then cancel
            component.Find("button.bg-blue-600").Click();
            var modal = component.FindComponent<AddReferenceItemModal>();
            component.InvokeAsync(() => modal.Instance.OnCancel.InvokeAsync());

            // Assert - Modal should be closed
            component.FindComponents<AddReferenceItemModal>().Should().BeEmpty();
        }

        [Fact]
        public void EnhancedReferenceSelect_ShowsCorrectBorderColorWhenInlineCreationEnabled()
        {
            // Arrange & Act
            var component = RenderComponent<EnhancedReferenceSelect<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Items, _testItems)
                .Add(p => p.Value, string.Empty)
                .Add(p => p.GetItemId, item => item.Id)
                .Add(p => p.GetItemName, item => item.Value)
                .Add(p => p.EnableInlineCreation, true)
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Assert
            var select = component.Find("select");
            select.GetAttribute("class").Should().Contain("border-blue-400");
        }
    }
}
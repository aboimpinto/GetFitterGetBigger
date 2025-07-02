using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Shared;
using GetFitterGetBigger.Admin.Models;
using GetFitterGetBigger.Admin.Models.Dtos;
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
    public class TagBasedMultiSelectTests : TestContext
    {
        private readonly Mock<IEquipmentService> _mockEquipmentService;
        private readonly Mock<IMuscleGroupsService> _mockMuscleGroupsService;
        private readonly Mock<IReferenceDataService> _mockReferenceDataService;

        public TagBasedMultiSelectTests()
        {
            _mockEquipmentService = new Mock<IEquipmentService>();
            _mockMuscleGroupsService = new Mock<IMuscleGroupsService>();
            _mockReferenceDataService = new Mock<IReferenceDataService>();
            
            Services.AddSingleton<IEquipmentService>(_mockEquipmentService.Object);
            Services.AddSingleton<IMuscleGroupsService>(_mockMuscleGroupsService.Object);
            Services.AddSingleton<IReferenceDataService>(_mockReferenceDataService.Object);
        }
        private readonly List<ReferenceDataDto> _testItems = new()
        {
            new() { Id = "1", Value = "Item 1", Description = "Description 1" },
            new() { Id = "2", Value = "Item 2", Description = "Description 2" },
            new() { Id = "3", Value = "Item 3", Description = "Description 3" }
        };

        [Fact]
        public void TagBasedMultiSelect_RendersWithEmptySelection()
        {
            // Arrange & Act
            var component = RenderComponent<TagBasedMultiSelect<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Items, _testItems)
                .Add(p => p.SelectedValues, new List<string>())
                .Add(p => p.GetItemId, item => item.Id)
                .Add(p => p.GetItemName, item => item.Value)
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Assert
            component.Find("select").Should().NotBeNull();
            component.Find("button").TextContent.Should().Contain("Add");
            component.Markup.Should().Contain("No equipment selected");
        }

        [Fact]
        public void TagBasedMultiSelect_DisplaysSelectedItemsAsTags()
        {
            // Arrange
            var selectedValues = new List<string> { "1", "2" };

            // Act
            var component = RenderComponent<TagBasedMultiSelect<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Items, _testItems)
                .Add(p => p.SelectedValues, selectedValues)
                .Add(p => p.GetItemId, item => item.Id)
                .Add(p => p.GetItemName, item => item.Value)
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Assert
            var tags = component.FindAll("span.inline-flex").ToList();
            tags.Should().HaveCount(2);
            tags[0].TextContent.Should().Contain("Item 1");
            tags[1].TextContent.Should().Contain("Item 2");
        }

        [Fact]
        public void TagBasedMultiSelect_AllowsAddingNewItemsFromDropdown()
        {
            // Arrange
            var selectedValues = new List<string>();
            var changedValues = new List<string>();
            
            var component = RenderComponent<TagBasedMultiSelect<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Items, _testItems)
                .Add(p => p.SelectedValues, selectedValues)
                .Add(p => p.SelectedValuesChanged, EventCallback.Factory.Create<List<string>>(this, values => changedValues = values))
                .Add(p => p.GetItemId, item => item.Id)
                .Add(p => p.GetItemName, item => item.Value)
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Act
            component.Find("select").Change("2");
            component.Find("button").Click();

            // Assert
            changedValues.Should().HaveCount(1);
            changedValues.Should().Contain("2");
        }

        [Fact]
        public void TagBasedMultiSelect_PreventsDuplicateSelections()
        {
            // Arrange
            var selectedValues = new List<string> { "1" };
            
            var component = RenderComponent<TagBasedMultiSelect<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Items, _testItems)
                .Add(p => p.SelectedValues, selectedValues)
                .Add(p => p.GetItemId, item => item.Id)
                .Add(p => p.GetItemName, item => item.Value)
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Act & Assert
            var options = component.FindAll("option").Skip(1).ToList(); // Skip placeholder
            options.Should().HaveCount(2); // Only items 2 and 3 should be available
            options.Select(o => o.GetAttribute("value")).Should().NotContain("1");
        }

        [Fact]
        public void TagBasedMultiSelect_RemovesItemsWhenTagXClicked()
        {
            // Arrange
            var selectedValues = new List<string> { "1", "2" };
            var changedValues = new List<string>();
            
            var component = RenderComponent<TagBasedMultiSelect<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Items, _testItems)
                .Add(p => p.SelectedValues, selectedValues)
                .Add(p => p.SelectedValuesChanged, EventCallback.Factory.Create<List<string>>(this, values => changedValues = values))
                .Add(p => p.GetItemId, item => item.Id)
                .Add(p => p.GetItemName, item => item.Value)
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Act
            var removeButton = component.Find("span.inline-flex button");
            removeButton.Click();

            // Assert
            changedValues.Should().HaveCount(1);
            changedValues.Should().NotContain("1");
            changedValues.Should().Contain("2");
        }

        [Fact]
        public void TagBasedMultiSelect_ShowsPlaceholderWhenNoSelection()
        {
            // Arrange & Act
            var component = RenderComponent<TagBasedMultiSelect<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Items, _testItems)
                .Add(p => p.SelectedValues, new List<string>())
                .Add(p => p.Placeholder, "Select items...")
                .Add(p => p.GetItemId, item => item.Id)
                .Add(p => p.GetItemName, item => item.Value)
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Assert
            var placeholder = component.Find("option").TextContent;
            placeholder.Should().Be("Select items...");
        }

        [Fact]
        public void TagBasedMultiSelect_ShowsInlineCreationLinkWhenEnabled()
        {
            // Arrange & Act
            var component = RenderComponent<TagBasedMultiSelect<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Items, _testItems)
                .Add(p => p.SelectedValues, new List<string>())
                .Add(p => p.EnableInlineCreation, true)
                .Add(p => p.EntityType, ReferenceEntityType.Equipment)
                .Add(p => p.GetItemId, item => item.Id)
                .Add(p => p.GetItemName, item => item.Value)
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Assert
            component.Markup.Should().Contain("Can't find equipment? Create here");
        }

        [Fact]
        public void TagBasedMultiSelect_HidesInlineCreationLinkWhenDisabled()
        {
            // Arrange & Act
            var component = RenderComponent<TagBasedMultiSelect<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Items, _testItems)
                .Add(p => p.SelectedValues, new List<string>())
                .Add(p => p.EnableInlineCreation, false)
                .Add(p => p.GetItemId, item => item.Id)
                .Add(p => p.GetItemName, item => item.Value)
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Assert
            component.Markup.Should().NotContain("Can't find equipment? Create here");
        }

        [Fact]
        public void TagBasedMultiSelect_HandlesKeyboardNavigation()
        {
            // Arrange
            var selectedValues = new List<string>();
            var changedValues = new List<string>();
            
            var component = RenderComponent<TagBasedMultiSelect<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Items, _testItems)
                .Add(p => p.SelectedValues, selectedValues)
                .Add(p => p.SelectedValuesChanged, EventCallback.Factory.Create<List<string>>(this, values => changedValues = values))
                .Add(p => p.GetItemId, item => item.Id)
                .Add(p => p.GetItemName, item => item.Value)
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Act
            var select = component.Find("select");
            select.Change("1");
            select.KeyDown(new KeyboardEventArgs { Key = "Enter" });

            // Assert
            changedValues.Should().HaveCount(1);
            changedValues.Should().Contain("1");
        }

        [Fact]
        public void TagBasedMultiSelect_DisablesControlsWhenDisabled()
        {
            // Arrange & Act
            var component = RenderComponent<TagBasedMultiSelect<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Items, _testItems)
                .Add(p => p.SelectedValues, new List<string>())
                .Add(p => p.Disabled, true)
                .Add(p => p.GetItemId, item => item.Id)
                .Add(p => p.GetItemName, item => item.Value)
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Assert
            component.Find("select").GetAttribute("disabled").Should().NotBeNull();
            component.Find("button").GetAttribute("disabled").Should().NotBeNull();
        }

        [Fact]
        public void TagBasedMultiSelect_OpensModalOnInlineCreationClick()
        {
            // Arrange
            var component = RenderComponent<TagBasedMultiSelect<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Items, _testItems)
                .Add(p => p.SelectedValues, new List<string>())
                .Add(p => p.EnableInlineCreation, true)
                .Add(p => p.EntityType, ReferenceEntityType.Equipment)
                .Add(p => p.GetItemId, item => item.Id)
                .Add(p => p.GetItemName, item => item.Value)
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Act
            var createLink = component.Find("button.text-blue-600");
            createLink.Click();

            // Assert
            component.FindComponent<AddReferenceItemModal>().Should().NotBeNull();
        }

        [Fact]
        public void TagBasedMultiSelect_OpensModalWithCtrlN()
        {
            // Arrange
            var component = RenderComponent<TagBasedMultiSelect<ReferenceDataDto>>(parameters => parameters
                .Add(p => p.Items, _testItems)
                .Add(p => p.SelectedValues, new List<string>())
                .Add(p => p.EnableInlineCreation, true)
                .Add(p => p.EntityType, ReferenceEntityType.Equipment)
                .Add(p => p.GetItemId, item => item.Id)
                .Add(p => p.GetItemName, item => item.Value)
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Act
            var select = component.Find("select");
            select.KeyDown(new KeyboardEventArgs { Key = "n", CtrlKey = true });

            // Assert
            component.FindComponent<AddReferenceItemModal>().Should().NotBeNull();
        }
    }
}
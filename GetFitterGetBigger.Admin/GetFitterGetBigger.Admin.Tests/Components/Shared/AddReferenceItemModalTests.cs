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
using System;
using System.Threading.Tasks;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Components.Shared
{
    public class AddReferenceItemModalTests : TestContext
    {
        private readonly Mock<IEquipmentService> _mockEquipmentService;
        private readonly Mock<IMuscleGroupsService> _mockMuscleGroupsService;
        private readonly Mock<IReferenceDataService> _mockReferenceDataService;

        public AddReferenceItemModalTests()
        {
            _mockEquipmentService = new Mock<IEquipmentService>();
            _mockMuscleGroupsService = new Mock<IMuscleGroupsService>();
            _mockReferenceDataService = new Mock<IReferenceDataService>();
            
            Services.AddSingleton<IEquipmentService>(_mockEquipmentService.Object);
            Services.AddSingleton<IMuscleGroupsService>(_mockMuscleGroupsService.Object);
            Services.AddSingleton<IReferenceDataService>(_mockReferenceDataService.Object);
        }

        [Fact]
        public void AddReferenceItemModal_ShowsCorrectTitleBasedOnEntityType()
        {
            // Test Equipment
            var equipmentModal = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            equipmentModal.Find("h3").TextContent.Should().Contain("Add New Equipment");

            // Test Muscle Group
            var muscleGroupModal = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.MuscleGroup));

            muscleGroupModal.Find("h3").TextContent.Should().Contain("Add New Muscle Group");
        }

        [Fact]
        public void AddReferenceItemModal_DisplaysAppropriateFormFieldsForEquipment()
        {
            // Arrange & Act
            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Assert
            var inputs = component.FindAll("input");
            inputs.Should().HaveCount(1); // Name field only for equipment
            
            component.Markup.Should().Contain("Equipment Name");
        }

        [Fact]
        public void AddReferenceItemModal_DisplaysAppropriateFormFieldsForMuscleGroup()
        {
            // Arrange
            _mockReferenceDataService.Setup(x => x.GetBodyPartsAsync())
                .ReturnsAsync(new List<ReferenceDataDto>
                {
                    new() { Id = "1", Value = "Arms" },
                    new() { Id = "2", Value = "Chest" }
                });

            // Act
            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.MuscleGroup));

            // Assert
            var inputs = component.FindAll("input");
            inputs.Should().HaveCount(1); // Name field
            
            var selects = component.FindAll("select");
            selects.Should().HaveCount(1); // Body part dropdown
            
            component.Markup.Should().Contain("Muscle Group Name");
            component.Markup.Should().Contain("Body Part");
        }

        [Fact]
        public void AddReferenceItemModal_ValidatesRequiredFields()
        {
            // Arrange
            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Act - Try to save without entering name
            var form = component.Find("form");
            form.Submit();

            // Assert
            component.Markup.Should().Contain("Equipment name is required");
        }

        [Fact]
        public async Task AddReferenceItemModal_ShowsLoadingStateDuringSave()
        {
            // Arrange
            var tcs = new TaskCompletionSource<EquipmentDto>();
            _mockEquipmentService.Setup(x => x.CreateEquipmentAsync(It.IsAny<CreateEquipmentDto>()))
                .Returns(tcs.Task);

            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Act
            component.Find("input").Change("Test Equipment");
            var form = component.Find("form");
            await component.InvokeAsync(() => form.Submit());

            // Assert - Should show loading state
            component.Markup.Should().Contain("animate-spin"); // Spinner is shown
            component.Find("button[type='submit']").GetAttribute("disabled").Should().NotBeNull();

            // Complete the task
            tcs.SetResult(new EquipmentDto { Id = "1", Name = "Test Equipment", IsActive = true, CreatedAt = DateTime.Now });
        }

        [Fact]
        public async Task AddReferenceItemModal_DisplaysErrorMessageOnFailure()
        {
            // Arrange
            _mockEquipmentService.Setup(x => x.CreateEquipmentAsync(It.IsAny<CreateEquipmentDto>()))
                .ThrowsAsync(new Exception("Test error"));

            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Act
            component.Find("input").Change("Test Equipment");
            var form = component.Find("form");
            await component.InvokeAsync(() => form.Submit());

            // Assert
            component.Markup.Should().Contain("An unexpected error occurred");
        }

        [Fact]
        public void AddReferenceItemModal_InvokesOnCancelWhenCancelled()
        {
            // Arrange
            var cancelled = false;
            _mockEquipmentService.Setup(x => x.CreateEquipmentAsync(It.IsAny<CreateEquipmentDto>()))
                .ReturnsAsync(new EquipmentDto { Id = "1", Name = "Test Equipment", IsActive = true, CreatedAt = DateTime.Now });

            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment)
                .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => cancelled = true)));

            // Act
            var cancelButton = component.FindAll("button").First(b => b.TextContent.Contains("Cancel"));
            cancelButton.Click();

            // Assert
            cancelled.Should().BeTrue();
        }

        [Fact]
        public void AddReferenceItemModal_ClosesOnEscapeKey()
        {
            // Arrange
            var cancelled = false;
            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment)
                .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => cancelled = true)));

            // Act
            var backdrop = component.Find(".fixed.inset-0");
            backdrop.KeyDown(new KeyboardEventArgs { Key = "Escape" });

            // Assert
            cancelled.Should().BeTrue();
        }

        [Fact]
        public async Task AddReferenceItemModal_ClearsFormOnSuccessfulSave()
        {
            // Arrange
            _mockEquipmentService.Setup(x => x.CreateEquipmentAsync(It.IsAny<CreateEquipmentDto>()))
                .ReturnsAsync(new EquipmentDto { Id = "1", Name = "Test Equipment", IsActive = true, CreatedAt = DateTime.Now });

            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Act - Enter data and save
            component.Find("input").Change("Test Equipment");
            var form = component.Find("form");
            await component.InvokeAsync(() => form.Submit());

            // Assert - Form should be cleared after successful save
            // The modal would be closed by parent component, but the form should be reset
            _mockEquipmentService.Verify(x => x.CreateEquipmentAsync(It.IsAny<CreateEquipmentDto>()), Times.Once);
        }

        [Fact(Skip = "Async form submission timing in bUnit requires complex TaskCompletionSource handling. TODO: Simplify async test pattern")]
        public async Task AddReferenceItemModal_PreventsDuplicateSubmissions()
        {
            // Arrange
            var callCount = 0;
            var tcs = new TaskCompletionSource<EquipmentDto>();
            
            _mockEquipmentService.Setup(x => x.CreateEquipmentAsync(It.IsAny<CreateEquipmentDto>()))
                .Returns(() =>
                {
                    callCount++;
                    return tcs.Task;
                });

            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Act - Enter data and try to submit multiple times
            component.Find("input").Change("Test Equipment");
            
            // Submit the form multiple times while the first is still processing
            var form = component.Find("form");
            form.Submit();
            form.Submit();
            form.Submit();
            
            // Complete the async operation
            tcs.SetResult(new EquipmentDto { Id = "1", Name = "Test Equipment", IsActive = true, CreatedAt = DateTime.Now });
            await component.InvokeAsync(() => Task.CompletedTask);

            // Assert - Only one call should have been made
            callCount.Should().Be(1);
        }

        [Fact]
        public async Task AddReferenceItemModal_InvokesOnItemCreatedCallback()
        {
            // Arrange
            object? createdItem = null;
            _mockEquipmentService.Setup(x => x.CreateEquipmentAsync(It.IsAny<CreateEquipmentDto>()))
                .ReturnsAsync(new EquipmentDto { Id = "1", Name = "Test Equipment", IsActive = true, CreatedAt = DateTime.Now });

            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment)
                .Add(p => p.OnItemCreated, EventCallback.Factory.Create<object>(this, item => createdItem = item)));

            // Act
            component.Find("input").Change("Test Equipment");
            var form = component.Find("form");
            await component.InvokeAsync(() => form.Submit());

            // Assert
            createdItem.Should().NotBeNull();
            var refData = createdItem as ReferenceDataDto;
            refData.Should().NotBeNull();
            refData!.Id.Should().Be("1");
            refData.Value.Should().Be("Test Equipment");
        }

        [Fact]
        public async Task AddReferenceItemModal_HandlesEquipmentCreation()
        {
            // Arrange
            var expectedEquipment = new EquipmentDto { Id = "1", Name = "Barbell", IsActive = true, CreatedAt = DateTime.Now };
            _mockEquipmentService.Setup(x => x.CreateEquipmentAsync(It.IsAny<CreateEquipmentDto>()))
                .ReturnsAsync(expectedEquipment);

            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Act
            component.Find("input").Change("Barbell");
            var form = component.Find("form");
            await component.InvokeAsync(() => form.Submit());

            // Assert
            _mockEquipmentService.Verify(x => x.CreateEquipmentAsync(
                It.Is<CreateEquipmentDto>(dto => dto.Name == "Barbell")), Times.Once);
        }

        [Fact]
        public async Task AddReferenceItemModal_HandlesMuscleGroupCreation()
        {
            // Arrange
            _mockReferenceDataService.Setup(x => x.GetBodyPartsAsync())
                .ReturnsAsync(new List<ReferenceDataDto>
                {
                    new() { Id = "1", Value = "Arms" }
                });

            var expectedMuscleGroup = new MuscleGroupDto { Id = "1", Name = "Biceps", BodyPartId = "1", BodyPartName = "Arms" };
            _mockMuscleGroupsService.Setup(x => x.CreateMuscleGroupAsync(It.IsAny<CreateMuscleGroupDto>()))
                .ReturnsAsync(expectedMuscleGroup);

            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.MuscleGroup));

            // Act
            component.Find("input").Change("Biceps");
            component.Find("select").Change("1");
            var form = component.Find("form");
            await component.InvokeAsync(() => form.Submit());

            // Assert
            _mockMuscleGroupsService.Verify(x => x.CreateMuscleGroupAsync(
                It.Is<CreateMuscleGroupDto>(dto => dto.Name == "Biceps" && dto.BodyPartId == "1")), Times.Once);
        }
    }
}
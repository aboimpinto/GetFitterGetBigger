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
using System.Collections.Generic;
using System.Linq;
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

        #region UI Interaction Tests

        [Fact]
        public void AddReferenceItemModal_UI_RendersEquipmentFormCorrectly()
        {
            // Arrange & Act
            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Assert - Check UI elements are rendered
            component.Find("[data-testid='modal-title']").TextContent.Should().Contain("Add New Equipment");
            component.Find("[data-testid='equipment-name-input']").Should().NotBeNull();
            component.Find("[data-testid='cancel-button']").Should().NotBeNull();
            component.Find("[data-testid='submit-button']").Should().NotBeNull();
            
            // Should not have muscle group fields
            component.FindAll("[data-testid='muscle-group-name-input']").Should().BeEmpty();
            component.FindAll("[data-testid='body-part-select']").Should().BeEmpty();
        }

        [Fact]
        public void AddReferenceItemModal_UI_RendersMuscleGroupFormCorrectly()
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
            component.Find("[data-testid='modal-title']").TextContent.Should().Contain("Add New Muscle Group");
            component.Find("[data-testid='muscle-group-name-input']").Should().NotBeNull();
            component.Find("[data-testid='body-part-select']").Should().NotBeNull();
            
            // Verify body parts are loaded
            var bodyPartOptions = component.Find("[data-testid='body-part-select']").QuerySelectorAll("option");
            bodyPartOptions.Should().HaveCount(3); // Placeholder + 2 body parts
        }

        [Fact]
        public async Task AddReferenceItemModal_UI_CreatesEquipmentThroughFormSubmission()
        {
            // Arrange
            var createdEquipment = new EquipmentDto { Id = "1", Name = "Barbell", IsActive = true };
            _mockEquipmentService.Setup(x => x.CreateEquipmentAsync(It.IsAny<CreateEquipmentDto>()))
                .ReturnsAsync(createdEquipment);

            object? callbackResult = null;
            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment)
                .Add(p => p.OnItemCreated, EventCallback.Factory.Create<object>(this, item => callbackResult = item)));

            // Act - Fill form through UI
            component.Find("[data-testid='equipment-name-input']").Change("Barbell");
            await component.InvokeAsync(() => 
                component.Find("[data-testid='add-reference-form']").Submit()
            );

            // Assert - Verify through UI and callbacks
            _mockEquipmentService.Verify(x => x.CreateEquipmentAsync(
                It.Is<CreateEquipmentDto>(dto => dto.Name == "Barbell")), Times.Once);
            
            callbackResult.Should().NotBeNull();
            callbackResult.Should().BeOfType<ReferenceDataDto>();
            var referenceData = (ReferenceDataDto)callbackResult!;
            referenceData.Value.Should().Be("Barbell");
        }

        [Fact]
        public async Task AddReferenceItemModal_UI_CreatesMuscleGroupThroughFormSubmission()
        {
            // Arrange
            _mockReferenceDataService.Setup(x => x.GetBodyPartsAsync())
                .ReturnsAsync(new List<ReferenceDataDto>
                {
                    new() { Id = "1", Value = "Arms" }
                });

            var createdMuscleGroup = new MuscleGroupDto 
            { 
                Id = "1", 
                Name = "Biceps", 
                BodyPartId = "1", 
                BodyPartName = "Arms" 
            };
            _mockMuscleGroupsService.Setup(x => x.CreateMuscleGroupAsync(It.IsAny<CreateMuscleGroupDto>()))
                .ReturnsAsync(createdMuscleGroup);

            object? callbackResult = null;
            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.MuscleGroup)
                .Add(p => p.OnItemCreated, EventCallback.Factory.Create<object>(this, item => callbackResult = item)));

            // Act
            component.Find("[data-testid='muscle-group-name-input']").Change("Biceps");
            component.Find("[data-testid='body-part-select']").Change("1");
            await component.InvokeAsync(() => 
                component.Find("[data-testid='add-reference-form']").Submit()
            );

            // Assert
            _mockMuscleGroupsService.Verify(x => x.CreateMuscleGroupAsync(
                It.Is<CreateMuscleGroupDto>(dto => dto.Name == "Biceps" && dto.BodyPartId == "1")), Times.Once);
            
            callbackResult.Should().NotBeNull();
            var referenceData = (ReferenceDataDto)callbackResult!;
            referenceData.Value.Should().Be("Biceps");
            referenceData.Description.Should().Contain("Arms");
        }

        [Fact]
        public async Task AddReferenceItemModal_UI_DisplaysValidationErrors()
        {
            // Arrange
            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Act - Submit empty form
            await component.InvokeAsync(() => 
                component.Find("[data-testid='add-reference-form']").Submit()
            );

            // Assert - Validation message should appear
            component.WaitForAssertion(() =>
            {
                component.Find("[data-testid='equipment-name-error']").TextContent
                    .Should().Contain("Equipment name is required");
            });
        }

        [Fact]
        public async Task AddReferenceItemModal_UI_DisplaysApiErrors()
        {
            // Arrange
            _mockEquipmentService.Setup(x => x.CreateEquipmentAsync(It.IsAny<CreateEquipmentDto>()))
                .ThrowsAsync(new HttpRequestException("Network error"));

            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Act
            component.Find("[data-testid='equipment-name-input']").Change("Test");
            await component.InvokeAsync(() => 
                component.Find("[data-testid='add-reference-form']").Submit()
            );

            // Assert
            component.WaitForAssertion(() =>
            {
                component.Find("[data-testid='error-message']").TextContent
                    .Should().Contain("Unable to connect to the server");
            });
        }

        [Fact]
        public async Task AddReferenceItemModal_UI_DisablesButtonsDuringSubmission()
        {
            // Arrange
            var tcs = new TaskCompletionSource<EquipmentDto>();
            _mockEquipmentService.Setup(x => x.CreateEquipmentAsync(It.IsAny<CreateEquipmentDto>()))
                .Returns(tcs.Task);

            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Act - Start submission
            component.Find("[data-testid='equipment-name-input']").Change("Test");
            var submitTask = component.InvokeAsync(() => 
                component.Find("[data-testid='add-reference-form']").Submit()
            );

            // Assert - Buttons should be disabled
            component.WaitForAssertion(() =>
            {
                component.Find("[data-testid='submit-button']").GetAttribute("disabled").Should().NotBeNull();
                component.Find("[data-testid='cancel-button']").GetAttribute("disabled").Should().NotBeNull();
                component.FindAll("[data-testid='loading-spinner']").Should().HaveCount(1);
            });

            // Complete the operation
            tcs.SetResult(new EquipmentDto { Id = "1", Name = "Test" });
            await submitTask;
        }

        [Fact]
        public void AddReferenceItemModal_UI_CloseOnCancelButtonClick()
        {
            // Arrange
            var cancelled = false;
            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment)
                .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => cancelled = true)));

            // Act
            component.Find("[data-testid='cancel-button']").Click();

            // Assert
            cancelled.Should().BeTrue();
        }

        #endregion

        #region Direct Logic Tests

        [Fact]
        public void AddReferenceItemModal_Logic_InitializesCorrectModel()
        {
            // Arrange & Act
            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Assert
            var instance = component.Instance;
            instance.model.Should().BeOfType<AddReferenceItemModal.EquipmentFormModel>();
            instance.isSubmitting.Should().BeFalse();
            instance.errorMessage.Should().BeNull();
        }

        [Fact]
        public async Task AddReferenceItemModal_Logic_CreatesEquipmentSuccessfully()
        {
            // Arrange
            var createdEquipment = new EquipmentDto { Id = "1", Name = "Dumbbell", IsActive = true };
            _mockEquipmentService.Setup(x => x.CreateEquipmentAsync(It.IsAny<CreateEquipmentDto>()))
                .ReturnsAsync(createdEquipment);

            object? callbackResult = null;
            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment)
                .Add(p => p.OnItemCreated, EventCallback.Factory.Create<object>(this, item => callbackResult = item)));

            var instance = component.Instance;

            // Act - Set model directly and submit
            instance.model = new AddReferenceItemModal.EquipmentFormModel { Name = "Dumbbell" };
            await component.InvokeAsync(async () => await instance.HandleSubmit());

            // Assert
            _mockEquipmentService.Verify(x => x.CreateEquipmentAsync(
                It.Is<CreateEquipmentDto>(dto => dto.Name == "Dumbbell")), Times.Once);
            
            instance.isSubmitting.Should().BeFalse();
            instance.errorMessage.Should().BeNull();
            callbackResult.Should().NotBeNull();
        }

        [Fact]
        public async Task AddReferenceItemModal_Logic_PreventsDuplicateSubmissions()
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

            var instance = component.Instance;
            instance.model = new AddReferenceItemModal.EquipmentFormModel { Name = "Test Equipment" };

            // Act - Start the first submission (this will set isSubmitting = true)
            var firstSubmitTask = component.InvokeAsync(async () => await instance.HandleSubmit());
            
            // Try to submit while the first is still in progress
            // These should be ignored due to isSubmitting check
            await component.InvokeAsync(async () => await instance.HandleSubmit());
            await component.InvokeAsync(async () => await instance.HandleSubmit());

            // Complete the async operation
            tcs.SetResult(new EquipmentDto { Id = "1", Name = "Test Equipment" });
            
            // Wait for the first submission to complete
            await firstSubmitTask;

            // Assert - Only one call should have been made
            callCount.Should().Be(1);
        }

        [Fact]
        public async Task AddReferenceItemModal_Logic_HandlesBusinessErrors()
        {
            // Arrange
            _mockEquipmentService.Setup(x => x.CreateEquipmentAsync(It.IsAny<CreateEquipmentDto>()))
                .ThrowsAsync(new InvalidOperationException("Equipment already exists"));

            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            var instance = component.Instance;
            instance.model = new AddReferenceItemModal.EquipmentFormModel { Name = "Existing Equipment" };

            // Act
            await component.InvokeAsync(async () => await instance.HandleSubmit());

            // Assert
            instance.errorMessage.Should().Be("Equipment already exists");
            instance.isSubmitting.Should().BeFalse();
        }

        [Fact]
        public async Task AddReferenceItemModal_Logic_HandlesNetworkErrors()
        {
            // Arrange
            _mockEquipmentService.Setup(x => x.CreateEquipmentAsync(It.IsAny<CreateEquipmentDto>()))
                .ThrowsAsync(new HttpRequestException("Connection refused"));

            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            var instance = component.Instance;
            instance.model = new AddReferenceItemModal.EquipmentFormModel { Name = "Test" };

            // Act
            await component.InvokeAsync(async () => await instance.HandleSubmit());

            // Assert
            instance.errorMessage.Should().Contain("Unable to connect to the server");
            instance.isSubmitting.Should().BeFalse();
        }

        [Fact]
        public async Task AddReferenceItemModal_Logic_LoadsBodyPartsForMuscleGroup()
        {
            // Arrange
            var bodyParts = new List<ReferenceDataDto>
            {
                new() { Id = "1", Value = "Arms" },
                new() { Id = "2", Value = "Chest" }
            };
            _mockReferenceDataService.Setup(x => x.GetBodyPartsAsync())
                .ReturnsAsync(bodyParts);

            // Act
            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.MuscleGroup));

            // Assert
            var instance = component.Instance;
            instance.bodyParts.Should().NotBeNull();
            instance.bodyParts.Should().HaveCount(2);
            instance.isLoadingBodyParts.Should().BeFalse();
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task AddReferenceItemModal_Integration_CompleteEquipmentCreationWorkflow()
        {
            // This test combines UI and Logic to test the complete workflow
            
            // Arrange
            var createdEquipment = new EquipmentDto { Id = "1", Name = "Kettlebell", IsActive = true };
            _mockEquipmentService.Setup(x => x.CreateEquipmentAsync(It.IsAny<CreateEquipmentDto>()))
                .ReturnsAsync(createdEquipment);

            var itemCreatedCount = 0;
            var cancelCount = 0;

            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment)
                .Add(p => p.OnItemCreated, EventCallback.Factory.Create<object>(this, _ => itemCreatedCount++))
                .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => cancelCount++)));

            // Act - Complete workflow through UI
            component.Find("[data-testid='equipment-name-input']").Change("Kettlebell");
            
            // Verify submit button is enabled
            component.Find("[data-testid='submit-button']").GetAttribute("disabled").Should().BeNull();
            
            // Submit form
            await component.InvokeAsync(() => 
                component.Find("[data-testid='add-reference-form']").Submit()
            );

            // Assert - Verify complete workflow
            _mockEquipmentService.Verify(x => x.CreateEquipmentAsync(It.IsAny<CreateEquipmentDto>()), Times.Once);
            itemCreatedCount.Should().Be(1);
            cancelCount.Should().Be(0);
            
            // Verify no error message
            component.FindAll("[data-testid='error-message']").Should().BeEmpty();
        }

        [Fact]
        public async Task AddReferenceItemModal_Integration_CompleteMuscleGroupCreationWorkflow()
        {
            // Arrange
            _mockReferenceDataService.Setup(x => x.GetBodyPartsAsync())
                .ReturnsAsync(new List<ReferenceDataDto>
                {
                    new() { Id = "1", Value = "Arms" },
                    new() { Id = "2", Value = "Chest" }
                });

            var createdMuscleGroup = new MuscleGroupDto 
            { 
                Id = "1", 
                Name = "Pectorals", 
                BodyPartId = "2", 
                BodyPartName = "Chest" 
            };
            _mockMuscleGroupsService.Setup(x => x.CreateMuscleGroupAsync(It.IsAny<CreateMuscleGroupDto>()))
                .ReturnsAsync(createdMuscleGroup);

            object? createdItem = null;
            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.MuscleGroup)
                .Add(p => p.OnItemCreated, EventCallback.Factory.Create<object>(this, item => createdItem = item)));

            // Act - Complete workflow
            component.Find("[data-testid='muscle-group-name-input']").Change("Pectorals");
            component.Find("[data-testid='body-part-select']").Change("2");
            
            await component.InvokeAsync(() => 
                component.Find("[data-testid='add-reference-form']").Submit()
            );

            // Assert
            _mockMuscleGroupsService.Verify(x => x.CreateMuscleGroupAsync(
                It.Is<CreateMuscleGroupDto>(dto => 
                    dto.Name == "Pectorals" && 
                    dto.BodyPartId == "2")), Times.Once);
            
            createdItem.Should().NotBeNull();
            var referenceData = (ReferenceDataDto)createdItem!;
            referenceData.Value.Should().Be("Pectorals");
            referenceData.Description.Should().Contain("Chest");
        }

        #endregion

        #region Accessibility Tests

        [Fact]
        public void AddReferenceItemModal_Accessibility_HasProperAriaAttributes()
        {
            // Arrange & Act
            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Assert
            var backdrop = component.Find("[data-testid='add-reference-modal-backdrop']");
            backdrop.GetAttribute("role").Should().Be("dialog");
            backdrop.GetAttribute("aria-modal").Should().Be("true");
            backdrop.GetAttribute("aria-labelledby").Should().NotBeNull();
            backdrop.GetAttribute("aria-describedby").Should().NotBeNull();
        }

        [Fact]
        public async Task AddReferenceItemModal_Accessibility_HandlesEscapeKey()
        {
            // Arrange
            var cancelCalled = false;
            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment)
                .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => cancelCalled = true)));

            // Act
            await component.InvokeAsync(() =>
            {
                var backdrop = component.Find("[data-testid='add-reference-modal-backdrop']");
                backdrop.KeyDown(new KeyboardEventArgs { Key = "Escape" });
            });

            // Assert
            cancelCalled.Should().BeTrue();
        }

        [Fact]
        public void AddReferenceItemModal_Accessibility_ClosesOnBackdropClick()
        {
            // Arrange
            var cancelCalled = false;
            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment)
                .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => cancelCalled = true)));

            // Act
            component.Find("[data-testid='add-reference-modal-backdrop']").Click();

            // Assert
            cancelCalled.Should().BeTrue();
        }

        [Fact]
        public void AddReferenceItemModal_Accessibility_RequiredFieldsMarked()
        {
            // Arrange & Act
            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Assert - Required field indicators
            component.Markup.Should().Contain("Equipment Name <span class=\"text-red-500\">*</span>");
        }

        #endregion
    }
}
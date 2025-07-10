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
        public void AddReferenceItemModal_Logic_LoadsBodyPartsForMuscleGroup()
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

        #region Boundary Value Tests

        [Fact]
        public async Task AddReferenceItemModal_BoundaryValue_HandlesMaxLengthStrings()
        {
            // Arrange - Create 100 character string (max allowed by StringLength attribute)
            var maxLengthName = new string('A', 100);
            var createdEquipment = new EquipmentDto { Id = "1", Name = maxLengthName, IsActive = true };
            _mockEquipmentService.Setup(x => x.CreateEquipmentAsync(It.IsAny<CreateEquipmentDto>()))
                .ReturnsAsync(createdEquipment);

            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Act - Test through UI
            component.Find("[data-testid='equipment-name-input']").Change(maxLengthName);
            await component.InvokeAsync(() =>
                component.Find("[data-testid='add-reference-form']").Submit()
            );

            // Assert
            _mockEquipmentService.Verify(x => x.CreateEquipmentAsync(
                It.Is<CreateEquipmentDto>(dto => dto.Name == maxLengthName)), Times.Once);
        }

        [Fact]
        public async Task AddReferenceItemModal_BoundaryValue_RejectsOverMaxLengthStrings()
        {
            // Arrange - Create 101 character string (over the limit)
            var overMaxLengthName = new string('B', 101);
            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Act - Test through UI
            component.Find("[data-testid='equipment-name-input']").Change(overMaxLengthName);
            await component.InvokeAsync(() =>
                component.Find("[data-testid='add-reference-form']").Submit()
            );

            // Assert - Should show validation error
            component.WaitForAssertion(() =>
            {
                component.Find("[data-testid='equipment-name-error']").TextContent
                    .Should().Contain("must be less than 100 characters");
            });
            _mockEquipmentService.Verify(x => x.CreateEquipmentAsync(It.IsAny<CreateEquipmentDto>()), Times.Never);
        }

        [Fact]
        public async Task AddReferenceItemModal_BoundaryValue_RejectsWhitespaceOnlyStrings()
        {
            // Arrange
            var whitespaceStrings = new[] { " ", "  ", "\t", "\n", "\r\n", "   \t\n   " };

            foreach (var whitespace in whitespaceStrings)
            {
                var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                    .Add(p => p.EntityType, ReferenceEntityType.Equipment));

                // Act
                component.Find("[data-testid='equipment-name-input']").Change(whitespace);
                await component.InvokeAsync(() =>
                    component.Find("[data-testid='add-reference-form']").Submit()
                );

                // Assert
                component.WaitForAssertion(() =>
                {
                    component.Find("[data-testid='equipment-name-error']").TextContent
                        .Should().Contain("Equipment name is required");
                });
            }

            _mockEquipmentService.Verify(x => x.CreateEquipmentAsync(It.IsAny<CreateEquipmentDto>()), Times.Never);
        }

        [Fact]
        public async Task AddReferenceItemModal_BoundaryValue_HandlesSpecialCharacters()
        {
            // Arrange - Test various special characters
            var specialCharNames = new[]
            {
                "Equipment @#$%",
                "Equipment_123",
                "Equipment-Pro+",
                "Equipment (Heavy)",
                "Equipment & More",
                "Equipment^2",
                "Equipment*Special*",
                "Equipment!Important!"
            };

            foreach (var specialName in specialCharNames)
            {
                var createdEquipment = new EquipmentDto { Id = Guid.NewGuid().ToString(), Name = specialName };
                _mockEquipmentService.Setup(x => x.CreateEquipmentAsync(It.Is<CreateEquipmentDto>(dto => dto.Name == specialName)))
                    .ReturnsAsync(createdEquipment);

                var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                    .Add(p => p.EntityType, ReferenceEntityType.Equipment));

                // Act
                component.Find("[data-testid='equipment-name-input']").Change(specialName);
                await component.InvokeAsync(() =>
                    component.Find("[data-testid='add-reference-form']").Submit()
                );

                // Assert - Should accept special characters
                _mockEquipmentService.Verify(x => x.CreateEquipmentAsync(
                    It.Is<CreateEquipmentDto>(dto => dto.Name == specialName)), Times.Once);
            }
        }

        [Fact]
        public async Task AddReferenceItemModal_BoundaryValue_HandlesUnicodeCharacters()
        {
            // Arrange - Test Unicode characters including emojis and accented characters
            var unicodeNames = new[]
            {
                "Équipement spécial",     // French accents
                "Gerät für Übungen",       // German umlauts
                "Оборудование",            // Cyrillic
                "設備",                    // Japanese
                "معدات",                   // Arabic
                "Equipment 🎯",            // Emoji
                "Dumbbells 💪",           // Another emoji
                "àéîõüñç Equipment"        // Various accents
            };

            foreach (var unicodeName in unicodeNames)
            {
                var createdEquipment = new EquipmentDto { Id = Guid.NewGuid().ToString(), Name = unicodeName };
                _mockEquipmentService.Setup(x => x.CreateEquipmentAsync(It.Is<CreateEquipmentDto>(dto => dto.Name == unicodeName)))
                    .ReturnsAsync(createdEquipment);

                var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                    .Add(p => p.EntityType, ReferenceEntityType.Equipment));

                // Act
                component.Find("[data-testid='equipment-name-input']").Change(unicodeName);
                await component.InvokeAsync(() =>
                    component.Find("[data-testid='add-reference-form']").Submit()
                );

                // Assert - Should accept Unicode characters
                _mockEquipmentService.Verify(x => x.CreateEquipmentAsync(
                    It.Is<CreateEquipmentDto>(dto => dto.Name == unicodeName)), Times.Once);
            }
        }

        [Fact]
        public async Task AddReferenceItemModal_BoundaryValue_HandlesSQLInjectionAttempts()
        {
            // Arrange - Test potential SQL injection strings
            var sqlInjectionAttempts = new[]
            {
                "'; DROP TABLE Equipment--",
                "1'; DELETE FROM Equipment WHERE '1'='1",
                "Equipment' OR '1'='1",
                "Equipment\"; DROP TABLE--",
                "Equipment'); DROP TABLE Equipment;--"
            };

            foreach (var sqlAttempt in sqlInjectionAttempts)
            {
                var createdEquipment = new EquipmentDto { Id = Guid.NewGuid().ToString(), Name = sqlAttempt };
                _mockEquipmentService.Setup(x => x.CreateEquipmentAsync(It.Is<CreateEquipmentDto>(dto => dto.Name == sqlAttempt)))
                    .ReturnsAsync(createdEquipment);

                var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                    .Add(p => p.EntityType, ReferenceEntityType.Equipment));

                // Act
                component.Find("[data-testid='equipment-name-input']").Change(sqlAttempt);
                await component.InvokeAsync(() =>
                    component.Find("[data-testid='add-reference-form']").Submit()
                );

                // Assert - Should safely handle SQL injection attempts as regular strings
                _mockEquipmentService.Verify(x => x.CreateEquipmentAsync(
                    It.Is<CreateEquipmentDto>(dto => dto.Name == sqlAttempt)), Times.Once);
            }
        }

        [Fact]
        public async Task AddReferenceItemModal_BoundaryValue_HandlesEmptyStringLogicTest()
        {
            // Arrange - Direct logic test for empty string
            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Act - Submit form with empty input through UI (validation should prevent submission)
            component.Find("[data-testid='equipment-name-input']").Change("");
            await component.InvokeAsync(() =>
                component.Find("[data-testid='add-reference-form']").Submit()
            );

            // Assert - Should not call service with empty string due to validation
            _mockEquipmentService.Verify(x => x.CreateEquipmentAsync(It.IsAny<CreateEquipmentDto>()), Times.Never);

            // Validation error should be shown
            component.WaitForAssertion(() =>
            {
                component.Find("[data-testid='equipment-name-error']").TextContent
                    .Should().Contain("Equipment name is required");
            });
        }

        [Fact]
        public async Task AddReferenceItemModal_BoundaryValue_MuscleGroupMaxLengthTest()
        {
            // Arrange - Test muscle group with max length name
            _mockReferenceDataService.Setup(x => x.GetBodyPartsAsync())
                .ReturnsAsync(new List<ReferenceDataDto>
                {
                    new() { Id = "1", Value = "Arms" }
                });

            var maxLengthName = new string('M', 100);
            var createdMuscleGroup = new MuscleGroupDto
            {
                Id = "1",
                Name = maxLengthName,
                BodyPartId = "1",
                BodyPartName = "Arms"
            };
            _mockMuscleGroupsService.Setup(x => x.CreateMuscleGroupAsync(It.IsAny<CreateMuscleGroupDto>()))
                .ReturnsAsync(createdMuscleGroup);

            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.MuscleGroup));

            // Act
            component.Find("[data-testid='muscle-group-name-input']").Change(maxLengthName);
            component.Find("[data-testid='body-part-select']").Change("1");
            await component.InvokeAsync(() =>
                component.Find("[data-testid='add-reference-form']").Submit()
            );

            // Assert
            _mockMuscleGroupsService.Verify(x => x.CreateMuscleGroupAsync(
                It.Is<CreateMuscleGroupDto>(dto => dto.Name == maxLengthName && dto.BodyPartId == "1")), Times.Once);
        }

        #endregion

        #region Rapid Interaction Tests

        [Fact]
        public async Task AddReferenceItemModal_RapidInteraction_HandlesDoubleClickSubmission()
        {
            // Arrange
            var submitCount = 0;
            var tcs = new TaskCompletionSource<EquipmentDto>();

            _mockEquipmentService.Setup(x => x.CreateEquipmentAsync(It.IsAny<CreateEquipmentDto>()))
                .Returns(() =>
                {
                    submitCount++;
                    return tcs.Task;
                });

            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Act - Fill form and double-click submit button rapidly
            component.Find("[data-testid='equipment-name-input']").Change("Double Click Test");

            // Simulate double-click by clicking twice rapidly
            var submitButton = component.Find("[data-testid='submit-button']");
            var firstClickTask = component.InvokeAsync(() => submitButton.Click());
            var secondClickTask = component.InvokeAsync(() => submitButton.Click());

            // Complete the async operation
            tcs.SetResult(new EquipmentDto { Id = "1", Name = "Double Click Test" });

            await firstClickTask;
            await secondClickTask;

            // Assert - Should only submit once due to isSubmitting flag
            submitCount.Should().Be(1);
        }

        [Fact]
        public async Task AddReferenceItemModal_RapidInteraction_HandlesRapidFormResubmissions()
        {
            // Arrange
            var submissionTimes = new List<DateTime>();
            _mockEquipmentService.Setup(x => x.CreateEquipmentAsync(It.IsAny<CreateEquipmentDto>()))
                .Callback(() => submissionTimes.Add(DateTime.Now))
                .ReturnsAsync(() => new EquipmentDto { Id = Guid.NewGuid().ToString(), Name = "Test" });

            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Act - Submit form rapidly multiple times
            for (int i = 0; i < 5; i++)
            {
                component.Find("[data-testid='equipment-name-input']").Change($"Test Item {i}");
                await component.InvokeAsync(() =>
                    component.Find("[data-testid='add-reference-form']").Submit()
                );

                // Small delay to simulate rapid user interaction
                await Task.Delay(50);
            }

            // Assert - All submissions should complete successfully
            submissionTimes.Should().HaveCount(5);

            // Verify submissions are spaced out (not simultaneous)
            for (int i = 1; i < submissionTimes.Count; i++)
            {
                var timeDiff = submissionTimes[i] - submissionTimes[i - 1];
                timeDiff.TotalMilliseconds.Should().BeGreaterThan(40); // At least 40ms apart
            }
        }

        [Fact]
        public async Task AddReferenceItemModal_RapidInteraction_HandlesKeyboardSpamDuringInput()
        {
            // Arrange
            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            var nameInput = component.Find("[data-testid='equipment-name-input']");

            // Act - Simulate keyboard spam with rapid changes
            var rapidInputs = new[] { "T", "Te", "Tes", "Test", "Test ", "Test E", "Test Eq", "Test Equ", "Test Equi", "Test Equip" };

            foreach (var input in rapidInputs)
            {
                nameInput.Change(input);
                // No delay - simulating very fast typing
            }

            // Final value
            nameInput.Change("Test Equipment");

            // Submit form
            await component.InvokeAsync(() =>
                component.Find("[data-testid='add-reference-form']").Submit()
            );

            // Assert - Form should handle rapid input changes gracefully
            _mockEquipmentService.Verify(x => x.CreateEquipmentAsync(
                It.Is<CreateEquipmentDto>(dto => dto.Name == "Test Equipment")), Times.Once);
        }

        [Fact]
        public async Task AddReferenceItemModal_RapidInteraction_HandlesRapidCancelClicks()
        {
            // Arrange
            var cancelCount = 0;
            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment)
                .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => cancelCount++)));

            // Act - Click cancel button multiple times rapidly
            var cancelButton = component.Find("[data-testid='cancel-button']");

            var tasks = new List<Task>();
            for (int i = 0; i < 5; i++)
            {
                tasks.Add(component.InvokeAsync(() => cancelButton.Click()));
            }

            await Task.WhenAll(tasks);

            // Assert - Component allows multiple cancel calls (not protected)
            cancelCount.Should().Be(5);
        }

        [Fact]
        public async Task AddReferenceItemModal_RapidInteraction_HandlesFormSubmitDuringLoading()
        {
            // Arrange
            var submitCount = 0;
            var tcs = new TaskCompletionSource<EquipmentDto>();

            _mockEquipmentService.Setup(x => x.CreateEquipmentAsync(It.IsAny<CreateEquipmentDto>()))
                .Returns(() =>
                {
                    submitCount++;
                    return tcs.Task;
                });

            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Act - Submit form
            component.Find("[data-testid='equipment-name-input']").Change("Test");
            var submitTask = component.InvokeAsync(() =>
                component.Find("[data-testid='add-reference-form']").Submit()
            );

            // Try to submit again while first is still loading
            await component.InvokeAsync(() =>
                component.Find("[data-testid='add-reference-form']").Submit()
            );

            // Complete the first submission
            tcs.SetResult(new EquipmentDto { Id = "1", Name = "Test" });
            await submitTask;

            // Assert - Only one submission should occur
            submitCount.Should().Be(1);
        }

        [Fact]
        public void AddReferenceItemModal_RapidInteraction_ShowsCorrectFormBasedOnEntityType()
        {
            // Test that the component shows the correct form based on entity type
            // In practice, the modal would be recreated when entity type changes

            // Test 1: Equipment type
            var equipmentComponent = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment));

            // Verify Equipment form is shown
            equipmentComponent.FindAll("[data-testid='equipment-name-input']").Should().HaveCount(1);
            equipmentComponent.FindAll("[data-testid='muscle-group-name-input']").Should().BeEmpty();
            equipmentComponent.Find("[data-testid='modal-title']").TextContent.Should().Contain("Equipment");

            // Test 2: MuscleGroup type
            var muscleGroupComponent = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.MuscleGroup));

            // Verify MuscleGroup form is shown
            muscleGroupComponent.FindAll("[data-testid='equipment-name-input']").Should().BeEmpty();
            muscleGroupComponent.FindAll("[data-testid='muscle-group-name-input']").Should().HaveCount(1);
            muscleGroupComponent.Find("[data-testid='modal-title']").TextContent.Should().Contain("Muscle Group");

            // Verify body parts select is present
            var bodyPartSelect = muscleGroupComponent.Find("[data-testid='body-part-select']");
            bodyPartSelect.Should().NotBeNull();
        }

        [Fact]
        public async Task AddReferenceItemModal_RapidInteraction_HandlesEscapeKeySpam()
        {
            // Arrange
            var cancelCount = 0;
            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment)
                .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => cancelCount++)));

            var backdrop = component.Find("[data-testid='add-reference-modal-backdrop']");

            // Act - Spam Escape key
            var tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(component.InvokeAsync(() =>
                    backdrop.KeyDown(new KeyboardEventArgs { Key = "Escape" })
                ));
            }

            await Task.WhenAll(tasks);

            // Assert - Component allows multiple escape key events (not protected)
            cancelCount.Should().Be(10);
        }

        [Fact]
        public async Task AddReferenceItemModal_RapidInteraction_HandlesBackdropClickSpam()
        {
            // Arrange
            var cancelCount = 0;
            var component = RenderComponent<AddReferenceItemModal>(parameters => parameters
                .Add(p => p.EntityType, ReferenceEntityType.Equipment)
                .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => cancelCount++)));

            var backdrop = component.Find("[data-testid='add-reference-modal-backdrop']");

            // Act - Click backdrop multiple times rapidly
            var tasks = new List<Task>();
            for (int i = 0; i < 5; i++)
            {
                tasks.Add(component.InvokeAsync(() => backdrop.Click()));
            }

            await Task.WhenAll(tasks);

            // Assert - Component allows multiple backdrop clicks (not protected)
            cancelCount.Should().Be(5);
        }

        #endregion
    }
}
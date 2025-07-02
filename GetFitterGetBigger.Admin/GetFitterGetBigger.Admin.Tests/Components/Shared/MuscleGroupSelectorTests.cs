using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Shared;
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
    public class MuscleGroupSelectorTests : TestContext
    {
        private readonly Mock<IEquipmentService> _mockEquipmentService;
        private readonly Mock<IMuscleGroupsService> _mockMuscleGroupsService;
        private readonly Mock<IReferenceDataService> _mockReferenceDataService;

        public MuscleGroupSelectorTests()
        {
            _mockEquipmentService = new Mock<IEquipmentService>();
            _mockMuscleGroupsService = new Mock<IMuscleGroupsService>();
            _mockReferenceDataService = new Mock<IReferenceDataService>();
            
            Services.AddSingleton<IEquipmentService>(_mockEquipmentService.Object);
            Services.AddSingleton<IMuscleGroupsService>(_mockMuscleGroupsService.Object);
            Services.AddSingleton<IReferenceDataService>(_mockReferenceDataService.Object);

            // Setup default body parts for muscle group creation modal
            _mockReferenceDataService.Setup(x => x.GetBodyPartsAsync())
                .ReturnsAsync(new List<ReferenceDataDto>
                {
                    new() { Id = "1", Value = "Arms" },
                    new() { Id = "2", Value = "Chest" },
                    new() { Id = "3", Value = "Back" },
                    new() { Id = "4", Value = "Shoulders" }
                });
        }

        private readonly List<ReferenceDataDto> _testMuscleGroups = new()
        {
            new() { Id = "1", Value = "Biceps", Description = "Biceps muscle" },
            new() { Id = "2", Value = "Triceps", Description = "Triceps muscle" },
            new() { Id = "3", Value = "Chest", Description = "Chest muscles" },
            new() { Id = "4", Value = "Back", Description = "Back muscles" },
            new() { Id = "5", Value = "Shoulders", Description = "Shoulder muscles" }
        };

        private readonly List<ReferenceDataDto> _testMuscleRoles = new()
        {
            new() { Id = "1", Value = "Primary", Description = "Primary muscle" },
            new() { Id = "2", Value = "Secondary", Description = "Secondary muscle" },
            new() { Id = "3", Value = "Stabilizer", Description = "Stabilizer muscle" }
        };

        #region UI Interaction Tests

        [Fact]
        public void MuscleGroupSelector_UI_RendersWithEmptyState()
        {
            // Arrange & Act
            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, new List<MuscleGroupRoleAssignmentDto>()));

            // Assert - Check UI elements
            component.Find("[data-testid='muscle-group-role-select']").Should().NotBeNull();
            component.Find("[data-testid='add-muscle-group-button']").Should().NotBeNull();
            component.Find("[data-testid='muscle-group-empty-state']").TextContent
                .Should().Contain("No muscle groups selected");
        }

        [Fact]
        public void MuscleGroupSelector_UI_ShowsRoleOptions()
        {
            // Arrange & Act
            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, new List<MuscleGroupRoleAssignmentDto>()));

            // Assert - Check role options through UI
            var roleSelect = component.Find("[data-testid='muscle-group-role-select']");
            var options = roleSelect.QuerySelectorAll("option").Skip(1).ToList(); // Skip placeholder
            options.Should().HaveCount(3);
            options[0].TextContent.Should().Be("Primary");
            options[1].TextContent.Should().Be("Secondary");
            options[2].TextContent.Should().Be("Stabilizer");
        }

        [Fact]
        public async Task MuscleGroupSelector_UI_AddsNewMuscleGroupWithRole()
        {
            // Arrange
            var selectedGroups = new List<MuscleGroupRoleAssignmentDto>();
            var changedGroups = new List<MuscleGroupRoleAssignmentDto>();

            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, selectedGroups)
                .Add(p => p.MuscleGroupsChanged, EventCallback.Factory.Create<List<MuscleGroupRoleAssignmentDto>>(this, groups => changedGroups = groups)));

            // Act - Use data-testid to find elements
            component.Find("[data-testid='muscle-group-role-select']").Change("Primary");
            
            // Wait for re-render after role selection
            component.Render();
            
            // Find and interact with muscle group select
            var muscleGroupSelect = component.Find("[data-testid='muscle-group-select']");
            muscleGroupSelect.Change("1");
            
            // Find and click the add button
            await component.InvokeAsync(() => 
                component.Find("[data-testid='add-muscle-group-button']").Click()
            );

            // Assert
            changedGroups.Should().HaveCount(1);
            changedGroups[0].MuscleGroupId.Should().Be("1");
            changedGroups[0].Role.Should().Be("Primary");
        }

        [Fact]
        public void MuscleGroupSelector_UI_DisplaysValidationError()
        {
            // Arrange & Act
            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, new List<MuscleGroupRoleAssignmentDto>())
                .Add(p => p.ShowValidationError, true));

            // Assert
            component.Find("[data-testid='muscle-group-validation-error']").TextContent
                .Should().Contain("At least one muscle group with Primary role is required");
        }

        [Fact]
        public async Task MuscleGroupSelector_UI_RemovesMuscleGroupWhenTagXClicked()
        {
            // Arrange
            var selectedGroups = new List<MuscleGroupRoleAssignmentDto>
            {
                new() { MuscleGroupId = "1", Role = "Primary" },
                new() { MuscleGroupId = "2", Role = "Secondary" }
            };

            var changedGroups = new List<MuscleGroupRoleAssignmentDto>();

            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, selectedGroups)
                .Add(p => p.MuscleGroupsChanged, EventCallback.Factory.Create<List<MuscleGroupRoleAssignmentDto>>(this, groups => changedGroups = groups)));

            // Act - Find and click the remove button on the first muscle group tag
            var removeButtons = component.FindAll("button[title^='Remove']");
            removeButtons.Should().HaveCount(2);
            
            await component.InvokeAsync(() => removeButtons[0].Click());

            // Assert
            changedGroups.Should().HaveCount(1);
            changedGroups[0].MuscleGroupId.Should().Be("2");
            changedGroups[0].Role.Should().Be("Secondary");
        }

        [Fact]
        public void MuscleGroupSelector_UI_DisablesControlsWhenDisabled()
        {
            // Arrange & Act
            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, new List<MuscleGroupRoleAssignmentDto>())
                .Add(p => p.Disabled, true));

            // Assert
            component.Find("[data-testid='muscle-group-role-select']")
                .GetAttribute("disabled").Should().NotBeNull();
            component.Find("[data-testid='add-muscle-group-button']")
                .GetAttribute("disabled").Should().NotBeNull();
            component.FindAll("[data-testid='create-muscle-group-button']")
                .Should().BeEmpty(); // Creation button should not be shown when disabled
        }

        [Fact]
        public void MuscleGroupSelector_UI_DisablesAddButtonWhenNoMuscleSelected()
        {
            // Arrange
            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, new List<MuscleGroupRoleAssignmentDto>()));

            // Assert - Initially the button should be disabled
            var addButton = component.Find("[data-testid='add-muscle-group-button']");
            addButton.GetAttribute("disabled").Should().NotBeNull();

            // Act - Select role but no muscle
            component.Find("[data-testid='muscle-group-role-select']").Change("Primary");
            component.Render();

            // Assert - Add button should still be disabled
            addButton = component.Find("[data-testid='add-muscle-group-button']");
            addButton.GetAttribute("disabled").Should().NotBeNull();
        }

        #endregion

        #region Direct Logic Tests

        [Fact]
        public void MuscleGroupSelector_Logic_InitializesCorrectly()
        {
            // Arrange & Act
            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, new List<MuscleGroupRoleAssignmentDto>()));

            var instance = component.Instance;

            // Assert
            instance.selectedRole.Should().BeEmpty();
            instance.selectedMuscleId.Should().BeEmpty();
            instance.MuscleGroups.Should().BeEmpty();
        }

        [Fact]
        public async Task MuscleGroupSelector_Logic_AddsNewMuscleGroup()
        {
            // Arrange
            var selectedGroups = new List<MuscleGroupRoleAssignmentDto>();
            var changedGroups = new List<MuscleGroupRoleAssignmentDto>();

            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, selectedGroups)
                .Add(p => p.MuscleGroupsChanged, EventCallback.Factory.Create<List<MuscleGroupRoleAssignmentDto>>(this, groups => changedGroups = groups)));

            var instance = component.Instance;

            // Act - Set state directly and call method
            instance.selectedRole = "Primary";
            instance.selectedMuscleId = "1";
            
            await component.InvokeAsync(async () => await instance.AddMuscleGroup());

            // Assert
            selectedGroups.Should().HaveCount(1);
            selectedGroups[0].MuscleGroupId.Should().Be("1");
            selectedGroups[0].Role.Should().Be("Primary");
            
            changedGroups.Should().HaveCount(1);
            changedGroups[0].MuscleGroupId.Should().Be("1");
            changedGroups[0].Role.Should().Be("Primary");
        }

        [Fact]
        public async Task MuscleGroupSelector_Logic_EnforcesSinglePrimaryRule()
        {
            // Arrange
            var selectedGroups = new List<MuscleGroupRoleAssignmentDto>
            {
                new() { MuscleGroupId = "1", Role = "Primary" }
            };

            var changedGroups = new List<MuscleGroupRoleAssignmentDto>();

            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, selectedGroups)
                .Add(p => p.MuscleGroupsChanged, EventCallback.Factory.Create<List<MuscleGroupRoleAssignmentDto>>(this, groups => changedGroups = groups)));

            var instance = component.Instance;

            // Act - Try to add another Primary
            instance.selectedRole = "Primary"; 
            instance.selectedMuscleId = "2";
            
            await component.InvokeAsync(async () => await instance.AddMuscleGroup());

            // Assert - Should replace the existing Primary
            changedGroups.Should().HaveCount(1);
            changedGroups[0].MuscleGroupId.Should().Be("2");
            changedGroups[0].Role.Should().Be("Primary");
        }

        [Fact]
        public void MuscleGroupSelector_Logic_FiltersAvailableMuscleGroups()
        {
            // Arrange
            var selectedGroups = new List<MuscleGroupRoleAssignmentDto>
            {
                new() { MuscleGroupId = "1", Role = "Secondary" },
                new() { MuscleGroupId = "2", Role = "Stabilizer" }
            };

            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, selectedGroups));

            // Act - Get available muscles through the EnhancedReferenceSelect component
            var enhancedSelect = component.FindComponent<EnhancedReferenceSelect<ReferenceDataDto>>();
            var availableItems = enhancedSelect.Instance.Items;

            // Assert
            availableItems.Should().HaveCount(3); // Only 3, 4, 5 should be available
            availableItems.Select(i => i.Id).Should().NotContain("1");
            availableItems.Select(i => i.Id).Should().NotContain("2");
            availableItems.Select(i => i.Id).Should().Contain(new[] { "3", "4", "5" });
        }

        [Fact]
        public void MuscleGroupSelector_Logic_FiltersAvailableRoles()
        {
            // Arrange - Start with a Primary muscle already selected
            var selectedGroups = new List<MuscleGroupRoleAssignmentDto>
            {
                new() { MuscleGroupId = "1", Role = "Primary" }
            };

            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, selectedGroups));

            // Act - Check available roles through UI
            var roleSelect = component.Find("[data-testid='muscle-group-role-select']");
            var options = roleSelect.QuerySelectorAll("option").Skip(1).ToList();

            // Assert - Primary should not be available
            options.Should().HaveCount(2);
            options.Select(o => o.TextContent).Should().NotContain("Primary");
            options.Select(o => o.TextContent).Should().Contain(new[] { "Secondary", "Stabilizer" });
        }

        [Fact]
        public async Task MuscleGroupSelector_Logic_ResetsFormAfterAdd()
        {
            // Arrange
            var selectedGroups = new List<MuscleGroupRoleAssignmentDto>();

            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, selectedGroups)
                .Add(p => p.MuscleGroupsChanged, EventCallback.Factory.Create<List<MuscleGroupRoleAssignmentDto>>(this, groups => selectedGroups = groups)));

            var instance = component.Instance;

            // Act - Set values and add muscle group
            instance.selectedRole = "Primary";
            instance.selectedMuscleId = "1";
            
            await component.InvokeAsync(async () => await instance.AddMuscleGroup());

            // Assert - Fields should be reset
            instance.selectedRole.Should().BeEmpty();
            instance.selectedMuscleId.Should().BeEmpty();
            
            // The muscle select should be disabled since no role is selected
            var enhancedSelect = component.FindComponent<EnhancedReferenceSelect<ReferenceDataDto>>();
            enhancedSelect.Instance.Disabled.Should().BeTrue();
        }

        [Fact]
        public async Task MuscleGroupSelector_Logic_IgnoresAddWithInvalidData()
        {
            // Arrange
            var selectedGroups = new List<MuscleGroupRoleAssignmentDto>();
            var callbackInvoked = false;

            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, selectedGroups)
                .Add(p => p.MuscleGroupsChanged, EventCallback.Factory.Create<List<MuscleGroupRoleAssignmentDto>>(this, groups => callbackInvoked = true)));

            var instance = component.Instance;

            // Act - Try to add with no role
            instance.selectedRole = "";
            instance.selectedMuscleId = "1";
            await component.InvokeAsync(async () => await instance.AddMuscleGroup());

            // Assert
            selectedGroups.Should().BeEmpty();
            callbackInvoked.Should().BeFalse();

            // Act - Try to add with no muscle
            instance.selectedRole = "Primary";
            instance.selectedMuscleId = "";
            await component.InvokeAsync(async () => await instance.AddMuscleGroup());

            // Assert
            selectedGroups.Should().BeEmpty();
            callbackInvoked.Should().BeFalse();
        }

        #endregion

        #region Integration Tests

        [Fact]
        public async Task MuscleGroupSelector_Integration_CompleteWorkflowWithTags()
        {
            // Arrange
            var selectedGroups = new List<MuscleGroupRoleAssignmentDto>();
            var changedGroups = new List<MuscleGroupRoleAssignmentDto>();

            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, selectedGroups)
                .Add(p => p.MuscleGroupsChanged, EventCallback.Factory.Create<List<MuscleGroupRoleAssignmentDto>>(this, groups => changedGroups = groups)));

            // Act - Add Primary muscle group
            component.Find("[data-testid='muscle-group-role-select']").Change("Primary");
            component.Find("[data-testid='muscle-group-select']").Change("1");
            await component.InvokeAsync(() => 
                component.Find("[data-testid='add-muscle-group-button']").Click()
            );

            // Verify Primary tag is displayed
            var tags = component.FindComponents<MuscleGroupTag>();
            tags.Should().HaveCount(1);
            tags[0].Instance.Role.Should().Be("Primary");
            tags[0].Instance.MuscleGroupName.Should().Be("Biceps");

            // Add Secondary muscle group
            component.Find("[data-testid='muscle-group-role-select']").Change("Secondary");
            component.Find("[data-testid='muscle-group-select']").Change("2");
            await component.InvokeAsync(() => 
                component.Find("[data-testid='add-muscle-group-button']").Click()
            );

            // Verify both tags are displayed
            tags = component.FindComponents<MuscleGroupTag>();
            tags.Should().HaveCount(2);

            // Try to add another Primary (should replace the first)
            component.Find("[data-testid='muscle-group-role-select']").Change("Primary");
            component.Find("[data-testid='muscle-group-select']").Change("3");
            await component.InvokeAsync(() => 
                component.Find("[data-testid='add-muscle-group-button']").Click()
            );

            // Assert - Should have 2 tags, with new Primary
            tags = component.FindComponents<MuscleGroupTag>();
            tags.Should().HaveCount(2);
            
            var primaryTag = tags.FirstOrDefault(t => t.Instance.Role == "Primary");
            primaryTag.Should().NotBeNull();
            primaryTag!.Instance.MuscleGroupName.Should().Be("Chest");
            
            // Verify success indicator
            component.Markup.Should().Contain("✓ Primary muscle group assigned");
        }

        [Fact]
        public void MuscleGroupSelector_Integration_ValidationScenarios()
        {
            // Arrange - Start with no Primary muscle and validation error showing
            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, new List<MuscleGroupRoleAssignmentDto>
                {
                    new() { MuscleGroupId = "1", Role = "Secondary" }
                })
                .Add(p => p.ShowValidationError, true));

            // Assert - Validation error should be visible
            component.Find("[data-testid='muscle-group-validation-error']").Should().NotBeNull();
            component.Markup.Should().NotContain("✓ Primary muscle group assigned");

            // Act - Update to add a Primary muscle
            component.SetParametersAndRender(parameters => parameters
                .Add(p => p.MuscleGroups, new List<MuscleGroupRoleAssignmentDto>
                {
                    new() { MuscleGroupId = "1", Role = "Primary" },
                    new() { MuscleGroupId = "2", Role = "Secondary" }
                })
                .Add(p => p.ShowValidationError, false));

            // Assert - Validation error should be gone and success indicator visible
            component.FindAll("[data-testid='muscle-group-validation-error']").Should().BeEmpty();
            component.Markup.Should().Contain("✓ Primary muscle group assigned");
        }

        #endregion

        #region Accessibility Tests

        [Fact]
        public void MuscleGroupSelector_Accessibility_RequiredFieldsMarked()
        {
            // Arrange & Act
            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, new List<MuscleGroupRoleAssignmentDto>())
                .Add(p => p.Disabled, false));

            // Assert - Required field indicator should be present
            component.Markup.Should().Contain("<span class=\"text-red-500\">*</span>");
        }

        [Fact]
        public void MuscleGroupSelector_Accessibility_DisabledStateExplanation()
        {
            // Arrange & Act
            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, new List<MuscleGroupRoleAssignmentDto>())
                .Add(p => p.Disabled, true));

            // Assert - Disabled explanation should be present
            component.Markup.Should().Contain("(disabled for Rest exercises)");
        }

        #endregion
    }
}
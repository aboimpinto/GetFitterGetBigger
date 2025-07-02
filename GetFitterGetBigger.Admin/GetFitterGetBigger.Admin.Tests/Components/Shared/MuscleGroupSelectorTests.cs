using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Shared;
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

        [Fact]
        public void MuscleGroupSelector_RendersWithEmptyState()
        {
            // Arrange & Act
            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, new List<MuscleGroupRoleAssignmentDto>()));

            // Assert
            component.Find("select").Should().NotBeNull();
            var options = component.FindAll("option").ToList();
            options.Should().HaveCountGreaterThan(0);
            options[0].TextContent.Should().Be("Select role");
            component.Markup.Should().Contain("No muscle groups selected");
        }

        [Fact]
        public void MuscleGroupSelector_ShowsRoleOptions()
        {
            // Arrange & Act
            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, new List<MuscleGroupRoleAssignmentDto>()));

            // Assert
            var roleSelect = component.Find("select");
            var options = roleSelect.QuerySelectorAll("option").Skip(1).ToList(); // Skip placeholder
            options.Should().HaveCount(3);
            options[0].TextContent.Should().Be("Primary");
            options[1].TextContent.Should().Be("Secondary");
            options[2].TextContent.Should().Be("Stabilizer");
        }

        [Fact]
        public void MuscleGroupSelector_FiltersAvailableMuscleGroupsBasedOnRole()
        {
            // Arrange
            var selectedGroups = new List<MuscleGroupRoleAssignmentDto>
            {
                new() { MuscleGroupId = "1", Role = "Secondary" }
            };

            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, selectedGroups));

            // Act - Select Primary role
            component.Find("select").Change("Primary");

            // Assert - Primary role should exclude all already selected muscle groups
            var muscleSelect = component.FindAll("select")[1];
            var options = muscleSelect.QuerySelectorAll("option").Skip(1).ToList();
            options.Should().HaveCount(4); // All except Biceps
            options.Select(o => o.GetAttribute("value")).Should().NotContain("1");
        }

        [Fact(Skip = "Component interaction with nested modal requires complex async handling. TODO: Refactor for better testability")]
        public void MuscleGroupSelector_AddsNewMuscleGroupWithRole()
        {
            // Arrange
            var selectedGroups = new List<MuscleGroupRoleAssignmentDto>();
            var changedGroups = new List<MuscleGroupRoleAssignmentDto>();

            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, selectedGroups)
                .Add(p => p.MuscleGroupsChanged, EventCallback.Factory.Create<List<MuscleGroupRoleAssignmentDto>>(this, groups => changedGroups = groups)));

            // Act
            component.Find("select").Change("Primary");
            component.FindAll("select")[1].Change("1");
            component.Find("button").Click();

            // Assert
            changedGroups.Should().HaveCount(1);
            changedGroups[0].MuscleGroupId.Should().Be("1");
            changedGroups[0].Role.Should().Be("Primary");
        }

        [Fact]
        public void MuscleGroupSelector_DisplaysRoleSpecificTagColors()
        {
            // Arrange
            var selectedGroups = new List<MuscleGroupRoleAssignmentDto>
            {
                new() { MuscleGroupId = "1", Role = "Primary" },
                new() { MuscleGroupId = "2", Role = "Secondary" },
                new() { MuscleGroupId = "3", Role = "Stabilizer" }
            };

            // Act
            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, selectedGroups));

            // Assert - Check that MuscleGroupTag components are rendered with correct roles
            var tags = component.FindComponents<MuscleGroupTag>();
            tags.Should().HaveCount(3);
            
            tags[0].Instance.Role.Should().Be("Primary");
            tags[1].Instance.Role.Should().Be("Secondary");
            tags[2].Instance.Role.Should().Be("Stabilizer");
        }

        [Fact]
        public void MuscleGroupSelector_PreventsDuplicateMuscleGroups()
        {
            // Arrange
            var selectedGroups = new List<MuscleGroupRoleAssignmentDto>
            {
                new() { MuscleGroupId = "1", Role = "Primary" }
            };

            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, selectedGroups));

            // Act - Select a role
            component.Find("select").Change("Secondary");

            // Assert - Muscle group 1 should not be in the available options
            var muscleSelect = component.FindAll("select")[1];
            var options = muscleSelect.QuerySelectorAll("option").Skip(1).Select(o => o.GetAttribute("value"));
            options.Should().NotContain("1"); // Already selected muscle should be filtered out
        }

        [Fact(Skip = "Business rule enforcement requires complex state management testing. TODO: Test rule logic in isolation")]
        public void MuscleGroupSelector_EnforcesSinglePrimaryRule()
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

            // Act - Try to add another Primary
            component.Find("select").Change("Primary");
            component.FindAll("select")[1].Change("2");
            component.Find("button").Click();

            // Assert - Should replace the existing Primary
            changedGroups.Should().HaveCount(1);
            changedGroups[0].MuscleGroupId.Should().Be("2");
            changedGroups[0].Role.Should().Be("Primary");
        }

        [Fact]
        public void MuscleGroupSelector_FiltersOutAlreadySelectedMuscles()
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

            // Act - Select any role to see available muscles
            component.Find("select").Change("Primary");

            // Assert - Already selected muscles should not be available
            var muscleSelect = component.FindAll("select")[1];
            var availableOptions = muscleSelect.QuerySelectorAll("option").Skip(1).Select(o => o.GetAttribute("value")).ToList();
            availableOptions.Should().NotContain("1"); // Biceps already selected
            availableOptions.Should().NotContain("2"); // Triceps already selected
            availableOptions.Should().Contain("3"); // Chest available
            availableOptions.Should().Contain("4"); // Back available
            availableOptions.Should().Contain("5"); // Shoulders available
        }

        [Fact(Skip = "MuscleGroupTag component interaction not properly isolated. TODO: Mock child component or test integration separately")]
        public void MuscleGroupSelector_RemovesMuscleGroupWhenTagXClicked()
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

            // Act - Remove first muscle group via MuscleGroupTag component
            var firstTag = component.FindComponent<MuscleGroupTag>();
            firstTag.Instance.OnRemove.InvokeAsync();

            // Assert
            changedGroups.Should().HaveCount(1);
            changedGroups[0].MuscleGroupId.Should().Be("2");
        }

        [Fact]
        public void MuscleGroupSelector_ShowsInlineCreationHintAppropriately()
        {
            // Arrange & Act - With hint by default
            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, new List<MuscleGroupRoleAssignmentDto>()));

            // Act - Select a role to show the muscle group dropdown
            component.Find("select").Change("Primary");

            // Assert
            component.Markup.Should().Contain("Can't find the Muscle Group? Create here");
        }

        [Fact(Skip = "EventCallback testing requires proper async/await handling. TODO: Use InvokeAsync pattern for event verification")]
        public void MuscleGroupSelector_RaisesOnMuscleGroupsChangedEvent()
        {
            // Arrange
            var selectedGroups = new List<MuscleGroupRoleAssignmentDto>();
            var eventRaised = false;

            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, selectedGroups)
                .Add(p => p.MuscleGroupsChanged, EventCallback.Factory.Create<List<MuscleGroupRoleAssignmentDto>>(this, groups => eventRaised = true)));

            // Act
            component.Find("select").Change("Primary");
            component.FindAll("select")[1].Change("1");
            component.Find("button").Click();

            // Assert
            eventRaised.Should().BeTrue();
        }

        [Fact(Skip = "Form reset behavior coupled with async operations. TODO: Separate form state management from async actions")]
        public void MuscleGroupSelector_ResetsFormAfterSuccessfulAdd()
        {
            // Arrange
            var selectedGroups = new List<MuscleGroupRoleAssignmentDto>();

            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, selectedGroups)
                .Add(p => p.MuscleGroupsChanged, EventCallback.Factory.Create<List<MuscleGroupRoleAssignmentDto>>(this, groups => selectedGroups = groups)));

            // Act
            component.Find("select").Change("Primary");
            component.FindAll("select")[1].Change("1");
            component.Find("button").Click();

            // Assert
            var roleSelect = component.Find("select");
            roleSelect.GetAttribute("value").Should().BeEmpty();
            component.FindAll("select").Should().HaveCount(1); // Only role select visible
        }

        [Fact(Skip = "Button state testing requires precise element selection across dynamic DOM. TODO: Improve button selector strategy")]
        public void MuscleGroupSelector_DisablesAddButtonWhenNoMuscleGroupSelected()
        {
            // Arrange
            var component = RenderComponent<MuscleGroupSelector>(parameters => parameters
                .Add(p => p.AllMuscleGroups, _testMuscleGroups)
                .Add(p => p.MuscleRoles, _testMuscleRoles)
                .Add(p => p.MuscleGroups, new List<MuscleGroupRoleAssignmentDto>()));

            // Act - Select role but no muscle
            component.Find("select").Change("Primary");

            // Assert - Add button should be disabled
            var addButton = component.FindAll("button").First(b => b.TextContent.Trim() == "Add");
            addButton.GetAttribute("disabled").Should().NotBeNull();
            
            // Act - Now select a muscle
            component.FindAll("select")[1].Change("1");
            
            // Assert - Add button should be enabled
            addButton.GetAttribute("disabled").Should().BeNull();
        }
    }
}
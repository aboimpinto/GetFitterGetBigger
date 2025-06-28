using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Tests.Components.Pages.Exercises
{
    // Note: Blazor component testing requires special setup and is often done with integration tests
    // These tests verify the component logic patterns rather than full rendering
    public class ExerciseListTests
    {
        [Fact]
        public void ExerciseFilterDto_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var filter = new ExerciseFilterDto();

            // Assert
            filter.Page.Should().Be(1);
            filter.PageSize.Should().Be(10);
            filter.Name.Should().BeNull();
            filter.DifficultyId.Should().BeNull();
            filter.MuscleGroupIds.Should().BeNull();
            filter.EquipmentIds.Should().BeNull();
            filter.IsActive.Should().BeNull();
        }

        [Fact]
        public void ExercisePagedResultDto_TotalPages_CalculatedCorrectly()
        {
            // Arrange
            var pagedResult = new ExercisePagedResultDto
            {
                TotalCount = 25,
                PageSize = 10
            };

            // Act & Assert
            pagedResult.TotalPages.Should().Be(3);
        }

        [Fact]
        public void ExercisePagedResultDto_TotalPages_HandlesExactDivision()
        {
            // Arrange
            var pagedResult = new ExercisePagedResultDto
            {
                TotalCount = 20,
                PageSize = 10
            };

            // Act & Assert
            pagedResult.TotalPages.Should().Be(2);
        }

        [Fact]
        public void ExercisePagedResultDto_TotalPages_HandlesZeroCount()
        {
            // Arrange
            var pagedResult = new ExercisePagedResultDto
            {
                TotalCount = 0,
                PageSize = 10
            };

            // Act & Assert
            pagedResult.TotalPages.Should().Be(0);
        }

        [Fact]
        public void ExerciseListDto_Properties_AreInitialized()
        {
            // Arrange & Act
            var dto = new ExerciseListDto();

            // Assert
            dto.Id.Should().BeEmpty();
            dto.Name.Should().BeEmpty();
            dto.Description.Should().BeEmpty();
            dto.Instructions.Should().BeEmpty();
            dto.Difficulty.Should().BeNull();
            dto.MuscleGroups.Should().NotBeNull();
            dto.MuscleGroups.Should().BeEmpty();
            dto.Equipment.Should().NotBeNull();
            dto.Equipment.Should().BeEmpty();
            dto.MovementPatterns.Should().NotBeNull();
            dto.MovementPatterns.Should().BeEmpty();
            dto.BodyParts.Should().NotBeNull();
            dto.BodyParts.Should().BeEmpty();
        }

        [Fact]
        public void MuscleGroupRoleAssignmentDto_DefaultValues()
        {
            // Arrange & Act
            var dto = new MuscleGroupRoleAssignmentDto();

            // Assert
            dto.MuscleGroupId.Should().BeEmpty();
            dto.Role.Should().BeEmpty();
        }

        [Fact]
        public void MuscleGroupListItemDto_DefaultValues()
        {
            // Arrange & Act
            var dto = new MuscleGroupListItemDto();

            // Assert
            dto.MuscleGroup.Should().BeNull();
            dto.Role.Should().BeNull();
        }

        [Fact]
        public void ReferenceDataDto_WithValues_PropertiesSetCorrectly()
        {
            // Arrange & Act
            var dto = new ReferenceDataDto
            {
                Id = "test-id",
                Value = "Test Value",
                Description = "Test Description"
            };

            // Assert
            dto.Id.Should().Be("test-id");
            dto.Value.Should().Be("Test Value");
            dto.Description.Should().Be("Test Description");
        }

        [Fact]
        public void ExerciseListDto_WithNestedData_PropertiesSetCorrectly()
        {
            // Arrange & Act
            var dto = new ExerciseListDto
            {
                Id = "123",
                Name = "Test Exercise",
                Difficulty = new ReferenceDataDto
                {
                    Id = "diff-1",
                    Value = "Intermediate",
                    Description = "Intermediate difficulty"
                },
                MuscleGroups = new List<MuscleGroupListItemDto>
                {
                    new()
                    {
                        MuscleGroup = new ReferenceDataDto
                        {
                            Id = "mg-1",
                            Value = "Chest",
                            Description = "Chest muscles"
                        },
                        Role = new ReferenceDataDto
                        {
                            Id = "role-1",
                            Value = "Primary",
                            Description = "Primary muscle"
                        }
                    }
                },
                Equipment = new List<ReferenceDataDto>
                {
                    new()
                    {
                        Id = "eq-1",
                        Value = "Barbell",
                        Description = "Barbell equipment"
                    }
                }
            };

            // Assert
            dto.Id.Should().Be("123");
            dto.Name.Should().Be("Test Exercise");
            dto.Difficulty.Should().NotBeNull();
            dto.Difficulty!.Value.Should().Be("Intermediate");
            dto.MuscleGroups.Should().HaveCount(1);
            dto.MuscleGroups[0].MuscleGroup!.Value.Should().Be("Chest");
            dto.MuscleGroups[0].Role!.Value.Should().Be("Primary");
            dto.Equipment.Should().HaveCount(1);
            dto.Equipment[0].Value.Should().Be("Barbell");
        }
    }
}
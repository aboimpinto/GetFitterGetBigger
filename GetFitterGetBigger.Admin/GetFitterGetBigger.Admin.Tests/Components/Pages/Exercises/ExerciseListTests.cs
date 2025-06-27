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
            dto.Name.Should().BeEmpty();
            dto.DifficultyName.Should().BeEmpty();
            dto.PrimaryMuscleGroups.Should().NotBeNull();
            dto.PrimaryMuscleGroups.Should().BeEmpty();
            dto.Equipment.Should().NotBeNull();
            dto.Equipment.Should().BeEmpty();
        }

        [Fact]
        public void MuscleGroupRoleAssignmentDto_DefaultValues()
        {
            // Arrange & Act
            var dto = new MuscleGroupRoleAssignmentDto();

            // Assert
            dto.MuscleGroupId.Should().Be(Guid.Empty);
            dto.Role.Should().BeEmpty();
        }
    }
}
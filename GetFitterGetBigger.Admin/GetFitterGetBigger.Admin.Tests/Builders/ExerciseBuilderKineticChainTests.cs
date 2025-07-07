using FluentAssertions;
using GetFitterGetBigger.Admin.Builders;
using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Tests.Builders
{
    public class ExerciseBuilderKineticChainTests
    {
        [Fact]
        public void ExerciseCreateDtoBuilder_WithKineticChainId_SetsValueCorrectly()
        {
            // Arrange
            var kineticChainId = "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4";

            // Act
            var dto = new ExerciseCreateDtoBuilder()
                .WithName("Test Exercise")
                .WithDescription("Test Description")
                .WithDifficultyId("difficulty-1")
                .WithKineticChainId(kineticChainId)
                .Build();

            // Assert
            dto.KineticChainId.Should().Be(kineticChainId);
        }

        [Fact]
        public void ExerciseCreateDtoBuilder_WithNullKineticChainId_SetsNullValue()
        {
            // Act
            var dto = new ExerciseCreateDtoBuilder()
                .WithName("Rest Exercise")
                .WithDescription("Rest Description")
                .WithDifficultyId("difficulty-1")
                .WithKineticChainId(null)
                .Build();

            // Assert
            dto.KineticChainId.Should().BeNull();
        }

        [Fact]
        public void ExerciseCreateDtoBuilder_FromExerciseDto_MapsKineticChainCorrectly()
        {
            // Arrange
            var exerciseDto = new ExerciseDto
            {
                Name = "Test Exercise",
                Description = "Test Description",
                Instructions = "Test Instructions",
                Difficulty = new ReferenceDataDto { Id = "difficulty-1", Value = "Easy" },
                KineticChain = new ReferenceDataDto 
                { 
                    Id = "kineticchaintype-2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b", 
                    Value = "Isolation",
                    Description = "Single-muscle movement"
                },
                IsUnilateral = false,
                IsActive = true
            };

            // Act
            var createDto = ExerciseCreateDtoBuilder.FromExerciseDto(exerciseDto);

            // Assert
            createDto.KineticChainId.Should().Be("kineticchaintype-2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b");
        }

        [Fact]
        public void ExerciseCreateDtoBuilder_FromExerciseDto_HandlesNullKineticChain()
        {
            // Arrange
            var exerciseDto = new ExerciseDto
            {
                Name = "Rest Exercise",
                Description = "Rest Description",
                Instructions = "Rest Instructions",
                Difficulty = new ReferenceDataDto { Id = "difficulty-1", Value = "Easy" },
                KineticChain = null,
                IsUnilateral = false,
                IsActive = true
            };

            // Act
            var createDto = ExerciseCreateDtoBuilder.FromExerciseDto(exerciseDto);

            // Assert
            createDto.KineticChainId.Should().BeNull();
        }

        [Fact]
        public void ExerciseUpdateDtoBuilder_WithKineticChainId_SetsValueCorrectly()
        {
            // Arrange
            var kineticChainId = "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4";

            // Act
            var dto = new ExerciseUpdateDtoBuilder()
                .WithName("Updated Exercise")
                .WithDescription("Updated Description")
                .WithDifficultyId("difficulty-2")
                .WithKineticChainId(kineticChainId)
                .Build();

            // Assert
            dto.KineticChainId.Should().Be(kineticChainId);
        }

        [Fact]
        public void ExerciseUpdateDtoBuilder_FromCreateDto_MapsKineticChainId()
        {
            // Arrange
            var createDto = new ExerciseCreateDto
            {
                Name = "Test Exercise",
                Description = "Test Description",
                DifficultyId = "difficulty-1",
                KineticChainId = "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4"
            };

            // Act
            var updateDto = ExerciseUpdateDtoBuilder.FromCreateDto(createDto);

            // Assert
            updateDto.KineticChainId.Should().Be(createDto.KineticChainId);
        }

        [Fact]
        public void ExerciseUpdateDtoBuilder_FromExerciseDto_MapsKineticChainCorrectly()
        {
            // Arrange
            var exerciseDto = new ExerciseDto
            {
                Name = "Test Exercise",
                Description = "Test Description",
                Instructions = "Test Instructions",
                Difficulty = new ReferenceDataDto { Id = "difficulty-1", Value = "Easy" },
                KineticChain = new ReferenceDataDto 
                { 
                    Id = "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4", 
                    Value = "Compound",
                    Description = "Multi-muscle movement"
                },
                IsUnilateral = true,
                IsActive = true
            };

            // Act
            var updateDto = ExerciseUpdateDtoBuilder.FromExerciseDto(exerciseDto);

            // Assert
            updateDto.KineticChainId.Should().Be("kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4");
        }

        [Fact]
        public void ExerciseUpdateDtoBuilder_FromExerciseDto_HandlesNullKineticChain()
        {
            // Arrange
            var exerciseDto = new ExerciseDto
            {
                Name = "Rest Exercise",
                Description = "Rest Description",
                Instructions = "Rest Instructions",
                Difficulty = new ReferenceDataDto { Id = "difficulty-1", Value = "Easy" },
                KineticChain = null,
                IsUnilateral = false,
                IsActive = true
            };

            // Act
            var updateDto = ExerciseUpdateDtoBuilder.FromExerciseDto(exerciseDto);

            // Assert
            updateDto.KineticChainId.Should().BeNull();
        }
    }
}
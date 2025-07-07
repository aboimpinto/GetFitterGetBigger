using System.Text.Json;
using GetFitterGetBigger.Admin.Models.Dtos;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.Models.Dtos
{
    public class ExerciseDtoTests
    {
        [Fact]
        public void ExerciseDto_SerializesAndDeserializes_WithKineticChain()
        {
            // Arrange
            var exerciseDto = new ExerciseDto
            {
                Id = "test-id",
                Name = "Test Exercise",
                Description = "Test Description",
                KineticChain = new ReferenceDataDto
                {
                    Id = "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4",
                    Value = "Compound",
                    Description = "Multi-muscle movement"
                }
            };

            // Act
            var json = JsonSerializer.Serialize(exerciseDto);
            var deserialized = JsonSerializer.Deserialize<ExerciseDto>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(exerciseDto.Id, deserialized.Id);
            Assert.Equal(exerciseDto.Name, deserialized.Name);
            Assert.NotNull(deserialized.KineticChain);
            Assert.Equal(exerciseDto.KineticChain.Id, deserialized.KineticChain.Id);
            Assert.Equal(exerciseDto.KineticChain.Value, deserialized.KineticChain.Value);
            Assert.Equal(exerciseDto.KineticChain.Description, deserialized.KineticChain.Description);
        }

        [Fact]
        public void ExerciseDto_SerializesAndDeserializes_WithNullKineticChain()
        {
            // Arrange
            var exerciseDto = new ExerciseDto
            {
                Id = "test-id",
                Name = "Rest Exercise",
                Description = "Rest Description",
                KineticChain = null
            };

            // Act
            var json = JsonSerializer.Serialize(exerciseDto);
            var deserialized = JsonSerializer.Deserialize<ExerciseDto>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(exerciseDto.Id, deserialized.Id);
            Assert.Equal(exerciseDto.Name, deserialized.Name);
            Assert.Null(deserialized.KineticChain);
        }

        [Fact]
        public void ExerciseListDto_SerializesAndDeserializes_WithKineticChain()
        {
            // Arrange
            var exerciseListDto = new ExerciseListDto
            {
                Id = "test-id",
                Name = "Test Exercise",
                Description = "Test Description",
                KineticChain = new ReferenceDataDto
                {
                    Id = "kineticchaintype-2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b",
                    Value = "Isolation",
                    Description = "Single-muscle movement"
                }
            };

            // Act
            var json = JsonSerializer.Serialize(exerciseListDto);
            var deserialized = JsonSerializer.Deserialize<ExerciseListDto>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(exerciseListDto.Id, deserialized.Id);
            Assert.Equal(exerciseListDto.Name, deserialized.Name);
            Assert.NotNull(deserialized.KineticChain);
            Assert.Equal(exerciseListDto.KineticChain.Id, deserialized.KineticChain.Id);
            Assert.Equal(exerciseListDto.KineticChain.Value, deserialized.KineticChain.Value);
            Assert.Equal(exerciseListDto.KineticChain.Description, deserialized.KineticChain.Description);
        }

        [Fact]
        public void ExerciseCreateDto_SerializesAndDeserializes_WithKineticChainId()
        {
            // Arrange
            var exerciseCreateDto = new ExerciseCreateDto
            {
                Name = "Test Exercise",
                Description = "Test Description",
                DifficultyId = "difficulty-id",
                KineticChainId = "kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4"
            };

            // Act
            var json = JsonSerializer.Serialize(exerciseCreateDto);
            var deserialized = JsonSerializer.Deserialize<ExerciseCreateDto>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(exerciseCreateDto.Name, deserialized.Name);
            Assert.Equal(exerciseCreateDto.Description, deserialized.Description);
            Assert.Equal(exerciseCreateDto.KineticChainId, deserialized.KineticChainId);
        }

        [Fact]
        public void ExerciseCreateDto_SerializesAndDeserializes_WithNullKineticChainId()
        {
            // Arrange
            var exerciseCreateDto = new ExerciseCreateDto
            {
                Name = "Rest Exercise",
                Description = "Rest Description",
                DifficultyId = "difficulty-id",
                KineticChainId = null
            };

            // Act
            var json = JsonSerializer.Serialize(exerciseCreateDto);
            var deserialized = JsonSerializer.Deserialize<ExerciseCreateDto>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(exerciseCreateDto.Name, deserialized.Name);
            Assert.Equal(exerciseCreateDto.Description, deserialized.Description);
            Assert.Null(deserialized.KineticChainId);
        }

        [Fact]
        public void ExerciseUpdateDto_InheritsKineticChainId_FromExerciseCreateDto()
        {
            // Arrange
            var exerciseUpdateDto = new ExerciseUpdateDto
            {
                Name = "Updated Exercise",
                Description = "Updated Description",
                DifficultyId = "difficulty-id",
                KineticChainId = "kineticchaintype-2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b"
            };

            // Act
            var json = JsonSerializer.Serialize(exerciseUpdateDto);
            var deserialized = JsonSerializer.Deserialize<ExerciseUpdateDto>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(exerciseUpdateDto.Name, deserialized.Name);
            Assert.Equal(exerciseUpdateDto.Description, deserialized.Description);
            Assert.Equal(exerciseUpdateDto.KineticChainId, deserialized.KineticChainId);
        }
    }
}
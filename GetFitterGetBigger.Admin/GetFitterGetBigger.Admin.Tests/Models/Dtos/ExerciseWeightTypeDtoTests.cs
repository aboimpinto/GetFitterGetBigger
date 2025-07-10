using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Dtos;

namespace GetFitterGetBigger.Admin.Tests.Models.Dtos
{
    public class ExerciseWeightTypeDtoTests
    {
        [Fact]
        public void ExerciseWeightTypeDto_SerializesAndDeserializes_Correctly()
        {
            // Arrange
            var exerciseWeightTypeDto = new ExerciseWeightTypeDto
            {
                Id = Guid.NewGuid(),
                Code = "BODYWEIGHT_ONLY",
                Name = "Bodyweight Only",
                Description = "Exercise uses bodyweight only",
                IsActive = true,
                DisplayOrder = 1
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Act
            var json = JsonSerializer.Serialize(exerciseWeightTypeDto, jsonOptions);
            var deserialized = JsonSerializer.Deserialize<ExerciseWeightTypeDto>(json, jsonOptions);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Id.Should().Be(exerciseWeightTypeDto.Id);
            deserialized.Code.Should().Be(exerciseWeightTypeDto.Code);
            deserialized.Name.Should().Be(exerciseWeightTypeDto.Name);
            deserialized.Description.Should().Be(exerciseWeightTypeDto.Description);
            deserialized.IsActive.Should().Be(exerciseWeightTypeDto.IsActive);
            deserialized.DisplayOrder.Should().Be(exerciseWeightTypeDto.DisplayOrder);
        }

        [Fact]
        public void ExerciseWeightTypeDto_WithNullDescription_SerializesCorrectly()
        {
            // Arrange
            var exerciseWeightTypeDto = new ExerciseWeightTypeDto
            {
                Id = Guid.NewGuid(),
                Code = "WEIGHT_REQUIRED",
                Name = "Weight Required",
                Description = null,
                IsActive = true,
                DisplayOrder = 2
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Act
            var json = JsonSerializer.Serialize(exerciseWeightTypeDto, jsonOptions);
            var deserialized = JsonSerializer.Deserialize<ExerciseWeightTypeDto>(json, jsonOptions);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Description.Should().BeNull();
        }

        [Fact]
        public void ExerciseWeightTypeDto_RequiredProperties_FailValidationWhenMissing()
        {
            // Arrange
            var exerciseWeightTypeDto = new ExerciseWeightTypeDto
            {
                Id = Guid.Empty, // Invalid required field
                Code = "", // Invalid required field
                Name = "", // Invalid required field
                IsActive = true,
                DisplayOrder = 1
            };

            var context = new ValidationContext(exerciseWeightTypeDto);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(exerciseWeightTypeDto, context, results, validateAllProperties: true);

            // Assert
            isValid.Should().BeFalse();
            results.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public void ExerciseWeightTypeDto_ValidObject_PassesValidation()
        {
            // Arrange
            var exerciseWeightTypeDto = new ExerciseWeightTypeDto
            {
                Id = Guid.NewGuid(),
                Code = "MACHINE_WEIGHT",
                Name = "Machine Weight",
                Description = "Exercise uses machine weight",
                IsActive = true,
                DisplayOrder = 5
            };

            var context = new ValidationContext(exerciseWeightTypeDto);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(exerciseWeightTypeDto, context, results, validateAllProperties: true);

            // Assert
            isValid.Should().BeTrue();
            results.Should().BeEmpty();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ExerciseWeightTypeDto_IsActiveProperty_SerializesCorrectly(bool isActive)
        {
            // Arrange
            var exerciseWeightTypeDto = new ExerciseWeightTypeDto
            {
                Id = Guid.NewGuid(),
                Code = "TEST",
                Name = "Test",
                IsActive = isActive,
                DisplayOrder = 1
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Act
            var json = JsonSerializer.Serialize(exerciseWeightTypeDto, jsonOptions);
            var deserialized = JsonSerializer.Deserialize<ExerciseWeightTypeDto>(json, jsonOptions);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.IsActive.Should().Be(isActive);
        }

        [Fact]
        public void ExerciseWeightTypeDto_DefaultValues_AreSetCorrectly()
        {
            // Arrange & Act
            var exerciseWeightTypeDto = new ExerciseWeightTypeDto
            {
                Id = Guid.NewGuid(),
                Code = "TEST",
                Name = "Test"
            };

            // Assert
            exerciseWeightTypeDto.IsActive.Should().BeTrue(); // Default value
            exerciseWeightTypeDto.DisplayOrder.Should().Be(0); // Default value
            exerciseWeightTypeDto.Description.Should().BeNull(); // Optional property
        }
    }
}
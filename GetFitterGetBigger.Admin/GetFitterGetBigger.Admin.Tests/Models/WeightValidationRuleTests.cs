using FluentAssertions;
using GetFitterGetBigger.Admin.Models;

namespace GetFitterGetBigger.Admin.Tests.Models
{
    public class WeightValidationRuleTests
    {
        [Fact]
        public void ValidateWeight_BodyweightOnly_WithNull_ReturnsTrue()
        {
            // Act
            var result = WeightValidationRule.ValidateWeight("BODYWEIGHT_ONLY", null);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ValidateWeight_BodyweightOnly_WithZero_ReturnsTrue()
        {
            // Act
            var result = WeightValidationRule.ValidateWeight("BODYWEIGHT_ONLY", 0m);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ValidateWeight_BodyweightOnly_WithPositiveValue_ReturnsFalse()
        {
            // Act
            var result = WeightValidationRule.ValidateWeight("BODYWEIGHT_ONLY", 10m);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ValidateWeight_BodyweightOnly_WithNegativeValue_ReturnsFalse()
        {
            // Act
            var result = WeightValidationRule.ValidateWeight("BODYWEIGHT_ONLY", -5m);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ValidateWeight_NoWeight_WithNull_ReturnsTrue()
        {
            // Act
            var result = WeightValidationRule.ValidateWeight("NO_WEIGHT", null);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ValidateWeight_NoWeight_WithZero_ReturnsTrue()
        {
            // Act
            var result = WeightValidationRule.ValidateWeight("NO_WEIGHT", 0m);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ValidateWeight_NoWeight_WithPositiveValue_ReturnsFalse()
        {
            // Act
            var result = WeightValidationRule.ValidateWeight("NO_WEIGHT", 10m);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ValidateWeight_BodyweightOptional_WithNull_ReturnsTrue()
        {
            // Act
            var result = WeightValidationRule.ValidateWeight("BODYWEIGHT_OPTIONAL", null);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ValidateWeight_BodyweightOptional_WithZero_ReturnsTrue()
        {
            // Act
            var result = WeightValidationRule.ValidateWeight("BODYWEIGHT_OPTIONAL", 0m);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ValidateWeight_BodyweightOptional_WithPositiveValue_ReturnsTrue()
        {
            // Act
            var result = WeightValidationRule.ValidateWeight("BODYWEIGHT_OPTIONAL", 10m);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ValidateWeight_BodyweightOptional_WithNegativeValue_ReturnsFalse()
        {
            // Act
            var result = WeightValidationRule.ValidateWeight("BODYWEIGHT_OPTIONAL", -5m);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ValidateWeight_WeightRequired_WithNull_ReturnsFalse()
        {
            // Act
            var result = WeightValidationRule.ValidateWeight("WEIGHT_REQUIRED", null);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ValidateWeight_WeightRequired_WithZero_ReturnsFalse()
        {
            // Act
            var result = WeightValidationRule.ValidateWeight("WEIGHT_REQUIRED", 0m);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ValidateWeight_WeightRequired_WithPositiveValue_ReturnsTrue()
        {
            // Act
            var result = WeightValidationRule.ValidateWeight("WEIGHT_REQUIRED", 10m);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ValidateWeight_WeightRequired_WithNegativeValue_ReturnsFalse()
        {
            // Act
            var result = WeightValidationRule.ValidateWeight("WEIGHT_REQUIRED", -5m);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ValidateWeight_MachineWeight_WithNull_ReturnsFalse()
        {
            // Act
            var result = WeightValidationRule.ValidateWeight("MACHINE_WEIGHT", null);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ValidateWeight_MachineWeight_WithZero_ReturnsFalse()
        {
            // Act
            var result = WeightValidationRule.ValidateWeight("MACHINE_WEIGHT", 0m);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ValidateWeight_MachineWeight_WithPositiveValue_ReturnsTrue()
        {
            // Act
            var result = WeightValidationRule.ValidateWeight("MACHINE_WEIGHT", 45.5m);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ValidateWeight_MachineWeight_WithNegativeValue_ReturnsFalse()
        {
            // Act
            var result = WeightValidationRule.ValidateWeight("MACHINE_WEIGHT", -5m);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ValidateWeight_UnknownWeightType_ReturnsFalse()
        {
            // Act
            var result = WeightValidationRule.ValidateWeight("UNKNOWN_TYPE", 10m);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ValidateWeight_EmptyWeightType_ReturnsFalse()
        {
            // Act
            var result = WeightValidationRule.ValidateWeight("", 10m);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void GetValidationMessage_BodyweightOnly_ReturnsCorrectMessage()
        {
            // Act
            var result = WeightValidationRule.GetValidationMessage("BODYWEIGHT_ONLY");

            // Assert
            result.Should().Be("This exercise uses bodyweight only. Weight cannot be specified.");
        }

        [Fact]
        public void GetValidationMessage_NoWeight_ReturnsCorrectMessage()
        {
            // Act
            var result = WeightValidationRule.GetValidationMessage("NO_WEIGHT");

            // Assert
            result.Should().Be("This exercise does not use weights. Weight cannot be specified.");
        }

        [Fact]
        public void GetValidationMessage_BodyweightOptional_ReturnsCorrectMessage()
        {
            // Act
            var result = WeightValidationRule.GetValidationMessage("BODYWEIGHT_OPTIONAL");

            // Assert
            result.Should().Be("This exercise can be done with or without additional weight.");
        }

        [Fact]
        public void GetValidationMessage_WeightRequired_ReturnsCorrectMessage()
        {
            // Act
            var result = WeightValidationRule.GetValidationMessage("WEIGHT_REQUIRED");

            // Assert
            result.Should().Be("This exercise requires a weight to be specified.");
        }

        [Fact]
        public void GetValidationMessage_MachineWeight_ReturnsCorrectMessage()
        {
            // Act
            var result = WeightValidationRule.GetValidationMessage("MACHINE_WEIGHT");

            // Assert
            result.Should().Be("This exercise requires a machine weight to be specified.");
        }

        [Fact]
        public void GetValidationMessage_UnknownType_ReturnsDefaultMessage()
        {
            // Act
            var result = WeightValidationRule.GetValidationMessage("UNKNOWN_TYPE");

            // Assert
            result.Should().Be("Unknown weight type.");
        }

        [Fact]
        public void RequiresWeightInput_BodyweightOnly_ReturnsFalse()
        {
            // Act
            var result = WeightValidationRule.RequiresWeightInput("BODYWEIGHT_ONLY");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void RequiresWeightInput_NoWeight_ReturnsFalse()
        {
            // Act
            var result = WeightValidationRule.RequiresWeightInput("NO_WEIGHT");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void RequiresWeightInput_BodyweightOptional_ReturnsTrue()
        {
            // Act
            var result = WeightValidationRule.RequiresWeightInput("BODYWEIGHT_OPTIONAL");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void RequiresWeightInput_WeightRequired_ReturnsTrue()
        {
            // Act
            var result = WeightValidationRule.RequiresWeightInput("WEIGHT_REQUIRED");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void RequiresWeightInput_MachineWeight_ReturnsTrue()
        {
            // Act
            var result = WeightValidationRule.RequiresWeightInput("MACHINE_WEIGHT");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void RequiresWeightInput_UnknownType_ReturnsFalse()
        {
            // Act
            var result = WeightValidationRule.RequiresWeightInput("UNKNOWN_TYPE");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void GetValidationMessage_AllWeightTypes_ReturnsNonEmptyMessages()
        {
            // Arrange
            var weightTypeCodes = new[]
            {
                "BODYWEIGHT_ONLY",
                "NO_WEIGHT",
                "BODYWEIGHT_OPTIONAL",
                "WEIGHT_REQUIRED",
                "MACHINE_WEIGHT"
            };

            // Act & Assert
            foreach (var code in weightTypeCodes)
            {
                var message = WeightValidationRule.GetValidationMessage(code);
                message.Should().NotBeNullOrEmpty($"Weight type '{code}' should have a validation message");
            }
        }

        [Fact]
        public void RequiresWeightInput_AllWeightTypes_ReturnsConsistentResults()
        {
            // Arrange
            var weightTypesRequiringInput = new[] { "BODYWEIGHT_OPTIONAL", "WEIGHT_REQUIRED", "MACHINE_WEIGHT" };
            var weightTypesNotRequiringInput = new[] { "BODYWEIGHT_ONLY", "NO_WEIGHT" };

            // Act & Assert
            foreach (var code in weightTypesRequiringInput)
            {
                WeightValidationRule.RequiresWeightInput(code).Should().BeTrue($"Weight type '{code}' should require weight input");
            }

            foreach (var code in weightTypesNotRequiringInput)
            {
                WeightValidationRule.RequiresWeightInput(code).Should().BeFalse($"Weight type '{code}' should not require weight input");
            }
        }
    }
}
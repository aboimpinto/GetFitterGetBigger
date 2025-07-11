using System.Text.Json;
using FluentAssertions;
using GetFitterGetBigger.Admin.JsonConverters;
using GetFitterGetBigger.Admin.Models.Dtos;
using Xunit;

namespace GetFitterGetBigger.Admin.Tests.JsonConverters
{
    public class ExerciseWeightTypeJsonConverterTests
    {
        private readonly JsonSerializerOptions _options;

        public ExerciseWeightTypeJsonConverterTests()
        {
            _options = new JsonSerializerOptions
            {
                Converters = { new ExerciseWeightTypeJsonConverter() }
            };
        }

        [Fact]
        public void Read_WithValidReferenceData_ReturnsExerciseWeightTypeDto()
        {
            // Arrange
            var json = @"{
                ""id"": ""exerciseweighttype-123e4567-e89b-12d3-a456-426614174000"",
                ""value"": ""Bodyweight Only"",
                ""description"": ""Uses bodyweight resistance only""
            }";

            // Act
            var result = JsonSerializer.Deserialize<ExerciseWeightTypeDto>(json, _options);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(Guid.Parse("123e4567-e89b-12d3-a456-426614174000"));
            result.Code.Should().Be("BODYWEIGHT_ONLY");
            result.Name.Should().Be("Bodyweight Only");
            result.Description.Should().Be("Uses bodyweight resistance only");
            result.IsActive.Should().BeTrue();
            result.DisplayOrder.Should().Be(0);
        }

        [Fact]
        public void Read_WithGuidIdFormat_ReturnsExerciseWeightTypeDto()
        {
            // Arrange
            var json = @"{
                ""id"": ""123e4567-e89b-12d3-a456-426614174000"",
                ""value"": ""Weight Required"",
                ""description"": ""External weight required""
            }";

            // Act
            var result = JsonSerializer.Deserialize<ExerciseWeightTypeDto>(json, _options);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(Guid.Parse("123e4567-e89b-12d3-a456-426614174000"));
            result.Code.Should().Be("WEIGHT_REQUIRED");
            result.Name.Should().Be("Weight Required");
        }

        [Fact]
        public void Read_WithNullDescription_ReturnsExerciseWeightTypeDtoWithNullDescription()
        {
            // Arrange
            var json = @"{
                ""id"": ""exerciseweighttype-123e4567-e89b-12d3-a456-426614174000"",
                ""value"": ""Machine Weight""
            }";

            // Act
            var result = JsonSerializer.Deserialize<ExerciseWeightTypeDto>(json, _options);

            // Assert
            result.Should().NotBeNull();
            result!.Description.Should().BeNull();
            result.Code.Should().Be("MACHINE_WEIGHT");
        }

        [Fact]
        public void Read_WithNull_ReturnsNull()
        {
            // Arrange
            var json = "null";

            // Act
            var result = JsonSerializer.Deserialize<ExerciseWeightTypeDto>(json, _options);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Read_WithMissingId_ReturnsNull()
        {
            // Arrange
            var json = @"{
                ""value"": ""Bodyweight Only"",
                ""description"": ""Uses bodyweight resistance only""
            }";

            // Act
            var result = JsonSerializer.Deserialize<ExerciseWeightTypeDto>(json, _options);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Read_WithMissingValue_ReturnsNull()
        {
            // Arrange
            var json = @"{
                ""id"": ""exerciseweighttype-123e4567-e89b-12d3-a456-426614174000"",
                ""description"": ""Uses bodyweight resistance only""
            }";

            // Act
            var result = JsonSerializer.Deserialize<ExerciseWeightTypeDto>(json, _options);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Read_WithEmptyId_ReturnsNull()
        {
            // Arrange
            var json = @"{
                ""id"": """",
                ""value"": ""Bodyweight Only"",
                ""description"": ""Uses bodyweight resistance only""
            }";

            // Act
            var result = JsonSerializer.Deserialize<ExerciseWeightTypeDto>(json, _options);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Read_WithInvalidGuid_ReturnsNull()
        {
            // Arrange
            var json = @"{
                ""id"": ""exerciseweighttype-invalid-guid"",
                ""value"": ""Bodyweight Only"",
                ""description"": ""Uses bodyweight resistance only""
            }";

            // Act
            var result = JsonSerializer.Deserialize<ExerciseWeightTypeDto>(json, _options);

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [InlineData("No Weight", "NO_WEIGHT")]
        [InlineData("Bodyweight Optional", "BODYWEIGHT_OPTIONAL")]
        [InlineData("Machine  Weight", "MACHINE__WEIGHT")] // Double space becomes double underscore
        [InlineData("weight required", "WEIGHT_REQUIRED")] // Lowercase input
        public void Read_WithVariousValueFormats_ConvertsCodeCorrectly(string value, string expectedCode)
        {
            // Arrange
            var json = $@"{{
                ""id"": ""exerciseweighttype-123e4567-e89b-12d3-a456-426614174000"",
                ""value"": ""{value}"",
                ""description"": ""Test description""
            }}";

            // Act
            var result = JsonSerializer.Deserialize<ExerciseWeightTypeDto>(json, _options);

            // Assert
            result.Should().NotBeNull();
            result!.Code.Should().Be(expectedCode);
            result.Name.Should().Be(value);
        }

        [Fact]
        public void Write_WithValidDto_WritesCorrectJson()
        {
            // Arrange
            var dto = new ExerciseWeightTypeDto
            {
                Id = Guid.Parse("123e4567-e89b-12d3-a456-426614174000"),
                Code = "BODYWEIGHT_ONLY",
                Name = "Bodyweight Only",
                Description = "Uses bodyweight resistance only",
                IsActive = true,
                DisplayOrder = 1
            };

            // Act
            var json = JsonSerializer.Serialize(dto, _options);
            var result = JsonSerializer.Deserialize<JsonElement>(json);

            // Assert
            result.GetProperty("id").GetString().Should().Be("exerciseweighttype-123e4567-e89b-12d3-a456-426614174000");
            result.GetProperty("value").GetString().Should().Be("Bodyweight Only");
            result.GetProperty("description").GetString().Should().Be("Uses bodyweight resistance only");
            result.TryGetProperty("code", out _).Should().BeFalse(); // Code should not be serialized
            result.TryGetProperty("isActive", out _).Should().BeFalse(); // IsActive should not be serialized
            result.TryGetProperty("displayOrder", out _).Should().BeFalse(); // DisplayOrder should not be serialized
        }

        [Fact]
        public void Write_WithNullDto_WritesNull()
        {
            // Arrange
            ExerciseWeightTypeDto? dto = null;

            // Act
            var json = JsonSerializer.Serialize(dto, _options);

            // Assert
            json.Should().Be("null");
        }

        [Fact]
        public void Write_WithNullDescription_WritesNullDescription()
        {
            // Arrange
            var dto = new ExerciseWeightTypeDto
            {
                Id = Guid.Parse("123e4567-e89b-12d3-a456-426614174000"),
                Code = "BODYWEIGHT_ONLY",
                Name = "Bodyweight Only",
                Description = null,
                IsActive = true,
                DisplayOrder = 0
            };

            // Act
            var json = JsonSerializer.Serialize(dto, _options);
            var result = JsonSerializer.Deserialize<JsonElement>(json);

            // Assert
            result.GetProperty("description").ValueKind.Should().Be(JsonValueKind.Null);
        }

        [Fact]
        public void RoundTrip_WithValidData_PreservesInformation()
        {
            // Arrange
            var original = new ExerciseWeightTypeDto
            {
                Id = Guid.Parse("123e4567-e89b-12d3-a456-426614174000"),
                Code = "BODYWEIGHT_ONLY",
                Name = "Bodyweight Only",
                Description = "Uses bodyweight resistance only",
                IsActive = true,
                DisplayOrder = 5
            };

            // Act
            var json = JsonSerializer.Serialize(original, _options);
            var deserialized = JsonSerializer.Deserialize<ExerciseWeightTypeDto>(json, _options);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.Id.Should().Be(original.Id);
            deserialized.Name.Should().Be(original.Name);
            deserialized.Description.Should().Be(original.Description);
            // Code is derived from Name during deserialization
            deserialized.Code.Should().Be("BODYWEIGHT_ONLY");
            // IsActive and DisplayOrder are set to defaults during deserialization
            deserialized.IsActive.Should().BeTrue();
            deserialized.DisplayOrder.Should().Be(0);
        }
    }
}
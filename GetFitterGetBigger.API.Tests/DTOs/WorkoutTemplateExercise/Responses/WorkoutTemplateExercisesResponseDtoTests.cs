using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise;
using GetFitterGetBigger.API.DTOs.WorkoutTemplateExercise.Responses;

namespace GetFitterGetBigger.API.Tests.DTOs.WorkoutTemplateExercise.Responses;

public class WorkoutTemplateExercisesResponseDtoTests
{
    [Fact]
    public void WorkoutTemplateExercisesResponseDto_WithSuccessfulData_ShouldCreateCorrectly()
    {
        // Arrange
        var data = WorkoutTemplateExercisesDto.Empty;
        var message = "Retrieved exercises successfully";

        // Act
        var response = new WorkoutTemplateExercisesResponseDto(true, data, message, new List<string>());

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().Be(data);
        response.Message.Should().Be(message);
        response.Errors.Should().BeEmpty();
    }

    [Fact]
    public void SuccessResponse_ShouldCreateCorrectStructure()
    {
        // Arrange
        var data = WorkoutTemplateExercisesDto.Empty;
        var message = "Success message";

        // Act
        var response = WorkoutTemplateExercisesResponseDto.SuccessResponse(data, message);

        // Assert
        response.Success.Should().BeTrue();
        response.Data.Should().Be(data);
        response.Message.Should().Be(message);
        response.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ErrorResponse_ShouldCreateCorrectStructure()
    {
        // Arrange
        var errors = new List<string> { "Error 1", "Error 2" };
        var message = "Operation failed";

        // Act
        var response = WorkoutTemplateExercisesResponseDto.ErrorResponse(errors, message);

        // Assert
        response.Success.Should().BeFalse();
        response.Data.Should().Be(WorkoutTemplateExercisesDto.Empty);
        response.Message.Should().Be(message);
        response.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void WorkoutTemplateExercisesResponseDto_WithDefaultValues_ShouldHandleCorrectly()
    {
        // Arrange & Act
        var response = new WorkoutTemplateExercisesResponseDto(
            false, 
            WorkoutTemplateExercisesDto.Empty);

        // Assert
        response.Success.Should().BeFalse();
        response.Data.Should().Be(WorkoutTemplateExercisesDto.Empty);
        response.Message.Should().Be("");
        response.Errors.Should().NotBeNull();
    }
}
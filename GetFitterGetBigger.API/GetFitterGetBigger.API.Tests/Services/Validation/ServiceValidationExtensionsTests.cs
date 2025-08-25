using FluentAssertions;
using GetFitterGetBigger.API.DTOs.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using Moq.AutoMock;

namespace GetFitterGetBigger.API.Tests.Services.Validation;

/// <summary>
/// Tests for ServiceValidationExtensions methods focusing on high Crap Score methods:
/// - AddServiceResultErrorsToValidation() (indirectly through public methods)
/// </summary>
public class ServiceValidationExtensionsTests
{
    [Fact]
    public async Task WithServiceResultAsync_SuccessfulResult_ReturnsDataWithoutErrors()
    {
        // Arrange
        var validation = ServiceValidate.For<TestDto>();
        var testData = new TestDto { Value = "test" };
        var successResult = ServiceResult<TestDto>.Success(testData);

        // Act
        var result = await validation.WithServiceResultAsync(() => Task.FromResult(successResult));

        // Assert
        result.Validation.HasErrors.Should().BeFalse();
        result.Data.Should().Be(testData);
    }

    [Fact]
    public async Task WithServiceResultAsync_FailedResultWithStructuredErrors_AddsStructuredErrors()
    {
        // Arrange
        var validation = ServiceValidate.For<TestDto>();
        var structuredError = ServiceError.NotFound("Entity");
        var failedResult = ServiceResult<TestDto>.Failure(TestDto.Empty, structuredError);

        // Act
        var result = await validation.WithServiceResultAsync(() => Task.FromResult(failedResult));

        // Assert
        result.Validation.HasErrors.Should().BeTrue();
        result.Data.Should().BeNull();
    }

    [Fact]
    public async Task WithServiceResultAsync_FailedResultWithStringErrors_AddsStringErrors()
    {
        // Arrange
        var validation = ServiceValidate.For<TestDto>();
        var failedResult = ServiceResult<TestDto>.Failure(TestDto.Empty, "Error 1", "Error 2");

        // Act
        var result = await validation.WithServiceResultAsync(() => Task.FromResult(failedResult));

        // Assert
        result.Validation.HasErrors.Should().BeTrue();
        result.Data.Should().BeNull();
    }

    #region Test DTOs

    private class TestDto : IEmptyDto<TestDto>
    {
        public string Value { get; set; } = string.Empty;
        public static TestDto Empty => new();
        public bool IsEmpty => string.IsNullOrEmpty(Value);
    }

    #endregion
}
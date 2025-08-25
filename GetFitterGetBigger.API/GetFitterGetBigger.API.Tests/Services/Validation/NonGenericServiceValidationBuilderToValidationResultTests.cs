using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using FluentAssertions;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services.Validation;

/// <summary>
/// Tests for the refactored NonGenericServiceValidationBuilder.ToValidationResultAsync method 
/// to ensure complexity reduction and proper functionality.
/// </summary>
public class NonGenericServiceValidationBuilderToValidationResultTests
{
    [Fact]
    public async Task ToValidationResultAsync_WithSyncErrors_ReturnsFailureImmediately()
    {
        // Arrange
        var builder = ServiceValidate.Build()
            .EnsureNotWhiteSpace("", "Value cannot be empty");

        // Act
        var result = await builder.ToValidationResultAsync();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Value cannot be empty");
    }

    [Fact]
    public async Task ToValidationResultAsync_WithAsyncStringErrors_ReturnsFailureWithErrors()
    {
        // Arrange
        var builder = ServiceValidate.Build()
            .EnsureAsync(
                async () => 
                {
                    await Task.Delay(1);
                    return false;
                },
                "Async string validation failed");

        // Act
        var result = await builder.ToValidationResultAsync();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Async string validation failed");
    }

    [Fact]
    public async Task ToValidationResultAsync_WithAsyncServiceErrors_ReturnsFailureWithServiceError()
    {
        // Arrange
        var serviceError = ServiceError.ValidationFailed("Async service error");
        var builder = ServiceValidate.Build()
            .EnsureAsync(
                async () => 
                {
                    await Task.Delay(1);
                    return false;
                },
                serviceError);

        // Act
        var result = await builder.ToValidationResultAsync();

        // Assert
        result.IsValid.Should().BeFalse();
        result.ServiceError.Should().NotBeNull();
        result.ServiceError!.Code.Should().Be(ServiceErrorCode.ValidationFailed);
        result.ServiceError.Message.Should().Be("Async service error");
    }

    [Fact]
    public async Task ToValidationResultAsync_WithMixedAsyncErrors_PreservesFirstServiceError()
    {
        // Arrange
        var firstServiceError = ServiceError.NotFound("Entity not found");
        var secondServiceError = ServiceError.ValidationFailed("Validation failed");
        var builder = ServiceValidate.Build()
            .EnsureAsync(
                async () => 
                {
                    await Task.Delay(1);
                    return false;
                },
                "String error")
            .EnsureAsync(
                async () => 
                {
                    await Task.Delay(1);
                    return false;
                },
                firstServiceError)
            .EnsureAsync(
                async () => 
                {
                    await Task.Delay(1);
                    return false;
                },
                secondServiceError);

        // Act
        var result = await builder.ToValidationResultAsync();

        // Assert
        result.IsValid.Should().BeFalse();
        result.ServiceError.Should().NotBeNull();
        result.ServiceError!.Code.Should().Be(ServiceErrorCode.NotFound, "Should preserve the first ServiceError");
    }

    [Fact]
    public async Task ToValidationResultAsync_WithAllValidations_ReturnsSuccess()
    {
        // Arrange
        var builder = ServiceValidate.Build()
            .EnsureNotWhiteSpace("valid value", "Should not fail")
            .EnsureAsync(
                async () => 
                {
                    await Task.Delay(1);
                    return true;
                },
                "Should not fail")
            .EnsureAsync(
                async () => 
                {
                    await Task.Delay(1);
                    return true;
                },
                ServiceError.ValidationFailed("Should not fail"));

        // Act
        var result = await builder.ToValidationResultAsync();

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.ServiceError.Should().BeNull();
    }

    [Fact]
    public async Task ToValidationResultAsync_WithSyncErrorsSkipsAsync_DoesNotExecuteAsyncValidations()
    {
        // Arrange
        var asyncExecuted = false;
        var builder = ServiceValidate.Build()
            .EnsureNotWhiteSpace("", "Sync error")
            .EnsureAsync(
                async () => 
                {
                    asyncExecuted = true;
                    await Task.Delay(1);
                    return false;
                },
                "Should not execute");

        // Act
        var result = await builder.ToValidationResultAsync();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Sync error");
        asyncExecuted.Should().BeFalse("Async validation should be skipped when sync errors exist");
    }

    [Fact]
    public async Task ToValidationResultAsync_MultipleAsyncValidations_ExecutesAll()
    {
        // Arrange
        var firstExecuted = false;
        var secondExecuted = false;
        var thirdExecuted = false;
        
        var builder = ServiceValidate.Build()
            .EnsureAsync(
                async () => 
                {
                    firstExecuted = true;
                    await Task.Delay(1);
                    return true;
                },
                "First validation")
            .EnsureAsync(
                async () => 
                {
                    secondExecuted = true;
                    await Task.Delay(1);
                    return true;
                },
                ServiceError.ValidationFailed("Second validation"))
            .EnsureAsync(
                async () => 
                {
                    thirdExecuted = true;
                    await Task.Delay(1);
                    return true;
                },
                "Third validation");

        // Act
        var result = await builder.ToValidationResultAsync();

        // Assert
        result.IsValid.Should().BeTrue();
        firstExecuted.Should().BeTrue("First async validation should execute");
        secondExecuted.Should().BeTrue("Second async validation should execute");
        thirdExecuted.Should().BeTrue("Third async validation should execute");
    }
}
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using FluentAssertions;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Services.Validation;

public class ServiceValidationBuilderExtensionsTests
{
    /// <summary>
    /// This test demonstrates the bug: async validations are not executed
    /// before calling whenValid in MatchAsync
    /// </summary>
    [Fact]
    public async Task MatchAsync_Should_Execute_Async_Validations_Before_WhenValid()
    {
        // Arrange
        var asyncValidationExecuted = false;
        var whenValidExecuted = false;
        
        // Create a builder with an async validation that should fail
        var builder = ServiceValidate.Build<ExerciseDto>()
            .EnsureAsync(
                async () =>
                {
                    asyncValidationExecuted = true;
                    await Task.Delay(1); // Simulate async work
                    return false; // Validation fails
                },
                ServiceError.ValidationFailed("Async validation failed"));

        // Act
        // Explicitly use the extension method overload that doesn't require whenInvalid
        var result = await ServiceValidationBuilderExtensions.MatchAsync(
            builder,
            whenValid: async () =>
            {
                whenValidExecuted = true;
                await Task.Delay(1);
                return ServiceResult<ExerciseDto>.Success(new ExerciseDto());
            });

        // Assert
        asyncValidationExecuted.Should().BeTrue("async validation should have been executed");
        whenValidExecuted.Should().BeFalse("whenValid should NOT be executed when validation fails");
        result.IsSuccess.Should().BeFalse("result should be failure when validation fails");
        result.Errors.Should().Contain("Async validation failed");
    }

    /// <summary>
    /// Test that async validations with ServiceError are executed correctly
    /// </summary>
    [Fact]
    public async Task MatchAsync_Should_Execute_Async_Validations_With_ServiceError()
    {
        // Arrange
        var asyncValidationExecuted = false;
        
        var builder = ServiceValidate.Build<ExerciseDto>()
            .EnsureAsync(
                async () =>
                {
                    asyncValidationExecuted = true;
                    await Task.Delay(1);
                    return false;
                },
                ServiceError.AlreadyExists("Exercise", "Test Exercise"));

        // Act
        var result = await builder.MatchAsync(
            whenValid: async () => await Task.FromResult(ServiceResult<ExerciseDto>.Success(new ExerciseDto())));

        // Assert
        asyncValidationExecuted.Should().BeTrue("async validation should have been executed");
        result.IsSuccess.Should().BeFalse();
        result.PrimaryErrorCode.Should().Be(ServiceErrorCode.AlreadyExists);
    }

    /// <summary>
    /// Test that multiple async validations are all executed
    /// </summary>
    [Fact]
    public async Task MatchAsync_Should_Execute_All_Async_Validations()
    {
        // Arrange
        var firstAsyncExecuted = false;
        var secondAsyncExecuted = false;
        
        var builder = ServiceValidate.Build<ExerciseDto>()
            .EnsureAsync(
                async () =>
                {
                    firstAsyncExecuted = true;
                    await Task.Delay(1);
                    return true; // First passes
                },
                ServiceError.ValidationFailed("First validation"))
            .EnsureAsync(
                async () =>
                {
                    secondAsyncExecuted = true;
                    await Task.Delay(1);
                    return false; // Second fails
                },
                ServiceError.ValidationFailed("Second validation failed"));

        // Act
        var result = await builder.MatchAsync(
            whenValid: async () => await Task.FromResult(ServiceResult<ExerciseDto>.Success(new ExerciseDto())));

        // Assert
        firstAsyncExecuted.Should().BeTrue("first async validation should have been executed");
        secondAsyncExecuted.Should().BeTrue("second async validation should have been executed");
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Second validation failed");
    }

    /// <summary>
    /// Test that sync validations still work correctly
    /// </summary>
    [Fact]
    public async Task MatchAsync_Should_Handle_Sync_Validations_Correctly()
    {
        // Arrange
        var builder = ServiceValidate.Build<ExerciseDto>()
            .Ensure(() => false, ServiceError.ValidationFailed("Sync validation failed"));

        // Act
        var result = await builder.MatchAsync(
            whenValid: async () => await Task.FromResult(ServiceResult<ExerciseDto>.Success(new ExerciseDto())));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Sync validation failed");
    }

    /// <summary>
    /// Test that mixed sync and async validations work together
    /// </summary>
    [Fact]
    public async Task MatchAsync_Should_Handle_Mixed_Sync_And_Async_Validations()
    {
        // Arrange
        var asyncExecuted = false;
        
        var builder = ServiceValidate.Build<ExerciseDto>()
            .Ensure(() => true, ServiceError.ValidationFailed("Sync validation")) // Sync passes
            .EnsureAsync(
                async () =>
                {
                    asyncExecuted = true;
                    await Task.Delay(1);
                    return false; // Async fails
                },
                ServiceError.ValidationFailed("Async validation failed"));

        // Act
        var result = await builder.MatchAsync(
            whenValid: async () => await Task.FromResult(ServiceResult<ExerciseDto>.Success(new ExerciseDto())));

        // Assert
        asyncExecuted.Should().BeTrue("async validation should have been executed");
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Async validation failed");
    }

    /// <summary>
    /// Test the positive case where all validations pass
    /// </summary>
    [Fact]
    public async Task MatchAsync_Should_Execute_WhenValid_When_All_Validations_Pass()
    {
        // Arrange
        var whenValidExecuted = false;
        
        var builder = ServiceValidate.Build<ExerciseDto>()
            .Ensure(() => true, ServiceError.ValidationFailed("Sync validation"))
            .EnsureAsync(
                async () =>
                {
                    await Task.Delay(1);
                    return true; // Async passes
                },
                ServiceError.ValidationFailed("Async validation"));

        // Act
        var result = await builder.MatchAsync(
            whenValid: async () =>
            {
                whenValidExecuted = true;
                await Task.Delay(1);
                return ServiceResult<ExerciseDto>.Success(new ExerciseDto { Name = "Success" });
            });

        // Assert
        whenValidExecuted.Should().BeTrue("whenValid should be executed when all validations pass");
        result.IsSuccess.Should().BeTrue();
        result.Data.Name.Should().Be("Success");
    }
}
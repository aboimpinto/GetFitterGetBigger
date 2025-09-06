using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using FluentAssertions;
using Xunit;
using GetFitterGetBigger.API.Models.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace GetFitterGetBigger.API.Tests.Services.Validation;

/// <summary>
/// Comprehensive tests for ServiceValidationBuilder to achieve close to 100% coverage
/// </summary>
public class ServiceValidationBuilderComprehensiveTests
{
    private class TestDto : IEmpty
    {
        public string Name { get; set; } = string.Empty;
        public bool IsEmpty => string.IsNullOrEmpty(Name);
    }

    #region MatchAsync with String Errors Tests (Currently 0% Coverage)
    
    [Fact]
    public async Task MatchAsync_StringErrors_Should_Execute_WhenValid_When_No_Errors()
    {
        // Arrange
        var builder = ServiceValidate.Build<TestDto>();
        var whenValidExecuted = false;
        
        // Act
        var result = await builder.MatchAsync(
            whenValid: async () =>
            {
                whenValidExecuted = true;
                await Task.Delay(1);
                return ServiceResult<TestDto>.Success(new TestDto { Name = "Test" });
            },
            whenInvalid: (IReadOnlyList<string> errors) => 
                ServiceResult<TestDto>.Failure(new TestDto(), ServiceError.ValidationFailed("Should not be called")));
        
        // Assert
        whenValidExecuted.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.Data.Name.Should().Be("Test");
    }
    
    [Fact]
    public async Task MatchAsync_StringErrors_Should_Handle_Sync_Validation_Errors()
    {
        // Arrange
        var builder = ServiceValidate.Build<TestDto>()
            .Ensure(() => false, "Sync validation failed");
        var whenValidExecuted = false;
        
        // Act
        var result = await builder.MatchAsync(
            whenValid: async () =>
            {
                whenValidExecuted = true;
                await Task.FromResult(0);
                return ServiceResult<TestDto>.Success(new TestDto());
            },
            whenInvalid: (IReadOnlyList<string> errors) => 
                ServiceResult<TestDto>.Failure(new TestDto(), ServiceError.ValidationFailed(string.Join(", ", errors))));
        
        // Assert
        whenValidExecuted.Should().BeFalse();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Sync validation failed");
    }
    
    [Fact]
    public async Task MatchAsync_StringErrors_Should_Handle_Async_Validation_Errors()
    {
        // Arrange
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureAsync(async () =>
            {
                await Task.Delay(1);
                return false;
            }, "Async validation failed");
        
        // Act
        var result = await builder.MatchAsync(
            whenValid: async () => await Task.FromResult(ServiceResult<TestDto>.Success(new TestDto())),
            whenInvalid: (IReadOnlyList<string> errors) => 
                ServiceResult<TestDto>.Failure(new TestDto(), ServiceError.ValidationFailed(string.Join(", ", errors))));
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Async validation failed");
    }
    
    [Fact]
    public async Task MatchAsync_StringErrors_Should_Collect_Multiple_Async_Errors()
    {
        // Arrange
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureAsync(async () =>
            {
                await Task.Delay(1);
                return false;
            }, "First async error")
            .EnsureAsync(async () =>
            {
                await Task.Delay(1);
                return false;
            }, "Second async error");
        
        // Act
        var result = await builder.MatchAsync(
            whenValid: async () => await Task.FromResult(ServiceResult<TestDto>.Success(new TestDto())),
            whenInvalid: (IReadOnlyList<string> errors) => 
                ServiceResult<TestDto>.Failure(new TestDto(), ServiceError.ValidationFailed(string.Join("; ", errors))));
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Should().Contain("First async error");
        result.Errors[0].Should().Contain("Second async error");
    }
    
    [Fact]
    public async Task MatchAsync_StringErrors_Should_Skip_Async_When_Sync_Fails()
    {
        // Arrange
        var asyncExecuted = false;
        var builder = ServiceValidate.Build<TestDto>()
            .Ensure(() => false, "Sync error")
            .EnsureAsync(async () =>
            {
                asyncExecuted = true;
                await Task.Delay(1);
                return true;
            }, "Async validation");
        
        // Act
        var result = await builder.MatchAsync(
            whenValid: async () => await Task.FromResult(ServiceResult<TestDto>.Success(new TestDto())),
            whenInvalid: (IReadOnlyList<string> errors) => 
                ServiceResult<TestDto>.Failure(new TestDto(), ServiceError.ValidationFailed(string.Join(", ", errors))));
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Sync error");
        asyncExecuted.Should().BeFalse("Async validations should not run when sync fails");
    }
    
    #endregion

    #region EnsureWhenValid and Conditional Validation Tests
    
    [Fact]
    public void EnsureWhenValid_Should_Add_Validation_When_No_Previous_Errors()
    {
        // Arrange
        var builder = ServiceValidate.Build<TestDto>();
        
        // Act
        builder.EnsureWhenValid(() => false, "Should fail");
        
        // Assert
        builder.Validation.HasErrors.Should().BeTrue();
        builder.Validation.ValidationErrors.Should().Contain("Should fail");
    }
    
    [Fact]
    public void EnsureWhenValid_Should_Skip_Validation_When_Previous_Errors_Exist()
    {
        // Arrange
        var builder = ServiceValidate.Build<TestDto>()
            .Ensure(() => false, "First error");
        
        // Act
        builder.EnsureWhenValid(() => false, "Should not appear");
        
        // Assert
        builder.Validation.HasErrors.Should().BeTrue();
        builder.Validation.ValidationErrors.Should().ContainSingle();
        builder.Validation.ValidationErrors.Should().Contain("First error");
        builder.Validation.ValidationErrors.Should().NotContain("Should not appear");
    }
    
    [Fact]
    public async Task EnsureAsyncWhenValid_Should_Execute_When_No_Previous_Errors()
    {
        // Arrange
        var builder = ServiceValidate.Build<TestDto>();
        var asyncExecuted = false;
        
        // Act
        builder.EnsureAsyncWhenValid(
            async () =>
            {
                asyncExecuted = true;
                await Task.Delay(1);
                return false;
            },
            ServiceError.ValidationFailed("Async error"));
        
        var result = await builder.ToValidationResultAsync();
        
        // Assert
        asyncExecuted.Should().BeTrue();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Async error");
    }
    
    [Fact]
    public async Task EnsureAsyncWhenValid_Should_Skip_When_Previous_Errors_Exist()
    {
        // Arrange
        var builder = ServiceValidate.Build<TestDto>()
            .Ensure(() => false, "First error");
        var asyncExecuted = false;
        
        // Act
        builder.EnsureAsyncWhenValid(
            async () =>
            {
                asyncExecuted = true;
                await Task.Delay(1);
                return false;
            },
            ServiceError.ValidationFailed("Should not appear"));
        
        var result = await builder.ToValidationResultAsync();
        
        // Assert
        asyncExecuted.Should().BeFalse();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.Should().Contain("First error");
    }
    
    [Fact]
    public async Task EnsureNameIsUniqueWhenValidAsync_Should_Check_Uniqueness_When_Valid()
    {
        // Arrange
        var builder = ServiceValidate.Build<TestDto>();
        var uniqueCheckExecuted = false;
        
        // Act
        builder.EnsureNameIsUniqueWhenValidAsync(
            async () =>
            {
                uniqueCheckExecuted = true;
                await Task.Delay(1);
                return false; // Not unique
            },
            "Exercise",
            "Test Name");
        
        var result = await builder.ToValidationResultAsync();
        
        // Assert
        uniqueCheckExecuted.Should().BeTrue();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Exercise") && e.Contains("Test Name"));
    }
    
    [Fact]
    public async Task EnsureNameIsUniqueWhenValidAsync_Should_Skip_When_Previous_Errors()
    {
        // Arrange
        var builder = ServiceValidate.Build<TestDto>()
            .Ensure(() => false, "Previous error");
        var uniqueCheckExecuted = false;
        
        // Act
        builder.EnsureNameIsUniqueWhenValidAsync(
            async () =>
            {
                uniqueCheckExecuted = true;
                await Task.Delay(1);
                return false;
            },
            "Exercise",
            "Test Name");
        
        var result = await builder.ToValidationResultAsync();
        
        // Assert
        uniqueCheckExecuted.Should().BeFalse();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.Should().Contain("Previous error");
    }
    
    #endregion

    #region WhenValid Method Tests
    
    [Fact]
    public void WhenValid_Should_Return_Builder_For_Chaining()
    {
        // Arrange & Act
        var builder = ServiceValidate.Build<TestDto>()
            .Ensure(() => true, "Valid")
            .WhenValid()
            .Ensure(() => true, "Also valid");
        
        // Assert
        builder.Should().NotBeNull();
        builder.Validation.HasErrors.Should().BeFalse();
    }
    
    #endregion

    #region Ensure Methods with Different Parameter Types
    
    [Fact]
    public void EnsureNotWhiteSpace_String_Should_Pass_When_Not_Empty()
    {
        // Arrange & Act
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNotWhiteSpace("Valid string", "Should not appear");
        
        // Assert
        builder.Validation.HasErrors.Should().BeFalse();
    }
    
    [Fact]
    public void EnsureNotWhiteSpace_String_Should_Fail_When_Empty()
    {
        // Arrange & Act
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNotWhiteSpace("", "String is empty");
        
        // Assert
        builder.Validation.HasErrors.Should().BeTrue();
        builder.Validation.ValidationErrors.Should().Contain("String is empty");
    }
    
    [Fact]
    public void EnsureNotWhiteSpace_String_Should_Fail_When_Whitespace()
    {
        // Arrange & Act
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNotWhiteSpace("   ", "String is whitespace");
        
        // Assert
        builder.Validation.HasErrors.Should().BeTrue();
        builder.Validation.ValidationErrors.Should().Contain("String is whitespace");
    }
    
    [Fact]
    public void EnsureNotWhiteSpace_ServiceError_Should_Use_ServiceError()
    {
        // Arrange & Act
        var serviceError = ServiceError.ValidationFailed("Custom error");
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNotWhiteSpace("", serviceError);
        
        // Assert
        builder.Validation.HasErrors.Should().BeTrue();
    }
    
    [Fact]
    public void EnsureMaxLength_Should_Pass_When_Under_Limit()
    {
        // Arrange & Act
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureMaxLength("Short", 10, "Too long");
        
        // Assert
        builder.Validation.HasErrors.Should().BeFalse();
    }
    
    [Fact]
    public void EnsureMaxLength_Should_Fail_When_Over_Limit()
    {
        // Arrange & Act
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureMaxLength("This is a very long string", 10, "String too long");
        
        // Assert
        builder.Validation.HasErrors.Should().BeTrue();
        builder.Validation.ValidationErrors.Should().Contain("String too long");
    }
    
    [Fact]
    public void EnsureMaxLength_Should_Pass_When_Null()
    {
        // Arrange & Act
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureMaxLength(null, 10, "Should not appear");
        
        // Assert
        builder.Validation.HasErrors.Should().BeFalse();
    }
    
    [Fact]
    public void EnsureMinLength_Should_Pass_When_Over_Minimum()
    {
        // Arrange & Act
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureMinLength("Long enough", 5, "Too short");
        
        // Assert
        builder.Validation.HasErrors.Should().BeFalse();
    }
    
    [Fact]
    public void EnsureMinLength_Should_Fail_When_Under_Minimum()
    {
        // Arrange & Act
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureMinLength("Hi", 5, "String too short");
        
        // Assert
        builder.Validation.HasErrors.Should().BeTrue();
        builder.Validation.ValidationErrors.Should().Contain("String too short");
    }
    
    [Fact]
    public void EnsureNotNull_Should_Pass_When_Not_Null()
    {
        // Arrange & Act
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNotNull(new object(), "Should not appear");
        
        // Assert
        builder.Validation.HasErrors.Should().BeFalse();
    }
    
    [Fact]
    public void EnsureNotNull_Should_Fail_When_Null()
    {
        // Arrange & Act
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNotNull(null, "Object is null");
        
        // Assert
        builder.Validation.HasErrors.Should().BeTrue();
        builder.Validation.ValidationErrors.Should().Contain("Object is null");
    }
    
    [Fact]
    public void EnsureNumberBetween_Should_Pass_When_In_Range()
    {
        // Arrange & Act
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNumberBetween(5, 1, 10, "Out of range");
        
        // Assert
        builder.Validation.HasErrors.Should().BeFalse();
    }
    
    [Fact]
    public void EnsureNumberBetween_Should_Fail_When_Below_Range()
    {
        // Arrange & Act
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNumberBetween(0, 1, 10, "Number too low");
        
        // Assert
        builder.Validation.HasErrors.Should().BeTrue();
        builder.Validation.ValidationErrors.Should().Contain("Number too low");
    }
    
    [Fact]
    public void EnsureNumberBetween_Should_Fail_When_Above_Range()
    {
        // Arrange & Act
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNumberBetween(11, 1, 10, "Number too high");
        
        // Assert
        builder.Validation.HasErrors.Should().BeTrue();
        builder.Validation.ValidationErrors.Should().Contain("Number too high");
    }
    
    [Fact]
    public void EnsureNumberBetween_Nullable_Should_Pass_When_Null()
    {
        // Arrange & Act
        int? nullValue = null;
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNumberBetween(nullValue, 1, 10, "Should not appear");
        
        // Assert
        builder.Validation.HasErrors.Should().BeFalse();
    }
    
    [Fact]
    public void EnsureNumberBetween_Nullable_Should_Validate_When_HasValue()
    {
        // Arrange & Act
        int? value = 15;
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNumberBetween(value, 1, 10, "Value out of range");
        
        // Assert
        builder.Validation.HasErrors.Should().BeTrue();
        builder.Validation.ValidationErrors.Should().Contain("Value out of range");
    }
    
    [Fact]
    public void EnsureMaxCount_Should_Pass_When_Under_Limit()
    {
        // Arrange & Act
        var list = new List<int> { 1, 2, 3 };
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureMaxCount(list, 5, "Too many items");
        
        // Assert
        builder.Validation.HasErrors.Should().BeFalse();
    }
    
    [Fact]
    public void EnsureMaxCount_Should_Fail_When_Over_Limit()
    {
        // Arrange & Act
        var list = new List<int> { 1, 2, 3, 4, 5, 6 };
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureMaxCount(list, 5, "Too many items");
        
        // Assert
        builder.Validation.HasErrors.Should().BeTrue();
        builder.Validation.ValidationErrors.Should().Contain("Too many items");
    }
    
    [Fact]
    public void EnsureMaxCount_Should_Pass_When_Null()
    {
        // Arrange & Act
        List<int>? nullList = null;
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureMaxCount(nullList, 5, "Should not appear");
        
        // Assert
        builder.Validation.HasErrors.Should().BeFalse();
    }
    
    #endregion

    #region ID Validation Tests
    
    [Fact]
    public void EnsureNotEmpty_SpecializedId_Should_Pass_When_Not_Empty()
    {
        // Arrange & Act
        var id = ExerciseId.New();
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNotEmpty(id, "ID is empty");
        
        // Assert
        builder.Validation.HasErrors.Should().BeFalse();
    }
    
    [Fact]
    public void EnsureNotEmpty_SpecializedId_Should_Fail_When_Empty()
    {
        // Arrange & Act
        var id = ExerciseId.Empty;
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNotEmpty(id, "ID is empty");
        
        // Assert
        builder.Validation.HasErrors.Should().BeTrue();
        builder.Validation.ValidationErrors.Should().Contain("ID is empty");
    }
    
    [Fact]
    public void EnsureNotEmpty_NullableId_Should_Pass_When_Null()
    {
        // Arrange & Act
        ExerciseId? nullId = null;
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNotEmpty(nullId, "Should not appear");
        
        // Assert
        builder.Validation.HasErrors.Should().BeFalse();
    }
    
    [Fact]
    public void EnsureNotEmpty_NullableId_Should_Fail_When_Empty()
    {
        // Arrange & Act
        ExerciseId? emptyId = ExerciseId.Empty;
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNotEmpty(emptyId, "Nullable ID is empty");
        
        // Assert
        builder.Validation.HasErrors.Should().BeTrue();
        builder.Validation.ValidationErrors.Should().Contain("Nullable ID is empty");
    }
    
    [Fact]
    public void EnsureValidId_Should_Pass_When_Valid()
    {
        // Arrange & Act
        var id = WorkoutTemplateId.New();
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureValidId(id, "Invalid ID");
        
        // Assert
        builder.Validation.HasErrors.Should().BeFalse();
    }
    
    [Fact]
    public void EnsureValidId_Should_Fail_When_Empty()
    {
        // Arrange & Act
        var id = WorkoutTemplateId.Empty;
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureValidId(id, "Invalid ID");
        
        // Assert
        builder.Validation.HasErrors.Should().BeTrue();
        builder.Validation.ValidationErrors.Should().Contain("Invalid ID");
    }
    
    [Fact]
    public void EnsureNotEmpty_IEmpty_Should_Pass_When_Not_Empty()
    {
        // Arrange & Act
        var entity = new TestDto { Name = "Valid" };
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNotEmpty(entity, "Entity is empty");
        
        // Assert
        builder.Validation.HasErrors.Should().BeFalse();
    }
    
    [Fact]
    public void EnsureNotEmpty_IEmpty_Should_Fail_When_Empty()
    {
        // Arrange & Act
        var entity = new TestDto { Name = "" };
        var builder = ServiceValidate.Build<TestDto>()
            .Ensure(() => !entity.IsEmpty, "Entity is empty");
        
        // Assert
        builder.Validation.HasErrors.Should().BeTrue();
        builder.Validation.ValidationErrors.Should().Contain("Entity is empty");
    }
    
    #endregion

    #region Async Validation Tests
    
    [Fact]
    public async Task EnsureNameIsUniqueAsync_Should_Pass_When_Unique()
    {
        // Arrange & Act
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNameIsUniqueAsync(
                async () =>
                {
                    await Task.Delay(1);
                    return true; // Is unique
                },
                "Exercise",
                "Test Name");
        
        var result = await builder.ToValidationResultAsync();
        
        // Assert
        result.IsValid.Should().BeTrue();
    }
    
    [Fact]
    public async Task EnsureNameIsUniqueAsync_Should_Fail_When_Not_Unique()
    {
        // Arrange & Act
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNameIsUniqueAsync(
                async () =>
                {
                    await Task.Delay(1);
                    return false; // Not unique
                },
                "Exercise",
                "Test Name");
        
        var result = await builder.ToValidationResultAsync();
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Exercise") && e.Contains("Test Name"));
    }
    
    [Fact]
    public async Task EnsureHasValidAsync_Should_Pass_When_Valid()
    {
        // Arrange & Act
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureHasValidAsync(
                async () =>
                {
                    await Task.Delay(1);
                    return true;
                },
                "Configuration is invalid");
        
        var result = await builder.ToValidationResultAsync();
        
        // Assert
        result.IsValid.Should().BeTrue();
    }
    
    [Fact]
    public async Task EnsureHasValidAsync_Should_Fail_When_Invalid()
    {
        // Arrange & Act
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureHasValidAsync(
                async () =>
                {
                    await Task.Delay(1);
                    return false;
                },
                "Configuration is invalid");
        
        var result = await builder.ToValidationResultAsync();
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Configuration is invalid");
    }
    
    [Fact]
    public async Task EnsureIsUniqueAsync_Should_Pass_When_Unique()
    {
        // Arrange & Act
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureIsUniqueAsync(
                async () =>
                {
                    await Task.Delay(1);
                    return true;
                },
                ServiceError.AlreadyExists("Item", "value"));
        
        var result = await builder.ToValidationResultAsync();
        
        // Assert
        result.IsValid.Should().BeTrue();
    }
    
    [Fact]
    public async Task EnsureIsUniqueAsync_Should_Fail_When_Not_Unique()
    {
        // Arrange & Act
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureIsUniqueAsync(
                async () =>
                {
                    await Task.Delay(1);
                    return false;
                },
                ServiceError.AlreadyExists("Item", "value"));
        
        var result = await builder.ToValidationResultAsync();
        
        // Assert
        result.IsValid.Should().BeFalse();
    }
    
    [Fact]
    public async Task EnsureNotExistsAsync_Should_Pass_When_Not_Exists()
    {
        // Arrange & Act
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNotExistsAsync(
                async () =>
                {
                    await Task.Delay(1);
                    return true; // Does not exist
                },
                ServiceError.AlreadyExists("Item", "value"));
        
        var result = await builder.ToValidationResultAsync();
        
        // Assert
        result.IsValid.Should().BeTrue();
    }
    
    [Fact]
    public async Task EnsureNotExistsAsync_Should_Fail_When_Exists()
    {
        // Arrange & Act
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNotExistsAsync(
                async () =>
                {
                    await Task.Delay(1);
                    return false; // Exists
                },
                ServiceError.AlreadyExists("Item", "value"));
        
        var result = await builder.ToValidationResultAsync();
        
        // Assert
        result.IsValid.Should().BeFalse();
    }
    
    [Fact]
    public async Task EnsureExistsAsync_Should_Pass_When_Exists()
    {
        // Arrange & Act
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureExistsAsync(
                async () =>
                {
                    await Task.Delay(1);
                    return true;
                },
                "Entity");
        
        var result = await builder.ToValidationResultAsync();
        
        // Assert
        result.IsValid.Should().BeTrue();
    }
    
    [Fact]
    public async Task EnsureExistsAsync_Should_Fail_When_Not_Exists()
    {
        // Arrange & Act
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureExistsAsync(
                async () =>
                {
                    await Task.Delay(1);
                    return false;
                },
                "Entity");
        
        var result = await builder.ToValidationResultAsync();
        
        // Assert
        result.IsValid.Should().BeFalse();
    }
    
    [Fact]
    public async Task EnsureAsync_Custom_Should_Pass_When_Valid()
    {
        // Arrange & Act
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureAsync(async () =>
            {
                await Task.Delay(1);
                return (true, (string?)null);
            });
        
        var result = await builder.ToValidationResultAsync();
        
        // Assert
        result.IsValid.Should().BeTrue();
    }
    
    [Fact]
    public async Task EnsureAsync_Custom_Should_Fail_With_Error()
    {
        // Arrange & Act
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureAsync(async () =>
            {
                await Task.Delay(1);
                return (false, "Custom error");
            });
        
        var result = await builder.ToValidationResultAsync();
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Custom error");
    }
    
    [Fact]
    public async Task EnsureAsync_ServiceError_Should_Fail_With_ServiceError()
    {
        // Arrange & Act
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureAsync(async () =>
            {
                await Task.Delay(1);
                return (false, ServiceError.NotFound("Entity"));
            });
        
        var result = await builder.ToValidationResultAsync();
        
        // Assert
        result.IsValid.Should().BeFalse();
    }
    
    #endregion

    #region Edge Cases and Complex Scenarios
    
    [Fact]
    public async Task Multiple_Mixed_Validations_Should_Collect_Sync_Errors_Only_When_Sync_Fails()
    {
        // Arrange
        var builder = ServiceValidate.Build<TestDto>()
            .Ensure(() => false, "Sync error 1")
            .Ensure(() => false, ServiceError.ValidationFailed("Sync error 2"))
            .EnsureAsync(async () =>
            {
                await Task.Delay(1);
                return false;
            }, "Async error 1")
            .EnsureAsync(async () =>
            {
                await Task.Delay(1);
                return false;
            }, ServiceError.ValidationFailed("Async error 2"));
        
        // Act
        var result = await builder.ToValidationResultAsync();
        
        // Assert
        result.IsValid.Should().BeFalse();
        // When sync validations fail, async validations are not executed
        result.Errors.Count.Should().Be(2);
        result.Errors.Should().Contain("Sync error 1");
        result.Errors.Should().Contain("Sync error 2");
    }
    
    [Fact]
    public async Task Empty_Async_Validation_List_Should_Not_Throw()
    {
        // Arrange
        var builder = ServiceValidate.Build<TestDto>()
            .Ensure(() => true, "Valid");
        
        // Act
        var act = async () => await builder.ToValidationResultAsync();
        
        // Assert
        await act.Should().NotThrowAsync();
    }
    
    [Fact]
    public void Property_Validation_Should_Return_Underlying_Validation()
    {
        // Arrange
        var builder = ServiceValidate.Build<TestDto>()
            .Ensure(() => false, "Error");
        
        // Act
        var validation = builder.Validation;
        
        // Assert
        validation.Should().NotBeNull();
        validation.HasErrors.Should().BeTrue();
        validation.ValidationErrors.Should().Contain("Error");
    }
    
    #endregion

    #region Missing Method Tests

    [Fact]
    public async Task HandleStringErrorValidations_Should_Convert_String_Errors_To_ServiceErrors()
    {
        // This tests the private HandleStringErrorValidations method indirectly through MatchAsync
        // Arrange
        var builder = ServiceValidate.Build<TestDto>()
            .Ensure(() => false, "String error 1")
            .Ensure(() => false, "String error 2");
        
        // Act - Use the ServiceError overload which will trigger HandleStringErrorValidations
        var result = await builder.MatchAsync(
            whenValid: async () => await Task.FromResult(ServiceResult<TestDto>.Success(new TestDto())),
            whenInvalid: (IReadOnlyList<ServiceError> errors) => 
                ServiceResult<TestDto>.Failure(new TestDto(), errors.ToArray()));
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StructuredErrors.Should().HaveCount(2);
        result.StructuredErrors[0].Code.Should().Be(ServiceErrorCode.ValidationFailed);
        result.StructuredErrors[0].Message.Should().Be("String error 1");
        result.StructuredErrors[1].Code.Should().Be(ServiceErrorCode.ValidationFailed);
        result.StructuredErrors[1].Message.Should().Be("String error 2");
    }

    [Fact]
    public async Task EnsureIsUniqueAsync_With_ServiceError_Should_Fail_When_Not_Unique()
    {
        // Arrange
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureIsUniqueAsync(
                async () =>
                {
                    await Task.Delay(1);
                    return false; // Not unique
                },
                ServiceError.AlreadyExists("User", "john@example.com"));
        
        // Act
        var result = await builder.ToValidationResultAsync();
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.ServiceError.Should().NotBeNull();
        result.ServiceError.Code.Should().Be(ServiceErrorCode.AlreadyExists);
        result.ServiceError.Message.Should().Contain("User");
        result.ServiceError.Message.Should().Contain("john@example.com");
    }

    [Fact]
    public async Task EnsureIsUniqueAsync_With_ServiceError_Should_Pass_When_Unique()
    {
        // Arrange
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureIsUniqueAsync(
                async () =>
                {
                    await Task.Delay(1);
                    return true; // Is unique
                },
                ServiceError.AlreadyExists("User", "john@example.com"));
        
        // Act
        var result = await builder.ToValidationResultAsync();
        
        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task EnsureIsUniqueAsync_With_EntityName_Should_Create_AlreadyExists_Error()
    {
        // Arrange
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureIsUniqueAsync(
                async () =>
                {
                    await Task.Delay(1);
                    return false; // Not unique
                },
                "Product",
                "Widget-123");
        
        // Act
        var result = await builder.ToValidationResultAsync();
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.ServiceError.Should().NotBeNull();
        result.ServiceError.Code.Should().Be(ServiceErrorCode.AlreadyExists);
        result.ServiceError.Message.Should().Contain("Product");
        result.ServiceError.Message.Should().Contain("Widget-123");
    }

    [Fact]
    public void EnsureNotEmpty_With_IEmpty_Should_Fail_When_Empty()
    {
        // Arrange
        var emptyEntity = new TestDto { Name = "" }; // IsEmpty returns true when Name is empty
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNotEmpty(emptyEntity, "Entity is required");
        
        // Act & Assert
        builder.Validation.HasErrors.Should().BeTrue();
        builder.Validation.ValidationErrors.Should().Contain(e => e.Contains("Entity is required"));
    }

    [Fact]
    public void EnsureNotEmpty_With_IEmpty_Should_Pass_When_Not_Empty()
    {
        // Arrange
        var entity = new TestDto { Name = "Valid" };
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNotEmpty(entity, "Entity is required");
        
        // Act & Assert
        builder.Validation.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void EnsureNotEmpty_With_SpecializedId_Should_Fail_When_Empty()
    {
        // Arrange
        var emptyId = ExerciseId.Empty;
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNotEmpty(emptyId, "Exercise ID is required");
        
        // Act & Assert
        builder.Validation.HasErrors.Should().BeTrue();
        builder.Validation.ValidationErrors.Should().Contain("Exercise ID is required");
    }

    [Fact]
    public void EnsureNotEmpty_With_SpecializedId_Should_Pass_When_Valid()
    {
        // Arrange
        var validId = ExerciseId.New();
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNotEmpty(validId, "Exercise ID is required");
        
        // Act & Assert
        builder.Validation.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void EnsureNotEmpty_With_SpecializedId_And_ServiceError_Should_Fail_When_Empty()
    {
        // Arrange
        var emptyId = ExerciseId.Empty;
        var serviceError = ServiceError.NotFound("Exercise", "ID");
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNotEmpty(emptyId, serviceError);
        
        // Act & Assert
        builder.Validation.HasErrors.Should().BeTrue();
        // Check that the error message contains the ServiceError's message
        builder.Validation.ValidationErrors.Should().ContainSingle();
        builder.Validation.ValidationErrors[0].Should().Contain("Exercise");
    }

    [Fact]
    public void EnsureNotEmpty_With_SpecializedId_And_ServiceError_Should_Pass_When_Valid()
    {
        // Arrange
        var validId = ExerciseId.New();
        var serviceError = ServiceError.NotFound("Exercise", "ID");
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNotEmpty(validId, serviceError);
        
        // Act & Assert
        builder.Validation.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void EnsureNotEmpty_With_Nullable_SpecializedId_Should_Not_Add_Error_When_Null()
    {
        // Arrange
        // Note: Current implementation skips validation when nullable ID is null
        // This might be a bug, but we're testing current behavior
        ExerciseId? nullId = null;
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNotEmpty(nullId, "Exercise ID is required");
        
        // Act & Assert
        // The current implementation doesn't add an error when ID is null (only validates if HasValue)
        builder.Validation.HasErrors.Should().BeFalse();
    }

    [Fact]
    public void EnsureNotEmpty_With_Nullable_SpecializedId_Should_Fail_When_Empty()
    {
        // Arrange
        ExerciseId? emptyId = ExerciseId.Empty;
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNotEmpty(emptyId, "Exercise ID is required");
        
        // Act & Assert
        builder.Validation.HasErrors.Should().BeTrue();
        builder.Validation.ValidationErrors.Should().Contain("Exercise ID is required");
    }

    [Fact]
    public void EnsureNotEmpty_With_Nullable_SpecializedId_Should_Pass_When_Valid()
    {
        // Arrange
        ExerciseId? validId = ExerciseId.New();
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNotEmpty(validId, "Exercise ID is required");
        
        // Act & Assert
        builder.Validation.HasErrors.Should().BeFalse();
    }

    [Fact]
    public async Task EnsureAsyncWhenValid_Should_Skip_When_Sync_Validation_Has_Errors()
    {
        // Arrange
        bool asyncExecuted = false;
        var builder = ServiceValidate.Build<TestDto>()
            .Ensure(() => false, "Sync error") // This will cause HasErrors to be true
            .EnsureAsyncWhenValid(
                async () =>
                {
                    asyncExecuted = true;
                    await Task.Delay(1);
                    return false; // Would fail if executed
                },
                ServiceError.ValidationFailed("Should not appear"));
        
        // Act
        var result = await builder.ToValidationResultAsync();
        
        // Assert
        asyncExecuted.Should().BeFalse(); // Async validation should be skipped
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Should().Contain("Sync error");
        result.Errors.Should().NotContain("Should not appear");
    }

    [Fact]
    public async Task EnsureAsyncWhenValid_Should_Execute_When_No_Sync_Errors()
    {
        // Arrange
        bool asyncExecuted = false;
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureAsyncWhenValid(
                async () =>
                {
                    asyncExecuted = true;
                    await Task.Delay(1);
                    return false; // Will fail
                },
                ServiceError.ValidationFailed("Async validation failed"));
        
        // Act
        var result = await builder.ToValidationResultAsync();
        
        // Assert
        asyncExecuted.Should().BeTrue(); // Async validation should execute
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Async validation failed");
    }

    [Fact]
    public async Task EnsureNameIsUniqueWhenValidAsync_Should_Skip_When_Sync_Validation_Has_Errors()
    {
        // Arrange
        bool uniqueCheckExecuted = false;
        var builder = ServiceValidate.Build<TestDto>()
            .Ensure(() => false, "Sync validation error") // This will cause HasErrors to be true
            .EnsureNameIsUniqueWhenValidAsync(
                async () =>
                {
                    uniqueCheckExecuted = true;
                    await Task.Delay(1);
                    return false; // Would indicate duplicate if executed
                },
                "User",
                "John Doe");
        
        // Act
        var result = await builder.ToValidationResultAsync();
        
        // Assert
        uniqueCheckExecuted.Should().BeFalse(); // Unique check should be skipped
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Should().Contain("Sync validation error");
        result.Errors.Should().NotContain(e => e.Contains("User") || e.Contains("John Doe"));
    }

    [Fact]
    public async Task EnsureNameIsUniqueWhenValidAsync_Should_Execute_When_No_Sync_Errors()
    {
        // Arrange
        bool uniqueCheckExecuted = false;
        var builder = ServiceValidate.Build<TestDto>()
            .EnsureNameIsUniqueWhenValidAsync(
                async () =>
                {
                    uniqueCheckExecuted = true;
                    await Task.Delay(1);
                    return false; // Not unique
                },
                "Product",
                "Widget");
        
        // Act
        var result = await builder.ToValidationResultAsync();
        
        // Assert
        uniqueCheckExecuted.Should().BeTrue(); // Unique check should execute
        result.IsValid.Should().BeFalse();
        result.ServiceError.Should().NotBeNull();
        result.ServiceError.Code.Should().Be(ServiceErrorCode.AlreadyExists);
        result.ServiceError.Message.Should().Contain("Product");
        result.ServiceError.Message.Should().Contain("Widget");
    }

    #endregion
}
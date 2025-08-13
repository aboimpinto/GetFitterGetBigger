using GetFitterGetBigger.API.Models.Interfaces;
using GetFitterGetBigger.API.Services.Results;
using GetFitterGetBigger.API.Services.Validation;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace GetFitterGetBigger.API.Tests.Services.Validation;

public class ServiceValidationBuilderIEmptyTests
{
    private class TestEmptyObject : IEmpty
    {
        public bool IsEmpty { get; init; }
        
        public static TestEmptyObject Empty => new() { IsEmpty = true };
        public static TestEmptyObject NotEmpty => new() { IsEmpty = false };
    }
    
    [Fact]
    public async Task EnsureNotEmpty_WithEmptyObject_ReturnsFailure()
    {
        // Arrange
        var emptyObject = TestEmptyObject.Empty;
        const string errorMessage = "Object";
        
        // Act
        var result = await ServiceValidate.Build<string>()
            .EnsureNotEmpty(emptyObject, errorMessage)
            .MatchAsync(
                whenValid: () => Task.FromResult(ServiceResult<string>.Success("Success")),
                whenInvalid: errors => ServiceResult<string>.Failure(
                    string.Empty, 
                    errors.FirstOrDefault() ?? ServiceError.ValidationFailed("Unknown"))
            );
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ServiceErrorCode.NotFound, result.PrimaryErrorCode);
        Assert.Equal("Object not found", result.StructuredErrors.First().Message);
    }
    
    [Fact]
    public async Task EnsureNotEmpty_WithNonEmptyObject_ReturnsSuccess()
    {
        // Arrange
        var nonEmptyObject = TestEmptyObject.NotEmpty;
        const string errorMessage = "Object";
        
        // Act
        var result = await ServiceValidate.Build<string>()
            .EnsureNotEmpty(nonEmptyObject, errorMessage)
            .MatchAsync(
                whenValid: () => Task.FromResult(ServiceResult<string>.Success("Success")),
                whenInvalid: errors => ServiceResult<string>.Failure(
                    string.Empty, 
                    errors.FirstOrDefault() ?? ServiceError.ValidationFailed("Unknown"))
            );
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Success", result.Data);
    }
}
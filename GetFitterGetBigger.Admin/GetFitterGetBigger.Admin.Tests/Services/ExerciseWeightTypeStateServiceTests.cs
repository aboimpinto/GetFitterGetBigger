using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.Services
{
    public class ExerciseWeightTypeStateServiceTests
    {
        private readonly Mock<IExerciseWeightTypeService> _exerciseWeightTypeServiceMock;
        private readonly ExerciseWeightTypeStateService _stateService;
        private int _stateChangeCount;

        public ExerciseWeightTypeStateServiceTests()
        {
            _exerciseWeightTypeServiceMock = new Mock<IExerciseWeightTypeService>();
            _stateService = new ExerciseWeightTypeStateService(_exerciseWeightTypeServiceMock.Object);
            _stateService.OnChange += () => _stateChangeCount++;
        }

        [Fact]
        public async Task LoadWeightTypesAsync_LoadsAndOrdersWeightTypes()
        {
            // Arrange
            var weightTypes = new List<ExerciseWeightTypeDto>
            {
                new() { Id = Guid.NewGuid(), Code = "WEIGHT_REQUIRED", Name = "Weight Required", IsActive = true, DisplayOrder = 3 },
                new() { Id = Guid.NewGuid(), Code = "BODYWEIGHT_ONLY", Name = "Bodyweight Only", IsActive = true, DisplayOrder = 1 },
                new() { Id = Guid.NewGuid(), Code = "MACHINE_WEIGHT", Name = "Machine Weight", IsActive = true, DisplayOrder = 2 },
                new() { Id = Guid.NewGuid(), Code = "INACTIVE_TYPE", Name = "Inactive", IsActive = false, DisplayOrder = 1 }
            };
            _exerciseWeightTypeServiceMock.Setup(x => x.GetWeightTypesAsync())
                .ReturnsAsync(weightTypes);

            // Act
            await _stateService.LoadWeightTypesAsync();

            // Assert
            _stateService.WeightTypes.Should().HaveCount(3); // Only active types
            _stateService.WeightTypes.First().Code.Should().Be("BODYWEIGHT_ONLY"); // DisplayOrder 1
            _stateService.WeightTypes.Last().Code.Should().Be("WEIGHT_REQUIRED"); // DisplayOrder 3
            _stateService.IsLoading.Should().BeFalse();
            _stateService.ErrorMessage.Should().BeNull();
            _stateChangeCount.Should().Be(2); // Once for loading start, once for loading end
        }

        [Fact]
        public async Task LoadWeightTypesAsync_WhenFails_SetsErrorMessage()
        {
            // Arrange
            _exerciseWeightTypeServiceMock.Setup(x => x.GetWeightTypesAsync())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            await _stateService.LoadWeightTypesAsync();

            // Assert
            _stateService.WeightTypes.Should().BeEmpty();
            _stateService.IsLoading.Should().BeFalse();
            _stateService.ErrorMessage.Should().NotBeNull();
            _stateService.ErrorMessage.Should().Contain("Failed to load exercise weight types");
            _stateChangeCount.Should().Be(2); // Once for loading start, once for error
        }

        [Fact]
        public async Task LoadWeightTypesAsync_SetsLoadingState()
        {
            // Arrange
            var weightTypes = new List<ExerciseWeightTypeDto>();
            _exerciseWeightTypeServiceMock.Setup(x => x.GetWeightTypesAsync())
                .ReturnsAsync(weightTypes);

            var loadingStates = new List<bool>();
            _stateService.OnChange += () => loadingStates.Add(_stateService.IsLoading);

            // Act
            await _stateService.LoadWeightTypesAsync();

            // Assert
            loadingStates.Should().HaveCount(2);
            loadingStates[0].Should().BeTrue(); // Loading started
            loadingStates[1].Should().BeFalse(); // Loading finished
        }

        [Fact]
        public async Task GetWeightTypeByCodeAsync_WithEmptyCache_LoadsAndReturns()
        {
            // Arrange
            var weightTypes = new List<ExerciseWeightTypeDto>
            {
                new() { Id = Guid.NewGuid(), Code = "BODYWEIGHT_ONLY", Name = "Bodyweight Only", IsActive = true, DisplayOrder = 1 }
            };
            _exerciseWeightTypeServiceMock.Setup(x => x.GetWeightTypesAsync())
                .ReturnsAsync(weightTypes);

            // Act
            var result = await _stateService.GetWeightTypeByCodeAsync("BODYWEIGHT_ONLY");

            // Assert
            result.Should().NotBeNull();
            result!.Code.Should().Be("BODYWEIGHT_ONLY");
            _exerciseWeightTypeServiceMock.Verify(x => x.GetWeightTypesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetWeightTypeByCodeAsync_WithLoadedCache_ReturnsFromCache()
        {
            // Arrange
            var weightTypes = new List<ExerciseWeightTypeDto>
            {
                new() { Id = Guid.NewGuid(), Code = "WEIGHT_REQUIRED", Name = "Weight Required", IsActive = true, DisplayOrder = 1 }
            };
            _exerciseWeightTypeServiceMock.Setup(x => x.GetWeightTypesAsync())
                .ReturnsAsync(weightTypes);

            await _stateService.LoadWeightTypesAsync();
            _exerciseWeightTypeServiceMock.Reset();

            // Act
            var result = await _stateService.GetWeightTypeByCodeAsync("WEIGHT_REQUIRED");

            // Assert
            result.Should().NotBeNull();
            result!.Code.Should().Be("WEIGHT_REQUIRED");
            _exerciseWeightTypeServiceMock.Verify(x => x.GetWeightTypesAsync(), Times.Never);
        }

        [Fact]
        public async Task GetWeightTypeByCodeAsync_WithNullOrEmpty_ReturnsNull()
        {
            // Act & Assert
            (await _stateService.GetWeightTypeByCodeAsync(null!)).Should().BeNull();
            (await _stateService.GetWeightTypeByCodeAsync("")).Should().BeNull();
            (await _stateService.GetWeightTypeByCodeAsync("   ")).Should().BeNull();
        }

        [Fact]
        public async Task GetWeightTypeByCodeAsync_CaseInsensitive_ReturnsCorrectType()
        {
            // Arrange
            var weightTypes = new List<ExerciseWeightTypeDto>
            {
                new() { Id = Guid.NewGuid(), Code = "BODYWEIGHT_ONLY", Name = "Bodyweight Only", IsActive = true, DisplayOrder = 1 }
            };
            _exerciseWeightTypeServiceMock.Setup(x => x.GetWeightTypesAsync())
                .ReturnsAsync(weightTypes);

            await _stateService.LoadWeightTypesAsync();

            // Act
            var result = await _stateService.GetWeightTypeByCodeAsync("bodyweight_only");

            // Assert
            result.Should().NotBeNull();
            result!.Code.Should().Be("BODYWEIGHT_ONLY");
        }

        [Fact]
        public async Task GetWeightTypeByIdAsync_WithEmptyId_ReturnsNull()
        {
            // Act
            var result = await _stateService.GetWeightTypeByIdAsync(Guid.Empty);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetWeightTypeByIdAsync_WithCachedData_ReturnsFromCache()
        {
            // Arrange
            var weightTypeId = Guid.NewGuid();
            var weightTypes = new List<ExerciseWeightTypeDto>
            {
                new() { Id = weightTypeId, Code = "MACHINE_WEIGHT", Name = "Machine Weight", IsActive = true, DisplayOrder = 1 }
            };
            _exerciseWeightTypeServiceMock.Setup(x => x.GetWeightTypesAsync())
                .ReturnsAsync(weightTypes);

            await _stateService.LoadWeightTypesAsync();

            // Act
            var result = await _stateService.GetWeightTypeByIdAsync(weightTypeId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(weightTypeId);
        }

        [Fact]
        public async Task GetWeightTypeByIdAsync_NotInCache_CallsApiService()
        {
            // Arrange
            var weightTypeId = Guid.NewGuid();
            var apiResult = new ExerciseWeightTypeDto 
            { 
                Id = weightTypeId, 
                Code = "API_RESULT", 
                Name = "API Result", 
                IsActive = true, 
                DisplayOrder = 1 
            };

            _exerciseWeightTypeServiceMock.Setup(x => x.GetWeightTypesAsync())
                .ReturnsAsync(new List<ExerciseWeightTypeDto>());
            _exerciseWeightTypeServiceMock.Setup(x => x.GetWeightTypeByIdAsync(weightTypeId))
                .ReturnsAsync(apiResult);

            // Act
            var result = await _stateService.GetWeightTypeByIdAsync(weightTypeId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(weightTypeId);
            result.Code.Should().Be("API_RESULT");
            _exerciseWeightTypeServiceMock.Verify(x => x.GetWeightTypeByIdAsync(weightTypeId), Times.Once);
        }

        [Fact]
        public async Task GetWeightTypeByIdAsync_ApiCallFails_SetsErrorAndReturnsNull()
        {
            // Arrange
            var weightTypeId = Guid.NewGuid();
            _exerciseWeightTypeServiceMock.Setup(x => x.GetWeightTypesAsync())
                .ReturnsAsync(new List<ExerciseWeightTypeDto>());
            _exerciseWeightTypeServiceMock.Setup(x => x.GetWeightTypeByIdAsync(weightTypeId))
                .ThrowsAsync(new Exception("API error"));

            // Act
            var result = await _stateService.GetWeightTypeByIdAsync(weightTypeId);

            // Assert
            result.Should().BeNull();
            _stateService.ErrorMessage.Should().NotBeNull();
            _stateService.ErrorMessage.Should().Contain("Failed to get weight type by ID");
        }

        [Fact]
        public async Task ValidateWeightAsync_ReturnsValidationResult()
        {
            // Act & Assert
            (await _stateService.ValidateWeightAsync("BODYWEIGHT_ONLY", null)).Should().BeTrue();
            (await _stateService.ValidateWeightAsync("BODYWEIGHT_ONLY", 0m)).Should().BeTrue();
            (await _stateService.ValidateWeightAsync("BODYWEIGHT_ONLY", 10m)).Should().BeFalse();
            (await _stateService.ValidateWeightAsync("WEIGHT_REQUIRED", null)).Should().BeFalse();
            (await _stateService.ValidateWeightAsync("WEIGHT_REQUIRED", 10m)).Should().BeTrue();
        }

        [Fact]
        public void GetValidationMessage_ReturnsCorrectMessages()
        {
            // Act & Assert
            _stateService.GetValidationMessage("BODYWEIGHT_ONLY")
                .Should().Be("This exercise uses bodyweight only. Weight cannot be specified.");
            _stateService.GetValidationMessage("WEIGHT_REQUIRED")
                .Should().Be("This exercise requires a weight to be specified.");
            _stateService.GetValidationMessage("UNKNOWN")
                .Should().Be("Unknown weight type.");
        }

        [Fact]
        public void RequiresWeightInput_ReturnsCorrectValues()
        {
            // Act & Assert
            _stateService.RequiresWeightInput("BODYWEIGHT_ONLY").Should().BeFalse();
            _stateService.RequiresWeightInput("NO_WEIGHT").Should().BeFalse();
            _stateService.RequiresWeightInput("BODYWEIGHT_OPTIONAL").Should().BeTrue();
            _stateService.RequiresWeightInput("WEIGHT_REQUIRED").Should().BeTrue();
            _stateService.RequiresWeightInput("MACHINE_WEIGHT").Should().BeTrue();
        }

        [Fact]
        public void ClearError_ClearsErrorAndNotifiesChange()
        {
            // Arrange
            _stateService.GetType()
                .GetField("_errorMessage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .SetValue(_stateService, "Some error");

            _stateChangeCount = 0;

            // Act
            _stateService.ClearError();

            // Assert
            _stateService.ErrorMessage.Should().BeNull();
            _stateChangeCount.Should().Be(1);
        }

        [Fact]
        public void ClearError_WhenNoError_DoesNotNotify()
        {
            // Arrange
            _stateChangeCount = 0;

            // Act
            _stateService.ClearError();

            // Assert
            _stateService.ErrorMessage.Should().BeNull();
            _stateChangeCount.Should().Be(0);
        }

        [Fact]
        public async Task StateService_HandlesMultipleConcurrentCalls()
        {
            // Arrange
            var weightTypes = new List<ExerciseWeightTypeDto>
            {
                new() { Id = Guid.NewGuid(), Code = "BODYWEIGHT_ONLY", Name = "Bodyweight Only", IsActive = true, DisplayOrder = 1 }
            };
            _exerciseWeightTypeServiceMock.Setup(x => x.GetWeightTypesAsync())
                .ReturnsAsync(weightTypes);

            // Act - Make multiple concurrent calls
            var tasks = new List<Task>
            {
                _stateService.LoadWeightTypesAsync(),
                _stateService.GetWeightTypeByCodeAsync("BODYWEIGHT_ONLY"),
                _stateService.GetWeightTypeByCodeAsync("WEIGHT_REQUIRED")
            };

            await Task.WhenAll(tasks);

            // Assert
            _stateService.WeightTypes.Should().HaveCount(1);
            _stateService.ErrorMessage.Should().BeNull();
        }
    }
}
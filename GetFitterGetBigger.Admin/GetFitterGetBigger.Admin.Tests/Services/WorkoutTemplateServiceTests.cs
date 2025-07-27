using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.Results;
using GetFitterGetBigger.Admin.Models.Errors;
using GetFitterGetBigger.Admin.Models.ReferenceData;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Services.DataProviders;
using GetFitterGetBigger.Admin.Builders;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.Services
{
    public class WorkoutTemplateServiceTests
    {
        private readonly Mock<IWorkoutTemplateDataProvider> _dataProviderMock;
        private readonly IMemoryCache _memoryCache;
        private readonly Mock<IGenericReferenceDataService> _referenceDataServiceMock;
        private readonly Mock<IWorkoutReferenceDataService> _workoutReferenceDataServiceMock;
        private readonly Mock<ILogger<WorkoutTemplateService>> _loggerMock;
        private readonly WorkoutTemplateService _workoutTemplateService;

        public WorkoutTemplateServiceTests()
        {
            _dataProviderMock = new Mock<IWorkoutTemplateDataProvider>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _referenceDataServiceMock = new Mock<IGenericReferenceDataService>();
            _workoutReferenceDataServiceMock = new Mock<IWorkoutReferenceDataService>();
            _loggerMock = new Mock<ILogger<WorkoutTemplateService>>();

            _workoutTemplateService = new WorkoutTemplateService(
                _dataProviderMock.Object,
                _memoryCache,
                _referenceDataServiceMock.Object,
                _workoutReferenceDataServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetWorkoutTemplatesAsync_WithValidFilter_ReturnsPagedResult()
        {
            // Arrange
            var filter = new WorkoutTemplateFilterDtoBuilder()
                .WithNamePattern("strength")
                .WithPageSize(10)
                .Build();

            var expectedData = new WorkoutTemplatePagedResultDtoBuilder()
                .WithItems(
                    new WorkoutTemplateDtoBuilder()
                        .WithName("Upper Body Strength")
                        .WithDifficulty("difficultylevel-intermediate", "Intermediate")
                        .Build(),
                    new WorkoutTemplateDtoBuilder()
                        .WithName("Lower Body Strength")
                        .WithDifficulty("difficultylevel-advanced", "Advanced")
                        .Build()
                )
                .WithTotalCount(2)
                .Build();

            _dataProviderMock.Setup(x => x.GetWorkoutTemplatesAsync(It.IsAny<WorkoutTemplateFilterDto>()))
                .ReturnsAsync(DataServiceResult<WorkoutTemplatePagedResultDto>.Success(expectedData));

            // Act
            var result = await _workoutTemplateService.GetWorkoutTemplatesAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.Items[0].Name.Should().Be("Upper Body Strength");
            
            _dataProviderMock.Verify(x => x.GetWorkoutTemplatesAsync(It.Is<WorkoutTemplateFilterDto>(f => 
                f.NamePattern == "strength" && f.PageSize == 10)), Times.Once);
        }

        [Fact]
        public async Task GetWorkoutTemplatesAsync_WithLargePageSize_CapsAt100()
        {
            // Arrange
            var filter = new WorkoutTemplateFilterDtoBuilder()
                .WithPageSize(150)
                .Build();

            var expectedData = new WorkoutTemplatePagedResultDtoBuilder().Build();
            
            _dataProviderMock.Setup(x => x.GetWorkoutTemplatesAsync(It.IsAny<WorkoutTemplateFilterDto>()))
                .ReturnsAsync(DataServiceResult<WorkoutTemplatePagedResultDto>.Success(expectedData));

            // Act
            await _workoutTemplateService.GetWorkoutTemplatesAsync(filter);

            // Assert
            _dataProviderMock.Verify(x => x.GetWorkoutTemplatesAsync(It.Is<WorkoutTemplateFilterDto>(f => 
                f.PageSize == 100)), Times.Once);
            
            _loggerMock.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("exceeds maximum")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetWorkoutTemplatesAsync_WhenDataProviderFails_ReturnsEmptyResult()
        {
            // Arrange
            var filter = new WorkoutTemplateFilterDtoBuilder().Build();
            _dataProviderMock.Setup(x => x.GetWorkoutTemplatesAsync(It.IsAny<WorkoutTemplateFilterDto>()))
                .ReturnsAsync(DataServiceResult<WorkoutTemplatePagedResultDto>.NetworkError("Connection failed"));

            // Act
            var result = await _workoutTemplateService.GetWorkoutTemplatesAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
            
            _loggerMock.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to retrieve workout templates")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetWorkoutTemplateByIdAsync_WhenNotInCache_FetchesFromDataProvider()
        {
            // Arrange
            var templateId = "template-123";
            var expectedTemplate = new WorkoutTemplateDtoBuilder()
                .WithId(templateId)
                .WithName("Test Template")
                .Build();
            
            _dataProviderMock.Setup(x => x.GetWorkoutTemplateByIdAsync(templateId))
                .ReturnsAsync(DataServiceResult<WorkoutTemplateDto>.Success(expectedTemplate));

            // Act
            var result = await _workoutTemplateService.GetWorkoutTemplateByIdAsync(templateId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(templateId);
            result.Name.Should().Be("Test Template");
            
            // Verify it was cached
            _memoryCache.TryGetValue($"workout_template_{templateId}", out WorkoutTemplateDto? cachedTemplate);
            cachedTemplate.Should().NotBeNull();
            cachedTemplate!.Id.Should().Be(templateId);
        }

        [Fact]
        public async Task GetWorkoutTemplateByIdAsync_WhenInCache_ReturnsCachedValue()
        {
            // Arrange
            var templateId = "template-123";
            var cachedTemplate = new WorkoutTemplateDtoBuilder()
                .WithId(templateId)
                .WithName("Cached Template")
                .Build();
            
            var cacheKey = $"workout_template_{templateId}";
            _memoryCache.Set(cacheKey, cachedTemplate);

            // Act
            var result = await _workoutTemplateService.GetWorkoutTemplateByIdAsync(templateId);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Cached Template");
            
            // Verify data provider was not called
            _dataProviderMock.Verify(x => x.GetWorkoutTemplateByIdAsync(It.IsAny<string>()), Times.Never);
            
            _loggerMock.Verify(x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cache HIT")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetWorkoutTemplateByIdAsync_WhenNotFound_ReturnsNull()
        {
            // Arrange
            var templateId = "nonexistent-id";
            _dataProviderMock.Setup(x => x.GetWorkoutTemplateByIdAsync(templateId))
                .ReturnsAsync(DataServiceResult<WorkoutTemplateDto>.NotFound("Template"));

            // Act
            var result = await _workoutTemplateService.GetWorkoutTemplateByIdAsync(templateId);

            // Assert
            result.Should().BeNull();
            
            _loggerMock.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreateWorkoutTemplateAsync_WithValidTemplate_ReturnsCreatedTemplate()
        {
            // Arrange
            var createDto = new CreateWorkoutTemplateDtoBuilder()
                .WithName("New Template")
                .Build();
            
            var createdTemplate = new WorkoutTemplateDtoBuilder()
                .WithId("new-id")
                .WithName("New Template")
                .Build();
            
            _dataProviderMock.Setup(x => x.CreateWorkoutTemplateAsync(It.IsAny<CreateWorkoutTemplateDto>()))
                .ReturnsAsync(DataServiceResult<WorkoutTemplateDto>.Success(createdTemplate));

            // Act
            var result = await _workoutTemplateService.CreateWorkoutTemplateAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be("new-id");
            result.Name.Should().Be("New Template");
        }

        [Fact]
        public async Task CreateWorkoutTemplateAsync_WithEmptyName_ThrowsArgumentException()
        {
            // Arrange
            var createDto = new CreateWorkoutTemplateDtoBuilder()
                .WithName("")
                .Build();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _workoutTemplateService.CreateWorkoutTemplateAsync(createDto));
        }

        [Fact]
        public async Task CreateWorkoutTemplateAsync_WithLongName_ThrowsArgumentException()
        {
            // Arrange
            var createDto = new CreateWorkoutTemplateDtoBuilder()
                .WithName(new string('a', 101))
                .Build();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                await _workoutTemplateService.CreateWorkoutTemplateAsync(createDto));
        }

        [Fact]
        public async Task CreateWorkoutTemplateAsync_WhenConflict_ThrowsInvalidOperationException()
        {
            // Arrange
            var createDto = new CreateWorkoutTemplateDtoBuilder()
                .WithName("Existing Template")
                .Build();
            
            _dataProviderMock.Setup(x => x.CreateWorkoutTemplateAsync(It.IsAny<CreateWorkoutTemplateDto>()))
                .ReturnsAsync(DataServiceResult<WorkoutTemplateDto>.Failure(new DataError(DataErrorCode.Conflict, "Template already exists")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _workoutTemplateService.CreateWorkoutTemplateAsync(createDto));
            
            exception.Message.Should().Contain("already exists");
        }

        [Fact]
        public async Task UpdateWorkoutTemplateAsync_ClearsCacheAfterUpdate()
        {
            // Arrange
            var templateId = "template-123";
            var updateDto = new UpdateWorkoutTemplateDtoBuilder()
                .WithName("Updated Template")
                .Build();
            
            var updatedTemplate = new WorkoutTemplateDtoBuilder()
                .WithId(templateId)
                .WithName("Updated Template")
                .Build();
            
            // Put something in cache first
            var cacheKey = $"workout_template_{templateId}";
            _memoryCache.Set(cacheKey, new WorkoutTemplateDtoBuilder().Build());
            
            _dataProviderMock.Setup(x => x.UpdateWorkoutTemplateAsync(templateId, It.IsAny<UpdateWorkoutTemplateDto>()))
                .ReturnsAsync(DataServiceResult<WorkoutTemplateDto>.Success(updatedTemplate));

            // Act
            var result = await _workoutTemplateService.UpdateWorkoutTemplateAsync(templateId, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Updated Template");
            
            // Verify cache was cleared
            _memoryCache.TryGetValue(cacheKey, out WorkoutTemplateDto? _).Should().BeFalse();
        }

        [Fact]
        public async Task DeleteWorkoutTemplateAsync_WhenSuccessful_ClearsCacheAndDoesNotThrow()
        {
            // Arrange
            var templateId = "template-123";
            var cacheKey = $"workout_template_{templateId}";
            _memoryCache.Set(cacheKey, new WorkoutTemplateDtoBuilder().Build());
            
            _dataProviderMock.Setup(x => x.DeleteWorkoutTemplateAsync(templateId))
                .ReturnsAsync(DataServiceResult<bool>.Success(true));

            // Act
            await _workoutTemplateService.DeleteWorkoutTemplateAsync(templateId);

            // Assert
            _memoryCache.TryGetValue(cacheKey, out WorkoutTemplateDto? _).Should().BeFalse();
            
            _loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Deleted workout template")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        // Note: SearchTemplatesByNameAsync method is not implemented in WorkoutTemplateService
        // This test should be removed or the method should be implemented

        [Fact]
        public async Task GetWorkoutStatesAsync_DelegatesToDataProvider()
        {
            // Arrange
            var expectedStates = new List<ReferenceDataDto>
            {
                new() { Id = "state-1", Value = "Draft" },
                new() { Id = "state-2", Value = "Published" }
            };
            
            _dataProviderMock.Setup(x => x.GetWorkoutStatesAsync())
                .ReturnsAsync(DataServiceResult<List<ReferenceDataDto>>.Success(expectedStates));

            // Act
            var result = await _workoutTemplateService.GetWorkoutStatesAsync();

            // Assert
            result.Should().HaveCount(2);
            result[0].Value.Should().Be("Draft");
            result[1].Value.Should().Be("Published");
        }

        [Fact]
        public async Task GetWorkoutCategoriesAsync_ConvertsFromWorkoutCategoryDto()
        {
            // Arrange
            var categories = new List<WorkoutCategoryDto>
            {
                new() { WorkoutCategoryId = "cat-1", Value = "Strength", Description = "Strength training" }
            };
            
            _workoutReferenceDataServiceMock.Setup(x => x.GetWorkoutCategoriesAsync())
                .ReturnsAsync(categories);

            // Act
            var result = await _workoutTemplateService.GetWorkoutCategoriesAsync();

            // Assert
            result.Should().HaveCount(1);
            result[0].Id.Should().Be("cat-1");
            result[0].Value.Should().Be("Strength");
            result[0].Description.Should().Be("Strength training");
        }
    }
}
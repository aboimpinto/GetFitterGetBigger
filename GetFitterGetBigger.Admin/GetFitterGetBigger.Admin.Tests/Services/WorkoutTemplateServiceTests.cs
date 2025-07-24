using System.Net;
using System.Text.Json;
using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Builders;
using GetFitterGetBigger.Admin.Tests.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.Services
{
    public class WorkoutTemplateServiceTests
    {
        private readonly MockHttpMessageHandler _httpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly Mock<IReferenceDataService> _referenceDataServiceMock;
        private readonly Mock<IWorkoutReferenceDataService> _workoutReferenceDataServiceMock;
        private readonly WorkoutTemplateService _workoutTemplateService;

        public WorkoutTemplateServiceTests()
        {
            _httpMessageHandler = new MockHttpMessageHandler();
            _httpClient = new HttpClient(_httpMessageHandler)
            {
                BaseAddress = new Uri("http://localhost:5214")
            };
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _referenceDataServiceMock = new Mock<IReferenceDataService>();
            _workoutReferenceDataServiceMock = new Mock<IWorkoutReferenceDataService>();

            _workoutTemplateService = new WorkoutTemplateService(
                _httpClient,
                _memoryCache,
                _referenceDataServiceMock.Object,
                _workoutReferenceDataServiceMock.Object);
        }

        [Fact]
        public async Task GetWorkoutTemplatesAsync_WithFilter_ReturnsPagedResult()
        {
            // Arrange
            var filter = new WorkoutTemplateFilterDtoBuilder()
                .WithNamePattern("strength")
                .WithPageSize(10)
                .Build();

            var expectedResult = new WorkoutTemplatePagedResultDtoBuilder()
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
                .WithPageNumber(1)
                .WithPageSize(10)
                .Build();

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedResult);

            // Act
            var result = await _workoutTemplateService.GetWorkoutTemplatesAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            _httpMessageHandler.VerifyRequest(request =>
                request.RequestUri!.PathAndQuery.Contains("namePattern=strength"));
        }

        [Fact]
        public async Task GetWorkoutTemplateByIdAsync_WhenNotCached_FetchesFromApiAndCaches()
        {
            // Arrange
            var templateId = "workouttemplate-03000001-0000-0000-0000-000000000001";
            var expectedTemplate = new WorkoutTemplateDtoBuilder()
                .WithId(templateId)
                .WithName("Upper Body Strength")
                .WithDescription("Focus on compound movements")
                .Build();

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedTemplate);

            // Act
            var result = await _workoutTemplateService.GetWorkoutTemplateByIdAsync(templateId);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("Upper Body Strength");

            // Verify it's cached
            _memoryCache.TryGetValue($"workout_template_{templateId}", out WorkoutTemplateDto? cached).Should().BeTrue();
            cached.Should().NotBeNull();
        }

        [Fact]
        public async Task GetWorkoutTemplateByIdAsync_WhenCached_ReturnsCachedValue()
        {
            // Arrange
            var templateId = "workouttemplate-03000001-0000-0000-0000-000000000001";
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
            _httpMessageHandler.VerifyNoRequests();
        }

        [Fact]
        public async Task CreateWorkoutTemplateAsync_WithValidData_ReturnsCreatedTemplate()
        {
            // Arrange
            var createDto = new CreateWorkoutTemplateDtoBuilder()
                .WithName("New Workout Template")
                .WithDescription("Test description")
                .Build();

            var expectedResponse = new WorkoutTemplateDtoBuilder()
                .WithName("New Workout Template")
                .WithDescription("Test description")
                .Build();

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedResponse);

            // Act
            var result = await _workoutTemplateService.CreateWorkoutTemplateAsync(createDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("New Workout Template");
            _httpMessageHandler.VerifyRequest(request =>
                request.Method == HttpMethod.Post &&
                request.RequestUri!.PathAndQuery == "/api/workout-templates");
        }

        [Fact]
        public async Task UpdateWorkoutTemplateAsync_WithValidData_ReturnsUpdatedTemplateAndClearsCache()
        {
            // Arrange
            var templateId = "workouttemplate-03000001-0000-0000-0000-000000000001";
            var updateDto = new UpdateWorkoutTemplateDto
            {
                Name = "Updated Template",
                Description = "Updated description",
                CategoryId = "workoutcategory-20000002-2000-4000-8000-200000000001",
                DifficultyId = "difficultylevel-9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a",
                EstimatedDurationMinutes = 45,
                IsPublic = true
            };

            var expectedResponse = new WorkoutTemplateDtoBuilder()
                .WithId(templateId)
                .WithName("Updated Template")
                .WithDescription("Updated description")
                .Build();

            // Cache an old version
            var cacheKey = $"workout_template_{templateId}";
            _memoryCache.Set(cacheKey, new WorkoutTemplateDtoBuilder().WithName("Old Name").Build());

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedResponse);

            // Act
            var result = await _workoutTemplateService.UpdateWorkoutTemplateAsync(templateId, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Updated Template");
            
            // Verify cache was cleared
            _memoryCache.TryGetValue(cacheKey, out WorkoutTemplateDto? cached).Should().BeFalse();
        }

        [Fact]
        public async Task DeleteWorkoutTemplateAsync_WithValidId_DeletesSuccessfullyAndClearsCache()
        {
            // Arrange
            var templateId = "workouttemplate-03000001-0000-0000-0000-000000000001";
            
            // Cache a template
            var cacheKey = $"workout_template_{templateId}";
            _memoryCache.Set(cacheKey, new WorkoutTemplateDtoBuilder().Build());

            _httpMessageHandler.SetupResponse(HttpStatusCode.NoContent);

            // Act
            await _workoutTemplateService.DeleteWorkoutTemplateAsync(templateId);

            // Assert
            _httpMessageHandler.VerifyRequest(request =>
                request.Method == HttpMethod.Delete &&
                request.RequestUri!.PathAndQuery == $"/api/workout-templates/{templateId}");
            
            // Verify cache was cleared
            _memoryCache.TryGetValue(cacheKey, out WorkoutTemplateDto? cached).Should().BeFalse();
        }

        [Fact]
        public async Task ChangeWorkoutTemplateStateAsync_WithValidData_ReturnsUpdatedTemplateAndClearsCache()
        {
            // Arrange
            var templateId = "workouttemplate-03000001-0000-0000-0000-000000000001";
            var changeStateDto = new ChangeWorkoutStateDto
            {
                NewStateId = "workoutstate-02000002-0000-0000-0000-000000000002"
            };

            var expectedResponse = new WorkoutTemplateDtoBuilder()
                .WithId(templateId)
                .WithWorkoutState("workoutstate-02000002-0000-0000-0000-000000000002", "PRODUCTION")
                .Build();

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedResponse);

            // Act
            var result = await _workoutTemplateService.ChangeWorkoutTemplateStateAsync(templateId, changeStateDto);

            // Assert
            result.Should().NotBeNull();
            result.WorkoutState.Value.Should().Be("PRODUCTION");
            _httpMessageHandler.VerifyRequest(request =>
                request.Method == HttpMethod.Put &&
                request.RequestUri!.PathAndQuery == $"/api/workout-templates/{templateId}/state");
        }

        [Fact]
        public async Task DuplicateWorkoutTemplateAsync_WithValidData_ReturnsNewTemplate()
        {
            // Arrange
            var templateId = "workouttemplate-03000001-0000-0000-0000-000000000001";
            var duplicateDto = new DuplicateWorkoutTemplateDto
            {
                NewName = "Copy of Original Template"
            };

            var expectedResponse = new WorkoutTemplateDtoBuilder()
                .WithId("workouttemplate-03000001-0000-0000-0000-000000000002")
                .WithName("Copy of Original Template")
                .WithWorkoutState("workoutstate-02000001-0000-0000-0000-000000000001", "DRAFT")
                .Build();

            _httpMessageHandler.SetupResponse(HttpStatusCode.Created, expectedResponse);

            // Act
            var result = await _workoutTemplateService.DuplicateWorkoutTemplateAsync(templateId, duplicateDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Copy of Original Template");
            result.WorkoutState.Value.Should().Be("DRAFT");
        }

        [Fact]
        public async Task CheckTemplateNameExistsAsync_WithExistingName_ReturnsTrue()
        {
            // Arrange
            var templateName = "Existing Template";
            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, true);

            // Act
            var result = await _workoutTemplateService.CheckTemplateNameExistsAsync(templateName);

            // Assert
            result.Should().BeTrue();
            _httpMessageHandler.VerifyRequest(request =>
                request.RequestUri!.PathAndQuery.Contains("name=Existing+Template"));
        }

        [Fact]
        public async Task GetWorkoutCategoriesAsync_WhenNotCached_FetchesFromServiceAndReturnsData()
        {
            // Arrange
            var expectedCategories = new List<WorkoutCategoryDto>
            {
                new WorkoutCategoryDto 
                { 
                    WorkoutCategoryId = "cat-1", 
                    Value = "Upper Body", 
                    Description = "Upper body workouts",
                    Icon = null,
                    Color = null,
                    PrimaryMuscleGroups = null,
                    DisplayOrder = 1,
                    IsActive = true
                },
                new WorkoutCategoryDto 
                { 
                    WorkoutCategoryId = "cat-2", 
                    Value = "Lower Body", 
                    Description = "Lower body workouts",
                    Icon = null,
                    Color = null,
                    PrimaryMuscleGroups = null,
                    DisplayOrder = 2,
                    IsActive = true
                }
            };

            _workoutReferenceDataServiceMock.Setup(x => x.GetWorkoutCategoriesAsync())
                .ReturnsAsync(expectedCategories);

            // Act
            var result = await _workoutTemplateService.GetWorkoutCategoriesAsync();

            // Assert
            result.Should().HaveCount(2);
            result.First().Value.Should().Be("Upper Body");
            result.First().Id.Should().Be("cat-1");
            result.First().Description.Should().Be("Upper body workouts");
            
            // Verify service was called
            _workoutReferenceDataServiceMock.Verify(x => x.GetWorkoutCategoriesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetDifficultyLevelsAsync_FetchesFromReferenceDataService()
        {
            // Arrange
            var expectedDifficulties = new List<ReferenceDataDto>
            {
                new ReferenceDataDto { Id = "diff-1", Value = "Beginner", Description = "For beginners" },
                new ReferenceDataDto { Id = "diff-2", Value = "Intermediate", Description = "For intermediate users" },
                new ReferenceDataDto { Id = "diff-3", Value = "Advanced", Description = "For advanced users" }
            };

            _referenceDataServiceMock.Setup(x => x.GetDifficultyLevelsAsync())
                .ReturnsAsync(expectedDifficulties);

            // Act
            var result = await _workoutTemplateService.GetDifficultyLevelsAsync();

            // Assert
            result.Should().HaveCount(3);
            result.First().Value.Should().Be("Beginner");
            result.Should().BeEquivalentTo(expectedDifficulties);
            
            // Verify service was called
            _referenceDataServiceMock.Verify(x => x.GetDifficultyLevelsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetWorkoutObjectivesAsync_FetchesFromWorkoutReferenceDataService()
        {
            // Arrange
            var expectedObjectives = new List<ReferenceDataDto>
            {
                new ReferenceDataDto { Id = "obj-1", Value = "Muscle Gain", Description = "Build muscle mass" },
                new ReferenceDataDto { Id = "obj-2", Value = "Fat Loss", Description = "Lose body fat" },
                new ReferenceDataDto { Id = "obj-3", Value = "Strength", Description = "Increase strength" },
                new ReferenceDataDto { Id = "obj-4", Value = "Endurance", Description = "Improve endurance" }
            };

            _workoutReferenceDataServiceMock.Setup(x => x.GetWorkoutObjectivesAsync())
                .ReturnsAsync(expectedObjectives);

            // Act
            var result = await _workoutTemplateService.GetWorkoutObjectivesAsync();

            // Assert
            result.Should().HaveCount(4);
            result.First().Value.Should().Be("Muscle Gain");
            result.Should().BeEquivalentTo(expectedObjectives);
            
            // Verify service was called
            _workoutReferenceDataServiceMock.Verify(x => x.GetWorkoutObjectivesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetWorkoutStatesAsync_WhenNotCached_FetchesFromApiAndCaches()
        {
            // Arrange
            var expectedStates = new List<ReferenceDataDto>
            {
                new ReferenceDataDto { Id = "state-1", Value = "DRAFT", Description = "Draft state" },
                new ReferenceDataDto { Id = "state-2", Value = "PRODUCTION", Description = "Production state" },
                new ReferenceDataDto { Id = "state-3", Value = "ARCHIVED", Description = "Archived state" }
            };

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedStates);

            // Act
            var result = await _workoutTemplateService.GetWorkoutStatesAsync();

            // Assert
            result.Should().HaveCount(3);
            result.First().Value.Should().Be("DRAFT");
            
            // Verify API was called
            _httpMessageHandler.VerifyRequest(request =>
                request.RequestUri!.PathAndQuery == "/api/workout-states");

            // Verify it's cached
            _memoryCache.TryGetValue("workout_states", out List<ReferenceDataDto>? cached).Should().BeTrue();
            cached.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetWorkoutStatesAsync_WhenCached_ReturnsCachedValue()
        {
            // Arrange
            var cachedStates = new List<ReferenceDataDto>
            {
                new ReferenceDataDto { Id = "state-1", Value = "CACHED_DRAFT", Description = "Cached draft" },
                new ReferenceDataDto { Id = "state-2", Value = "CACHED_PRODUCTION", Description = "Cached production" }
            };

            _memoryCache.Set("workout_states", cachedStates);

            // Act
            var result = await _workoutTemplateService.GetWorkoutStatesAsync();

            // Assert
            result.Should().HaveCount(2);
            result.First().Value.Should().Be("CACHED_DRAFT");
            
            // Verify no API call was made
            _httpMessageHandler.VerifyNoRequests();
        }

        [Fact]
        public async Task GetWorkoutTemplatesAsync_WithApiError_ThrowsHttpRequestException()
        {
            // Arrange
            var filter = new WorkoutTemplateFilterDtoBuilder().Build();
            _httpMessageHandler.SetupResponse(HttpStatusCode.InternalServerError, "Internal Server Error");

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(async () =>
                await _workoutTemplateService.GetWorkoutTemplatesAsync(filter));
        }

        [Fact]
        public async Task SearchTemplatesByNameAsync_WithPattern_ReturnsMatchingTemplates()
        {
            // Arrange
            var namePattern = "strength";
            var expectedTemplates = new List<WorkoutTemplateDto>
            {
                new WorkoutTemplateDtoBuilder().WithName("Upper Body Strength").Build(),
                new WorkoutTemplateDtoBuilder().WithName("Core Strength").Build()
            };

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedTemplates);

            // Act
            var result = await _workoutTemplateService.SearchTemplatesByNameAsync(namePattern);

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllSatisfy(t => t.Name.Should().Contain("Strength"));
            _httpMessageHandler.VerifyRequest(request =>
                request.RequestUri!.PathAndQuery.Contains("namePattern=strength"));
        }

        [Fact]
        public async Task GetTemplateExercisesAsync_WithValidTemplateId_ReturnsExercises()
        {
            // Arrange
            var templateId = "workouttemplate-03000001-0000-0000-0000-000000000001";
            var expectedExercises = new List<WorkoutTemplateExerciseDto>
            {
                new WorkoutTemplateExerciseDto
                {
                    ExerciseId = "exercise-01000001-0000-0000-0000-000000000001",
                    ExerciseName = "Bench Press",
                    OrderIndex = 1,
                    Sets = 3,
                    TargetReps = "8-10",
                    RestSeconds = 90,
                    Notes = "Focus on form"
                },
                new WorkoutTemplateExerciseDto
                {
                    ExerciseId = "exercise-01000001-0000-0000-0000-000000000002",
                    ExerciseName = "Incline Dumbbell Press",
                    OrderIndex = 2,
                    Sets = 3,
                    TargetReps = "10-12",
                    RestSeconds = 60
                }
            };

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedExercises);

            // Act
            var result = await _workoutTemplateService.GetTemplateExercisesAsync(templateId);

            // Assert
            result.Should().HaveCount(2);
            result.First().ExerciseName.Should().Be("Bench Press");
            result.Last().ExerciseName.Should().Be("Incline Dumbbell Press");
        }
    }
}
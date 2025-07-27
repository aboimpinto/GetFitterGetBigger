using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Results;
using GetFitterGetBigger.Admin.Models.Errors;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services.DataProviders;
using GetFitterGetBigger.Admin.Builders;
using GetFitterGetBigger.Admin.Tests.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.Services.DataProviders
{
    public class HttpWorkoutTemplateDataProviderTests
    {
        private readonly MockHttpMessageHandler _httpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;
        private readonly Mock<ILogger<HttpWorkoutTemplateDataProvider>> _loggerMock;
        private readonly HttpWorkoutTemplateDataProvider _dataProvider;

        public HttpWorkoutTemplateDataProviderTests()
        {
            _httpMessageHandler = new MockHttpMessageHandler();
            _httpClient = new HttpClient(_httpMessageHandler)
            {
                BaseAddress = new Uri("http://localhost:5214")
            };
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _loggerMock = new Mock<ILogger<HttpWorkoutTemplateDataProvider>>();

            _dataProvider = new HttpWorkoutTemplateDataProvider(
                _httpClient,
                _memoryCache,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetWorkoutTemplatesAsync_WithSuccessfulResponse_ReturnsSuccessResult()
        {
            // Arrange
            var filter = new WorkoutTemplateFilterDtoBuilder()
                .WithNamePattern("strength")
                .WithPageSize(10)
                .Build();

            var expectedData = new WorkoutTemplatePagedResultDtoBuilder()
                .WithItems(new WorkoutTemplateDtoBuilder().WithName("Test Template").Build())
                .WithTotalCount(1)
                .Build();

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedData);

            // Act
            var result = await _dataProvider.GetWorkoutTemplatesAsync(filter);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Items.Should().HaveCount(1);
            result.Data.Items[0].Name.Should().Be("Test Template");
        }

        [Fact]
        public async Task GetWorkoutTemplatesAsync_With404Response_ReturnsNotFoundFailure()
        {
            // Arrange
            var filter = new WorkoutTemplateFilterDtoBuilder().Build();
            _httpMessageHandler.SetupResponse(HttpStatusCode.NotFound, "Not found");

            // Act
            var result = await _dataProvider.GetWorkoutTemplatesAsync(filter);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.IsNotFound.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Code.Should().Be(DataErrorCode.NotFound);
            result.Errors[0].Message.Should().Be("Resource not found");
        }

        [Fact]
        public async Task GetWorkoutTemplatesAsync_WithBadRequestResponse_ReturnsBadRequestFailure()
        {
            // Arrange
            var filter = new WorkoutTemplateFilterDtoBuilder().Build();
            _httpMessageHandler.SetupResponse(HttpStatusCode.BadRequest, "Invalid request");

            // Act
            var result = await _dataProvider.GetWorkoutTemplatesAsync(filter);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.IsBadRequest.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Code.Should().Be(DataErrorCode.BadRequest);
            result.Errors[0].Message.Should().Be("Invalid request");
        }

        [Fact]
        public async Task GetWorkoutTemplatesAsync_WithUnauthorizedResponse_ReturnsUnauthorizedFailure()
        {
            // Arrange
            var filter = new WorkoutTemplateFilterDtoBuilder().Build();
            _httpMessageHandler.SetupResponse(HttpStatusCode.Unauthorized, "Authentication required");

            // Act
            var result = await _dataProvider.GetWorkoutTemplatesAsync(filter);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.IsUnauthorized.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Code.Should().Be(DataErrorCode.Unauthorized);
            result.Errors[0].Message.Should().Be("Unauthorized access");
        }

        [Fact]
        public async Task GetWorkoutTemplateByIdAsync_WithSuccessfulResponse_ReturnsSuccessResult()
        {
            // Arrange
            var templateId = "template-123";
            var expectedTemplate = new WorkoutTemplateDtoBuilder()
                .WithId(templateId)
                .WithName("Test Template")
                .Build();

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedTemplate);

            // Act
            var result = await _dataProvider.GetWorkoutTemplateByIdAsync(templateId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(templateId);
            result.Data.Name.Should().Be("Test Template");
        }

        [Fact]
        public async Task CreateWorkoutTemplateAsync_WithSuccessfulResponse_ReturnsCreatedTemplate()
        {
            // Arrange
            var createDto = new CreateWorkoutTemplateDtoBuilder()
                .WithName("New Template")
                .Build();

            var createdTemplate = new WorkoutTemplateDtoBuilder()
                .WithId("new-id")
                .WithName("New Template")
                .Build();

            _httpMessageHandler.SetupResponse(HttpStatusCode.Created, createdTemplate);

            // Act
            var result = await _dataProvider.CreateWorkoutTemplateAsync(createDto);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be("new-id");
        }

        [Fact]
        public async Task CreateWorkoutTemplateAsync_WithConflictResponse_ReturnsConflictFailure()
        {
            // Arrange
            var createDto = new CreateWorkoutTemplateDtoBuilder().Build();
            _httpMessageHandler.SetupResponse(HttpStatusCode.Conflict, "Template already exists");

            // Act
            var result = await _dataProvider.CreateWorkoutTemplateAsync(createDto);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].Code.Should().Be(DataErrorCode.Conflict);
            result.Errors[0].Message.Should().Be("Resource conflict");
        }

        [Fact]
        public async Task DeleteWorkoutTemplateAsync_WithSuccessfulResponse_ReturnsSuccess()
        {
            // Arrange
            var templateId = "template-123";
            _httpMessageHandler.SetupResponse(HttpStatusCode.NoContent);

            // Act
            var result = await _dataProvider.DeleteWorkoutTemplateAsync(templateId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();
        }


        [Fact]
        public async Task BuildQueryString_WithAllFilters_BuildsCorrectQuery()
        {
            // Arrange
            var filter = new WorkoutTemplateFilterDtoBuilder()
                .WithPage(2)
                .WithPageSize(20)
                .WithNamePattern("test pattern")
                .WithCategoryId("cat-123")
                .WithDifficultyId("diff-456")
                .WithStateId("state-789")
                .WithIsPublic(true)
                .Build();

            _httpMessageHandler.SetupResponse(HttpStatusCode.OK, new WorkoutTemplatePagedResultDtoBuilder().Build());

            // Act
            await _dataProvider.GetWorkoutTemplatesAsync(filter);

            // Assert
            _httpMessageHandler.Requests.Should().HaveCount(1);
            var request = _httpMessageHandler.Requests[0];
            request.RequestUri.Should().NotBeNull();
            var query = request.RequestUri!.Query;
            query.Should().Contain("page=2");
            query.Should().Contain("pageSize=20");
            query.Should().Contain("namePattern=");
            query.Should().Contain("test");
            query.Should().Contain("pattern");
            query.Should().Contain("categoryId=cat-123");
            query.Should().Contain("difficultyId=diff-456");
            query.Should().Contain("stateId=state-789");
            query.Should().Contain("isPublic=true");
        }

        // Note: SearchTemplatesByNameAsync is not implemented in IWorkoutTemplateDataProvider
        // This test should be removed or the method should be added to the interface
        // [Fact]
        // public async Task SearchTemplatesByNameAsync_EncodesNamePattern()
        // {
        //     // Arrange
        //     var namePattern = "test & special chars";
        //     
        //     _httpMessageHandler.SetupResponse(HttpStatusCode.OK, new List<WorkoutTemplateDto>());
        //
        //     // Act
        //     await _dataProvider.SearchTemplatesByNameAsync(namePattern);
        //
        //     // Assert
        //     _httpMessageHandler.Requests.Should().HaveCount(1);
        //     var request = _httpMessageHandler.Requests[0];
        //     request.RequestUri.Should().NotBeNull();
        //     request.RequestUri!.PathAndQuery.Should().Contain("api/workout-templates/search");
        //     request.RequestUri.Query.Should().Contain("namePattern=");
        //     // URL encoding may vary, so check that it contains the pattern
        //     request.RequestUri.Query.Should().Contain("test");
        //     request.RequestUri.Query.Should().Contain("special");
        //     request.RequestUri.Query.Should().Contain("chars");
        // }

        // Note: GetTemplatesByCategoryAsync is not implemented in IWorkoutTemplateDataProvider
        // This test should be removed or the method should be added to the interface
        // [Fact]
        // public async Task GetTemplatesByCategoryAsync_UsesCorrectEndpoint()
        // {
        //     // Arrange
        //     var categoryId = "cat-123";
        //     var expectedUrl = $"/api/workout-templates/filter/category/{categoryId}";
        //     
        //     _httpMessageHandler.SetupResponse(HttpStatusCode.OK, new List<WorkoutTemplateDto>());
        //
        //     // Act
        //     await _dataProvider.GetTemplatesByCategoryAsync(categoryId);
        //
        //     // Assert
        //     _httpMessageHandler.VerifyRequest(req => 
        //         req.RequestUri != null && req.RequestUri.PathAndQuery == expectedUrl);
        // }

        // Note: GetTemplatesByStateAsync is not implemented in IWorkoutTemplateDataProvider
        // This test should be removed or the method should be added to the interface
        // [Fact]
        // public async Task GetTemplatesByStateAsync_UsesCorrectEndpoint()
        // {
        //     // Arrange
        //     var stateId = "state-123";
        //     var expectedUrl = $"/api/workout-templates/by-state/{stateId}";
        //     
        //     _httpMessageHandler.SetupResponse(HttpStatusCode.OK, new List<WorkoutTemplateDto>());
        //
        //     // Act
        //     await _dataProvider.GetTemplatesByStateAsync(stateId);
        //
        //     // Assert
        //     _httpMessageHandler.VerifyRequest(req => 
        //         req.RequestUri != null && req.RequestUri.PathAndQuery == expectedUrl);
        // }
    }
}
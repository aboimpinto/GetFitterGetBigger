using System.Net;
using FluentAssertions;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Tests.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.Services;

public class WorkoutReferenceDataServiceTests
{
    private readonly MockHttpMessageHandler _httpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly Mock<ILogger<WorkoutReferenceDataService>> _loggerMock;
    private readonly WorkoutReferenceDataService _service;

    public WorkoutReferenceDataServiceTests()
    {
        _httpMessageHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_httpMessageHandler)
        {
            BaseAddress = new Uri("http://localhost:5214")
        };
        _cache = new MemoryCache(new MemoryCacheOptions());
        _loggerMock = new Mock<ILogger<WorkoutReferenceDataService>>();
        _service = new WorkoutReferenceDataService(_httpClient, _cache, _loggerMock.Object);
    }

    [Fact]
    public async Task GetWorkoutObjectivesAsync_Success_ReturnsObjectives()
    {
        // Arrange
        var expectedObjectives = new List<ReferenceDataDto>
        {
            new() { Id = "obj-1", Value = "Strength", Description = "Build muscle strength" },
            new() { Id = "obj-2", Value = "Endurance", Description = "Improve endurance" }
        };

        _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedObjectives);

        // Act
        var result = await _service.GetWorkoutObjectivesAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Value.Should().Be("Strength");
        result[1].Value.Should().Be("Endurance");
    }

    [Fact]
    public async Task GetWorkoutObjectivesAsync_UsesCache_OnSecondCall()
    {
        // Arrange
        var objectives = new List<ReferenceDataDto>
        {
            new() { Id = "obj-1", Value = "Strength", Description = "Build muscle strength" }
        };

        _httpMessageHandler.SetupResponse(HttpStatusCode.OK, objectives);

        // Act
        var firstCall = await _service.GetWorkoutObjectivesAsync();
        var secondCall = await _service.GetWorkoutObjectivesAsync();

        // Assert
        firstCall.Should().BeEquivalentTo(secondCall);
        _httpMessageHandler.Requests.Should().HaveCount(1); // Only one HTTP call should be made
    }

    [Fact]
    public async Task GetWorkoutCategoriesAsync_Success_ReturnsCategories()
    {
        // Arrange
        var expectedResponse = new WorkoutCategoriesResponseDto
        {
            WorkoutCategories = new List<WorkoutCategoryDto>
            {
                new()
                {
                    WorkoutCategoryId = "cat-1",
                    Value = "HIIT",
                    Description = "High Intensity Interval Training",
                    Icon = "timer",
                    Color = "#FF5733",
                    PrimaryMuscleGroups = "Full Body",
                    DisplayOrder = 1,
                    IsActive = true
                }
            }
        };

        _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedResponse);

        // Act
        var result = await _service.GetWorkoutCategoriesAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Value.Should().Be("HIIT");
        result[0].Icon.Should().Be("timer");
        result[0].Color.Should().Be("#FF5733");
    }

    [Fact]
    public async Task GetExecutionProtocolsAsync_WithExecutionProtocolDto_Success()
    {
        // Arrange
        var expectedProtocols = new List<ExecutionProtocolDto>
        {
            new()
            {
                ExecutionProtocolId = "proto-1",
                Code = "STANDARD",
                Value = "Standard",
                Description = "Standard protocol",
                TimeBase = false,
                RepBase = true,
                RestPattern = "Fixed",
                IntensityLevel = "Medium",
                DisplayOrder = 1,
                IsActive = true
            }
        };

        _httpMessageHandler.SetupResponse(HttpStatusCode.OK, expectedProtocols);

        // Act
        var result = await _service.GetExecutionProtocolsAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Code.Should().Be("STANDARD");
        result[0].RestPattern.Should().Be("Fixed");
    }

    [Fact]
    public async Task GetWorkoutObjectiveByIdAsync_ExistingId_ReturnsObjective()
    {
        // Arrange
        var objectives = new List<ReferenceDataDto>
        {
            new() { Id = "obj-1", Value = "Strength", Description = "Build muscle strength" },
            new() { Id = "obj-2", Value = "Endurance", Description = "Improve endurance" }
        };

        _httpMessageHandler.SetupResponse(HttpStatusCode.OK, objectives);

        // Act
        var result = await _service.GetWorkoutObjectiveByIdAsync("obj-2");

        // Assert
        result.Should().NotBeNull();
        result!.Value.Should().Be("Endurance");
    }

    [Fact]
    public async Task GetWorkoutObjectiveByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var objectives = new List<ReferenceDataDto>
        {
            new() { Id = "obj-1", Value = "Strength", Description = "Build muscle strength" }
        };

        _httpMessageHandler.SetupResponse(HttpStatusCode.OK, objectives);

        // Act
        var result = await _service.GetWorkoutObjectiveByIdAsync("non-existing");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetExecutionProtocolByCodeAsync_Success_ReturnsProtocol()
    {
        // Arrange
        var protocol = new ExecutionProtocolDto
        {
            ExecutionProtocolId = "proto-1",
            Code = "AMRAP",
            Value = "As Many Reps As Possible",
            Description = "Complete as many reps as possible",
            DisplayOrder = 1,
            IsActive = true,
            TimeBase = true,
            RepBase = false
        };

        _httpMessageHandler.SetupResponse(HttpStatusCode.OK, protocol);

        // Act
        var result = await _service.GetExecutionProtocolByCodeAsync("AMRAP");

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Be("AMRAP");
        result.TimeBase.Should().BeTrue();
    }

    [Fact]
    public async Task GetExecutionProtocolByCodeAsync_NotFound_ReturnsNull()
    {
        // Arrange
        _httpMessageHandler.SetupResponse(HttpStatusCode.NotFound);

        // Act
        var result = await _service.GetExecutionProtocolByCodeAsync("INVALID");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetWorkoutCategoriesAsync_EmptyResponse_ReturnsEmptyList()
    {
        // Arrange
        var emptyResponse = new WorkoutCategoriesResponseDto();

        _httpMessageHandler.SetupResponse(HttpStatusCode.OK, emptyResponse);

        // Act
        var result = await _service.GetWorkoutCategoriesAsync();

        // Assert
        result.Should().BeEmpty();
    }
}
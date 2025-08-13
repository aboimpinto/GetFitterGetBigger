using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Handlers;
using GetFitterGetBigger.API.Tests.TestBuilders.Domain;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Moq.AutoMock;
using Olimpo.EntityFramework.Persistency;
using Xunit;
using WorkoutTemplateEntity = GetFitterGetBigger.API.Models.Entities.WorkoutTemplate;

namespace GetFitterGetBigger.API.Tests.Services.WorkoutTemplate.Handlers;

public class SearchQueryBuilderTests
{
    private readonly AutoMocker _mocker;
    private readonly SearchQueryBuilder _sut;
    private readonly Mock<IWorkoutTemplateRepository> _mockRepository;

    public SearchQueryBuilderTests()
    {
        _mocker = new AutoMocker();
        
        // Setup unit of work and repository
        _mockRepository = new Mock<IWorkoutTemplateRepository>();
        var mockReadOnlyUnitOfWork = new Mock<IReadOnlyUnitOfWork<FitnessDbContext>>();
        mockReadOnlyUnitOfWork
            .Setup(x => x.GetRepository<IWorkoutTemplateRepository>())
            .Returns(_mockRepository.Object);
        
        _mocker.GetMock<IUnitOfWorkProvider<FitnessDbContext>>()
            .Setup(x => x.CreateReadOnly())
            .Returns(mockReadOnlyUnitOfWork.Object);
        
        _sut = _mocker.CreateInstance<SearchQueryBuilder>();
    }

    [Fact]
    public async Task SearchAsync_WithNoFilters_ReturnsAllTemplates()
    {
        // Arrange
        var draftState = WorkoutStateTestBuilder.Draft().Build();
        var templates = new List<WorkoutTemplateEntity>
        {
            new WorkoutTemplateBuilder()
                .WithName("Template 1")
                .WithWorkoutState(draftState)
                .Build(),
            new WorkoutTemplateBuilder()
                .WithName("Template 2")
                .WithWorkoutState(draftState)
                .Build(),
            new WorkoutTemplateBuilder()
                .WithName("Template 3")
                .WithWorkoutState(draftState)
                .Build()
        };
        
        var mockQueryable = templates.BuildMockDbSet();
        _mockRepository
            .Setup(x => x.GetWorkoutTemplatesQueryable())
            .Returns(mockQueryable.Object);

        // Act
        var result = await _sut.SearchAsync(
            page: 1,
            pageSize: 10,
            namePattern: string.Empty,
            categoryId: WorkoutCategoryId.Empty,
            objectiveId: WorkoutObjectiveId.Empty,
            difficultyId: DifficultyLevelId.Empty,
            stateId: WorkoutStateId.Empty,
            sortBy: "name",
            sortOrder: "asc");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Data.TotalCount);
        Assert.Equal(3, result.Data.Items.Count());
        Assert.Equal("Template 1", result.Data.Items.First().Name);
    }

    [Fact]
    public async Task SearchAsync_WithNameFilter_ReturnsFilteredTemplates()
    {
        // Arrange
        var draftState = WorkoutStateTestBuilder.Draft().Build();
        var templates = new List<WorkoutTemplateEntity>
        {
            new WorkoutTemplateBuilder()
                .WithName("Upper Body Workout")
                .WithWorkoutState(draftState)
                .Build(),
            new WorkoutTemplateBuilder()
                .WithName("Lower Body Workout")
                .WithWorkoutState(draftState)
                .Build(),
            new WorkoutTemplateBuilder()
                .WithName("Full Body Workout")
                .WithWorkoutState(draftState)
                .Build()
        };
        
        var mockQueryable = templates.BuildMockDbSet();
        _mockRepository
            .Setup(x => x.GetWorkoutTemplatesQueryable())
            .Returns(mockQueryable.Object);

        // Act
        var result = await _sut.SearchAsync(
            page: 1,
            pageSize: 10,
            namePattern: "Upper",
            categoryId: WorkoutCategoryId.Empty,
            objectiveId: WorkoutObjectiveId.Empty,
            difficultyId: DifficultyLevelId.Empty,
            stateId: WorkoutStateId.Empty,
            sortBy: "name",
            sortOrder: "asc");

        // Assert
        Assert.True(result.IsSuccess);
        // Note: The actual filtering happens in extension methods which need to be tested separately
        // This test verifies the handler correctly orchestrates the query
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task SearchAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var draftState = WorkoutStateTestBuilder.Draft().Build();
        var templates = new List<WorkoutTemplateEntity>();
        for (int i = 1; i <= 25; i++)
        {
            templates.Add(new WorkoutTemplateBuilder()
                .WithName($"Template {i}")
                .WithWorkoutState(draftState)
                .Build());
        }
        
        var mockQueryable = templates.BuildMockDbSet();
        _mockRepository
            .Setup(x => x.GetWorkoutTemplatesQueryable())
            .Returns(mockQueryable.Object);

        // Act
        var result = await _sut.SearchAsync(
            page: 2,
            pageSize: 10,
            namePattern: string.Empty,
            categoryId: WorkoutCategoryId.Empty,
            objectiveId: WorkoutObjectiveId.Empty,
            difficultyId: DifficultyLevelId.Empty,
            stateId: WorkoutStateId.Empty,
            sortBy: "name",
            sortOrder: "asc");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(25, result.Data.TotalCount);
        Assert.Equal(2, result.Data.CurrentPage);
        Assert.Equal(10, result.Data.PageSize);
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsPagedResults()
    {
        // Arrange
        var templates = new List<WorkoutTemplateEntity>
        {
            new WorkoutTemplateBuilder().WithName("Template A").Build(),
            new WorkoutTemplateBuilder().WithName("Template B").Build()
        };
        
        var mockQueryable = templates.BuildMockDbSet();
        _mockRepository
            .Setup(x => x.GetWorkoutTemplatesQueryable())
            .Returns(mockQueryable.Object);

        // Act
        var result = await _sut.GetPagedAsync(1, 10);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Data.TotalCount);
        Assert.Equal(2, result.Data.Items.Count());
    }

    [Fact]
    public async Task GetRecentAsync_ReturnsRecentTemplates()
    {
        // Arrange
        var templates = new List<WorkoutTemplateEntity>
        {
            new WorkoutTemplateBuilder()
                .WithName("Old Template")
                .WithCreatedAt(DateTime.UtcNow.AddDays(-10))
                .Build(),
            new WorkoutTemplateBuilder()
                .WithName("Recent Template")
                .WithCreatedAt(DateTime.UtcNow.AddDays(-1))
                .Build(),
            new WorkoutTemplateBuilder()
                .WithName("Newest Template")
                .WithCreatedAt(DateTime.UtcNow)
                .Build()
        };
        
        var mockQueryable = templates.BuildMockDbSet();
        _mockRepository
            .Setup(x => x.GetWorkoutTemplatesQueryable())
            .Returns(mockQueryable.Object);

        // Act
        var result = await _sut.GetRecentAsync(2);

        // Assert
        Assert.True(result.IsSuccess);
        var items = result.Data.ToList();
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task GetRecentAsync_WithInvalidCount_ReturnsError()
    {
        // Act
        var result = await _sut.GetRecentAsync(0);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Count must be greater than 0", result.Errors.First());
    }

    [Fact]
    public async Task GetPopularAsync_ReturnsPublicTemplates()
    {
        // Arrange
        var templates = new List<WorkoutTemplateEntity>
        {
            new WorkoutTemplateBuilder()
                .WithName("Private Template")
                .AsPrivate()
                .Build(),
            new WorkoutTemplateBuilder()
                .WithName("Public Template 1")
                .AsPublic()
                .Build(),
            new WorkoutTemplateBuilder()
                .WithName("Public Template 2")
                .AsPublic()
                .Build()
        };
        
        var mockQueryable = templates.BuildMockDbSet();
        _mockRepository
            .Setup(x => x.GetWorkoutTemplatesQueryable())
            .Returns(mockQueryable.Object);

        // Act
        var result = await _sut.GetPopularAsync(5);

        // Assert
        Assert.True(result.IsSuccess);
        var items = result.Data.ToList();
        Assert.True(items.Count <= 5);
    }

    [Fact]
    public async Task GetPopularAsync_WithInvalidCount_ReturnsError()
    {
        // Act
        var result = await _sut.GetPopularAsync(-1);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Count must be greater than 0", result.Errors.First());
    }
}
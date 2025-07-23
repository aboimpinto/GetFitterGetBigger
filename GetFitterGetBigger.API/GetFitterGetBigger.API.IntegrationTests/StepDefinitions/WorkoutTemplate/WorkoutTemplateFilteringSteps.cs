using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplate;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Helpers;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Fixtures;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Microsoft.EntityFrameworkCore;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace GetFitterGetBigger.API.IntegrationTests.StepDefinitions.WorkoutTemplate;

[Binding]
public class WorkoutTemplateFilteringSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly PostgreSqlTestFixture _fixture;
    private HttpResponseMessage? _lastResponse;
    private PagedResponse<WorkoutTemplateDto>? _pageResponse;
    private List<WorkoutTemplateDto> _filteredTemplates = new();
    private readonly Dictionary<string, WorkoutTemplateId> _createdTemplates = new();
    private Stopwatch _stopwatch = new();

    public WorkoutTemplateFilteringSteps(ScenarioContext scenarioContext, PostgreSqlTestFixture fixture)
    {
        _scenarioContext = scenarioContext;
        _fixture = fixture;
    }

    [Given(@"I have created (\d+) workout templates")]
    public async Task GivenIHaveCreatedWorkoutTemplates(int count)
    {
        var userId = _scenarioContext.Get<string>("CurrentUserId");
        var client = _fixture.CreateClient();

        for (int i = 1; i <= count; i++)
        {
            var categoryId = await GetWorkoutCategoryId("Upper Body");
            var difficultyId = await GetDifficultyLevelId("Intermediate");
            
            var command = new CreateWorkoutTemplateCommand
            {
                Name = $"Template {i:D3}",
                Description = $"Test template {i}",
                CategoryId = categoryId,
                DifficultyId = difficultyId,
                EstimatedDurationMinutes = 45 + (i % 4) * 15, // Vary duration
                IsPublic = i % 2 == 0, // Alternate public/private
                CreatedBy = UserId.ParseOrEmpty(userId),
                Tags = new List<string> { "test", $"batch{i / 10}" }
            };

            var response = await client.PostAsJsonAsync("/api/workout-templates", command);
            response.IsSuccessStatusCode.Should().BeTrue();
        }
    }

    [When(@"I request my templates with page size (\d+)")]
    public async Task WhenIRequestMyTemplatesWithPageSize(int pageSize)
    {
        var userId = _scenarioContext.Get<string>("CurrentUserId");
        var client = _fixture.CreateClient();
        
        _lastResponse = await client.GetAsync($"/api/workout-templates?creatorId={userId}&page=1&pageSize={pageSize}");
        _scenarioContext.SetLastResponse(_lastResponse);

        if (_lastResponse.IsSuccessStatusCode)
        {
            _pageResponse = await _lastResponse.Content.ReadFromJsonAsync<PagedResponse<WorkoutTemplateDto>>();
        }
    }

    [Then(@"I should receive (\d+) templates in the first page")]
    public void ThenIShouldReceiveTemplatesInTheFirstPage(int expectedCount)
    {
        _pageResponse.Should().NotBeNull();
        _pageResponse!.Items.Should().HaveCount(expectedCount);
    }

    [Then(@"the total count should be (\d+)")]
    public void ThenTheTotalCountShouldBe(int expectedTotal)
    {
        _pageResponse.Should().NotBeNull();
        _pageResponse!.TotalCount.Should().Be(expectedTotal);
    }

    [Then(@"all templates should belong to me")]
    public void ThenAllTemplatesShouldBelongToMe()
    {
        var userId = _scenarioContext.Get<string>("CurrentUserId");
        _pageResponse.Should().NotBeNull();
        _pageResponse!.Items.Should().OnlyContain(t => t.CreatedBy == userId);
    }

    [Given(@"the following workout templates exist:")]
    public async Task GivenTheFollowingWorkoutTemplatesExist(Table table)
    {
        var client = _fixture.CreateClient();

        foreach (var row in table.Rows)
        {
            var categoryId = await GetWorkoutCategoryId("Upper Body");
            var difficultyId = await GetDifficultyLevelId("Intermediate");
            
            var command = new CreateWorkoutTemplateCommand
            {
                Name = row["Name"],
                Description = $"Template: {row["Name"]}",
                CategoryId = categoryId,
                DifficultyId = difficultyId,
                EstimatedDurationMinutes = 60,
                IsPublic = true,
                CreatedBy = UserId.ParseOrEmpty(row["CreatedBy"]),
                Tags = new List<string>()
            };

            var response = await client.PostAsJsonAsync("/api/workout-templates", command);
            
            if (response.IsSuccessStatusCode)
            {
                var template = await response.Content.ReadFromJsonAsync<WorkoutTemplateDto>();
                _createdTemplates[row["Name"]] = WorkoutTemplateId.ParseOrEmpty(template!.Id);
            }
        }
    }

    [When(@"I search for templates with name containing ""(.*)""")]
    public async Task WhenISearchForTemplatesWithNameContaining(string searchPattern)
    {
        var client = _fixture.CreateClient();
        _lastResponse = await client.GetAsync($"/api/workout-templates/search?namePattern={searchPattern}");
        _scenarioContext.SetLastResponse(_lastResponse);

        if (_lastResponse.IsSuccessStatusCode)
        {
            _filteredTemplates = await _lastResponse.Content.ReadFromJsonAsync<List<WorkoutTemplateDto>>() ?? new();
        }
    }

    [Then(@"I should receive (\d+) templates")]
    public void ThenIShouldReceiveTemplates(int expectedCount)
    {
        _filteredTemplates.Should().HaveCount(expectedCount);
    }

    [Then(@"all template names should contain ""(.*)""")]
    public void ThenAllTemplateNamesShouldContain(string expectedSubstring)
    {
        _filteredTemplates.Should().OnlyContain(t => 
            t.Name.Contains(expectedSubstring, StringComparison.OrdinalIgnoreCase));
    }

    [Given(@"workout templates exist in different categories")]
    public async Task GivenWorkoutTemplatesExistInDifferentCategories()
    {
        var client = _fixture.CreateClient();
        var categories = new[] { "Upper Body", "Lower Body", "Full Body" };
        var userId = _scenarioContext.Get<string>("CurrentUserId") ?? "user-01000001-0000-0000-0000-000000000001";

        foreach (var category in categories)
        {
            var categoryId = await GetWorkoutCategoryId(category);
            var difficultyId = await GetDifficultyLevelId("Intermediate");
            
            for (int i = 0; i < 3; i++)
            {
                var command = new CreateWorkoutTemplateCommand
                {
                    Name = $"{category} Workout {i + 1}",
                    Description = $"Test template for {category}",
                    CategoryId = categoryId,
                    DifficultyId = difficultyId,
                    EstimatedDurationMinutes = 60,
                    IsPublic = true,
                    CreatedBy = UserId.ParseOrEmpty(userId),
                    Tags = new List<string> { category.ToLower().Replace(" ", "-") }
                };

                await client.PostAsJsonAsync("/api/workout-templates", command);
            }
        }
    }

    [When(@"I filter templates by category ""(.*)""")]
    public async Task WhenIFilterTemplatesByCategory(string categoryName)
    {
        var categoryId = await GetWorkoutCategoryId(categoryName);
        var client = _fixture.CreateClient();
        
        _lastResponse = await client.GetAsync($"/api/workout-templates/by-category/{categoryId}");
        _scenarioContext.SetLastResponse(_lastResponse);

        if (_lastResponse.IsSuccessStatusCode)
        {
            _filteredTemplates = await _lastResponse.Content.ReadFromJsonAsync<List<WorkoutTemplateDto>>() ?? new();
        }
    }

    [Then(@"all returned templates should have category ""(.*)""")]
    public void ThenAllReturnedTemplatesShouldHaveCategory(string expectedCategory)
    {
        _filteredTemplates.Should().NotBeEmpty();
        _filteredTemplates.Should().OnlyContain(t => t.Category != null && t.Category.Value == expectedCategory);
    }

    [Given(@"workout templates exist with different difficulty levels")]
    public async Task GivenWorkoutTemplatesExistWithDifferentDifficultyLevels()
    {
        var client = _fixture.CreateClient();
        var difficulties = new[] { "Beginner", "Intermediate", "Advanced" };
        var userId = _scenarioContext.Get<string>("CurrentUserId") ?? "user-01000001-0000-0000-0000-000000000001";
        var categoryId = await GetWorkoutCategoryId("Upper Body");

        foreach (var difficulty in difficulties)
        {
            var difficultyId = await GetDifficultyLevelId(difficulty);
            
            for (int i = 0; i < 2; i++)
            {
                var command = new CreateWorkoutTemplateCommand
                {
                    Name = $"{difficulty} Workout {i + 1}",
                    Description = $"Test template for {difficulty} level",
                    CategoryId = categoryId,
                    DifficultyId = difficultyId,
                    EstimatedDurationMinutes = 60,
                    IsPublic = true,
                    CreatedBy = UserId.ParseOrEmpty(userId),
                    Tags = new List<string> { difficulty.ToLower() }
                };

                await client.PostAsJsonAsync("/api/workout-templates", command);
            }
        }
    }

    [When(@"I filter templates by difficulty ""(.*)""")]
    public async Task WhenIFilterTemplatesByDifficulty(string difficultyName)
    {
        var difficultyId = await GetDifficultyLevelId(difficultyName);
        var client = _fixture.CreateClient();
        
        _lastResponse = await client.GetAsync($"/api/workout-templates/by-difficulty/{difficultyId}");
        _scenarioContext.SetLastResponse(_lastResponse);

        if (_lastResponse.IsSuccessStatusCode)
        {
            _filteredTemplates = await _lastResponse.Content.ReadFromJsonAsync<List<WorkoutTemplateDto>>() ?? new();
        }
    }

    [Then(@"all returned templates should have difficulty ""(.*)""")]
    public void ThenAllReturnedTemplatesShouldHaveDifficulty(string expectedDifficulty)
    {
        _filteredTemplates.Should().NotBeEmpty();
        _filteredTemplates.Should().OnlyContain(t => t.Difficulty != null && t.Difficulty.Value == expectedDifficulty);
    }

    [Given(@"workout templates exist with different objectives")]
    public async Task GivenWorkoutTemplatesExistWithDifferentObjectives()
    {
        // Implementation would involve creating templates with different objectives
        // This is a placeholder for now as objectives are many-to-many relationships
        await Task.CompletedTask;
    }

    [When(@"I filter templates by objective ""(.*)""")]
    public async Task WhenIFilterTemplatesByObjective(string objectiveName)
    {
        var objectiveId = await GetWorkoutObjectiveId(objectiveName);
        var client = _fixture.CreateClient();
        
        _lastResponse = await client.GetAsync($"/api/workout-templates/by-objective/{objectiveId}");
        _scenarioContext.SetLastResponse(_lastResponse);

        if (_lastResponse.IsSuccessStatusCode)
        {
            _filteredTemplates = await _lastResponse.Content.ReadFromJsonAsync<List<WorkoutTemplateDto>>() ?? new();
        }
    }

    [Then(@"all returned templates should have objective ""(.*)""")]
    public void ThenAllReturnedTemplatesShouldHaveObjective(string expectedObjective)
    {
        _filteredTemplates.Should().NotBeEmpty();
        _filteredTemplates.Should().OnlyContain(t => 
            t.Objectives != null && t.Objectives.Any(o => o.Value == expectedObjective));
    }

    [Given(@"(\d+) workout templates exist in the system")]
    public async Task GivenWorkoutTemplatesExistInTheSystem(int count)
    {
        var client = _fixture.CreateClient();
        var categoryId = await GetWorkoutCategoryId("Upper Body");
        var difficultyId = await GetDifficultyLevelId("Intermediate");
        
        // Create templates in batches to improve performance
        var batchSize = 50;
        var batches = (count + batchSize - 1) / batchSize;
        
        for (int batch = 0; batch < batches; batch++)
        {
            var tasks = new List<Task>();
            var itemsInBatch = Math.Min(batchSize, count - (batch * batchSize));
            
            for (int i = 0; i < itemsInBatch; i++)
            {
                var index = (batch * batchSize) + i + 1;
                var command = new CreateWorkoutTemplateCommand
                {
                    Name = $"Performance Test Template {index:D5}",
                    Description = $"Template for performance testing #{index}",
                    CategoryId = categoryId,
                    DifficultyId = difficultyId,
                    EstimatedDurationMinutes = 60,
                    IsPublic = true,
                    CreatedBy = UserId.ParseOrEmpty($"user-{(index % 10):D2}000001-0000-0000-0000-000000000001"),
                    Tags = new List<string> { "performance", $"batch{batch}" }
                };

                tasks.Add(client.PostAsJsonAsync("/api/workout-templates", command));
            }
            
            await Task.WhenAll(tasks);
        }
    }

    [When(@"I request templates with pagination \(page (\d+), size (\d+)\)")]
    public async Task WhenIRequestTemplatesWithPagination(int page, int pageSize)
    {
        var client = _fixture.CreateClient();
        
        _stopwatch.Restart();
        _lastResponse = await client.GetAsync($"/api/workout-templates?page={page}&pageSize={pageSize}");
        _stopwatch.Stop();
        
        _scenarioContext.SetLastResponse(_lastResponse);

        if (_lastResponse.IsSuccessStatusCode)
        {
            _pageResponse = await _lastResponse.Content.ReadFromJsonAsync<PagedResponse<WorkoutTemplateDto>>();
        }
    }

    [Then(@"the response should return within (\d+)ms")]
    public void ThenTheResponseShouldReturnWithinMs(int maxMilliseconds)
    {
        _stopwatch.ElapsedMilliseconds.Should().BeLessThan(maxMilliseconds,
            $"Response took {_stopwatch.ElapsedMilliseconds}ms, which exceeds the {maxMilliseconds}ms limit");
    }

    [Then(@"I should receive exactly (\d+) templates")]
    public void ThenIShouldReceiveExactlyTemplates(int expectedCount)
    {
        _pageResponse.Should().NotBeNull();
        _pageResponse!.Items.Should().HaveCount(expectedCount);
    }

    // Helper methods
    private async Task<WorkoutCategoryId> GetWorkoutCategoryId(string categoryName)
    {
        WorkoutCategoryId categoryId = WorkoutCategoryId.Empty;
        
        await _fixture.ExecuteDbContextAsync(async context =>
        {
            var category = await context.WorkoutCategories
                .FirstOrDefaultAsync(c => c.Value == categoryName);
            
            if (category != null)
            {
                categoryId = category.WorkoutCategoryId;
            }
        });

        return categoryId;
    }

    private async Task<DifficultyLevelId> GetDifficultyLevelId(string difficultyName)
    {
        DifficultyLevelId difficultyId = DifficultyLevelId.Empty;
        
        await _fixture.ExecuteDbContextAsync(async context =>
        {
            var difficulty = await context.DifficultyLevels
                .FirstOrDefaultAsync(d => d.Value == difficultyName);
            
            if (difficulty != null)
            {
                difficultyId = difficulty.DifficultyLevelId;
            }
        });

        return difficultyId;
    }

    private async Task<WorkoutObjectiveId> GetWorkoutObjectiveId(string objectiveName)
    {
        WorkoutObjectiveId objectiveId = WorkoutObjectiveId.Empty;
        
        await _fixture.ExecuteDbContextAsync(async context =>
        {
            var objective = await context.WorkoutObjectives
                .FirstOrDefaultAsync(o => o.Value == objectiveName);
            
            if (objective != null)
            {
                objectiveId = objective.WorkoutObjectiveId;
            }
        });

        return objectiveId;
    }
}
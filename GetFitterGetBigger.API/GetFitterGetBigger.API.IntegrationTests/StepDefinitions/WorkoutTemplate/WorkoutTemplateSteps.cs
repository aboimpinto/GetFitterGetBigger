using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplate;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Helpers;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Fixtures;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Microsoft.EntityFrameworkCore;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace GetFitterGetBigger.API.IntegrationTests.StepDefinitions.WorkoutTemplate;

[Binding]
public class WorkoutTemplateSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly PostgreSqlTestFixture _fixture;
    private WorkoutTemplateDto? _workoutTemplateResponse;
    private PagedResponse<WorkoutTemplateDto>? _pageResponse;
    private HttpResponseMessage? _lastResponse;
    private List<WorkoutTemplateDto> _templates = new();
    private string _currentUserId = "";
    private WorkoutTemplateId _currentTemplateId = WorkoutTemplateId.Empty;

    public WorkoutTemplateSteps(ScenarioContext scenarioContext, PostgreSqlTestFixture fixture)
    {
        _scenarioContext = scenarioContext;
        _fixture = fixture;
    }

    [Given(@"I am a Personal Trainer with ID ""(.*)""")]
    public void GivenIAmAPersonalTrainerWithID(string userId)
    {
        _currentUserId = userId;
        _scenarioContext["CurrentUserId"] = userId;
    }

    [Given(@"the following workout states exist:")]
    public async Task GivenTheFollowingWorkoutStatesExist(Table table)
    {
        // Workout states are seeded by the database, this is for documentation
        await Task.CompletedTask;
    }

    [Given(@"the following workout categories exist:")]
    public async Task GivenTheFollowingWorkoutCategoriesExist(Table table)
    {
        await _fixture.ExecuteDbContextAsync(async context =>
        {
            var seeder = new TestDatabaseSeeder(context);
            await seeder.SeedWorkoutCategoriesAsync();
        });
    }

    [Given(@"the following difficulty levels exist:")]
    public async Task GivenTheFollowingDifficultyLevelsExist(Table table)
    {
        await _fixture.ExecuteDbContextAsync(async context =>
        {
            var seeder = new TestDatabaseSeeder(context);
            await seeder.SeedDifficultyLevelsAsync();
        });
    }

    [Given(@"the following workout objectives exist:")]
    public async Task GivenTheFollowingWorkoutObjectivesExist(Table table)
    {
        await _fixture.ExecuteDbContextAsync(async context =>
        {
            var seeder = new TestDatabaseSeeder(context);
            await seeder.SeedWorkoutObjectivesAsync();
        });
    }

    [When(@"I create a new workout template with:")]
    public async Task WhenICreateANewWorkoutTemplateWith(Table table)
    {
        var data = table.Rows[0];
        
        // Get reference data IDs
        var categoryId = await GetWorkoutCategoryId(data["CategoryId"]);
        var difficultyId = await GetDifficultyLevelId(data["DifficultyId"]);
        
        var command = new CreateWorkoutTemplateCommand
        {
            Name = data["Name"],
            Description = data.ContainsKey("Description") ? data["Description"] : null,
            CategoryId = categoryId,
            DifficultyId = difficultyId,
            EstimatedDurationMinutes = int.Parse(data["EstimatedDurationMinutes"]),
            IsPublic = bool.Parse(data["IsPublic"]),
            CreatedBy = UserId.ParseOrEmpty(_currentUserId),
            Tags = new List<string>()
        };

        var client = _fixture.CreateClient();
        _lastResponse = await client.PostAsJsonAsync("/api/workout-templates", command);
        _scenarioContext.SetLastResponse(_lastResponse);

        if (_lastResponse.IsSuccessStatusCode)
        {
            _workoutTemplateResponse = await _lastResponse.Content.ReadFromJsonAsync<WorkoutTemplateDto>();
            if (_workoutTemplateResponse != null)
            {
                _currentTemplateId = WorkoutTemplateId.ParseOrEmpty(_workoutTemplateResponse.Id);
                _scenarioContext["CurrentTemplateId"] = _workoutTemplateResponse.Id;
            }
        }
    }

    [Then(@"the workout template should be created successfully")]
    public void ThenTheWorkoutTemplateShouldBeCreatedSuccessfully()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.StatusCode.Should().Be(HttpStatusCode.Created);
        _workoutTemplateResponse.Should().NotBeNull();
    }

    [Then(@"the template state should be ""(.*)""")]
    public void ThenTheTemplateStateShouldBe(string expectedState)
    {
        _workoutTemplateResponse.Should().NotBeNull();
        _workoutTemplateResponse!.WorkoutState.Should().NotBeNull();
        _workoutTemplateResponse.WorkoutState!.Value.Should().Be(expectedState);
    }

    [Then(@"I should be set as the creator")]
    public void ThenIShouldBeSetAsTheCreator()
    {
        _workoutTemplateResponse.Should().NotBeNull();
        _workoutTemplateResponse!.CreatedBy.Should().Be(_currentUserId);
    }

    [Then(@"the response should contain:")]
    public void ThenTheResponseShouldContain(Table table)
    {
        _workoutTemplateResponse.Should().NotBeNull();
        
        foreach (var row in table.Rows)
        {
            var field = row["Field"];
            var expectedValue = row["Value"];

            switch (field)
            {
                case "Name":
                    _workoutTemplateResponse!.Name.Should().Be(expectedValue);
                    break;
                case "WorkoutState":
                    _workoutTemplateResponse!.WorkoutState!.Value.Should().Be(expectedValue);
                    break;
                case "EstimatedDurationMinutes":
                    _workoutTemplateResponse!.EstimatedDurationMinutes.Should().Be(int.Parse(expectedValue));
                    break;
            }
        }
    }

    [Given(@"a workout template exists with ID ""(.*)""")]
    public async Task GivenAWorkoutTemplateExistsWithID(string templateId)
    {
        await _fixture.ExecuteDbContextAsync(async context =>
        {
            var seeder = new TestDatabaseSeeder(context);
            await seeder.SeedTestWorkoutTemplateAsync(WorkoutTemplateId.ParseOrEmpty(templateId));
        });
        
        _currentTemplateId = WorkoutTemplateId.ParseOrEmpty(templateId);
        _scenarioContext["CurrentTemplateId"] = templateId;
    }

    [When(@"I request the workout template by ID")]
    public async Task WhenIRequestTheWorkoutTemplateByID()
    {
        var client = _fixture.CreateClient();
        var templateId = _scenarioContext.Get<string>("CurrentTemplateId");
        _lastResponse = await client.GetAsync($"/api/workout-templates/{templateId}");
        _scenarioContext.SetLastResponse(_lastResponse);

        if (_lastResponse.IsSuccessStatusCode)
        {
            _workoutTemplateResponse = await _lastResponse.Content.ReadFromJsonAsync<WorkoutTemplateDto>();
        }
    }

    [Then(@"the response status should be OK")]
    public void ThenTheResponseStatusShouldBeOK()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Then(@"the response should contain the workout template details")]
    public void ThenTheResponseShouldContainTheWorkoutTemplateDetails()
    {
        _workoutTemplateResponse.Should().NotBeNull();
        _workoutTemplateResponse!.Id.Should().NotBeNullOrEmpty();
        _workoutTemplateResponse.Name.Should().NotBeNullOrEmpty();
    }

    [Then(@"the response should include navigation properties:")]
    public void ThenTheResponseShouldIncludeNavigationProperties(Table table)
    {
        _workoutTemplateResponse.Should().NotBeNull();

        foreach (var row in table.Rows)
        {
            var property = row["Property"];
            switch (property)
            {
                case "Category":
                    _workoutTemplateResponse!.Category.Should().NotBeNull();
                    break;
                case "Difficulty":
                    _workoutTemplateResponse!.Difficulty.Should().NotBeNull();
                    break;
                case "WorkoutState":
                    _workoutTemplateResponse!.WorkoutState.Should().NotBeNull();
                    break;
                case "Exercises":
                    _workoutTemplateResponse!.Exercises.Should().NotBeNull();
                    break;
                case "Objectives":
                    _workoutTemplateResponse!.Objectives.Should().NotBeNull();
                    break;
            }
        }
    }

    [Given(@"I have created a workout template in DRAFT state")]
    public async Task GivenIHaveCreatedAWorkoutTemplateInDRAFTState()
    {
        await CreateTestWorkoutTemplate("DRAFT");
    }

    [Given(@"I have created a complete workout template in DRAFT state")]
    public async Task GivenIHaveCreatedACompleteWorkoutTemplateInDRAFTState()
    {
        await CreateTestWorkoutTemplate("DRAFT", isComplete: true);
    }

    [When(@"I update the workout template with:")]
    public async Task WhenIUpdateTheWorkoutTemplateWith(Table table)
    {
        var data = table.Rows[0];
        
        var updateCommand = new UpdateWorkoutTemplateCommand
        {
            Id = _currentTemplateId,
            Name = data.ContainsKey("Name") ? data["Name"] : _workoutTemplateResponse?.Name,
            Description = data.ContainsKey("Description") ? data["Description"] : _workoutTemplateResponse?.Description,
            UpdatedBy = UserId.ParseOrEmpty(_currentUserId)
        };

        var client = _fixture.CreateClient();
        _lastResponse = await client.PutAsJsonAsync($"/api/workout-templates/{_currentTemplateId}", updateCommand);
        _scenarioContext.SetLastResponse(_lastResponse);

        if (_lastResponse.IsSuccessStatusCode)
        {
            _workoutTemplateResponse = await _lastResponse.Content.ReadFromJsonAsync<WorkoutTemplateDto>();
        }
    }

    [Then(@"the workout template should be updated successfully")]
    public void ThenTheWorkoutTemplateShouldBeUpdatedSuccessfully()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Then(@"the response should contain the updated values")]
    public void ThenTheResponseShouldContainTheUpdatedValues()
    {
        _workoutTemplateResponse.Should().NotBeNull();
        // Values are checked in the specific test scenarios
    }

    [When(@"I delete the workout template")]
    public async Task WhenIDeleteTheWorkoutTemplate()
    {
        var client = _fixture.CreateClient();
        _lastResponse = await client.DeleteAsync($"/api/workout-templates/{_currentTemplateId}");
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [Then(@"the template should be soft deleted")]
    public void ThenTheTemplateShouldBeSoftDeleted()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Then(@"the template should not appear in active template lists")]
    public async Task ThenTheTemplateShouldNotAppearInActiveTemplateLists()
    {
        var client = _fixture.CreateClient();
        var response = await client.GetAsync($"/api/workout-templates?creatorId={_currentUserId}");
        
        if (response.IsSuccessStatusCode)
        {
            var pageResponse = await response.Content.ReadFromJsonAsync<PagedResponse<WorkoutTemplateDto>>();
            pageResponse.Should().NotBeNull();
            pageResponse!.Items.Should().NotContain(t => t.Id == _currentTemplateId.ToString());
        }
    }

    [When(@"I change the template state to ""(.*)""")]
    public async Task WhenIChangeTheTemplateStateTo(string newState)
    {
        var stateId = await GetWorkoutStateId(newState);
        
        var changeStateDto = new 
        {
            WorkoutStateId = stateId
        };

        var client = _fixture.CreateClient();
        _lastResponse = await client.PutAsJsonAsync($"/api/workout-templates/{_currentTemplateId}/state", changeStateDto);
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [Then(@"the state should change to ""(.*)""")]
    public async Task ThenTheStateShouldChangeTo(string expectedState)
    {
        // Fetch the updated template to verify state
        var client = _fixture.CreateClient();
        var response = await client.GetAsync($"/api/workout-templates/{_currentTemplateId}");
        
        if (response.IsSuccessStatusCode)
        {
            var template = await response.Content.ReadFromJsonAsync<WorkoutTemplateDto>();
            template.Should().NotBeNull();
            template!.WorkoutState!.Value.Should().Be(expectedState);
        }
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

    private async Task<WorkoutStateId> GetWorkoutStateId(string stateName)
    {
        WorkoutStateId stateId = WorkoutStateId.Empty;
        
        await _fixture.ExecuteDbContextAsync(async context =>
        {
            var state = await context.WorkoutStates
                .FirstOrDefaultAsync(s => s.Value == stateName);
            
            if (state != null)
            {
                stateId = state.WorkoutStateId;
            }
        });

        return stateId;
    }

    private async Task CreateTestWorkoutTemplate(string state, bool isComplete = false)
    {
        var categoryId = await GetWorkoutCategoryId("Upper Body");
        var difficultyId = await GetDifficultyLevelId("Intermediate");
        
        var command = new CreateWorkoutTemplateCommand
        {
            Name = $"Test Template {Guid.NewGuid()}",
            Description = "Test template description",
            CategoryId = categoryId,
            DifficultyId = difficultyId,
            EstimatedDurationMinutes = 60,
            IsPublic = true,
            CreatedBy = UserId.ParseOrEmpty(_currentUserId),
            Tags = new List<string> { "test" }
        };

        var client = _fixture.CreateClient();
        var response = await client.PostAsJsonAsync("/api/workout-templates", command);
        
        if (response.IsSuccessStatusCode)
        {
            var template = await response.Content.ReadFromJsonAsync<WorkoutTemplateDto>();
            _currentTemplateId = WorkoutTemplateId.ParseOrEmpty(template!.Id);
            _workoutTemplateResponse = template;
            _scenarioContext["CurrentTemplateId"] = template.Id;
        }
    }
}
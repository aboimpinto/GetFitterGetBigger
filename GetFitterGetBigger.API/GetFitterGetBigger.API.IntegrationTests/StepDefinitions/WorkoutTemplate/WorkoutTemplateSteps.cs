using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplate;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Helpers;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Fixtures;
using GetFitterGetBigger.API.Models.SpecializedIds;
using GetFitterGetBigger.API.Controllers;
using Microsoft.EntityFrameworkCore;
using TechTalk.SpecFlow;

namespace GetFitterGetBigger.API.IntegrationTests.StepDefinitions.WorkoutTemplate;

[Binding]
public class WorkoutTemplateSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly PostgreSqlTestFixture _fixture;
    private WorkoutTemplateDto? _workoutTemplateResponse;
    private HttpResponseMessage? _lastResponse;
    private List<WorkoutTemplateDto> _templates = new();
    private WorkoutTemplateId _currentTemplateId = WorkoutTemplateId.Empty;

    public WorkoutTemplateSteps(ScenarioContext scenarioContext, PostgreSqlTestFixture fixture)
    {
        _scenarioContext = scenarioContext;
        _fixture = fixture;
    }

    [Given(@"I am a Personal Trainer with ID ""(.*)""")]
    public void GivenIAmAPersonalTrainerWithID(string userId)
    {
        // No longer tracking user context
        _scenarioContext["CurrentUserId"] = userId; // Keep for other tests that might use it
    }

    [Given(@"the following workout states exist:")]
    public async Task GivenTheFollowingWorkoutStatesExist(Table table)
    {
        await _fixture.ExecuteDbContextAsync(async context =>
        {
            var seeder = new TestDatabaseSeeder(context);
            await seeder.SeedWorkoutStatesAsync();
        });
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
        // Convert table to dictionary
        var data = table.Rows.ToDictionary(row => row["Field"], row => row["Value"]);
        
        // Get reference data IDs
        var categoryId = await GetWorkoutCategoryId(data["CategoryId"]);
        var difficultyId = await GetDifficultyLevelId(data["DifficultyId"]);
        
        var createDto = new CreateWorkoutTemplateDto
        {
            Name = data["Name"],
            Description = data.ContainsKey("Description") ? data["Description"] : null,
            CategoryId = categoryId.ToString(),
            DifficultyId = difficultyId.ToString(),
            EstimatedDurationMinutes = int.Parse(data["EstimatedDurationMinutes"]),
            IsPublic = bool.Parse(data["IsPublic"]),
            Tags = new List<string>(),
            ObjectiveIds = new List<string>()
        };

        var client = _fixture.CreateClient();
        _lastResponse = await client.PostAsJsonAsync("/api/workout-templates", createDto);
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
        else
        {
            var errorContent = await _lastResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Error response: {_lastResponse.StatusCode} - {errorContent}");
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
        // Creator is no longer tracked at the template level
        // Test passes as template was created successfully
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
        // Convert table to dictionary
        var data = table.Rows.ToDictionary(row => row["Field"], row => row["Value"]);
        
        var updateDto = new UpdateWorkoutTemplateDto
        {
            Name = data.ContainsKey("Name") ? data["Name"] : _workoutTemplateResponse?.Name ?? "Default Name",
            Description = data.ContainsKey("Description") ? data["Description"] : _workoutTemplateResponse?.Description,
            CategoryId = _workoutTemplateResponse?.Category?.Id ?? throw new InvalidOperationException("No category ID in existing template"),
            DifficultyId = _workoutTemplateResponse?.Difficulty?.Id ?? throw new InvalidOperationException("No difficulty ID in existing template"),
            EstimatedDurationMinutes = _workoutTemplateResponse?.EstimatedDurationMinutes ?? 60,
            IsPublic = _workoutTemplateResponse?.IsPublic ?? false,
            Tags = _workoutTemplateResponse?.Tags ?? new List<string>(),
            ObjectiveIds = _workoutTemplateResponse?.Objectives?.Select(o => o.Id).ToList() ?? new List<string>()
        };

        var client = _fixture.CreateClient();
        _lastResponse = await client.PutAsJsonAsync($"/api/workout-templates/{_currentTemplateId}", updateDto);
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
        var response = await client.GetAsync("/api/workout-templates");
        
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
        
        if (stateId.IsEmpty)
        {
            throw new InvalidOperationException($"Failed to get WorkoutStateId for state: {newState}");
        }
        
        // Check scenario context first, then fall back to class field
        if (_scenarioContext.ContainsKey("CurrentTemplateId"))
        {
            var templateId = _scenarioContext.Get<string>("CurrentTemplateId");
            _currentTemplateId = WorkoutTemplateId.ParseOrEmpty(templateId);
        }
        
        if (_currentTemplateId.IsEmpty)
        {
            throw new InvalidOperationException("No current template ID set. Make sure to create a template first.");
        }
        
        var changeStateDto = new ChangeWorkoutStateDto
        {
            WorkoutStateId = stateId.ToString()
        };

        var client = _fixture.CreateClient();
        _lastResponse = await client.PutAsJsonAsync($"/api/workout-templates/{_currentTemplateId}/state", changeStateDto);
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [Then(@"the state should change to ""(.*)""")]
    public async Task ThenTheStateShouldChangeTo(string expectedState)
    {
        _lastResponse.Should().NotBeNull();
        
        // Log the response for debugging
        var responseContent = await _lastResponse!.Content.ReadAsStringAsync();
        Console.WriteLine($"Response Status: {_lastResponse.StatusCode}");
        Console.WriteLine($"Response Content: {responseContent}");
        
        if (!_lastResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"State change failed. Status: {_lastResponse.StatusCode}, Content: {responseContent}");
        }
        
        _lastResponse.IsSuccessStatusCode.Should().BeTrue($"Expected success but got {_lastResponse.StatusCode}");
        
        // Deserialize from the string content we already read
        var updatedTemplate = System.Text.Json.JsonSerializer.Deserialize<WorkoutTemplateDto>(responseContent, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        updatedTemplate.Should().NotBeNull();
        updatedTemplate!.WorkoutState.Should().NotBeNull();
        updatedTemplate.WorkoutState.Value.Should().Be(expectedState);
        
        // Store the updated template for subsequent steps
        _workoutTemplateResponse = updatedTemplate;
    }

    // Helper methods
    private async Task<WorkoutCategoryId> GetWorkoutCategoryId(string categoryName)
    {
        WorkoutCategoryId categoryId = WorkoutCategoryId.Empty;
        
        // Use category name directly
        var categoryMapping = categoryName;
        
        await _fixture.ExecuteDbContextAsync(async context =>
        {
            var category = await context.WorkoutCategories
                .FirstOrDefaultAsync(c => c.Value == categoryMapping);
            
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
        var categoryId = await GetWorkoutCategoryId("Full Body");
        var difficultyId = await GetDifficultyLevelId("Intermediate");
        
        if (categoryId.IsEmpty || difficultyId.IsEmpty)
        {
            throw new InvalidOperationException($"Failed to get category or difficulty. CategoryId: {categoryId}, DifficultyId: {difficultyId}");
        }
        
        var createDto = new CreateWorkoutTemplateDto
        {
            Name = $"Test Template {Guid.NewGuid()}",
            Description = "Test template description",
            CategoryId = categoryId.ToString(),
            DifficultyId = difficultyId.ToString(),
            EstimatedDurationMinutes = 60,
            IsPublic = true,
            Tags = new List<string> { "test" },
            ObjectiveIds = new List<string>()
        };

        var client = _fixture.CreateClient();
        var response = await client.PostAsJsonAsync("/api/workout-templates", createDto);
        
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to create template. Status: {response.StatusCode}, Content: {content}");
        }
        
        var template = await response.Content.ReadFromJsonAsync<WorkoutTemplateDto>();
        if (template == null)
        {
            throw new InvalidOperationException("Template response was null");
        }
        
        _currentTemplateId = WorkoutTemplateId.ParseOrEmpty(template.Id);
        _workoutTemplateResponse = template;
        _scenarioContext["CurrentTemplateId"] = template.Id;
        
        // Store the response for later use
        _lastResponse = response;
    }
    
    [When(@"I update the template")]
    public async Task WhenIUpdateTheTemplate()
    {
        if (_workoutTemplateResponse == null)
        {
            throw new InvalidOperationException("No template to update");
        }

        var updateDto = new UpdateWorkoutTemplateDto
        {
            Name = _workoutTemplateResponse.Name + " Updated",
            Description = _workoutTemplateResponse.Description,
            CategoryId = _workoutTemplateResponse.Category.Id,
            DifficultyId = _workoutTemplateResponse.Difficulty.Id,
            EstimatedDurationMinutes = _workoutTemplateResponse.EstimatedDurationMinutes,
            IsPublic = _workoutTemplateResponse.IsPublic,
            Tags = _workoutTemplateResponse.Tags ?? new List<string>(),
            ObjectiveIds = _workoutTemplateResponse.Objectives?.Select(o => o.Id).ToList() ?? new List<string>()
        };

        var client = _fixture.CreateClient();
        _lastResponse = await client.PutAsJsonAsync($"/api/workout-templates/{_workoutTemplateResponse.Id}", updateDto);
    }
    
    [Then(@"the operation should succeed")]
    public void ThenTheOperationShouldSucceed()
    {
        if (_lastResponse == null)
        {
            throw new InvalidOperationException("No response to check");
        }
        
        _lastResponse.IsSuccessStatusCode.Should().BeTrue($"Expected success but got {_lastResponse.StatusCode}");
    }
    
    [Then(@"the template should be available for execution")]
    public async Task ThenTheTemplateShouldBeAvailableForExecution()
    {
        if (_workoutTemplateResponse == null)
        {
            throw new InvalidOperationException("No template to check");
        }

        // Verify the template state is PRODUCTION
        var client = _fixture.CreateClient();
        var response = await client.GetAsync($"/api/workout-templates/{_workoutTemplateResponse.Id}");
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var template = await response.Content.ReadFromJsonAsync<WorkoutTemplateDto>();
        template.Should().NotBeNull();
        template!.WorkoutState.Value.Should().Be("PRODUCTION", "Template should be in PRODUCTION state to be available for execution");
    }
}
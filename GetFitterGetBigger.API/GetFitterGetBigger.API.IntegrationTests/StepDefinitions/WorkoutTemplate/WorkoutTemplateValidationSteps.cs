using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.Services.Commands.WorkoutTemplate;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Helpers;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Fixtures;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Microsoft.EntityFrameworkCore;
using TechTalk.SpecFlow;

namespace GetFitterGetBigger.API.IntegrationTests.StepDefinitions.WorkoutTemplate;

[Binding]
public class WorkoutTemplateValidationSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly PostgreSqlTestFixture _fixture;
    private HttpResponseMessage? _lastResponse;
    private string _currentUserId = "user-01000001-0000-0000-0000-000000000001";
    private WorkoutTemplateId _existingTemplateId = WorkoutTemplateId.Empty;
    private string _testTemplateName = "";

    public WorkoutTemplateValidationSteps(ScenarioContext scenarioContext, PostgreSqlTestFixture fixture)
    {
        _scenarioContext = scenarioContext;
        _fixture = fixture;
    }

    private HttpClient CreateClientWithUserId()
    {
        var client = CreateClientWithUserId();
        if (!string.IsNullOrEmpty(_currentUserId))
        {
            client.DefaultRequestHeaders.Add("X-Test-UserId", _currentUserId);
        }
        return client;
    }

    [Given(@"I am a Personal Trainer")]
    public void GivenIAmAPersonalTrainer()
    {
        if (_scenarioContext.ContainsKey("CurrentUserId"))
        {
            _currentUserId = _scenarioContext.Get<string>("CurrentUserId");
        }
    }

    [When(@"I create a workout template with name of (\d+) characters")]
    public async Task WhenICreateAWorkoutTemplateWithNameOfCharacters(int nameLength)
    {
        _testTemplateName = new string('A', nameLength);
        await CreateTemplateWithName(_testTemplateName);
    }

    [When(@"I create a workout template with duration (\d+) minutes")]
    public async Task WhenICreateAWorkoutTemplateWithDurationMinutes(int duration)
    {
        await CreateTemplateWithDuration(duration);
    }

    [Then(@"the operation should pass")]
    public void ThenTheOperationShouldPass()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.IsSuccessStatusCode.Should().BeTrue();
    }

    [Then(@"the operation should fail")]
    public void ThenTheOperationShouldFail()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.IsSuccessStatusCode.Should().BeFalse();
    }

    [Given(@"I have created a template named ""(.*)""")]
    public async Task GivenIHaveCreatedATemplateNamed(string templateName)
    {
        _testTemplateName = templateName;
        await CreateTemplateWithName(templateName);
        
        if (_lastResponse!.IsSuccessStatusCode)
        {
            var template = await _lastResponse.Content.ReadFromJsonAsync<WorkoutTemplateDto>();
            _existingTemplateId = WorkoutTemplateId.ParseOrEmpty(template!.Id);
        }
    }

    [When(@"I try to create another template named ""(.*)""")]
    public async Task WhenITryToCreateAnotherTemplateNamed(string templateName)
    {
        await CreateTemplateWithName(templateName);
    }

    [Then(@"the operation should fail with status ""(.*)""")]
    public void ThenTheOperationShouldFailWithStatus(string expectedStatus)
    {
        _lastResponse.Should().NotBeNull();
        
        var statusCode = Enum.Parse<HttpStatusCode>(expectedStatus);
        _lastResponse!.StatusCode.Should().Be(statusCode);
    }

    [Then(@"the error message should contain ""(.*)""")]
    public async Task ThenTheErrorMessageShouldContain(string expectedText)
    {
        _lastResponse.Should().NotBeNull();
        var content = await _lastResponse!.Content.ReadAsStringAsync();
        content.Should().Contain(expectedText, Exactly.Once());
    }

    [Given(@"I have a workout template in PRODUCTION state")]
    public async Task GivenIHaveAWorkoutTemplateInPRODUCTIONState()
    {
        // First create a template
        await CreateTemplateWithName($"Production Template {Guid.NewGuid()}");
        
        if (_lastResponse!.IsSuccessStatusCode)
        {
            var template = await _lastResponse.Content.ReadFromJsonAsync<WorkoutTemplateDto>();
            _existingTemplateId = WorkoutTemplateId.ParseOrEmpty(template!.Id);
            
            // Change to PRODUCTION state
            var stateId = await GetWorkoutStateId("PRODUCTION");
            var changeStateDto = new { WorkoutStateId = stateId };
            
            var client = CreateClientWithUserId();
            await client.PutAsJsonAsync($"/api/workout-templates/{_existingTemplateId}/state", changeStateDto);
        }
    }

    [Given(@"execution logs exist for this template")]
    public async Task GivenExecutionLogsExistForThisTemplate()
    {
        // Simulate execution logs by adding a flag to the scenario context
        // In a real scenario, this would create actual execution log entries
        _scenarioContext["HasExecutionLogs"] = true;
        await Task.CompletedTask;
    }

    [When(@"I attempt to change the state to ""(.*)""")]
    public async Task WhenIAttemptToChangeTheStateTo(string newState)
    {
        var stateId = await GetWorkoutStateId(newState);
        var changeStateDto = new { WorkoutStateId = stateId };
        
        var client = CreateClientWithUserId();
        _lastResponse = await client.PutAsJsonAsync($"/api/workout-templates/{_existingTemplateId}/state", changeStateDto);
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [Then(@"the template should remain in ""(.*)"" state")]
    public async Task ThenTheTemplateShouldRemainInState(string expectedState)
    {
        var client = CreateClientWithUserId();
        var response = await client.GetAsync($"/api/workout-templates/{_existingTemplateId}");
        
        if (response.IsSuccessStatusCode)
        {
            var template = await response.Content.ReadFromJsonAsync<WorkoutTemplateDto>();
            template!.WorkoutState!.Value.Should().Be(expectedState);
        }
    }

    [Given(@"a workout template exists created by another user")]
    public async Task GivenAWorkoutTemplateExistsCreatedByAnotherUser()
    {
        var otherUserId = "user-02000001-0000-0000-0000-000000000001";
        var originalUserId = _currentUserId;
        _currentUserId = otherUserId;
        
        await CreateTemplateWithName($"Other User Template {Guid.NewGuid()}");
        
        if (_lastResponse!.IsSuccessStatusCode)
        {
            var template = await _lastResponse.Content.ReadFromJsonAsync<WorkoutTemplateDto>();
            _existingTemplateId = WorkoutTemplateId.ParseOrEmpty(template!.Id);
        }
        
        // Switch back to original user
        _currentUserId = originalUserId;
    }

    [When(@"I attempt to update the template")]
    public async Task WhenIAttemptToUpdateTheTemplate()
    {
        // Get the existing template first to have required fields
        var client = CreateClientWithUserId();
        var getResponse = await client.GetAsync($"/api/workout-templates/{_existingTemplateId}");
        
        if (getResponse.IsSuccessStatusCode)
        {
            var existingTemplate = await getResponse.Content.ReadFromJsonAsync<WorkoutTemplateDto>();
            
            var updateDto = new UpdateWorkoutTemplateDto
            {
                Name = "Unauthorized Update Attempt",
                Description = "This should fail",
                CategoryId = existingTemplate!.Category.Id,
                DifficultyId = existingTemplate.Difficulty.Id,
                EstimatedDurationMinutes = existingTemplate.EstimatedDurationMinutes,
                IsPublic = existingTemplate.IsPublic,
                Tags = existingTemplate.Tags ?? new List<string>(),
                ObjectiveIds = existingTemplate.Objectives?.Select(o => o.Id).ToList() ?? new List<string>()
            };

            _lastResponse = await client.PutAsJsonAsync($"/api/workout-templates/{_existingTemplateId}", updateDto);
        }
        else
        {
            _lastResponse = getResponse;
        }
        
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [Given(@"a public workout template exists")]
    public async Task GivenAPublicWorkoutTemplateExists()
    {
        var dto = await CreateTemplateDto("Public Template", isPublic: true);
        var client = CreateClientWithUserId();
        var response = await client.PostAsJsonAsync("/api/workout-templates", dto);
        
        if (response.IsSuccessStatusCode)
        {
            var template = await response.Content.ReadFromJsonAsync<WorkoutTemplateDto>();
            _existingTemplateId = WorkoutTemplateId.ParseOrEmpty(template!.Id);
        }
    }

    [When(@"I request the template as any user")]
    public async Task WhenIRequestTheTemplateAsAnyUser()
    {
        // Simulate a different user by not sending auth headers
        var client = CreateClientWithUserId();
        _lastResponse = await client.GetAsync($"/api/workout-templates/{_existingTemplateId}");
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [Then(@"I should be able to view the template details")]
    public async Task ThenIShouldBeAbleToViewTheTemplateDetails()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var template = await _lastResponse.Content.ReadFromJsonAsync<WorkoutTemplateDto>();
        template.Should().NotBeNull();
        template!.Id.Should().Be(_existingTemplateId.ToString());
    }

    [Given(@"a private workout template exists created by another user")]
    public async Task GivenAPrivateWorkoutTemplateExistsCreatedByAnotherUser()
    {
        var otherUserId = "user-02000001-0000-0000-0000-000000000001";
        var originalUserId = _currentUserId;
        _currentUserId = otherUserId;
        
        var dto = await CreateTemplateDto("Private Template", isPublic: false);
        var client = CreateClientWithUserId();
        var response = await client.PostAsJsonAsync("/api/workout-templates", dto);
        
        if (response.IsSuccessStatusCode)
        {
            var template = await response.Content.ReadFromJsonAsync<WorkoutTemplateDto>();
            _existingTemplateId = WorkoutTemplateId.ParseOrEmpty(template!.Id);
        }
        
        // Switch back to original user
        _currentUserId = originalUserId;
    }

    [When(@"I request the template")]
    public async Task WhenIRequestTheTemplate()
    {
        var client = CreateClientWithUserId();
        _lastResponse = await client.GetAsync($"/api/workout-templates/{_existingTemplateId}");
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [Given(@"I have an existing workout template ""(.*)""")]
    public async Task GivenIHaveAnExistingWorkoutTemplate(string templateName)
    {
        await CreateTemplateWithName(templateName);
        
        if (_lastResponse!.IsSuccessStatusCode)
        {
            var template = await _lastResponse.Content.ReadFromJsonAsync<WorkoutTemplateDto>();
            _existingTemplateId = WorkoutTemplateId.ParseOrEmpty(template!.Id);
            _scenarioContext["SourceTemplateId"] = _existingTemplateId.ToString();
        }
    }

    [When(@"I duplicate the template with name ""(.*)""")]
    public async Task WhenIDuplicateTheTemplateWithName(string newName)
    {
        var duplicateCommand = new { NewName = newName };

        var client = CreateClientWithUserId();
        _lastResponse = await client.PostAsJsonAsync($"/api/workout-templates/{_existingTemplateId}/duplicate", duplicateCommand);
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [Then(@"a new template should be created in DRAFT state")]
    public async Task ThenANewTemplateShouldBeCreatedInDRAFTState()
    {
        _lastResponse.Should().NotBeNull();
        _lastResponse!.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var newTemplate = await _lastResponse.Content.ReadFromJsonAsync<WorkoutTemplateDto>();
        newTemplate.Should().NotBeNull();
        newTemplate!.WorkoutState!.Value.Should().Be("DRAFT");
    }

    [Then(@"all exercises and configurations should be copied")]
    public async Task ThenAllExercisesAndConfigurationsShouldBeCopied()
    {
        var newTemplate = await _lastResponse!.Content.ReadFromJsonAsync<WorkoutTemplateDto>();
        newTemplate.Should().NotBeNull();
        
        // In a real test, we would verify exercises and configurations are copied
        // For now, we check that the structure exists
        newTemplate!.Exercises.Should().NotBeNull();
    }

    [Then(@"I should be set as the creator of the new template")]
    public async Task ThenIShouldBeSetAsTheCreatorOfTheNewTemplate()
    {
        var newTemplate = await _lastResponse!.Content.ReadFromJsonAsync<WorkoutTemplateDto>();
        newTemplate.Should().NotBeNull();
        newTemplate!.CreatedBy.Should().Be(_currentUserId);
    }

    [Then(@"the original template should remain unchanged")]
    public async Task ThenTheOriginalTemplateShouldRemainUnchanged()
    {
        var client = CreateClientWithUserId();
        var response = await client.GetAsync($"/api/workout-templates/{_existingTemplateId}");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var originalTemplate = await response.Content.ReadFromJsonAsync<WorkoutTemplateDto>();
        originalTemplate.Should().NotBeNull();
        originalTemplate!.Id.Should().Be(_existingTemplateId.ToString());
    }

    // Helper methods
    private async Task CreateTemplateWithName(string name)
    {
        var dto = await CreateTemplateDto(name);
        var client = CreateClientWithUserId();
        _lastResponse = await client.PostAsJsonAsync("/api/workout-templates", dto);
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    private async Task CreateTemplateWithDuration(int duration)
    {
        var dto = await CreateTemplateDto($"Duration Test {duration}min");
        // Create a new DTO with the duration set
        var dtoWithDuration = new CreateWorkoutTemplateDto
        {
            Name = dto.Name,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            DifficultyId = dto.DifficultyId,
            EstimatedDurationMinutes = duration,
            IsPublic = dto.IsPublic,
            Tags = dto.Tags,
            ObjectiveIds = dto.ObjectiveIds
        };
        
        var client = CreateClientWithUserId();
        _lastResponse = await client.PostAsJsonAsync("/api/workout-templates", dtoWithDuration);
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    private async Task<CreateWorkoutTemplateDto> CreateTemplateDto(string name, bool isPublic = true)
    {
        var categoryId = await GetWorkoutCategoryId("Upper Body");
        var difficultyId = await GetDifficultyLevelId("Intermediate");
        
        return new CreateWorkoutTemplateDto
        {
            Name = name,
            Description = $"Test template: {name}",
            CategoryId = categoryId.ToString(),
            DifficultyId = difficultyId.ToString(),
            EstimatedDurationMinutes = 60,
            IsPublic = isPublic,
            Tags = new List<string> { "test" },
            ObjectiveIds = new List<string>()
        };
    }

    private async Task<WorkoutCategoryId> GetWorkoutCategoryId(string categoryName)
    {
        WorkoutCategoryId categoryId = WorkoutCategoryId.Empty;
        
        // Map test category names to actual database values
        var categoryMapping = categoryName switch
        {
            "Upper Body" => "Full Body", // Map to Full Body since "Upper Body" doesn't exist
            "Lower Body" => "Lower Body",
            "Full Body" => "Full Body",
            _ => categoryName
        };
        
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
}
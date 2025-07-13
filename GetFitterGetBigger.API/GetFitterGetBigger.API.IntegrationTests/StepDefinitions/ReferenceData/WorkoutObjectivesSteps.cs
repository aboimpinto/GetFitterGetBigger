using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Helpers;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Fixtures;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Microsoft.EntityFrameworkCore;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace GetFitterGetBigger.API.IntegrationTests.StepDefinitions.ReferenceData;

[Binding]
public class WorkoutObjectivesSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly PostgreSqlTestFixture _fixture;
    private WorkoutObjectivesResponseDto? _workoutObjectivesResponse;
    private WorkoutObjectiveDto? _workoutObjectiveResult;
    private Dictionary<string, object>? _errorResponse;

    public WorkoutObjectivesSteps(ScenarioContext scenarioContext, PostgreSqlTestFixture fixture)
    {
        _scenarioContext = scenarioContext;
        _fixture = fixture;
    }

    [Given(@"the following workout objectives exist in the database:")]
    public async Task GivenTheFollowingWorkoutObjectivesExistInTheDatabase(Table table)
    {
        await _fixture.ExecuteDbContextAsync(async context =>
        {
            // Use seeded data via TestDatabaseSeeder
            var seeder = new TestDatabaseSeeder(context);
            await seeder.SeedWorkoutObjectivesAsync();
        });
    }

    [Then(@"the response should contain (\d+) workout objectives")]
    public async Task ThenTheResponseShouldContainWorkoutObjectives(int expectedCount)
    {
        var response = _scenarioContext.GetLastResponse();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        _workoutObjectivesResponse = await response.Content.ReadFromJsonAsync<WorkoutObjectivesResponseDto>();
        _workoutObjectivesResponse.Should().NotBeNull();
        _workoutObjectivesResponse!.WorkoutObjectives.Should().HaveCount(expectedCount);
    }

    [Then(@"each workout objective should have the following fields:")]
    public void ThenEachWorkoutObjectiveShouldHaveTheFollowingFields(Table table)
    {
        _workoutObjectivesResponse.Should().NotBeNull();
        
        foreach (var objective in _workoutObjectivesResponse!.WorkoutObjectives)
        {
            foreach (var row in table.Rows)
            {
                var fieldName = row["Field"];
                var isRequired = bool.Parse(row["Required"]);
                
                // Map camelCase field names to PascalCase property names
                var propertyName = fieldName switch
                {
                    "workoutObjectiveId" => "WorkoutObjectiveId",
                    "value" => "Value", 
                    "description" => "Description",
                    "displayOrder" => "DisplayOrder",
                    "isActive" => "IsActive",
                    _ => fieldName
                };
                
                var property = typeof(WorkoutObjectiveDto).GetProperty(propertyName);
                property.Should().NotBeNull($"Field {fieldName} should exist");
                
                if (isRequired)
                {
                    var value = property!.GetValue(objective);
                    value.Should().NotBeNull($"Required field {fieldName} should not be null");
                }
            }
        }
    }

    [Then(@"the workout objectives should be ordered by displayOrder ascending")]
    public void ThenTheWorkoutObjectivesShouldBeOrderedByDisplayOrderAscending()
    {
        _workoutObjectivesResponse.Should().NotBeNull();
        _workoutObjectivesResponse!.WorkoutObjectives
            .Should().BeInAscendingOrder(x => x.DisplayOrder);
    }

    [Then(@"no inactive objectives should be included")]
    public void ThenNoInactiveObjectivesShouldBeIncluded()
    {
        _workoutObjectivesResponse.Should().NotBeNull();
        _workoutObjectivesResponse!.WorkoutObjectives
            .Should().OnlyContain(x => x.IsActive == true);
    }

    [Then(@"the response should include both active and inactive objectives")]
    public void ThenTheResponseShouldIncludeBothActiveAndInactiveObjectives()
    {
        _workoutObjectivesResponse.Should().NotBeNull();
        _workoutObjectivesResponse!.WorkoutObjectives
            .Should().Contain(x => x.IsActive == true)
            .And.Contain(x => x.IsActive == false);
    }

    [Then(@"the response should contain a workout objective with:")]
    public async Task ThenTheResponseShouldContainAWorkoutObjectiveWith(Table table)
    {
        var response = _scenarioContext.GetLastResponse();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        _workoutObjectiveResult = await response.Content.ReadFromJsonAsync<WorkoutObjectiveDto>();
        _workoutObjectiveResult.Should().NotBeNull();
        
        foreach (var row in table.Rows)
        {
            var fieldName = row["Field"];
            var expectedValue = row["Value"];
            
            // Map camelCase field names to PascalCase property names
            var propertyName = fieldName switch
            {
                "workoutObjectiveId" => "WorkoutObjectiveId",
                "value" => "Value", 
                "description" => "Description",
                "displayOrder" => "DisplayOrder",
                "isActive" => "IsActive",
                _ => fieldName
            };
            
            var property = typeof(WorkoutObjectiveDto).GetProperty(propertyName);
            property.Should().NotBeNull($"Field {fieldName} should exist");
            
            var value = property!.GetValue(_workoutObjectiveResult);
            var actualValue = value is bool boolValue ? boolValue.ToString().ToLower() : value?.ToString();
            actualValue.Should().Be(expectedValue, $"Field {fieldName} should have value {expectedValue}");
        }
    }

    [Then(@"the response should contain an error with:")]
    public async Task ThenTheResponseShouldContainAnErrorWith(Table table)
    {
        var response = _scenarioContext.GetLastResponse();
        var content = await response.Content.ReadAsStringAsync();
        
        _errorResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
        _errorResponse.Should().NotBeNull();
        
        foreach (var row in table.Rows)
        {
            var fieldName = row["Field"];
            var expectedValue = row["Value"];
            
            _errorResponse.Should().ContainKey(fieldName);
            _errorResponse![fieldName].ToString().Should().Be(expectedValue);
        }
    }

    [Then(@"the response should have cache control headers")]
    public void ThenTheResponseShouldHaveCacheControlHeaders()
    {
        var response = _scenarioContext.GetLastResponse();
        response.Headers.CacheControl.Should().NotBeNull();
    }

    [Then(@"the cache duration should be (\d+) seconds")]
    public void ThenTheCacheDurationShouldBeSeconds(int expectedSeconds)
    {
        var response = _scenarioContext.GetLastResponse();
        response.Headers.CacheControl.Should().NotBeNull();
        response.Headers.CacheControl!.MaxAge.Should().Be(TimeSpan.FromSeconds(expectedSeconds));
    }
}
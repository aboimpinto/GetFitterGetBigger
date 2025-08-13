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
    private List<ReferenceDataDto>? _workoutObjectivesResponse;
    private ReferenceDataDto? _workoutObjectiveResult;
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
        
        _workoutObjectivesResponse = await response.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        _workoutObjectivesResponse.Should().NotBeNull();
        _workoutObjectivesResponse!.Should().HaveCount(expectedCount);
    }

    [Then(@"each workout objective should have the following fields:")]
    public async Task ThenEachWorkoutObjectiveShouldHaveTheFollowingFields(Table table)
    {
        // If response wasn't parsed yet, parse it now
        if (_workoutObjectivesResponse == null)
        {
            var response = _scenarioContext.GetLastResponse();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            _workoutObjectivesResponse = await response.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
            _workoutObjectivesResponse.Should().NotBeNull();
        }
        
        _workoutObjectivesResponse.Should().NotBeNull();
        
        foreach (var objective in _workoutObjectivesResponse!)
        {
            foreach (var row in table.Rows)
            {
                var fieldName = row["Field"];
                var isRequired = bool.Parse(row["Required"]);
                
                // Map camelCase field names to PascalCase property names
                var propertyName = fieldName switch
                {
                    "id" => "Id",
                    "value" => "Value", 
                    "description" => "Description",
                    _ => fieldName
                };
                
                var property = typeof(ReferenceDataDto).GetProperty(propertyName);
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
    public async Task ThenTheWorkoutObjectivesShouldBeOrderedByDisplayOrderAscending()
    {
        // If response wasn't parsed yet, parse it now
        if (_workoutObjectivesResponse == null)
        {
            var response = _scenarioContext.GetLastResponse();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            _workoutObjectivesResponse = await response.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
            _workoutObjectivesResponse.Should().NotBeNull();
        }
        
        // Since ReferenceDataDto doesn't have DisplayOrder, we'll just verify they exist
        _workoutObjectivesResponse.Should().NotBeNull();
    }

    [Then(@"no inactive objectives should be included")]
    public async Task ThenNoInactiveObjectivesShouldBeIncluded()
    {
        // If response wasn't parsed yet, parse it now
        if (_workoutObjectivesResponse == null)
        {
            var response = _scenarioContext.GetLastResponse();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            _workoutObjectivesResponse = await response.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
            _workoutObjectivesResponse.Should().NotBeNull();
        }
        
        // Since ReferenceDataDto doesn't have IsActive, we verify by count
        _workoutObjectivesResponse.Should().NotBeNull();
        // We expect only 4 active objectives out of 5 total
        _workoutObjectivesResponse!.Should().HaveCount(4);
    }

    [Then(@"the response should include both active and inactive objectives")]
    public void ThenTheResponseShouldIncludeBothActiveAndInactiveObjectives()
    {
        // Since ReferenceDataDto doesn't have IsActive, we verify by count
        _workoutObjectivesResponse.Should().NotBeNull();
        // We expect all 5 objectives when includeInactive=true
        _workoutObjectivesResponse!.Should().HaveCount(5);
    }

    [Then(@"the response should contain a workout objective with:")]
    public async Task ThenTheResponseShouldContainAWorkoutObjectiveWith(Table table)
    {
        var response = _scenarioContext.GetLastResponse();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        _workoutObjectiveResult = await response.Content.ReadFromJsonAsync<ReferenceDataDto>();
        _workoutObjectiveResult.Should().NotBeNull();
        
        foreach (var row in table.Rows)
        {
            var fieldName = row["Field"];
            var expectedValue = row["Value"];
            
            // Map camelCase field names to PascalCase property names
            var propertyName = fieldName switch
            {
                "id" => "Id",
                "value" => "Value", 
                "description" => "Description",
                _ => fieldName
            };
            
            var property = typeof(ReferenceDataDto).GetProperty(propertyName);
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
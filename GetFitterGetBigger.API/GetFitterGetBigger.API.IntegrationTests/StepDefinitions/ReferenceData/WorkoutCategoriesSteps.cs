using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
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
public class WorkoutCategoriesSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly PostgreSqlTestFixture _fixture;
    private WorkoutCategoriesResponseDto? _workoutCategoriesResponse;
    private WorkoutCategoryDto? _workoutCategoryResult;

    public WorkoutCategoriesSteps(ScenarioContext scenarioContext, PostgreSqlTestFixture fixture)
    {
        _scenarioContext = scenarioContext;
        _fixture = fixture;
    }

    [Given(@"the following workout categories exist in the database:")]
    public async Task GivenTheFollowingWorkoutCategoriesExistInTheDatabase(Table table)
    {
        await _fixture.ExecuteDbContextAsync(async context =>
        {
            // Use seeded data via TestDatabaseSeeder
            var seeder = new TestDatabaseSeeder(context);
            await seeder.SeedWorkoutCategoriesAsync();
        });
    }

    [Then(@"the response should contain (\d+) workout categories")]
    public async Task ThenTheResponseShouldContainWorkoutCategories(int expectedCount)
    {
        var response = _scenarioContext.GetLastResponse();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        _workoutCategoriesResponse = await response.Content.ReadFromJsonAsync<WorkoutCategoriesResponseDto>();
        _workoutCategoriesResponse.Should().NotBeNull();
        _workoutCategoriesResponse!.WorkoutCategories.Should().HaveCount(expectedCount);
    }

    [Then(@"each workout category should have the following fields:")]
    public void ThenEachWorkoutCategoryShouldHaveTheFollowingFields(Table table)
    {
        _workoutCategoriesResponse.Should().NotBeNull();
        
        foreach (var category in _workoutCategoriesResponse!.WorkoutCategories)
        {
            foreach (var row in table.Rows)
            {
                var fieldName = row["Field"];
                var isRequired = bool.Parse(row["Required"]);
                
                // Map camelCase field names to PascalCase property names
                var propertyName = fieldName switch
                {
                    "workoutCategoryId" => "WorkoutCategoryId",
                    "value" => "Value", 
                    "description" => "Description",
                    "icon" => "Icon",
                    "color" => "Color",
                    "primaryMuscleGroups" => "PrimaryMuscleGroups",
                    "displayOrder" => "DisplayOrder",
                    "isActive" => "IsActive",
                    _ => fieldName
                };
                
                var property = typeof(WorkoutCategoryDto).GetProperty(propertyName);
                property.Should().NotBeNull($"Field {fieldName} should exist");
                
                if (isRequired)
                {
                    var value = property!.GetValue(category);
                    value.Should().NotBeNull($"Required field {fieldName} should not be null");
                }
            }
        }
    }

    [Then(@"the workout categories should be ordered by displayOrder ascending")]
    public void ThenTheWorkoutCategoriesShouldBeOrderedByDisplayOrderAscending()
    {
        _workoutCategoriesResponse.Should().NotBeNull();
        _workoutCategoriesResponse!.WorkoutCategories
            .Should().BeInAscendingOrder(x => x.DisplayOrder);
    }

    [Then(@"no inactive categories should be included")]
    public void ThenNoInactiveCategoriesShouldBeIncluded()
    {
        _workoutCategoriesResponse.Should().NotBeNull();
        _workoutCategoriesResponse!.WorkoutCategories
            .Should().OnlyContain(x => x.IsActive == true);
    }

    [Then(@"the response should include both active and inactive categories")]
    public void ThenTheResponseShouldIncludeBothActiveAndInactiveCategories()
    {
        _workoutCategoriesResponse.Should().NotBeNull();
        _workoutCategoriesResponse!.WorkoutCategories
            .Should().Contain(x => x.IsActive == true)
            .And.Contain(x => x.IsActive == false);
    }

    [Then(@"the response should contain a workout category with:")]
    public async Task ThenTheResponseShouldContainAWorkoutCategoryWith(Table table)
    {
        var response = _scenarioContext.GetLastResponse();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        _workoutCategoryResult = await response.Content.ReadFromJsonAsync<WorkoutCategoryDto>();
        _workoutCategoryResult.Should().NotBeNull();
        
        foreach (var row in table.Rows)
        {
            var fieldName = row["Field"];
            var expectedValue = row["Value"];
            
            // Map camelCase field names to PascalCase property names
            var propertyName = fieldName switch
            {
                "workoutCategoryId" => "WorkoutCategoryId",
                "value" => "Value", 
                "description" => "Description",
                "icon" => "Icon",
                "color" => "Color",
                "primaryMuscleGroups" => "PrimaryMuscleGroups",
                "displayOrder" => "DisplayOrder",
                "isActive" => "IsActive",
                _ => fieldName
            };
            
            var property = typeof(WorkoutCategoryDto).GetProperty(propertyName);
            property.Should().NotBeNull($"Field {fieldName} should exist");
            
            var value = property!.GetValue(_workoutCategoryResult);
            var actualValue = value is bool boolValue ? boolValue.ToString().ToLower() : value?.ToString();
            actualValue.Should().Be(expectedValue, $"Field {fieldName} should have value {expectedValue}");
        }
    }

    [Then(@"each category's icon field should contain a valid emoji character")]
    public async Task ThenEachCategorysIconFieldShouldContainAValidEmojiCharacter()
    {
        // If response wasn't parsed yet, parse it now
        if (_workoutCategoriesResponse == null)
        {
            var response = _scenarioContext.GetLastResponse();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            _workoutCategoriesResponse = await response.Content.ReadFromJsonAsync<WorkoutCategoriesResponseDto>();
        }
        
        _workoutCategoriesResponse.Should().NotBeNull();
        
        foreach (var category in _workoutCategoriesResponse!.WorkoutCategories)
        {
            category.Icon.Should().NotBeNullOrEmpty();
            // For simplicity, just check that the icon field is not empty
            // More complex emoji validation would require additional libraries
            category.Icon.Length.Should().BeGreaterThan(0);
            
            // Basic check: emojis typically result in strings with length > 1 due to Unicode encoding
            // or contain characters outside the basic ASCII range
            var hasNonAscii = category.Icon.Any(c => c > 127);
            hasNonAscii.Should().BeTrue($"Icon '{category.Icon}' should contain emoji characters");
        }
    }
}
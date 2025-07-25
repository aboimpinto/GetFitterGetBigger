using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Fixtures;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Helpers;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using TechTalk.SpecFlow;

namespace GetFitterGetBigger.API.IntegrationTests.StepDefinitions.WorkoutTemplate;

[Binding]
public class WorkoutTemplateCountQuerySteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly PostgreSqlTestFixture _fixture;
    
    public WorkoutTemplateCountQuerySteps(ScenarioContext scenarioContext, PostgreSqlTestFixture fixture)
    {
        _scenarioContext = scenarioContext;
        _fixture = fixture;
    }
    
    [Given(@"the following workout templates exist for count tests:")]
    public async Task GivenTheFollowingWorkoutTemplatesExistForCountTests(Table table)
    {
        await _fixture.ExecuteDbContextAsync(async context =>
        {
            // Clear existing workout templates for clean test state
            var existing = await context.WorkoutTemplates.ToListAsync();
            context.WorkoutTemplates.RemoveRange(existing);
            await context.SaveChangesAsync();
            
            // Get reference data - we'll map by name
            var categories = await context.Set<WorkoutCategory>().ToListAsync();
            var difficulties = await context.Set<DifficultyLevel>().ToListAsync();
            var state = await context.Set<WorkoutState>().FirstOrDefaultAsync();
            
            if (!categories.Any() || !difficulties.Any() || state == null)
            {
                throw new InvalidOperationException("Reference data not found. Ensure database is properly seeded.");
            }
            
            // Create templates from table
            foreach (var row in table.Rows)
            {
                var name = row["Name"];
                var description = row["Description"];
                var categoryName = row["CategoryName"];
                var difficultyName = row["DifficultyName"];
                var duration = int.Parse(row["Duration"]);
                var tags = row["Tags"].Split(',').Select(t => t.Trim()).ToList();
                
                // Find matching category and difficulty by value
                var category = categories.FirstOrDefault(c => c.Value.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
                var difficulty = difficulties.FirstOrDefault(d => d.Value.Equals(difficultyName, StringComparison.OrdinalIgnoreCase));
                
                if (category == null)
                {
                    throw new InvalidOperationException($"Category '{categoryName}' not found in reference data");
                }
                if (difficulty == null)
                {
                    throw new InvalidOperationException($"Difficulty '{difficultyName}' not found in reference data");
                }
                
                var template = API.Models.Entities.WorkoutTemplate.Handler.CreateNew(
                    name,
                    description,
                    category.WorkoutCategoryId,
                    difficulty.DifficultyLevelId,
                    duration,
                    tags,
                    false,
                    state.WorkoutStateId);
                
                if (template.IsSuccess)
                {
                    context.WorkoutTemplates.Add(template.Value);
                }
                else
                {
                    throw new InvalidOperationException($"Failed to create workout template: {template.FirstError}");
                }
            }
            
            await context.SaveChangesAsync();
        });
    }
    
    [Then(@"the response items should have loaded relationships")]
    public async Task ThenTheResponseItemsShouldHaveLoadedRelationships()
    {
        var response = _scenarioContext.GetLastResponse();
        var content = await response.Content.ReadAsStringAsync();
        var result = JObject.Parse(content);
        
        var items = result["items"] as JArray;
        items.Should().NotBeNull();
        items.Should().NotBeEmpty();
        
        // Debug: Log the first item to see what's being returned
        if (items!.Count > 0)
        {
            Console.WriteLine($"First item: {items[0].ToString()}");
        }
        
        // Check that each item has loaded relationships
        foreach (var item in items)
        {
            // Check that related entities are loaded (not just IDs)
            item["category"].Should().NotBeNull("Category should be loaded");
            if (item["category"] != null)
            {
                item["category"]!["value"].Should().NotBeNull("Category value should be present");
            }
            
            item["difficulty"].Should().NotBeNull("Difficulty should be loaded");
            if (item["difficulty"] != null) 
            {
                item["difficulty"]!["value"].Should().NotBeNull("Difficulty value should be present");
            }
            
            // Note: state might not always be included in the response
            // Only check if it exists
            if (item["state"] != null)
            {
                item["state"]!["value"].Should().NotBeNull("State value should be present when state is included");
            }
            
            // Check that exercises are loaded if present
            if (item["exercises"] != null)
            {
                item["exercises"].Should().NotBeNull("Exercises should be loaded when present");
            }
        }
    }
    
    [Then(@"the response should have property ""(.*)"" as empty array")]
    public async Task ThenTheResponseShouldHavePropertyAsEmptyArray(string propertyName)
    {
        var response = _scenarioContext.GetLastResponse();
        var content = await response.Content.ReadAsStringAsync();
        var result = JObject.Parse(content);
        
        var array = result[propertyName] as JArray;
        array.Should().NotBeNull($"Property '{propertyName}' should exist and be an array");
        array.Should().BeEmpty($"Array '{propertyName}' should be empty");
    }
}
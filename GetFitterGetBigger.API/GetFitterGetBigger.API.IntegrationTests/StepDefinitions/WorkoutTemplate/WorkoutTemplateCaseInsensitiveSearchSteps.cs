using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Fixtures;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Helpers;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace GetFitterGetBigger.API.IntegrationTests.StepDefinitions.WorkoutTemplate;

[Binding]
public class WorkoutTemplateCaseInsensitiveSearchSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly PostgreSqlTestFixture _fixture;
    
    public WorkoutTemplateCaseInsensitiveSearchSteps(ScenarioContext scenarioContext, PostgreSqlTestFixture fixture)
    {
        _scenarioContext = scenarioContext;
        _fixture = fixture;
    }
    
    [Given(@"the following workout templates exist for case insensitive search:")]
    public async Task GivenTheFollowingWorkoutTemplatesExistForCaseInsensitiveSearch(Table table)
    {
        await _fixture.ExecuteDbContextAsync(async context =>
        {
            // Clear existing workout templates for clean test state
            var existing = await context.WorkoutTemplates.ToListAsync();
            context.WorkoutTemplates.RemoveRange(existing);
            await context.SaveChangesAsync();
            
            // Get reference data
            var category = await context.Set<WorkoutCategory>().FirstOrDefaultAsync();
            var difficulty = await context.Set<DifficultyLevel>().FirstOrDefaultAsync();
            // Use DRAFT state to ensure templates are visible in queries (not ARCHIVED)
            var state = await context.Set<WorkoutState>()
                .FirstOrDefaultAsync(s => s.Value == "DRAFT");
            var executionProtocol = await context.Set<ExecutionProtocol>()
                .FirstOrDefaultAsync(p => p.Value == "Reps and Sets");
            
            if (category == null || difficulty == null || state == null || executionProtocol == null)
            {
                throw new InvalidOperationException("Reference data not found. Ensure database is properly seeded.");
            }
            
            // Create templates from table
            foreach (var row in table.Rows)
            {
                var name = row["Name"];
                var description = row["Description"];
                var duration = int.Parse(row["Duration"]);
                var tags = row["Tags"].Split(',').Select(t => t.Trim()).ToList();
                
                var template = API.Models.Entities.WorkoutTemplate.Handler.CreateNew(
                    name,
                    description,
                    category.WorkoutCategoryId,
                    difficulty.DifficultyLevelId,
                    duration,
                    tags,
                    false,
                    state.WorkoutStateId,
                    executionProtocol.ExecutionProtocolId);
                
                if (template.IsSuccess)
                {
                    context.WorkoutTemplates.Add(template.Value);
                }
            }
            
            await context.SaveChangesAsync();
            
            // Debug log
            var count = await context.WorkoutTemplates.CountAsync();
            Console.WriteLine($"Seeded {table.Rows.Count} workout templates. Total in DB: {count}");
        });
    }
    
    [Given(@"the following additional workout templates exist for case insensitive search:")]
    public async Task GivenTheFollowingAdditionalWorkoutTemplatesExistForCaseInsensitiveSearch(Table table)
    {
        await _fixture.ExecuteDbContextAsync(async context =>
        {
            // Get reference data
            var category = await context.Set<WorkoutCategory>().FirstOrDefaultAsync();
            var difficulty = await context.Set<DifficultyLevel>().FirstOrDefaultAsync();
            // Use DRAFT state to ensure templates are visible in queries (not ARCHIVED)
            var state = await context.Set<WorkoutState>()
                .FirstOrDefaultAsync(s => s.Value == "DRAFT");
            var executionProtocol = await context.Set<ExecutionProtocol>()
                .FirstOrDefaultAsync(p => p.Value == "Reps and Sets");
            
            if (category == null || difficulty == null || state == null || executionProtocol == null)
            {
                throw new InvalidOperationException("Reference data not found.");
            }
            
            // Add additional templates from table
            foreach (var row in table.Rows)
            {
                var name = row["Name"];
                var description = row["Description"];
                var duration = int.Parse(row["Duration"]);
                var tags = row["Tags"].Split(',').Select(t => t.Trim()).ToList();
                
                var template = API.Models.Entities.WorkoutTemplate.Handler.CreateNew(
                    name,
                    description,
                    category.WorkoutCategoryId,
                    difficulty.DifficultyLevelId,
                    duration,
                    tags,
                    false,
                    state.WorkoutStateId,
                    executionProtocol.ExecutionProtocolId);
                
                if (template.IsSuccess)
                {
                    context.WorkoutTemplates.Add(template.Value);
                }
            }
            
            await context.SaveChangesAsync();
        });
    }
    
    [Then(@"the response items should contain templates with names:")]
    public async Task ThenTheResponseItemsShouldContainTemplatesWithNames(Table expectedNames)
    {
        var response = _scenarioContext.GetLastResponse();
        var content = await response.Content.ReadAsStringAsync();
        var result = JObject.Parse(content);
        
        var items = result["items"] as JArray;
        items.Should().NotBeNull();
        
        var actualNames = items!.Select(item => item["name"]?.ToString()).ToList();
        var expectedNamesList = expectedNames.Rows.Select(r => r["Name"]).ToList();
        
        // Log for debugging
        Console.WriteLine($"Actual names found: {string.Join(", ", actualNames)}");
        Console.WriteLine($"Expected names: {string.Join(", ", expectedNamesList)}");
        
        actualNames.Should().BeEquivalentTo(expectedNamesList);
    }
    
    [Then(@"all response items should contain ""(.*)"" in name \(case insensitive\)")]
    public async Task ThenAllResponseItemsShouldContainInNameCaseInsensitive(string pattern)
    {
        var response = _scenarioContext.GetLastResponse();
        var content = await response.Content.ReadAsStringAsync();
        var result = JObject.Parse(content);
        
        var items = result["items"] as JArray;
        items.Should().NotBeNull();
        
        foreach (var item in items!)
        {
            var name = item["name"]?.ToString();
            name.Should().NotBeNullOrEmpty();
            name!.ToLower().Should().Contain(pattern.ToLower());
        }
    }
    
    [Then(@"the response items should be sorted by name ascending")]
    public async Task ThenTheResponseItemsShouldBeSortedByNameAscending()
    {
        var response = _scenarioContext.GetLastResponse();
        var content = await response.Content.ReadAsStringAsync();
        var result = JObject.Parse(content);
        
        var items = result["items"] as JArray;
        items.Should().NotBeNull();
        
        var names = items!.Select(item => item["name"]?.ToString()).ToList();
        names.Should().BeInAscendingOrder();
    }
    
    [Then(@"the response should have property ""(.*)"" as array with (\d+) items?")]
    public async Task ThenTheResponseShouldHavePropertyAsArrayWithItems(string propertyName, int expectedCount)
    {
        var response = _scenarioContext.GetLastResponse();
        var content = await response.Content.ReadAsStringAsync();
        
        using var jsonDocument = JsonDocument.Parse(content);
        var root = jsonDocument.RootElement;
        
        root.TryGetProperty(propertyName, out var property).Should().BeTrue($"Expected property '{propertyName}' to exist");
        property.ValueKind.Should().Be(JsonValueKind.Array, $"Expected property '{propertyName}' to be an array");
        
        property.GetArrayLength().Should().Be(expectedCount, 
            $"expected {expectedCount} items in '{propertyName}' array but found {property.GetArrayLength()}");
    }
}
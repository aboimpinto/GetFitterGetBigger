using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Fixtures;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Helpers;
using System.Text.Json;
using FluentAssertions;
using TechTalk.SpecFlow;
using Xunit;

namespace GetFitterGetBigger.API.IntegrationTests.StepDefinitions.ReferenceData;

[Binding]
public class BodyPartsCachingSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly PostgreSqlTestFixture _fixture;
    
    public BodyPartsCachingSteps(ScenarioContext scenarioContext, PostgreSqlTestFixture fixture)
    {
        _scenarioContext = scenarioContext;
        _fixture = fixture;
    }
    
    [Given(@"I am tracking database queries")]
    public void GivenIAmTrackingDatabaseQueries()
    {
        // Reset the query tracker
        var tracker = _fixture.Factory.GetQueryTracker();
        tracker?.Reset();
    }
    
    [Given(@"I reset the database query counter")]
    public void GivenIResetTheDatabaseQueryCounter()
    {
        // Reset the query tracker
        var tracker = _fixture.Factory.GetQueryTracker();
        tracker?.Reset();
    }
    
    [Then(@"the database query count should be (.*)")]
    public void ThenTheDatabaseQueryCountShouldBe(int expectedCount)
    {
        var tracker = _fixture.Factory.GetQueryTracker();
        if (tracker == null)
        {
            throw new InvalidOperationException("Database query tracker is not initialized");
        }
        
        // Count actual SELECT queries on the BodyParts table
        var actualCount = tracker.GetQueryCountForTable("BodyParts");
        
        Assert.True(actualCount == expectedCount, 
            $"Expected {expectedCount} database queries to BodyParts table but found {actualCount}. " +
            $"This indicates that caching is {(actualCount > expectedCount ? "NOT" : "")} working properly. " +
            $"{(actualCount > expectedCount ? "Each API request is hitting the database instead of using cache." : "Cache is working as expected.")}");
    }
    
    [Given(@"the response contains at least (\d+) items")]
    [Then(@"the response contains at least (\d+) items")]
    public void ThenTheResponseContainsAtLeastItems(int minimumCount)
    {
        var content = _scenarioContext.GetLastResponseContent();
        var jsonDocument = JsonDocument.Parse(content);
        
        jsonDocument.RootElement.ValueKind.Should().Be(JsonValueKind.Array, 
            "expected response to be a JSON array");
        
        jsonDocument.RootElement.GetArrayLength().Should().BeGreaterOrEqualTo(minimumCount,
            $"expected array to have at least {minimumCount} items");
    }
    
    [Given(@"I store the second item from the response as ""(.*)""")]
    [Then(@"I store the second item from the response as ""(.*)""")]
    public void ThenIStoreTheSecondItemFromTheResponseAs(string variableName)
    {
        var content = _scenarioContext.GetLastResponseContent();
        var jsonDocument = JsonDocument.Parse(content);
        
        jsonDocument.RootElement.ValueKind.Should().Be(JsonValueKind.Array, 
            "expected response to be a JSON array");
        
        jsonDocument.RootElement.GetArrayLength().Should().BeGreaterOrEqualTo(2,
            "expected array to have at least 2 items to get the second item");
        
        var secondItem = jsonDocument.RootElement[1];
        
        // Store as JSON string to avoid value type issues
        _scenarioContext.SetTestData(variableName, secondItem.GetRawText());
    }
    
    [Given(@"the database has at least (\d+) body parts")]
    public async Task GivenTheDatabaseHasAtLeastBodyParts(int minimumCount)
    {
        // First ensure we have reference data
        var databaseSteps = new StepDefinitions.Database.DatabaseSteps(_scenarioContext, _fixture);
        await Task.Run(() => databaseSteps.GivenTheDatabaseHasReferenceData());
        
        // Then get all body parts to verify count
        var httpClient = _scenarioContext.GetHttpClient();
        var response = await httpClient.GetAsync("/api/ReferenceTables/BodyParts");
        var content = await response.Content.ReadAsStringAsync();
        
        var jsonDocument = JsonDocument.Parse(content);
        jsonDocument.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        jsonDocument.RootElement.GetArrayLength().Should().BeGreaterOrEqualTo(minimumCount,
            $"Database should have at least {minimumCount} body parts");
        
        // Store the response for use by subsequent steps
        _scenarioContext.SetLastResponse(response);
        _scenarioContext.SetLastResponseContent(content);
    }
    
    [Given(@"I store the first body part ID as ""(.*)""")]
    public void GivenIStoreTheFirstBodyPartIdAs(string variableName)
    {
        var content = _scenarioContext.GetLastResponseContent();
        var jsonDocument = JsonDocument.Parse(content);
        
        jsonDocument.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        jsonDocument.RootElement.GetArrayLength().Should().BeGreaterThan(0,
            "Expected at least one body part to get the first ID");
        
        var firstItem = jsonDocument.RootElement[0];
        if (firstItem.TryGetProperty("id", out var idProperty))
        {
            _scenarioContext.SetTestData(variableName, idProperty.GetString()!);
        }
        else
        {
            throw new InvalidOperationException("First body part does not have an 'id' property");
        }
    }
    
    [Given(@"I store the second body part ID as ""(.*)""")]
    public void GivenIStoreTheSecondBodyPartIdAs(string variableName)
    {
        var content = _scenarioContext.GetLastResponseContent();
        var jsonDocument = JsonDocument.Parse(content);
        
        jsonDocument.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        jsonDocument.RootElement.GetArrayLength().Should().BeGreaterOrEqualTo(2,
            "Expected at least 2 body parts to get the second ID");
        
        var secondItem = jsonDocument.RootElement[1];
        if (secondItem.TryGetProperty("id", out var idProperty))
        {
            _scenarioContext.SetTestData(variableName, idProperty.GetString()!);
        }
        else
        {
            throw new InvalidOperationException("Second body part does not have an 'id' property");
        }
    }
}
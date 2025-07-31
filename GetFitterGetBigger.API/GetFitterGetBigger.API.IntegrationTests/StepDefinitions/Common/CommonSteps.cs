using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Fixtures;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Helpers;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API.IntegrationTests.TestBuilders;
using Microsoft.Extensions.DependencyInjection;
using TechTalk.SpecFlow;

namespace GetFitterGetBigger.API.IntegrationTests.StepDefinitions.Common;

[Binding]
public class CommonSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly PostgreSqlTestFixture _fixture;
    
    public CommonSteps(ScenarioContext scenarioContext, PostgreSqlTestFixture fixture)
    {
        _scenarioContext = scenarioContext;
        _fixture = fixture;
    }
    
    [Given(@"I wait for (\d+) seconds?")]
    [When(@"I wait for (\d+) seconds?")]
    [Then(@"I wait for (\d+) seconds?")]
    public async Task WaitForSeconds(int seconds)
    {
        await Task.Delay(TimeSpan.FromSeconds(seconds));
    }
    
    [Given(@"I wait for (\d+) milliseconds?")]
    [When(@"I wait for (\d+) milliseconds?")]
    [Then(@"I wait for (\d+) milliseconds?")]
    public async Task WaitForMilliseconds(int milliseconds)
    {
        await Task.Delay(milliseconds);
    }
    
    [Given(@"I set the variable ""(.*)"" to ""(.*)""")]
    [When(@"I set the variable ""(.*)"" to ""(.*)""")]
    public void SetVariable(string variableName, string value)
    {
        var resolvedValue = _scenarioContext.ResolvePlaceholders(value);
        _scenarioContext.SetTestData(variableName, resolvedValue);
    }
    
    [Given(@"I generate a new GUID as ""(.*)""")]
    [When(@"I generate a new GUID as ""(.*)""")]
    public void GenerateNewGuid(string variableName)
    {
        var guid = Guid.NewGuid().ToString();
        _scenarioContext.SetTestData(variableName, guid);
    }
    
    [Given(@"I generate a random number between (\d+) and (\d+) as ""(.*)""")]
    [When(@"I generate a random number between (\d+) and (\d+) as ""(.*)""")]
    public void GenerateRandomNumber(int min, int max, string variableName)
    {
        var random = new Random();
        var number = random.Next(min, max + 1);
        _scenarioContext.SetTestData(variableName, number.ToString());
    }
    
    [Given(@"I generate a timestamp as ""(.*)""")]
    [When(@"I generate a timestamp as ""(.*)""")]
    public void GenerateTimestamp(string variableName)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        _scenarioContext.SetTestData(variableName, timestamp);
    }
    
    [Given(@"today is ""(.*)""")]
    public void TodayIs(string date)
    {
        // This could be used to mock date/time in tests if needed
        _scenarioContext.SetTestData("Today", date);
    }
    
    [Then(@"debug: print ""(.*)""")]
    public void DebugPrint(string message)
    {
        var resolvedMessage = _scenarioContext.ResolvePlaceholders(message);
        Console.WriteLine($"[DEBUG] {resolvedMessage}");
    }
    
    [Then(@"debug: print the response")]
    public void DebugPrintResponse()
    {
        var content = _scenarioContext.GetLastResponseContent();
        var statusCode = _scenarioContext.GetLastResponseStatusCode();
        
        Console.WriteLine($"[DEBUG] Response Status: {statusCode}");
        Console.WriteLine($"[DEBUG] Response Content: {content}");
    }
    
    [Then(@"debug: print all variables")]
    public void DebugPrintAllVariables()
    {
        Console.WriteLine("[DEBUG] All ScenarioContext Variables:");
        
        foreach (var key in _scenarioContext.Keys)
        {
            if (key.StartsWith("TestData_"))
            {
                var variableName = key.Substring("TestData_".Length);
                var value = _scenarioContext[key];
                Console.WriteLine($"  {variableName} = {value}");
            }
        }
    }
    
    [Given(@"the test is tagged with ""(.*)""")]
    public void TheTestIsTaggedWith(string tag)
    {
        // This can be used in combination with scenario tags
        // to conditionally execute certain steps
        var scenarioInfo = _scenarioContext.ScenarioInfo;
        if (!scenarioInfo.Tags.Contains(tag))
        {
            throw new PendingStepException($"This scenario requires the @{tag} tag");
        }
    }
    
    [Given(@"the feature flag ""(.*)"" is (enabled|disabled)")]
    public void TheFeatureFlagIs(string flagName, string state)
    {
        // This could be extended to actually configure feature flags
        // For now, just store the state
        var isEnabled = state.ToLower() == "enabled";
        _scenarioContext.SetTestData($"FeatureFlag_{flagName}", isEnabled.ToString());
    }
    
    [Given(@"the system has been initialized with seed data")]
    public async Task GivenTheSystemHasBeenInitializedWithSeedData()
    {
        // Initialize seed data using SeedDataBuilder
        await _fixture.ExecuteDbContextAsync(async context =>
        {
            var seedBuilder = new SeedDataBuilder(context);
            await seedBuilder.WithAllReferenceDataAsync();
        });
    }

    [AfterScenario]
    public async Task AfterScenario()
    {
        // Clean up any created entities
        if (_scenarioContext.ScenarioExecutionStatus == ScenarioExecutionStatus.TestError)
        {
            // Log error details for debugging
            Console.WriteLine($"Scenario failed: {_scenarioContext.ScenarioInfo.Title}");
            
            if (_scenarioContext.TestError != null)
            {
                Console.WriteLine($"Error: {_scenarioContext.TestError.Message}");
                Console.WriteLine($"Stack Trace: {_scenarioContext.TestError.StackTrace}");
            }
        }
    }
}
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Fixtures;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Helpers;
using Microsoft.Extensions.DependencyInjection;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Infrastructure;

namespace GetFitterGetBigger.API.IntegrationTests.Hooks;

[Binding]
public class WebApiHooks
{
    private readonly ScenarioContext _scenarioContext;
    private readonly ISpecFlowOutputHelper _outputHelper;
    private readonly PostgreSqlTestFixture _fixture;
    
    public WebApiHooks(ScenarioContext scenarioContext, ISpecFlowOutputHelper outputHelper, PostgreSqlTestFixture fixture)
    {
        _scenarioContext = scenarioContext;
        _outputHelper = outputHelper;
        _fixture = fixture;
    }
    
    [BeforeScenario(Order = 2)]
    public void InitializeHttpClient()
    {
        // Create a new HttpClient for this scenario
        var httpClient = _fixture.CreateClient();
        _scenarioContext.SetHttpClient(httpClient);
        
        _outputHelper.WriteLine($"Initialized HttpClient for scenario: {_scenarioContext.ScenarioInfo.Title}");
    }
    
    [AfterScenario]
    public void CleanupHttpClient()
    {
        // Dispose of the HttpClient after the scenario
        var httpClient = _scenarioContext.GetHttpClient();
        httpClient?.Dispose();
        
        // Clear any stored responses
        _scenarioContext.Remove("LastResponse");
        _scenarioContext.Remove("LastResponseContent");
        _scenarioContext.Remove("AuthToken");
    }
    
    [BeforeStep]
    public void LogStepExecution()
    {
        var stepInfo = _scenarioContext.StepContext.StepInfo;
        _outputHelper.WriteLine($"Executing step: {stepInfo.StepDefinitionType} {stepInfo.Text}");
    }
    
    [AfterStep]
    public void LogStepResult()
    {
        var stepInfo = _scenarioContext.StepContext.StepInfo;
        var status = _scenarioContext.TestError == null ? "Passed" : "Failed";
        
        _outputHelper.WriteLine($"Step result: {status}");
        
        if (_scenarioContext.TestError != null)
        {
            _outputHelper.WriteLine($"Error: {_scenarioContext.TestError.Message}");
            
            // Log the last HTTP response if available
            if (_scenarioContext.ContainsKey("LastResponse"))
            {
                var response = _scenarioContext.GetLastResponse();
                _outputHelper.WriteLine($"Last HTTP Status: {response.StatusCode}");
                
                if (_scenarioContext.ContainsKey("LastResponseContent"))
                {
                    var content = _scenarioContext.Get<string>("LastResponseContent");
                    _outputHelper.WriteLine($"Response Content: {content}");
                }
            }
        }
    }
    
    [BeforeFeature]
    public static void LogFeatureStart(FeatureContext featureContext, ISpecFlowOutputHelper outputHelper)
    {
        outputHelper.WriteLine($"Starting feature: {featureContext.FeatureInfo.Title}");
        outputHelper.WriteLine($"Tags: {string.Join(", ", featureContext.FeatureInfo.Tags)}");
    }
    
    [AfterFeature]
    public static void LogFeatureEnd(FeatureContext featureContext, ISpecFlowOutputHelper outputHelper)
    {
        outputHelper.WriteLine($"Completed feature: {featureContext.FeatureInfo.Title}");
    }
}
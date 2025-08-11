using System.Linq;
using TechTalk.SpecFlow;
using Xunit.Abstractions;

namespace GetFitterGetBigger.API.IntegrationTests.Hooks;

[Binding]
public class CollectionLoggingHook
{
    private readonly ScenarioContext _scenarioContext;
    private readonly ITestOutputHelper _outputHelper;

    // Inject both helpers via the constructor
    public CollectionLoggingHook(ScenarioContext scenarioContext, ITestOutputHelper outputHelper)
    {
        _scenarioContext = scenarioContext;
        _outputHelper = outputHelper;
    }

    [BeforeScenario(Order = 1)]
    public void LogCollectionName()
    {
        // Log the scenario name first
        _outputHelper.WriteLine($"🔍 Starting Scenario: {_scenarioContext.ScenarioInfo.Title}");
        
        // Find a tag that starts with "collection:", which is how SpecFlow assigns collections
        var collectionTag = _scenarioContext.ScenarioInfo.Tags
            .FirstOrDefault(t => t.StartsWith("collection:"));

        if (collectionTag != null)
        {
            // The collection name is the part of the tag after the colon
            var collectionName = collectionTag.Split(':')[1];
            _outputHelper.WriteLine($"✅ Scenario is running in Test Collection: '{collectionName}'");
            _outputHelper.WriteLine($"   Thread ID: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
        }
        else
        {
            // If no tag is found, xUnit uses a default collection for the test class
            _outputHelper.WriteLine("ℹ️ Scenario is running in a default (unnamed) Test Collection.");
            _outputHelper.WriteLine($"   Thread ID: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
        }
        
        // Log all tags for debugging
        _outputHelper.WriteLine($"   All tags: [{string.Join(", ", _scenarioContext.ScenarioInfo.Tags)}]");
    }
}
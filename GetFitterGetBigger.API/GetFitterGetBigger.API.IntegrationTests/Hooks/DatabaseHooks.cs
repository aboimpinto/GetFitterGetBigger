using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Fixtures;
using TechTalk.SpecFlow;

namespace GetFitterGetBigger.API.IntegrationTests.Hooks;

[Binding]
public class DatabaseHooks
{
    private readonly PostgreSqlTestFixture _fixture;
    
    public DatabaseHooks(PostgreSqlTestFixture fixture)
    {
        _fixture = fixture;
    }
    
    [BeforeScenario(Order = 1)]
    public async Task BeforeScenario()
    {
        // Clean test data before each scenario (reference data remains)
        // This is safer than cleaning after scenario as it ensures clean state
        await _fixture.CleanDatabaseAsync();
    }
    
    [AfterScenario]
    public async Task AfterScenario()
    {
        // Optional: could clean up here too if needed
        // But cleaning before scenario is usually sufficient
    }
    
    [BeforeFeature]
    public static async Task BeforeFeature(PostgreSqlTestFixture fixture)
    {
        // Ensure container is started before feature execution
        await fixture.InitializeAsync();
    }
    
    [AfterFeature]
    public static async Task AfterFeature(PostgreSqlTestFixture fixture)
    {
        // Clean up after all scenarios in the feature
        await fixture.CleanDatabaseAsync();
    }
}
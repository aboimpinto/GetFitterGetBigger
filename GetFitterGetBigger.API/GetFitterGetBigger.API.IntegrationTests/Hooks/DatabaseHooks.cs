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
        // Ensure database is initialized with reference data
        await _fixture.InitializeDatabaseAsync();
    }
    
    [AfterScenario]
    public async Task AfterScenario()
    {
        // Clean up test data after each scenario to ensure isolation
        await _fixture.CleanDatabaseAsync();
        
        // Re-seed reference data for next scenario
        await _fixture.InitializeDatabaseAsync();
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
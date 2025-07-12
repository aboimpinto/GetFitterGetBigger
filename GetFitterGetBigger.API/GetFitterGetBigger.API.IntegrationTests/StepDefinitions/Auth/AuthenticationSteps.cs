using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Helpers;
using TechTalk.SpecFlow;

namespace GetFitterGetBigger.API.IntegrationTests.StepDefinitions.Auth;

[Binding]
public class AuthenticationSteps
{
    private readonly ScenarioContext _scenarioContext;
    
    public AuthenticationSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }
    
    [Given(@"I am authenticated as a user")]
    public void GivenIAmAuthenticatedAsAUser()
    {
        // In our simplified auth system, all users get Free-Tier claims
        // The HttpClient is already configured with auth in PostgreSqlTestFixture
        // This step is mainly for documentation purposes
        _scenarioContext.Set(true, "IsAuthenticated");
    }
}
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Helpers;
using GetFitterGetBigger.API.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TechTalk.SpecFlow;
using Xunit;

namespace GetFitterGetBigger.API.IntegrationTests.StepDefinitions.Authentication;

[Binding]
public class AuthenticationSteps
{
    private readonly ScenarioContext _scenarioContext;
    
    public AuthenticationSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }
    
    [Given(@"I am authenticated as a ""(.*)""")]
    public Task GivenIAmAuthenticatedAs(string role)
    {
        // ⚠️ IMPORTANT: Authentication roles implementation
        // Current known roles: "PT-Tier", "Admin-Tier", "Free-Tier"
        // TODO: Verify with stakeholders what claims each role should have
        
        var token = GenerateTestToken(role);
        _scenarioContext.SetAuthToken(token);
        
        // Add Authorization header to HttpClient
        var httpClient = _scenarioContext.GetHttpClient();
        httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        
        return Task.CompletedTask;
    }
    
    [Given(@"I am not authenticated")]
    public void GivenIAmNotAuthenticated()
    {
        // Clear any existing authentication
        _scenarioContext.ClearAuthToken();
        
        // Remove Authorization header from HttpClient
        var httpClient = _scenarioContext.GetHttpClient();
        httpClient.DefaultRequestHeaders.Authorization = null;
    }
    
    [Given(@"I have a valid JWT token")]
    public void GivenIHaveAValidJwtToken()
    {
        // Generate a valid token with minimal claims
        var token = GenerateTestToken("Free-Tier");
        _scenarioContext.SetAuthToken(token);
        
        // Add Authorization header to HttpClient
        var httpClient = _scenarioContext.GetHttpClient();
        httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }
    
    [Given(@"I have an expired JWT token")]
    public void GivenIHaveAnExpiredJwtToken()
    {
        // Generate an expired token
        var token = GenerateTestToken("Free-Tier", isExpired: true);
        _scenarioContext.SetAuthToken(token);
        
        // Add Authorization header to HttpClient
        var httpClient = _scenarioContext.GetHttpClient();
        httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }
    
    [Given(@"I have an invalid JWT token")]
    public void GivenIHaveAnInvalidJwtToken()
    {
        // Use a malformed token
        var token = "invalid.token.here";
        _scenarioContext.SetAuthToken(token);
        
        // Add Authorization header to HttpClient
        var httpClient = _scenarioContext.GetHttpClient();
        httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }
    
    [When(@"I authenticate with email ""(.*)""")]
    public async Task WhenIAuthenticateWithEmail(string email)
    {
        var httpClient = _scenarioContext.GetHttpClient();
        var request = new AuthenticationRequest(email);
        
        var response = await httpClient.PostAsJsonAsync("/api/auth/login", request);
        
        _scenarioContext.SetLastResponse(response);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            _scenarioContext.SetLastResponseContent(content);
            
            // Extract token from response if successful
            var authResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
            if (authResponse?.Token != null)
            {
                _scenarioContext.SetAuthToken(authResponse.Token);
                httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", authResponse.Token);
            }
        }
    }
    
    [Then(@"I should receive a valid authentication token")]
    public async Task ThenIShouldReceiveAValidAuthenticationToken()
    {
        var response = _scenarioContext.GetLastResponse();
        response.EnsureSuccessStatusCode();
        
        // Get the content that was already read in the When step
        var contentString = _scenarioContext.GetLastResponseContent();
        Assert.NotEmpty(contentString);
        
        // Debug: output the response content
        Console.WriteLine($"Response content: {contentString}");
        
        var authResponse = System.Text.Json.JsonSerializer.Deserialize<AuthenticationResponse>(contentString, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });
        Assert.NotNull(authResponse);
        Assert.NotEmpty(authResponse.Token);
        Assert.NotNull(authResponse.Claims);
        Assert.True(authResponse.Claims.Count > 0, $"Claims collection is empty. Claims count: {authResponse.Claims?.Count ?? -1}");
    }
    
    [Then(@"the token should contain claim ""(.*)"" with value ""(.*)""")]
    public async Task ThenTheTokenShouldContainClaimWithValue(string claimType, string claimValue)
    {
        // Get the content that was already read in the When step
        var contentString = _scenarioContext.GetLastResponseContent();
        Assert.NotEmpty(contentString);
        
        var authResponse = System.Text.Json.JsonSerializer.Deserialize<AuthenticationResponse>(contentString, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });
        Assert.NotNull(authResponse);
        Assert.NotNull(authResponse.Claims);
        
        // For email claims, the JWT token itself contains the email in its claims
        // But our Claims collection contains user claims (like Free-Tier)
        // We need to decode the JWT token to check for email
        if (claimType == "email")
        {
            // Decode the JWT token to check its claims
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(authResponse.Token);
            var emailClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "email" || c.Type == ClaimTypes.Email);
            Assert.NotNull(emailClaim);
            Assert.Equal(claimValue, emailClaim.Value);
        }
        else
        {
            // Check in the Claims collection for other claim types
            var claim = authResponse.Claims.FirstOrDefault(c => c.ClaimType == claimType);
            Assert.NotNull(claim);
            Assert.Equal(claimValue, claim.Resource ?? claim.ClaimType);
        }
    }
    
    /// <summary>
    /// Helper method to generate test JWT tokens
    /// </summary>
    private string GenerateTestToken(string role, bool isExpired = false)
    {
        // Use test configuration values
        var key = "TestKeyForUnitTestingAndDevelopment";
        var issuer = "TestIssuer";
        var audience = "TestAudience";
        
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim("role", role)
        };
        
        // Add role-specific claims based on known roles
        // ⚠️ TODO: These claim mappings need to be verified with stakeholders
        switch (role)
        {
            case "PT-Tier":
                claims.Add(new Claim("access", "PT-Tier"));
                claims.Add(new Claim("can_manage_exercises", "true"));
                claims.Add(new Claim("can_manage_equipment", "true"));
                break;
            case "Admin-Tier":
                claims.Add(new Claim("access", "Admin-Tier"));
                claims.Add(new Claim("can_manage_exercises", "true"));
                claims.Add(new Claim("can_manage_equipment", "true"));
                claims.Add(new Claim("can_manage_users", "true"));
                break;
            case "Free-Tier":
                claims.Add(new Claim("access", "Free-Tier"));
                claims.Add(new Claim("can_view_exercises", "true"));
                break;
            default:
                // For unknown roles, add minimal claims
                claims.Add(new Claim("access", role));
                break;
        }
        
        var expires = isExpired 
            ? DateTime.UtcNow.AddMinutes(-30) 
            : DateTime.UtcNow.AddHours(1);
        
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    // New step definitions for duplicate email testing
    
    [Given(@"the system is properly configured for authentication")]
    public void GivenTheSystemIsProperlyConfiguredForAuthentication()
    {
        // This step is mainly for documentation purposes
        // The test infrastructure should already be properly set up
        // Just verify that HttpClient is available
        var httpClient = _scenarioContext.GetHttpClient();
        Assert.NotNull(httpClient);
    }
    
    [Given(@"a user with email ""(.*)"" has already been created")]
    public async Task GivenAUserWithEmailHasAlreadyBeenCreated(string email)
    {
        // Create the user by authenticating once, then clear the auth state
        await WhenIAuthenticateWithEmail(email);
        
        // Verify the first authentication was successful
        var response = _scenarioContext.GetLastResponse();
        response.EnsureSuccessStatusCode();
        
        // Clear authentication state to simulate a fresh session
        GivenIAmNotAuthenticated();
    }
    
    [When(@"I authenticate with email ""(.*)"" again")]
    public async Task WhenIAuthenticateWithEmailAgain(string email)
    {
        // This is the same as the regular authentication step
        await WhenIAuthenticateWithEmail(email);
    }
    
    [When(@"I authenticate with email ""(.*)"" a third time")]
    public async Task WhenIAuthenticateWithEmailAThirdTime(string email)
    {
        // This is the same as the regular authentication step
        await WhenIAuthenticateWithEmail(email);
    }
    
    [Then(@"the authentication should not cause any database constraint violations")]
    public void ThenTheAuthenticationShouldNotCauseAnyDatabaseConstraintViolations()
    {
        // Verify that the last response was successful (no 500 errors)
        var response = _scenarioContext.GetLastResponse();
        Assert.True(response.IsSuccessStatusCode, 
            $"Expected successful response but got {response.StatusCode}. " +
            "This could indicate a database constraint violation.");
    }
    
    [Then(@"no database errors should occur during the process")]
    public void ThenNoDatabaseErrorsShouldOccurDuringTheProcess()
    {
        // Same as above - verify no 500 errors occurred
        ThenTheAuthenticationShouldNotCauseAnyDatabaseConstraintViolations();
    }
}
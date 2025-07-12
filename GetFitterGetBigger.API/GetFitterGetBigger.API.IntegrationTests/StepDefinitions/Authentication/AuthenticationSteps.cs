using System.Net.Http.Headers;
using System.Net.Http.Json;
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
        
        var response = await httpClient.PostAsJsonAsync("/api/auth/authenticate", request);
        
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
        
        var authResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
        Assert.NotNull(authResponse);
        Assert.NotEmpty(authResponse.Token);
        Assert.NotEmpty(authResponse.Claims);
    }
    
    [Then(@"the token should contain claim ""(.*)"" with value ""(.*)""")]
    public async Task ThenTheTokenShouldContainClaimWithValue(string claimType, string claimValue)
    {
        var response = _scenarioContext.GetLastResponse();
        var authResponse = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
        
        Assert.NotNull(authResponse);
        var claim = authResponse.Claims.FirstOrDefault(c => c.ClaimType == claimType);
        Assert.NotNull(claim);
        Assert.Equal(claimValue, claim.Id);
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
}
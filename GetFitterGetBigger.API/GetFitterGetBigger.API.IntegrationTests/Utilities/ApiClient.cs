using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Helpers;
using TechTalk.SpecFlow;

namespace GetFitterGetBigger.API.IntegrationTests.Utilities;

/// <summary>
/// Fluent API client builder for constructing HTTP requests in BDD scenarios
/// </summary>
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ScenarioContext _scenarioContext;
    private readonly Dictionary<string, string> _headers;
    private string? _endpoint;
    private HttpMethod? _method;
    private object? _body;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public ApiClient(HttpClient httpClient, ScenarioContext scenarioContext)
    {
        _httpClient = httpClient;
        _scenarioContext = scenarioContext;
        _headers = new Dictionary<string, string>();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }
    
    public static ApiClient Create(ScenarioContext scenarioContext)
    {
        var httpClient = scenarioContext.GetHttpClient();
        return new ApiClient(httpClient, scenarioContext);
    }
    
    public ApiClient WithEndpoint(string endpoint)
    {
        _endpoint = _scenarioContext.ResolvePlaceholders(endpoint);
        return this;
    }
    
    public ApiClient WithMethod(HttpMethod method)
    {
        _method = method;
        return this;
    }
    
    public ApiClient WithMethod(string method)
    {
        _method = method.ToUpper() switch
        {
            "GET" => HttpMethod.Get,
            "POST" => HttpMethod.Post,
            "PUT" => HttpMethod.Put,
            "DELETE" => HttpMethod.Delete,
            "PATCH" => HttpMethod.Patch,
            "HEAD" => HttpMethod.Head,
            "OPTIONS" => HttpMethod.Options,
            _ => throw new ArgumentException($"Unknown HTTP method: {method}")
        };
        return this;
    }
    
    public ApiClient WithHeader(string name, string value)
    {
        _headers[name] = value;
        return this;
    }
    
    public ApiClient WithAuthToken(string? token = null)
    {
        token ??= _scenarioContext.GetAuthToken();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }
        return this;
    }
    
    public ApiClient WithBody(object body)
    {
        _body = body;
        return this;
    }
    
    public ApiClient WithJsonBody(string json)
    {
        var resolvedJson = _scenarioContext.ResolvePlaceholders(json);
        _body = JsonSerializer.Deserialize<JsonElement>(resolvedJson, _jsonOptions);
        return this;
    }
    
    public async Task<HttpResponseMessage> SendAsync()
    {
        if (string.IsNullOrEmpty(_endpoint))
            throw new InvalidOperationException("Endpoint must be specified");
        
        if (_method == null)
            throw new InvalidOperationException("HTTP method must be specified");
        
        var request = new HttpRequestMessage(_method, _endpoint);
        
        // Add custom headers
        foreach (var header in _headers)
        {
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
        
        // Add body if present
        if (_body != null)
        {
            var json = JsonSerializer.Serialize(_body, _jsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }
        
        // Send request
        var response = await _httpClient.SendAsync(request);
        
        // Store response in scenario context
        _scenarioContext.SetLastResponse(response);
        
        // Store response content if available
        if (response.Content.Headers.ContentLength > 0)
        {
            var content = await response.Content.ReadAsStringAsync();
            _scenarioContext.SetLastResponseContent(content);
        }
        
        return response;
    }
    
    public async Task<T?> SendAsync<T>()
    {
        var response = await SendAsync();
        
        if (!response.IsSuccessStatusCode)
            return default;
        
        return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
    }
    
    // Convenience methods for common operations
    public Task<HttpResponseMessage> GetAsync(string endpoint)
    {
        return WithEndpoint(endpoint)
            .WithMethod(HttpMethod.Get)
            .SendAsync();
    }
    
    public Task<T?> GetAsync<T>(string endpoint)
    {
        return WithEndpoint(endpoint)
            .WithMethod(HttpMethod.Get)
            .SendAsync<T>();
    }
    
    public Task<HttpResponseMessage> PostAsync(string endpoint, object body)
    {
        return WithEndpoint(endpoint)
            .WithMethod(HttpMethod.Post)
            .WithBody(body)
            .SendAsync();
    }
    
    public Task<T?> PostAsync<T>(string endpoint, object body)
    {
        return WithEndpoint(endpoint)
            .WithMethod(HttpMethod.Post)
            .WithBody(body)
            .SendAsync<T>();
    }
    
    public Task<HttpResponseMessage> PutAsync(string endpoint, object body)
    {
        return WithEndpoint(endpoint)
            .WithMethod(HttpMethod.Put)
            .WithBody(body)
            .SendAsync();
    }
    
    public Task<T?> PutAsync<T>(string endpoint, object body)
    {
        return WithEndpoint(endpoint)
            .WithMethod(HttpMethod.Put)
            .WithBody(body)
            .SendAsync<T>();
    }
    
    public Task<HttpResponseMessage> DeleteAsync(string endpoint)
    {
        return WithEndpoint(endpoint)
            .WithMethod(HttpMethod.Delete)
            .SendAsync();
    }
}
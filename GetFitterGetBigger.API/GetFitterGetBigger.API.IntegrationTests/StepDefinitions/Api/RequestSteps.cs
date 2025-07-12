using System.Net;
using System.Net.Http.Headers;
using System.Text;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Helpers;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace GetFitterGetBigger.API.IntegrationTests.StepDefinitions.Api;

[Binding]
public class RequestSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly Dictionary<string, string> _pendingHeaders = new();
    
    public RequestSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }
    
    [Given(@"I send a (GET|POST|PUT|DELETE) request to ""(.*)""")]
    [When(@"I send a (GET|POST|PUT|DELETE) request to ""(.*)""")]
    public async Task WhenISendARequestTo(string method, string endpoint)
    {
        var httpClient = _scenarioContext.GetHttpClient();
        
        // Apply any pending headers
        ApplyPendingHeaders(httpClient);
        
        // Resolve any placeholders in the endpoint
        endpoint = _scenarioContext.ResolvePlaceholders(endpoint);
        
        HttpResponseMessage response = method.ToUpper() switch
        {
            "GET" => await httpClient.GetAsync(endpoint),
            "POST" => await httpClient.PostAsync(endpoint, null),
            "PUT" => await httpClient.PutAsync(endpoint, null),
            "DELETE" => await httpClient.DeleteAsync(endpoint),
            _ => throw new NotSupportedException($"HTTP method {method} is not supported")
        };
        
        await StoreResponse(response);
    }
    
    [Given(@"I send a (GET|POST|PUT|DELETE) request to ""(.*)"" with body:")]
    [When(@"I send a (GET|POST|PUT|DELETE) request to ""(.*)"" with body:")]
    public async Task WhenISendARequestWithBody(string method, string endpoint, string body)
    {
        var httpClient = _scenarioContext.GetHttpClient();
        
        // Apply any pending headers
        ApplyPendingHeaders(httpClient);
        
        // Resolve any placeholders in the endpoint and body
        endpoint = _scenarioContext.ResolvePlaceholders(endpoint);
        body = _scenarioContext.ResolvePlaceholders(body);
        
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        
        HttpResponseMessage response = method.ToUpper() switch
        {
            "GET" => throw new NotSupportedException("GET requests should not have a body"),
            "POST" => await httpClient.PostAsync(endpoint, content),
            "PUT" => await httpClient.PutAsync(endpoint, content),
            "DELETE" => await httpClient.SendAsync(new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(httpClient.BaseAddress!, endpoint),
                Content = content
            }),
            _ => throw new NotSupportedException($"HTTP method {method} is not supported")
        };
        
        await StoreResponse(response);
    }
    
    [When(@"I send a (GET|POST|PUT|DELETE) request to ""(.*)"" with form data:")]
    public async Task WhenISendARequestWithFormData(string method, string endpoint, Table formData)
    {
        var httpClient = _scenarioContext.GetHttpClient();
        
        // Apply any pending headers
        ApplyPendingHeaders(httpClient);
        
        // Resolve any placeholders in the endpoint
        endpoint = _scenarioContext.ResolvePlaceholders(endpoint);
        
        var formContent = new FormUrlEncodedContent(
            formData.Rows.Select(row => 
                new KeyValuePair<string, string>(
                    row["Field"], 
                    _scenarioContext.ResolvePlaceholders(row["Value"])
                )
            )
        );
        
        HttpResponseMessage response = method.ToUpper() switch
        {
            "GET" => throw new NotSupportedException("GET requests should not have form data"),
            "POST" => await httpClient.PostAsync(endpoint, formContent),
            "PUT" => await httpClient.PutAsync(endpoint, formContent),
            "DELETE" => throw new NotSupportedException("DELETE requests typically don't use form data"),
            _ => throw new NotSupportedException($"HTTP method {method} is not supported")
        };
        
        await StoreResponse(response);
    }
    
    [When(@"I send a multipart (POST|PUT) request to ""(.*)"" with:")]
    public async Task WhenISendAMultipartRequest(string method, string endpoint, Table multipartData)
    {
        var httpClient = _scenarioContext.GetHttpClient();
        
        // Apply any pending headers
        ApplyPendingHeaders(httpClient);
        
        // Resolve any placeholders in the endpoint
        endpoint = _scenarioContext.ResolvePlaceholders(endpoint);
        
        using var content = new MultipartFormDataContent();
        
        foreach (var row in multipartData.Rows)
        {
            var fieldName = row["Field"];
            var fieldValue = _scenarioContext.ResolvePlaceholders(row["Value"]);
            var fieldType = row.ContainsKey("Type") ? row["Type"] : "text";
            
            switch (fieldType.ToLower())
            {
                case "text":
                    content.Add(new StringContent(fieldValue), fieldName);
                    break;
                case "file":
                    // For file uploads, the value should be the file path
                    var fileBytes = await File.ReadAllBytesAsync(fieldValue);
                    var fileContent = new ByteArrayContent(fileBytes);
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                    content.Add(fileContent, fieldName, Path.GetFileName(fieldValue));
                    break;
                default:
                    throw new NotSupportedException($"Multipart field type {fieldType} is not supported");
            }
        }
        
        HttpResponseMessage response = method.ToUpper() switch
        {
            "POST" => await httpClient.PostAsync(endpoint, content),
            "PUT" => await httpClient.PutAsync(endpoint, content),
            _ => throw new NotSupportedException($"HTTP method {method} is not supported for multipart")
        };
        
        await StoreResponse(response);
    }
    
    [When(@"I add header ""(.*)"" with value ""(.*)""")]
    public void WhenIAddHeader(string name, string value)
    {
        // Store headers to be applied on next request
        _pendingHeaders[name] = _scenarioContext.ResolvePlaceholders(value);
    }
    
    [When(@"I add the following headers:")]
    public void WhenIAddTheFollowingHeaders(Table headers)
    {
        foreach (var row in headers.Rows)
        {
            var name = row["Name"];
            var value = _scenarioContext.ResolvePlaceholders(row["Value"]);
            _pendingHeaders[name] = value;
        }
    }
    
    [When(@"I set the request timeout to (\d+) seconds")]
    public void WhenISetTheRequestTimeout(int seconds)
    {
        var httpClient = _scenarioContext.GetHttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(seconds);
    }
    
    [Given(@"I send an authenticated (GET|POST|PUT|DELETE) request to ""(.*)"" with token ""(.*)""")]
    [When(@"I send an authenticated (GET|POST|PUT|DELETE) request to ""(.*)"" with token ""(.*)""")]
    public async Task WhenISendAnAuthenticatedRequestTo(string method, string endpoint, string token)
    {
        var httpClient = _scenarioContext.GetHttpClient();
        
        // Resolve placeholders in token and endpoint
        var resolvedToken = _scenarioContext.ResolvePlaceholders(token);
        endpoint = _scenarioContext.ResolvePlaceholders(endpoint);
        
        // Temporarily add Authorization header
        httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", resolvedToken);
        
        try
        {
            HttpResponseMessage response = method.ToUpper() switch
            {
                "GET" => await httpClient.GetAsync(endpoint),
                "POST" => await httpClient.PostAsync(endpoint, null),
                "PUT" => await httpClient.PutAsync(endpoint, null),
                "DELETE" => await httpClient.DeleteAsync(endpoint),
                _ => throw new NotSupportedException($"HTTP method {method} is not supported")
            };
            
            await StoreResponse(response);
        }
        finally
        {
            // Always remove the Authorization header after the request
            httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
    
    private void ApplyPendingHeaders(HttpClient httpClient)
    {
        foreach (var header in _pendingHeaders)
        {
            // Remove existing header if present
            httpClient.DefaultRequestHeaders.Remove(header.Key);
            
            // Add the new header value
            httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
        }
        
        // Clear pending headers after applying
        _pendingHeaders.Clear();
    }
    
    private async Task StoreResponse(HttpResponseMessage response)
    {
        _scenarioContext.SetLastResponse(response);
        
        // Always try to read the content, even for error responses
        var content = await response.Content.ReadAsStringAsync();
        _scenarioContext.SetLastResponseContent(content);
        
        // Store response headers that might be useful
        if (response.Headers.Location != null)
        {
            _scenarioContext.SetTestData("LastLocationHeader", response.Headers.Location.ToString());
        }
        
        // Store any created entity ID from Location header (common pattern)
        if (response.StatusCode == HttpStatusCode.Created && response.Headers.Location != null)
        {
            var locationPath = response.Headers.Location.ToString();
            var segments = locationPath.Split('/');
            if (segments.Length > 0)
            {
                var entityId = segments[^1]; // Last segment often contains the ID
                _scenarioContext.SetTestData("LastCreatedId", entityId);
            }
        }
    }
}
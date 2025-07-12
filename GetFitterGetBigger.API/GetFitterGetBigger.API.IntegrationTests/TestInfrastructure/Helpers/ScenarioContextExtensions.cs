using System.Net;
using System.Text.Json;
using TechTalk.SpecFlow;

namespace GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Helpers;

/// <summary>
/// Extension methods for ScenarioContext to provide type-safe storage and retrieval of test data
/// </summary>
public static class ScenarioContextExtensions
{
    private const string HttpClientKey = "HttpClient";
    private const string HttpResponseKey = "LastHttpResponse";
    private const string HttpResponseContentKey = "LastHttpResponseContent";
    private const string AuthTokenKey = "AuthToken";
    private const string CreatedEntityIdsKey = "CreatedEntityIds";
    private const string TestDataKeyPrefix = "TestData_";
    
    #region HttpClient Management
    
    /// <summary>
    /// Stores the HttpClient for the current scenario
    /// </summary>
    public static void SetHttpClient(this ScenarioContext context, HttpClient client)
    {
        context[HttpClientKey] = client;
    }
    
    /// <summary>
    /// Gets the HttpClient for the current scenario
    /// </summary>
    public static HttpClient GetHttpClient(this ScenarioContext context)
    {
        if (!context.TryGetValue(HttpClientKey, out var client))
        {
            throw new InvalidOperationException("HttpClient not found in ScenarioContext. Ensure it was set during scenario setup.");
        }
        
        return (HttpClient)client;
    }
    
    #endregion
    
    #region HTTP Response Management
    
    /// <summary>
    /// Stores the last HTTP response
    /// </summary>
    public static void SetLastResponse(this ScenarioContext context, HttpResponseMessage response)
    {
        context[HttpResponseKey] = response;
    }
    
    /// <summary>
    /// Gets the last HTTP response
    /// </summary>
    public static HttpResponseMessage GetLastResponse(this ScenarioContext context)
    {
        if (!context.TryGetValue(HttpResponseKey, out var response))
        {
            throw new InvalidOperationException("No HTTP response found in ScenarioContext. Ensure an HTTP request was made.");
        }
        
        return (HttpResponseMessage)response;
    }
    
    /// <summary>
    /// Gets the status code of the last HTTP response
    /// </summary>
    public static HttpStatusCode GetLastResponseStatusCode(this ScenarioContext context)
    {
        return context.GetLastResponse().StatusCode;
    }
    
    /// <summary>
    /// Stores the last HTTP response content as string
    /// </summary>
    public static void SetLastResponseContent(this ScenarioContext context, string content)
    {
        context[HttpResponseContentKey] = content;
    }
    
    /// <summary>
    /// Gets the last HTTP response content as string
    /// </summary>
    public static string GetLastResponseContent(this ScenarioContext context)
    {
        if (!context.TryGetValue(HttpResponseContentKey, out var content))
        {
            throw new InvalidOperationException("No HTTP response content found in ScenarioContext.");
        }
        
        return (string)content;
    }
    
    #endregion
    
    #region Authentication Management
    
    /// <summary>
    /// Stores the authentication token
    /// </summary>
    public static void SetAuthToken(this ScenarioContext context, string token)
    {
        context[AuthTokenKey] = token;
    }
    
    /// <summary>
    /// Gets the authentication token
    /// </summary>
    public static string? GetAuthToken(this ScenarioContext context)
    {
        return context.TryGetValue(AuthTokenKey, out var token) ? (string)token : null;
    }
    
    /// <summary>
    /// Checks if the scenario has an authentication token
    /// </summary>
    public static bool HasAuthToken(this ScenarioContext context)
    {
        return context.ContainsKey(AuthTokenKey);
    }
    
    /// <summary>
    /// Clears the authentication token
    /// </summary>
    public static void ClearAuthToken(this ScenarioContext context)
    {
        if (context.ContainsKey(AuthTokenKey))
        {
            context.Remove(AuthTokenKey);
        }
    }
    
    #endregion
    
    #region Test Data Management
    
    /// <summary>
    /// Stores a test entity with a key for later retrieval
    /// </summary>
    public static void SetTestData<T>(this ScenarioContext context, string key, T data) where T : class
    {
        context[$"{TestDataKeyPrefix}{key}"] = data;
    }
    
    /// <summary>
    /// Gets a test entity by key
    /// </summary>
    public static T GetTestData<T>(this ScenarioContext context, string key) where T : class
    {
        var fullKey = $"{TestDataKeyPrefix}{key}";
        if (!context.TryGetValue(fullKey, out var data))
        {
            throw new InvalidOperationException($"Test data with key '{key}' not found in ScenarioContext.");
        }
        
        return (T)data;
    }
    
    /// <summary>
    /// Tries to get a test entity by key
    /// </summary>
    public static bool TryGetTestData<T>(this ScenarioContext context, string key, out T? data) where T : class
    {
        var fullKey = $"{TestDataKeyPrefix}{key}";
        if (context.TryGetValue(fullKey, out var value))
        {
            data = (T)value;
            return true;
        }
        
        data = null;
        return false;
    }
    
    /// <summary>
    /// Stores a created entity ID for cleanup purposes
    /// </summary>
    public static void AddCreatedEntityId(this ScenarioContext context, string entityType, string entityId)
    {
        if (!context.TryGetValue(CreatedEntityIdsKey, out var idsObj))
        {
            idsObj = new Dictionary<string, List<string>>();
            context[CreatedEntityIdsKey] = idsObj;
        }
        
        var ids = (Dictionary<string, List<string>>)idsObj;
        if (!ids.ContainsKey(entityType))
        {
            ids[entityType] = new List<string>();
        }
        
        ids[entityType].Add(entityId);
    }
    
    /// <summary>
    /// Gets all created entity IDs of a specific type
    /// </summary>
    public static List<string> GetCreatedEntityIds(this ScenarioContext context, string entityType)
    {
        if (!context.TryGetValue(CreatedEntityIdsKey, out var idsObj))
        {
            return new List<string>();
        }
        
        var ids = (Dictionary<string, List<string>>)idsObj;
        return ids.TryGetValue(entityType, out var entityIds) ? entityIds : new List<string>();
    }
    
    #endregion
    
    #region Placeholder Resolution
    
    /// <summary>
    /// Resolves placeholders in text (e.g., "<EntityName.PropertyName>") with actual values from test data
    /// </summary>
    public static string ResolvePlaceholders(this ScenarioContext context, string text)
    {
        // First, handle simple placeholders in the format <key>
        var simplePlaceholderPattern = @"<([^.>]+)>";
        var simpleMatches = System.Text.RegularExpressions.Regex.Matches(text, simplePlaceholderPattern);
        
        foreach (System.Text.RegularExpressions.Match match in simpleMatches)
        {
            // Skip if this looks like a property placeholder (contains a dot)
            if (match.Value.Contains('.'))
                continue;
                
            var key = match.Groups[1].Value;
            
            try
            {
                // Try to get the test data
                var fullKey = $"{TestDataKeyPrefix}{key}";
                if (context.TryGetValue(fullKey, out var value) && value != null)
                {
                    text = text.Replace(match.Value, value.ToString());
                }
            }
            catch
            {
                // If we can't resolve a placeholder, leave it as is
            }
        }
        
        // Find all placeholders in the format <EntityName.PropertyName>
        var placeholderPattern = @"<([^.>]+)\.([^>]+)>";
        var matches = System.Text.RegularExpressions.Regex.Matches(text, placeholderPattern);
        
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var entityKey = match.Groups[1].Value;
            var propertyName = match.Groups[2].Value;
            
            try
            {
                // Try to get the test data
                var fullKey = $"{TestDataKeyPrefix}{entityKey}";
                if (context.TryGetValue(fullKey, out var entityObj) && entityObj != null)
                {
                    string? value = null;
                    
                    // Check if it's a JSON string
                    if (entityObj is string jsonString && IsValidJson(jsonString))
                    {
                        // Parse JSON and extract property
                        var jsonDoc = JsonDocument.Parse(jsonString);
                        if (jsonDoc.RootElement.TryGetProperty(propertyName, out var jsonProperty))
                        {
                            value = jsonProperty.ValueKind switch
                            {
                                JsonValueKind.String => jsonProperty.GetString(),
                                JsonValueKind.Number => jsonProperty.GetRawText(),
                                JsonValueKind.True => "true",
                                JsonValueKind.False => "false",
                                JsonValueKind.Null => null,
                                _ => jsonProperty.GetRawText()
                            };
                        }
                    }
                    else
                    {
                        // Use reflection to get the property value (original behavior)
                        var property = entityObj.GetType().GetProperty(propertyName);
                        if (property != null)
                        {
                            value = property.GetValue(entityObj)?.ToString();
                        }
                    }
                    
                    if (value != null)
                    {
                        text = text.Replace(match.Value, value);
                    }
                }
            }
            catch
            {
                // If we can't resolve a placeholder, leave it as is
            }
        }
        
        return text;
    }
    
    /// <summary>
    /// Checks if a string is valid JSON
    /// </summary>
    private static bool IsValidJson(string jsonString)
    {
        try
        {
            JsonDocument.Parse(jsonString);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    #endregion
}
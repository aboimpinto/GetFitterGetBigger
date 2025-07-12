using System.Text.Json;
using System.Text.Json.Nodes;

namespace GetFitterGetBigger.API.IntegrationTests.Utilities;

/// <summary>
/// Helper class for JSON manipulation and validation in BDD tests
/// </summary>
public static class JsonHelper
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };
    
    /// <summary>
    /// Parse JSON string to a dynamic object for easy property access
    /// </summary>
    public static JsonNode? Parse(string json)
    {
        return JsonNode.Parse(json);
    }
    
    /// <summary>
    /// Serialize object to JSON string
    /// </summary>
    public static string Serialize(object obj)
    {
        return JsonSerializer.Serialize(obj, DefaultOptions);
    }
    
    /// <summary>
    /// Deserialize JSON string to specified type
    /// </summary>
    public static T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, DefaultOptions);
    }
    
    /// <summary>
    /// Get value from JSON using JSONPath-like syntax
    /// </summary>
    public static string? GetValue(string json, string path)
    {
        var node = JsonNode.Parse(json);
        if (node == null) return null;
        
        // Simple path parsing (e.g., "data.id", "items[0].name")
        var parts = path.Split('.');
        JsonNode? current = node;
        
        foreach (var part in parts)
        {
            if (current == null) return null;
            
            // Handle array indexing
            if (part.Contains('[') && part.Contains(']'))
            {
                var propertyName = part.Substring(0, part.IndexOf('['));
                var indexStr = part.Substring(part.IndexOf('[') + 1, part.IndexOf(']') - part.IndexOf('[') - 1);
                
                if (!string.IsNullOrEmpty(propertyName))
                {
                    current = current[propertyName];
                }
                
                if (current != null && int.TryParse(indexStr, out var index))
                {
                    current = current[index];
                }
            }
            else
            {
                current = current[part];
            }
        }
        
        return current?.ToString();
    }
    
    /// <summary>
    /// Compare two JSON strings for equality (ignoring formatting)
    /// </summary>
    public static bool AreEqual(string json1, string json2)
    {
        try
        {
            var obj1 = JsonSerializer.Deserialize<JsonElement>(json1);
            var obj2 = JsonSerializer.Deserialize<JsonElement>(json2);
            return obj1.GetRawText() == obj2.GetRawText();
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Validate JSON against expected structure
    /// </summary>
    public static void ValidateJson(string actualJson, string expectedJson)
    {
        var actual = JsonSerializer.Deserialize<JsonElement>(actualJson);
        var expected = JsonSerializer.Deserialize<JsonElement>(expectedJson);
        
        if (!JsonElementsAreEqual(actual, expected))
        {
            throw new InvalidOperationException($"JSON validation failed.\nExpected: {expectedJson}\nActual: {actualJson}");
        }
    }
    
    private static bool JsonElementsAreEqual(JsonElement a, JsonElement b)
    {
        if (a.ValueKind != b.ValueKind)
            return false;
            
        switch (a.ValueKind)
        {
            case JsonValueKind.Object:
                var aProps = a.EnumerateObject().OrderBy(p => p.Name).ToList();
                var bProps = b.EnumerateObject().OrderBy(p => p.Name).ToList();
                
                if (aProps.Count != bProps.Count)
                    return false;
                    
                for (int i = 0; i < aProps.Count; i++)
                {
                    if (aProps[i].Name != bProps[i].Name || 
                        !JsonElementsAreEqual(aProps[i].Value, bProps[i].Value))
                        return false;
                }
                return true;
                
            case JsonValueKind.Array:
                var aItems = a.EnumerateArray().ToList();
                var bItems = b.EnumerateArray().ToList();
                
                if (aItems.Count != bItems.Count)
                    return false;
                    
                for (int i = 0; i < aItems.Count; i++)
                {
                    if (!JsonElementsAreEqual(aItems[i], bItems[i]))
                        return false;
                }
                return true;
                
            case JsonValueKind.String:
                return a.GetString() == b.GetString();
                
            case JsonValueKind.Number:
                return a.GetDouble() == b.GetDouble();
                
            case JsonValueKind.True:
            case JsonValueKind.False:
                return a.GetBoolean() == b.GetBoolean();
                
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return true;
                
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Check if JSON contains a specific property
    /// </summary>
    public static bool HasProperty(string json, string propertyPath)
    {
        return GetValue(json, propertyPath) != null;
    }
    
    /// <summary>
    /// Format JSON string for better readability
    /// </summary>
    public static string Format(string json)
    {
        try
        {
            var element = JsonSerializer.Deserialize<JsonElement>(json);
            return JsonSerializer.Serialize(element, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch
        {
            return json;
        }
    }
    
    /// <summary>
    /// Create a JSON object from key-value pairs
    /// </summary>
    public static string CreateJson(params (string key, object value)[] properties)
    {
        var dict = properties.ToDictionary(p => p.key, p => p.value);
        return JsonSerializer.Serialize(dict, DefaultOptions);
    }
    
    /// <summary>
    /// Merge two JSON objects
    /// </summary>
    public static string Merge(string json1, string json2)
    {
        var obj1 = JsonSerializer.Deserialize<Dictionary<string, object>>(json1, DefaultOptions) ?? new();
        var obj2 = JsonSerializer.Deserialize<Dictionary<string, object>>(json2, DefaultOptions) ?? new();
        
        foreach (var kvp in obj2)
        {
            obj1[kvp.Key] = kvp.Value;
        }
        
        return JsonSerializer.Serialize(obj1, DefaultOptions);
    }
    
    /// <summary>
    /// Convert JSON to formatted table string for logging
    /// </summary>
    public static string ToTableFormat(string json)
    {
        try
        {
            var obj = JsonSerializer.Deserialize<Dictionary<string, object>>(json, DefaultOptions);
            if (obj == null) return json;
            
            var maxKeyLength = obj.Keys.Max(k => k.Length);
            var lines = obj.Select(kvp => $"{kvp.Key.PadRight(maxKeyLength)} : {kvp.Value}");
            
            return string.Join(Environment.NewLine, lines);
        }
        catch
        {
            return json;
        }
    }
}
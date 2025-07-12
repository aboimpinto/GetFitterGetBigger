using System.Net;
using System.Text.Json;
using FluentAssertions;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Helpers;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace GetFitterGetBigger.API.IntegrationTests.StepDefinitions.Api;

[Binding]
public class ResponseSteps
{
    private readonly ScenarioContext _scenarioContext;
    
    public ResponseSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }
    
    [Then(@"the response status should be (\d+)")]
    public void ThenTheResponseStatusShouldBe(int expectedStatusCode)
    {
        var actualStatusCode = (int)_scenarioContext.GetLastResponseStatusCode();
        actualStatusCode.Should().Be(expectedStatusCode, 
            $"expected status code {expectedStatusCode} but got {actualStatusCode}");
    }
    
    [Then(@"the response status should be ""(.*)""")]
    public void ThenTheResponseStatusShouldBe(string statusName)
    {
        var expectedStatusCode = statusName.ToLower() switch
        {
            "ok" => 200,
            "created" => 201,
            "accepted" => 202,
            "no content" => 204,
            "bad request" => 400,
            "unauthorized" => 401,
            "forbidden" => 403,
            "not found" => 404,
            "conflict" => 409,
            "unprocessable entity" => 422,
            "internal server error" => 500,
            _ => throw new NotSupportedException($"Status name '{statusName}' is not recognized")
        };
        
        ThenTheResponseStatusShouldBe(expectedStatusCode);
    }
    
    [Then(@"the response should contain ""(.*)""")]
    public void ThenTheResponseShouldContain(string expectedText)
    {
        var content = _scenarioContext.GetLastResponseContent();
        content.Should().Contain(expectedText, 
            $"expected response to contain '{expectedText}'");
    }
    
    [Then(@"the response should not contain ""(.*)""")]
    public void ThenTheResponseShouldNotContain(string unexpectedText)
    {
        var content = _scenarioContext.GetLastResponseContent();
        content.Should().NotContain(unexpectedText, 
            $"expected response to not contain '{unexpectedText}'");
    }
    
    [Then(@"the response should be empty")]
    public void ThenTheResponseShouldBeEmpty()
    {
        var content = _scenarioContext.GetLastResponseContent();
        content.Should().BeEmpty("expected response to be empty");
    }
    
    [Then(@"the response should have property ""(.*)"" with value ""(.*)""")]
    public void ThenTheResponseShouldHaveProperty(string jsonPath, string expectedValue)
    {
        var content = _scenarioContext.GetLastResponseContent();
        var jsonDocument = JsonDocument.Parse(content);
        
        var actualValue = GetJsonValue(jsonDocument.RootElement, jsonPath);
        actualValue.Should().Be(expectedValue, 
            $"expected property '{jsonPath}' to have value '{expectedValue}' but got '{actualValue}'");
    }
    
    [Then(@"the response should have property ""(.*)""")]
    public void ThenTheResponseShouldHaveProperty(string jsonPath)
    {
        var content = _scenarioContext.GetLastResponseContent();
        var jsonDocument = JsonDocument.Parse(content);
        
        var value = GetJsonValue(jsonDocument.RootElement, jsonPath);
        value.Should().NotBeNull($"expected property '{jsonPath}' to exist");
    }
    
    [Then(@"the response should not have property ""(.*)""")]
    public void ThenTheResponseShouldNotHaveProperty(string jsonPath)
    {
        var content = _scenarioContext.GetLastResponseContent();
        var jsonDocument = JsonDocument.Parse(content);
        
        var value = GetJsonValue(jsonDocument.RootElement, jsonPath);
        value.Should().BeNull($"expected property '{jsonPath}' to not exist");
    }
    
    [Then(@"the response should be a valid JSON")]
    public void ThenTheResponseShouldBeValidJson()
    {
        var content = _scenarioContext.GetLastResponseContent();
        
        Action parseJson = () => JsonDocument.Parse(content);
        parseJson.Should().NotThrow<JsonException>("expected response to be valid JSON");
    }
    
    [Then(@"the response should be an array with (\d+) items?")]
    public void ThenTheResponseShouldBeAnArrayWithItems(int expectedCount)
    {
        var content = _scenarioContext.GetLastResponseContent();
        var jsonDocument = JsonDocument.Parse(content);
        
        jsonDocument.RootElement.ValueKind.Should().Be(JsonValueKind.Array, 
            "expected response to be a JSON array");
        
        jsonDocument.RootElement.GetArrayLength().Should().Be(expectedCount,
            $"expected array to have {expectedCount} items");
    }
    
    [Then(@"the response array should contain at least (\d+) items?")]
    public void ThenTheResponseArrayShouldContainAtLeastItems(int minimumCount)
    {
        var content = _scenarioContext.GetLastResponseContent();
        var jsonDocument = JsonDocument.Parse(content);
        
        jsonDocument.RootElement.ValueKind.Should().Be(JsonValueKind.Array, 
            "expected response to be a JSON array");
        
        jsonDocument.RootElement.GetArrayLength().Should().BeGreaterOrEqualTo(minimumCount,
            $"expected array to have at least {minimumCount} items");
    }
    
    [Then(@"the response should match the schema:")]
    public void ThenTheResponseShouldMatchTheSchema(string jsonSchema)
    {
        // This is a simplified schema validation
        // In a real implementation, you might want to use a JSON Schema validator
        var content = _scenarioContext.GetLastResponseContent();
        var actualJson = JsonDocument.Parse(content);
        var schemaJson = JsonDocument.Parse(jsonSchema);
        
        ValidateJsonAgainstSchema(actualJson.RootElement, schemaJson.RootElement);
    }
    
    [Then(@"the response headers should contain ""(.*)"" with value ""(.*)""")]
    public void ThenTheResponseHeadersShouldContain(string headerName, string headerValue)
    {
        var response = _scenarioContext.GetLastResponse();
        
        response.Headers.Should().ContainKey(headerName,
            $"expected response headers to contain '{headerName}'");
        
        var actualValues = response.Headers.GetValues(headerName);
        actualValues.Should().Contain(headerValue,
            $"expected header '{headerName}' to contain value '{headerValue}'");
    }
    
    [Then(@"store the response property ""(.*)"" as ""(.*)""")]
    public void ThenStoreTheResponsePropertyAs(string jsonPath, string variableName)
    {
        var content = _scenarioContext.GetLastResponseContent();
        var jsonDocument = JsonDocument.Parse(content);
        
        var value = GetJsonValue(jsonDocument.RootElement, jsonPath);
        value.Should().NotBeNull($"property '{jsonPath}' not found in response");
        
        _scenarioContext.SetTestData(variableName, value!);
    }
    
    [Then(@"the response should have the following properties:")]
    public void ThenTheResponseShouldHaveTheFollowingProperties(Table expectedProperties)
    {
        var content = _scenarioContext.GetLastResponseContent();
        var jsonDocument = JsonDocument.Parse(content);
        
        foreach (var row in expectedProperties.Rows)
        {
            var propertyPath = row["Property"];
            var expectedValue = row["Value"];
            
            var actualValue = GetJsonValue(jsonDocument.RootElement, propertyPath);
            actualValue.Should().Be(expectedValue,
                $"expected property '{propertyPath}' to have value '{expectedValue}'");
        }
    }
    
    private string? GetJsonValue(JsonElement element, string path)
    {
        var segments = path.Split('.');
        var current = element;
        
        foreach (var segment in segments)
        {
            // Handle array indexing
            if (segment.Contains('[') && segment.Contains(']'))
            {
                var propertyName = segment.Substring(0, segment.IndexOf('['));
                var indexStr = segment.Substring(segment.IndexOf('[') + 1, segment.IndexOf(']') - segment.IndexOf('[') - 1);
                
                if (!string.IsNullOrEmpty(propertyName))
                {
                    if (!current.TryGetProperty(propertyName, out current))
                        return null;
                }
                
                if (int.TryParse(indexStr, out var index))
                {
                    if (current.ValueKind != JsonValueKind.Array || index >= current.GetArrayLength())
                        return null;
                    
                    current = current[index];
                }
            }
            else
            {
                // Regular property access
                if (!current.TryGetProperty(segment, out current))
                    return null;
            }
        }
        
        return current.ValueKind switch
        {
            JsonValueKind.String => current.GetString(),
            JsonValueKind.Number => current.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => null,
            _ => current.GetRawText()
        };
    }
    
    private void ValidateJsonAgainstSchema(JsonElement actual, JsonElement schema)
    {
        // This is a simplified implementation
        // For production use, consider using a proper JSON Schema validator
        
        if (schema.TryGetProperty("type", out var typeElement))
        {
            var expectedType = typeElement.GetString();
            var actualType = actual.ValueKind switch
            {
                JsonValueKind.Object => "object",
                JsonValueKind.Array => "array",
                JsonValueKind.String => "string",
                JsonValueKind.Number => "number",
                JsonValueKind.True or JsonValueKind.False => "boolean",
                JsonValueKind.Null => "null",
                _ => "unknown"
            };
            
            actualType.Should().Be(expectedType, "JSON type mismatch");
        }
        
        if (schema.TryGetProperty("properties", out var propertiesElement) && actual.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in propertiesElement.EnumerateObject())
            {
                if (actual.TryGetProperty(property.Name, out var actualProperty))
                {
                    ValidateJsonAgainstSchema(actualProperty, property.Value);
                }
                else if (schema.TryGetProperty("required", out var requiredElement))
                {
                    var requiredProperties = requiredElement.EnumerateArray()
                        .Select(e => e.GetString())
                        .ToList();
                    
                    if (requiredProperties.Contains(property.Name))
                    {
                        throw new InvalidOperationException($"Required property '{property.Name}' is missing");
                    }
                }
            }
        }
    }
}
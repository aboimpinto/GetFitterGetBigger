# Testing Strategy

To improve the robustness of the application and prevent runtime errors like the `JsonException` encountered with the `ReferenceDataService`, we will introduce a unit testing strategy.

This document outlines the approach for adding unit tests, starting with the `ReferenceDataService`.

## 1. Testing Framework

-   **Framework:** We will use **xUnit**, a popular, free, open-source, community-focused unit testing tool for the .NET Framework.
-   **Mocking Library:** We will use **Moq**, a popular and powerful mocking library for .NET, to isolate our services from their dependencies (like `HttpClient` and `IMemoryCache`).

## 2. Project Setup

A new xUnit Test Project will be added to the solution.

-   **Project Name:** `GetFitterGetBigger.Admin.Tests`
-   **Dependencies:** This project will need to reference the main `GetFitterGetBigger.Admin` project and add NuGet packages for `xUnit`, `Moq`, and `Microsoft.Extensions.Http.Mocks`.

## 3. Unit Testing the `ReferenceDataService`

We will create a test class `ReferenceDataServiceTests.cs` to verify the behavior of the `ReferenceDataService`.

### Key Scenarios to Test:

1.  **Happy Path (Successful API Call):**
    -   **Goal:** Verify that when the API returns valid data, the service correctly deserializes it and returns it.
    -   **Setup:** Mock `HttpClient` to return a successful `HttpResponseMessage` with a valid JSON string.
    -   **Assert:** The service method returns the expected `IEnumerable<ReferenceDataDto>`.

2.  **Data Type Mismatch (The Recent Bug):**
    -   **Goal:** Verify that the service can handle cases where the API sends a number as a string (e.g., `"id": "123"`).
    -   **Setup:** Mock `HttpClient` to return JSON where the `id` is a string.
    -   **Assert:** The service correctly deserializes the data without throwing an exception (this is covered by the DTO change to use `string Id`).

3.  **Caching Behavior:**
    -   **Goal:** Verify that the service correctly uses the cache.
    -   **Setup:**
        -   Call a service method once to populate the cache.
        -   Call the same method a second time.
    -   **Assert:**
        -   Verify that `HttpClient`'s `SendAsync` method was only called **once**.
        -   Verify that `IMemoryCache`'s `TryGetValue` was called on the second request.

4.  **API Failure:**
    -   **Goal:** Verify that the service handles API errors gracefully.
    -   **Setup:** Mock `HttpClient` to return an unsuccessful status code (e.g., 500 Internal Server Error).
    -   **Assert:** The service should throw an `HttpRequestException` or handle it in a defined way.

### Example Test (for Happy Path)

```csharp
[Fact]
public async Task GetDifficultyLevelsAsync_WhenApiCallIsSuccessful_ReturnsData()
{
    // Arrange
    var mockHttp = new Mock<HttpMessageHandler>();
    var response = new HttpResponseMessage
    {
        StatusCode = System.Net.HttpStatusCode.OK,
        Content = new StringContent("[{\"id\": \"1\", \"name\": \"Beginner\"}]")
    };

    mockHttp.Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>()
        )
        .ReturnsAsync(response);

    var httpClient = new HttpClient(mockHttp.Object);
    var mockCache = new Mock<IMemoryCache>();
    // ... setup cache mock ...

    var config = new ConfigurationBuilder().AddInMemoryCollection(new... {{"ApiBaseUrl", "http://test"}}).Build();
    
    var service = new ReferenceDataService(httpClient, mockCache.Object, config);

    // Act
    var result = await service.GetDifficultyLevelsAsync();

    // Assert
    Assert.NotNull(result);
    Assert.Single(result);
    Assert.Equal("Beginner", result.First().Name);
}
```

## Next Steps

1.  Create the `GetFitterGetBigger.Admin.Tests` project.
2.  Add the necessary NuGet packages (`xunit`, `moq`, etc.).
3.  Implement the `ReferenceDataServiceTests.cs` with the scenarios outlined above.

By implementing this testing strategy, we can catch integration and data format issues early, leading to a more stable and reliable application.

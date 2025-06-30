# API Endpoint Configuration Implementation Tasks

## Feature Branch: `feature/api-endpoint-configuration`

### Category 1: Core Endpoint Service
- **Task 1.1:** Create IApiEndpointService interface for URL building `[ReadyToDevelop]`
- **Task 1.2:** Implement ApiEndpointService with configuration support `[ReadyToDevelop]`
- **Task 1.3:** Write unit tests for ApiEndpointService `[ReadyToDevelop]`
- **Task 1.4:** Create ApiEndpoints static class with all endpoint constants `[ReadyToDevelop]`
- **Task 1.5:** Write tests for endpoint constants validation `[ReadyToDevelop]`

### Category 2: Configuration Updates
- **Task 2.1:** Update appsettings.json with ApiVersion configuration `[ReadyToDevelop]`
- **Task 2.2:** Update appsettings.Development.json to ensure consistency `[ReadyToDevelop]`
- **Task 2.3:** Add configuration validation on startup `[ReadyToDevelop]`
- **Task 2.4:** Write tests for configuration validation `[ReadyToDevelop]`

### Category 3: Service Registration
- **Task 3.1:** Register ApiEndpointService in Program.cs as singleton `[ReadyToDevelop]`
- **Task 3.2:** Configure dependency injection for endpoint service `[ReadyToDevelop]`
- **Task 3.3:** Write integration tests for service registration `[ReadyToDevelop]`

### Category 4: Refactor Existing Services
- **Task 4.1:** Refactor AuthService to use ApiEndpointService `[ReadyToDevelop]`
- **Task 4.2:** Update AuthService tests for new endpoint handling `[ReadyToDevelop]`
- **Task 4.3:** Refactor ReferenceDataService to use ApiEndpointService `[ReadyToDevelop]`
- **Task 4.4:** Update ReferenceDataService tests for new endpoint handling `[ReadyToDevelop]`
- **Task 4.5:** Ensure consistent HttpClient configuration across services `[ReadyToDevelop]`

### Category 5: Error Handling Foundation
- **Task 5.1:** Create ApiException class for API-specific errors `[ReadyToDevelop]`
- **Task 5.2:** Add basic error response parsing to services `[ReadyToDevelop]`
- **Task 5.3:** Write tests for error handling scenarios `[ReadyToDevelop]`

### Category 6: Documentation
- **Task 6.1:** Create API-CONFIGURATION.md documentation file `[ReadyToDevelop]`
- **Task 6.2:** Document endpoint service usage patterns `[ReadyToDevelop]`
- **Task 6.3:** Add inline documentation to interfaces and services `[ReadyToDevelop]`

## Notes
- Each implementation task must be immediately followed by its test task
- No task is complete until build passes and all tests are green
- Keep build warnings to minimum
- This feature is foundational and blocks Exercise Management and Workout Builder features
- Focus on creating a flexible, testable architecture that can be extended

## Success Criteria
- All API URLs are centrally managed
- Easy to add new endpoints without code duplication
- Support for API versioning
- Consistent URL formatting across all services
- All existing functionality continues to work
- Comprehensive test coverage for URL building logic
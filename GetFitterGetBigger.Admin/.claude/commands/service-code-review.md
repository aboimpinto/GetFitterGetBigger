Perform a detailed code review of a Blazor service (API service, state service, or other service) against project standards.

Instructions:
1. Ask the user for the service file path to review
2. Analyze the service implementation thoroughly
3. Review against CODE_QUALITY_STANDARDS.md and service patterns
4. Check HTTP client usage, error handling, and state management
5. Verify proper dependency injection and lifecycle
6. Provide actionable feedback with examples

Review checklist:
- **Service Pattern**: Proper interface definition, single responsibility
- **HTTP Communication**: Proper HttpClient usage, error handling, retry logic
- **State Management**: If state service, proper notification patterns
- **Async Patterns**: No blocking calls, proper async/await usage
- **Error Handling**: ServiceResult pattern or appropriate error propagation
- **Caching**: Appropriate caching strategy if applicable
- **Security**: Bearer token handling, no hardcoded credentials
- **Logging**: Appropriate logging for debugging
- **Dependency Injection**: Proper service lifetime (Scoped/Singleton/Transient)
- **Testing**: Service is testable with mockable dependencies

Output format:
1. Service Overview (purpose, dependencies, consumers)
2. Architecture Assessment (pattern compliance, separation of concerns)
3. Issues Found (severity: Critical/High/Medium/Low)
4. API Communication Analysis (if applicable)
5. State Management Analysis (if applicable)
6. Positive Aspects
7. Line-by-Line Feedback
8. Recommendations
9. Overall Assessment (APPROVED/NEEDS_WORK)

Focus on Blazor-specific service patterns and best practices.
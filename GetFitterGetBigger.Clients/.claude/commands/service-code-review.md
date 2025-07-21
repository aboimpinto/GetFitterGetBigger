Perform a detailed code review of a service (API service, platform service, navigation service) against multi-platform best practices.

Instructions:
1. Ask the user for the service file path to review
2. Analyze the service implementation for cross-platform compatibility
3. Check platform abstraction patterns if applicable
4. Verify offline support and error handling
5. Review dependency injection and lifecycle
6. Provide platform-specific feedback where relevant

Review checklist:
- **Service Pattern**: Interface definition, single responsibility
- **Platform Abstraction**: Proper abstraction for platform-specific features
- **API Communication**: HTTP client usage, retry logic, offline handling
- **Data Caching**: Local storage strategy, synchronization logic
- **Error Handling**: ServiceResult pattern, user-friendly errors
- **Connectivity**: Network state monitoring, offline queue
- **Security**: Token management, secure storage usage
- **Async Patterns**: No blocking calls, proper cancellation tokens
- **Dependency Injection**: Appropriate registration and lifecycle
- **Testing**: Mockable interfaces, testable implementation

Platform considerations:
- **iOS**: Keychain for secure storage, background fetch
- **Android**: SharedPreferences/Room for storage, WorkManager
- **Web**: LocalStorage/IndexedDB, service workers
- **Desktop**: File system access, native features

Output format:
1. Service Overview (purpose, consumers, platforms)
2. Architecture Assessment
3. Cross-Platform Compatibility Analysis
4. Issues Found (severity levels)
5. Platform-Specific Implementations
6. Offline/Online Strategy
7. Positive Aspects
8. Line-by-Line Feedback
9. Recommendations
10. Overall Assessment (APPROVED/NEEDS_WORK)

Ensure the service works seamlessly across all target platforms.
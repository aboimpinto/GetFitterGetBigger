Create a new platform-specific service with proper abstraction for cross-platform usage.

Instructions:
1. Ask for the service name, purpose, and required platforms
2. Create the shared interface in the common project
3. Create platform-specific implementations
4. Set up dependency injection for each platform
5. Include fallback behavior for unsupported platforms
6. Add unit tests for the service

Shared interface structure:
```csharp
// In shared project
public interface I[Service]Service
{
    Task<Result> PerformActionAsync(parameters);
    bool IsSupported { get; }
}
```

Platform implementations:
1. iOS: Using iOS-specific APIs (UIKit, Foundation)
2. Android: Using Android-specific APIs (Android.*, Java interop)
3. Web: Using web APIs (JavaScript interop if needed)
4. Desktop: Using desktop-specific APIs

Service patterns:
- Return default/empty results for unsupported platforms
- Use platform checks sparingly
- Proper error handling for platform-specific failures
- Async methods for potentially long operations
- Cancellation token support

Common platform services:
- File storage service
- Camera/media service
- Location service
- Notification service
- Device info service
- Secure storage service

Dependency injection setup for each platform's startup/bootstrap code.
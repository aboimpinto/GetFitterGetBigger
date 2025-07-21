# GetFitterGetBigger Clients

Multi-platform client applications for GetFitterGetBigger, supporting iOS, Android, Web, and Desktop platforms.

## Overview

This project contains the client applications that allow users to access GetFitterGetBigger on their preferred devices. Built using Xamarin.Forms/MAUI for maximum code sharing while respecting platform-specific design guidelines.

## Project Structure

```
GetFitterGetBigger.Clients/
├── GetFitterGetBigger.Clients/          # Shared code project
│   ├── ViewModels/                      # MVVM ViewModels (platform-agnostic)
│   ├── Views/                           # Shared XAML views
│   ├── Services/                        # Service interfaces
│   ├── Models/                          # Data models
│   └── Resources/                       # Shared resources (strings, images)
├── GetFitterGetBigger.Clients.iOS/      # iOS-specific implementation
├── GetFitterGetBigger.Clients.Android/  # Android-specific implementation
├── GetFitterGetBigger.Clients.Web/      # Web implementation (if applicable)
├── GetFitterGetBigger.Clients.Desktop/  # Desktop implementation
├── GetFitterGetBigger.Clients.Tests/    # Unit tests
└── memory-bank/                         # Project documentation
```

## Quick Start

### Prerequisites

- .NET 9.0 SDK
- Visual Studio 2022 or Visual Studio for Mac
- Xcode (for iOS development)
- Android SDK (for Android development)
- Platform-specific SDKs

### Building

```bash
# Build all projects
dotnet build

# Build specific platform
dotnet build GetFitterGetBigger.Clients.Android
dotnet build GetFitterGetBigger.Clients.iOS
```

### Running

#### Android
```bash
dotnet build -t:Run -f net9.0-android GetFitterGetBigger.Clients.Android
```

#### iOS (Mac only)
```bash
dotnet build -t:Run -f net9.0-ios GetFitterGetBigger.Clients.iOS
```

#### Desktop
```bash
dotnet run --project GetFitterGetBigger.Clients.Desktop
```

## Architecture

### MVVM Pattern
- **Models**: Data structures and business entities
- **Views**: Platform-specific or shared UI (XAML)
- **ViewModels**: Business logic and data binding (platform-agnostic)

### Platform Abstraction
Platform-specific features are abstracted through interfaces:
```csharp
public interface IDeviceService
{
    string Platform { get; }
    Task<bool> RequestPermissionAsync(Permission permission);
}
```

### Code Sharing Strategy
- ~85% shared code in ViewModels and Services
- ~15% platform-specific for native features
- Shared UI where possible, platform-specific when needed

## Features

### Cross-Platform
- User authentication (OAuth)
- Exercise management
- Workout planning
- Progress tracking
- Offline support with sync

### Platform-Specific
- **iOS**: Face ID/Touch ID, Apple Health integration
- **Android**: Fingerprint auth, Google Fit integration
- **Desktop**: Keyboard shortcuts, multi-window support
- **Web**: PWA capabilities, responsive design

## Development

### Code Quality Standards
- Universal standards: See memory-bank/CODE_QUALITY_STANDARDS.md
- Multi-platform specific: See memory-bank/CLIENTS-CODE_QUALITY_STANDARDS.md
- Code review process: See memory-bank/CODE_REVIEW_PROCESS.md

### Testing

Run unit tests:
```bash
dotnet test GetFitterGetBigger.Clients.Tests
```

Run UI tests (platform-specific):
```bash
# Android UI tests
dotnet test GetFitterGetBigger.Clients.UITests.Android

# iOS UI tests
dotnet test GetFitterGetBigger.Clients.UITests.iOS
```

### Platform Guidelines

Follow platform-specific design guidelines:
- **iOS**: [Human Interface Guidelines](https://developer.apple.com/design/human-interface-guidelines/)
- **Android**: [Material Design](https://material.io/design)
- **Windows**: [Fluent Design System](https://docs.microsoft.com/en-us/windows/apps/design/)

## Claude AI Assistant

This project includes custom slash commands for Claude AI to assist with multi-platform development:

- `/feature-code-review` - Comprehensive feature review across all platforms
- `/view-code-review` - Review View/ViewModel pairs
- `/service-code-review` - Review service implementations
- `/platform-implementation-review` - Review platform-specific code
- `/ui-consistency-check` - Ensure UI consistency across platforms
- `/new-view` - Create new MVVM view/viewmodel
- `/new-platform-service` - Create platform service with abstractions
- `/start-implementing` - Begin feature implementation
- `/catch-up` - Get up to speed with project context

See `.claude/commands/README.md` for detailed command documentation.

## Configuration

### API Configuration
Update API endpoints in each platform's configuration:
- iOS: `Info.plist`
- Android: `AndroidManifest.xml` or `Resources/values/strings.xml`
- Desktop: `appsettings.json`

### Platform-Specific Settings
Each platform project contains its own configuration for:
- Push notifications
- Analytics
- Crash reporting
- Feature flags

## Deployment

### iOS
1. Archive in Visual Studio for Mac or Xcode
2. Upload to App Store Connect
3. Submit for review

### Android
1. Build APK or AAB
2. Sign with release key
3. Upload to Google Play Console

### Desktop
1. Build platform-specific packages
2. Sign with appropriate certificates
3. Distribute via store or direct download

## Contributing

1. Create feature branch from master
2. Follow MVVM patterns and platform abstractions
3. Ensure all platforms are tested
4. Submit pull request with platform impact analysis

## Documentation

- Feature documentation: See memory-bank/features/
- API integration: See memory-bank/api-integration.md
- Platform guides: See memory-bank/platform-*.md files
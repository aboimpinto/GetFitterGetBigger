# Claude Slash Commands Reference - Clients Project

This directory contains custom slash commands for Claude to use in the GetFitterGetBigger Clients (Multi-platform) project.

## Code Review Commands

### `/feature-code-review`
Performs a comprehensive final code review for the entire feature across all platforms.
- Checks: MVVM compliance, cross-platform consistency, platform guidelines
- Validates: Offline support, navigation, performance
- Output: Complete feature review report
- Location: `/memory-bank/features/2-IN_PROGRESS/FEAT-XXX/code-reviews/`
- When to use: Before moving a feature to COMPLETED status

### `/view-code-review`
Performs a detailed review of a View/ViewModel pair.
- Reviews: XAML/UI and ViewModel implementation
- Checks: MVVM patterns, data binding, commands
- Validates: Platform UI compliance, accessibility
- When to use: Reviewing individual screens/pages

### `/service-code-review`
Reviews service implementations (API, platform, navigation services).
- Focuses on: Cross-platform compatibility, abstraction patterns
- Checks: Offline support, error handling, platform-specific code
- When to use: Reviewing service layer implementations

### `/platform-implementation-review`
Reviews platform-specific implementations of shared interfaces.
- Checks: Interface compliance, platform guidelines
- Validates: Native feature usage, performance
- When to use: Reviewing iOS/Android/Web/Desktop specific code

### `/ui-consistency-check`
Checks UI/UX consistency across all platforms for a feature.
- Compares: Visual design, functionality, navigation
- Validates: Brand consistency, platform conventions
- When to use: Ensuring consistent user experience

## Development Commands

### `/start-implementing`
Begin implementing a feature with multi-platform approach.
- Creates platform-aware implementation plan
- Identifies shared vs platform-specific components
- Focuses on MVVM and cross-platform architecture

### `/new-view`
Create a new View/ViewModel pair following MVVM.
- Creates: ViewModel with proper patterns
- Generates: XAML view with data binding
- Includes: Platform-specific variations if needed

### `/new-platform-service`
Create a platform service with proper abstraction.
- Creates: Shared interface
- Implements: Platform-specific versions
- Configures: Dependency injection

## Other Commands

### `/catch-up`
Read the memory-bank and understand multi-platform context.
- Reviews: Architecture, platform structure
- Focuses on: MVVM patterns, platform abstractions

## Usage Examples

### Reviewing a View/ViewModel
```
/view-code-review
[Claude asks]: Which View/ViewModel would you like me to review?
[You provide]: Review EquipmentPage and EquipmentViewModel
```

### Creating Platform Service
```
/new-platform-service
[Claude asks]: What service do you need and for which platforms?
[You provide]: Create a FileStorageService for iOS, Android, and Desktop
```

### UI Consistency Check
```
/ui-consistency-check
[Claude asks]: Which feature should I check for consistency?
[You provide]: Check the Exercise List screen across all platforms
```

## Multi-Platform Architecture

These commands support development for:
- **iOS**: Native iOS app using Xamarin.iOS/MAUI
- **Android**: Native Android app using Xamarin.Android/MAUI
- **Web**: Progressive Web App (if applicable)
- **Desktop**: Windows/macOS desktop apps

## Key Patterns

### MVVM Architecture
- ViewModels: Platform-agnostic business logic
- Views: Platform-specific or shared UI
- Services: Abstracted platform features
- Navigation: Centralized navigation service

### Platform Abstraction
```
Shared Project
├── ViewModels/       (shared)
├── Services/         (interfaces)
├── Models/           (shared)
└── Views/            (shared XAML)

Platform Projects
├── iOS/
│   └── Services/     (iOS implementations)
├── Android/
│   └── Services/     (Android implementations)
└── Desktop/
    └── Services/     (Desktop implementations)
```

## Best Practices

1. **Code Sharing**: Maximize shared code in ViewModels and Services
2. **Platform Respect**: Follow each platform's design guidelines
3. **Abstraction**: Use interfaces for platform-specific features
4. **Testing**: Unit test ViewModels, UI test each platform
5. **Offline First**: Design for offline with sync capabilities
6. **Responsive**: Design UI that adapts to all screen sizes

## Standards References

All commands follow:
- `CODE_QUALITY_STANDARDS.md` (universal)
- `CLIENTS-CODE_QUALITY_STANDARDS.md` (multi-platform specific)
- `CODE_REVIEW_PROCESS.md` (review process)
- Platform guidelines (iOS HIG, Material Design, etc.)
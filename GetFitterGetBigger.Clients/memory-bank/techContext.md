# Technical Context

> For a comprehensive overview of the technology stack across the entire ecosystem, please refer to the [Shared Memory Bank](/Shared/memory-bank/techContext.md).

## Client-Specific Technology Stack

The GetFitterGetBigger Client Applications are built using the following technologies:

### Core Technologies
- **Avalonia UI**: Cross-platform UI framework for .NET
- **C#**: Primary programming language
- **.NET 9.0**: Latest version of Microsoft's development platform (upgraded from .NET 7)
- **SQLite**: Lightweight embedded database for local storage

### Client Platforms
- **Android**: Mobile application for Android devices
- **iOS**: Mobile application for Apple devices
- **Browser**: Web application for browser access
- **Desktop**: Native application for Windows, macOS, and Linux

### Framework Components
- **MVVM Pattern**: For UI architecture
- **Olimpo Libraries**: Custom libraries for application infrastructure
  - Olimpo.BootstrapperManager: For application initialization
  - Olimpo.EventAggregatorManager: For loosely coupled communication
  - Olimpo.NavigationManager: For view navigation
  - Olimpo.IoC: For dependency injection
  - Olimpo.Controls: For custom UI controls
  - Olimpo.Common: For shared utilities

### API Communication
- **RESTful API Client**: For communication with the backend
- **JSON Serialization**: For data interchange
- **JWT Authentication**: For secure API access

## Development Setup

### Prerequisites
- **.NET 9.0 SDK**: For development and building
- **Visual Studio Code**: With C# extension
- **Avalonia UI Templates**: For UI development
- **SQLite Browser**: For database inspection

### Project Structure
- **GetFitterGetBigger**: Core project with shared code
- **GetFitterGetBigger.Android**: Android-specific implementation
- **GetFitterGetBigger.iOS**: iOS-specific implementation
- **GetFitterGetBigger.Browser**: Web-specific implementation
- **GetFitterGetBigger.Desktop**: Desktop-specific implementation

## Technical Constraints

### Cross-Platform Considerations
- **UI Consistency**: Maintaining consistent UI across platforms
- **Platform-Specific Features**: Handling platform-specific capabilities
- **Performance Optimization**: Ensuring good performance on all devices

### Offline Support
- **Local Data Storage**: For offline functionality
- **Synchronization**: When connectivity is restored
- **Conflict Resolution**: For handling data conflicts

### Mobile Constraints
- **Battery Usage**: Minimizing battery consumption
- **Memory Usage**: Efficient memory management
- **Screen Size Adaptation**: Supporting various screen sizes

## Dependencies

### External Packages
- **Avalonia UI**: Core UI framework
- **SQLite-Net**: For local database access
- **Newtonsoft.Json**: For JSON serialization
- **RestSharp**: For REST API communication

### Internal Dependencies
- **Shared Models**: From the Shared project
- **API Endpoints**: For data retrieval and updates
- **Authentication Services**: For secure access

## Tool Usage Patterns

### UI Development
- **XAML**: For defining UI layouts
- **Styles and Themes**: For consistent appearance
- **Data Binding**: For connecting UI to ViewModels

### Data Management
- **Repository Pattern**: For data access abstraction
- **Local Caching**: For offline functionality
- **Synchronization**: For keeping data up-to-date

### Navigation
- **ViewModel-First Navigation**: Using Olimpo.NavigationManager
- **Navigation History**: For back navigation support
- **Deep Linking**: For direct access to specific views

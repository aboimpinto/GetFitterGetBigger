# System Patterns

> For a comprehensive overview of the entire ecosystem architecture, please refer to the [Shared Memory Bank](/Shared/memory-bank/systemPatterns.md).

## Client Applications Architecture

The GetFitterGetBigger Client Applications follow a modular architecture designed for cross-platform compatibility and a consistent user experience across different devices.

### Core Architecture

- **Modular Design**: Separate modules for workout execution, progress tracking, and user management
- **Cross-Platform**: Single codebase supporting Android, iOS, Browser, and Desktop
- **API Integration**: Communication with the central API for data retrieval and synchronization
- **Offline Support**: Local data storage for offline functionality

## Key Technical Decisions

- **Avalonia UI**: Cross-platform UI framework for consistent user experience
- **C#**: Primary programming language for all client applications
- **SQLite**: Local database for offline data storage
- **RESTful API Communication**: For interaction with the central API

## Design Patterns in Use

- **Model-View-ViewModel (MVVM)**: For separation of concerns in UI development
- **Dependency Injection (DI)**: For managing dependencies and facilitating testing
- **Repository Pattern**: For abstracting data access operations
- **Observer Pattern**: For event handling and UI updates
- **Command Pattern**: For encapsulating UI actions

## Component Relationships

- **UI Layer**: Avalonia UI components for user interaction
- **ViewModel Layer**: Business logic and state management
- **Model Layer**: Data structures and business entities
- **Repository Layer**: Data access and synchronization
- **Service Layer**: Cross-cutting concerns and utilities

### Communication Flow

- **UI to ViewModel**: Through data binding and commands
- **ViewModel to Model**: Through direct method calls
- **ViewModel to Repository**: For data access operations
- **Repository to API**: For remote data operations
- **Cross-Module Communication**: Through the Event Aggregator

## Critical Implementation Paths

1. **Workout Execution Flow**
   - Workout selection and initialization
   - Exercise sequence navigation
   - Set/rep tracking and recording
   - Rest period management
   - Workout completion and feedback

2. **Offline Synchronization**
   - Local data storage during offline usage
   - Conflict resolution during synchronization
   - Background synchronization when connectivity is restored

3. **Progress Tracking**
   - Performance metrics calculation
   - Progress visualization
   - Achievement recognition
   - Historical data analysis

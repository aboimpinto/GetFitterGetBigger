# Active Context

## Current Work Focus:
Initializing the memory bank and setting up the project structure.

## Recent Changes:
- Created the `memory-bank` directory.
- Created the `projectbrief.md` file.
- Created the `productContext.md` file.
- Updated the `Olimpo.NavigationManager.md` file with new examples and details on registering navigatable ViewModels and navigating using string identifiers.

## Next Steps:
- Create the remaining core files: `systemPatterns.md`, `techContext.md`, and `progress.md`.
- Define the system architecture and key technical decisions.
- Set up the development environment and dependencies.

## Active Decisions and Considerations:
- Choosing the right architecture for the application.
- Selecting the appropriate technologies and frameworks.
- Defining the data model and database schema.

## Important Patterns and Preferences:
- Using a modular architecture to promote code reusability and maintainability.
- Following SOLID principles to ensure code quality and flexibility.
- Implementing a consistent coding style and naming conventions.

## Learnings and Project Insights:
- The importance of a well-defined project brief and product context.
- The need for a clear understanding of the target audience and their needs.
- The value of a modular architecture and SOLID principles.

## TimeBaseExerciseViewModel Binding:
- Added necessary code to `TimeBaseExerciseViewModel.cs` to bind to the fields in the View.
- Implemented the  `ILoadableViewModel` Interface and when the LoadAsync is called, initiate a Observable.Interval with 1 second delay and update the Time field  with the  format "00:00".
- When the time reach the end raise the event TimedSetFinishedEvent, using the IEventAggregator interface.
- In the constructor, the field Time should be initialized with the string "00:00"
- Corrected the property name from `Seconds` to `TimeInSeconds` to match the `TimeBaseExerciseWorkoutRound` class.

## Active Features:
- [Workouts Retrieval](features/WorkoutsRetrival.md) - Retrieve and initializes the available workouts at startup
- [Olimpo.NavigationManager](features/Olimpo.NavigationManager.md) - Facilitates navigation between views using a ViewModel-first approach
- [Olimpo.EventAggregator](features/Olimpo.EventAggregator.md) - Decouples modules by allowing them to communicate via messages
- [Olimpo.Bootstrapper](features/Olimpo.Bootstrapper.md) - Initializes and configures the application at startup
- [NavigationManager] Added the option to add the View to History in the `NavigateAsync` method.

# Progress

## What Works:
- Created the `memory-bank` directory and core files.
- Defined the project brief and product context.
- Established the system architecture and key technical decisions.
- Set up the development environment and dependencies.
- Implemented the  `ILoadableViewModel` Interface and when the LoadAsync is called, initiate a Observable.Interval with 1 second delay and update the Time field  with the  format "00:00" in the TimeBaseExerciseViewModel.
- When the time reach the end raise the event TimedSetFinishedEvent, using the IEventAggregator interface.
- In the constructor, the field Time should be initialized with the string "00:00"
- Fix the Olimpo.EventAggregatorManager namespace issue.
- Implement the interface IHandle<TimedSetFinishedEvent> in the WorkoutWorkflowViewModel and in the method implementation, increase the WorkoutWorflowStep and load the step.
- When a timed based set is reached, hide the button area in the view. Have in consideration when the user reach the end of the workout, there is a button for that also. When the event TimedSetFinishedEvent is received, make the buttons area visible again.
- Implemented the new feature where the user can click on the back button in the device and the app can navigate to the previous opened screen.
- Implemented the GoBack feature in the Olimpo.NavigationManager: The navigation history now correctly stores and retrieves previous screens, allowing users to navigate back as expected.
- Implemented forward and backward navigation transitions: When navigating forward, a forward transition is applied, and when navigating back, a backward transition is applied.

## Proposed Features

### Client-Side Caching for Reference Data

To improve performance, reduce network usage, and provide a better offline experience, a client-side caching strategy for API reference data will be implemented.

*   **Mechanism:** A suitable caching library for Avalonia/mobile development (e.g., Akavache, or a custom service using `System.Runtime.Caching`) will be used to store the data locally. A dedicated service will be responsible for managing this cache.
*   **Dual Invalidation Strategy:**
    1.  **Time-Based Expiration:** The cached data will have a default expiration time that varies based on the data's volatility. For example, static data like `BodyParts` can be cached for 24 hours, while more dynamic data like `Equipment` might be cached for only 30-60 minutes.
    2.  **Explicit Refresh:** The application will provide a mechanism for the user to manually refresh the reference data, such as a "Check for updates" button in the settings screen. This action will clear the cache and force a new fetch from the API.

## New Tasks

*   **Task:** Implement Client-Side Caching for Reference Data
    *   **Description:** Implement the client-side caching strategy as outlined in the "Proposed Features" section. This includes selecting a caching library/mechanism, creating a service to manage the cache, and implementing both time-based and explicit cache invalidation.
    *   **Status:** `[SUBMITTED]`

## What's Left to Build:

## Delayed Tasks
- [DELAYED] Implement user authentication and authorization.
- [DELAYED] Develop the workout planning module.
- [DELAYED] Develop the nutritional guidance module.
- [DELAYED] Develop the progress tracking module.
- [DELAYED] Design and implement the user interface.
- [DELAYED] Test and deploy the application.
- [DELAYED] Implement the `WorkoutDatabase` and `WorkoutRepository` components for WorkoutPersistency.
- [DELAYED] Implement the internet connection check, workout data download, data persistence, data loading, versioning, error handling, and security considerations for WorkoutsRetrival.
- [DELAYED] Implement the workout summary screen, workout progression logic, and workout completion summary for WorkoutWorkflow.

## Current Status:
The project is in the initial stages of development. The memory bank has been initialized and the core files have been created.

## Known Issues:
- None at this time.

## Evolution of Project Decisions:
- The initial decision to use Avalonia UI for cross-platform development has been validated by the availability of mature UI components and tooling.
- The decision to use SQLite for local data storage has been confirmed based on its simplicity and performance.
- Implemented the IHandlesBackButton interface in the NavigationManager.
- [COMPLETED] Add timed set into the WorkoutWorkflow.
- [COMPLETED] Add timed set into the WorkoutWorkflow.
- [COMPLETED] Add WeightRepBase View
- [COMPLETED] Implemented access to the MainView's ViewModel from MainActivity on back click.
- [COMPLETED] Disable the lock device when the application is running.
- [COMPLETED] Refactor OnBackPressed in MainActivity.cs
- [COMPLETED] Refactor the WorkoutsView.
- [COMPLETED] Update the 'NavigationOptions' class to include 'ViewName' and 'Parameters' properties.
- [COMPLETED] Modify the 'NavigateAsync' method to use the updated 'NavigationOptions' class.
- [COMPLETED] Modify the 'NavigateAsync' method to add the option to add the View to History.

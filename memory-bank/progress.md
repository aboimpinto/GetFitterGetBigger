# Progress

## What Works:
- Created the `memory-bank` directory and core files.
- Defined the project brief and product context.
- Established the system architecture and key technical decisions.
- Set up the development environment and dependencies.
- Implemented the  `ILoadableViewModel` Interface and when the LoadAsync is called, initiate a Observable.Interval with 1 second delay and update the Time field  with the  format "00:00" in the TimeBaseExerciseViewModel.
- When the time reach the end raise the event TimedSetFinishedEvent, using the IEventAggregator interface.
- In the constructor, the field Time should be initialized with the string "00:00"

## What's Left to Build:
- Fix the Olimpo.EventAggregatorManager namespace issue.
- Add timed set into the WorkoutWorkflow.
- Implement user authentication and authorization.
- Develop the workout planning module.
- Develop the nutritional guidance module.
- Develop the progress tracking module.
- Design and implement the user interface.
- Test and deploy the application.

## Current Status:
The project is in the initial stages of development. The memory bank has been initialized and the core files have been created.

## Known Issues:
- None at this time.

## Evolution of Project Decisions:
- The initial decision to use Avalonia UI for cross-platform development has been validated by the availability of mature UI components and tooling.
- The decision to use SQLite for local data storage has been confirmed based on its simplicity and performance.

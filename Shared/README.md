# GetFitterGetBigger.Shared

This project contains shared models and code used across the different components of the GetFitterGetBigger ecosystem.

## Models

The shared models include:

- **Workout**: Represents a workout in the GetFitterGetBigger ecosystem.
- **Exercise**: Represents an exercise in the GetFitterGetBigger ecosystem.

## Usage

To use these shared models in other projects, add a reference to the GetFitterGetBigger.Shared project:

```xml
<ProjectReference Include="..\Shared\GetFitterGetBigger.Shared.csproj" />
```

Then, import the namespace in your code:

```csharp
using GetFitterGetBigger.Shared.Models;
```

## Memory Bank

The memory bank for this project contains documentation specific to the shared models and code. It follows the standard structure:

- `projectbrief.md` - Foundation document that shapes all other files
- `productContext.md` - Why this project exists, problems it solves, how it should work
- `activeContext.md` - Current work focus, recent changes, next steps
- `systemPatterns.md` - System architecture, key technical decisions, design patterns
- `techContext.md` - Technologies used, development setup, technical constraints
- `progress.md` - What works, what's left to build, current status, known issues

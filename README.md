# GetFitterGetBigger Ecosystem

This repository contains the components of the GetFitterGetBigger ecosystem, a platform designed to connect coaches with customers for fitness training.

## Components

The ecosystem consists of the following components:

1. **GetFitterGetBigger.Clients** - Client applications developed with C# Avalonia:
   - Android client
   - iOS client
   - Web client
   - Desktop client

2. **GetFitterGetBigger.Admin** - A Blazor application used by Personal Trainers to:
   - Create workouts, plans, and exercises
   - Authorize users
   - Assign users to plans

3. **GetFitterGetBigger.API** - A C# minimal API with Swagger that serves as the backend for both the Admin and Client applications.

4. **Shared** - Contains shared models and code used across the different projects.

## API Documentation

API documentation can be found in the `api-docs` folder. This documentation describes the interfaces between the different components of the ecosystem.

### API Documentation Structure

Each API endpoint is documented in its own file, with a consistent structure:

- **Endpoint URL**
- **HTTP Method**
- **Request Parameters**
- **Request Body Format** (if applicable)
- **Response Codes and Formats**
- **Authentication Requirements**
- **Example Requests and Responses**
- **Projects Using This Endpoint** (Admin, Client, or both)

### Metadata

Each API documentation file includes metadata indicating which projects use the endpoint. This metadata is used by the memory bank update script to propagate changes to the relevant projects.

Example metadata:

```yaml
---
used_by:
  - admin
  - client
---
```

### Memory Bank Update Script

The memory bank update script (`scripts/UpdateMemoryBanks.cs`) automatically updates the memory banks when changes are made to the API documentation. This script:

1. Scans the `api-docs` folder for changes
2. Identifies which projects use the affected API endpoints based on the metadata
3. Updates the `techContext.md` and `activeContext.md` files in those projects' memory banks

To run the script:

- On Windows: Run `scripts/update-memory-banks.bat`
- On Linux/macOS: Run `scripts/update-memory-banks.sh`

## Memory Bank

Each component has its own `memory-bank` folder containing documentation specific to that component. The memory bank follows a standard structure:

- `projectbrief.md` - Foundation document that shapes all other files
- `productContext.md` - Why this project exists, problems it solves, how it should work
- `activeContext.md` - Current work focus, recent changes, next steps
- `systemPatterns.md` - System architecture, key technical decisions, design patterns
- `techContext.md` - Technologies used, development setup, technical constraints
- `progress.md` - What works, what's left to build, current status, known issues

## Workflow for API Changes

### Scenario 1: Missing Field in API Return (Admin Development)

When developing the Admin project and you notice a field is missing in an API response:

1. Document the missing field in the Admin project's `activeContext.md` file.
2. Update the API documentation in the `api-docs` folder to include the missing field.
3. Run the memory bank update script to propagate the changes to the relevant projects.
4. Implement the changes in the API project.
5. Update the shared models if necessary.
6. Update the client projects if they also use the affected endpoint.

### Scenario 2: New API Call (Client Development)

When developing the client project and you need a new API call:

1. Define the new API endpoint (URL, HTTP method, request parameters, etc.).
2. Create a new API documentation file in the `api-docs` folder.
3. Include metadata indicating which projects will use the new endpoint.
4. Run the memory bank update script to propagate the changes to the relevant projects.
5. Implement the new endpoint in the API project.
6. Update the shared models if necessary.
7. Implement the client-side code to use the new API call.

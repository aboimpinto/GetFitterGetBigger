# Reference Tables Feature

## Overview

The Reference Tables feature provides a centralized section within the Admin Application for managing core data entities that are used throughout the system. These tables contain relatively static data that serves as the foundation for creating more complex entities like exercises and workouts.

Examples of reference tables include:
- Body Parts
- Difficulty Levels
- Equipment
- Muscle Groups

## Purpose

The primary goals of this feature are to:

1.  **Centralize Data Management:** Provide a single, intuitive interface for viewing and (in the future) managing all reference data.
2.  **Ensure Data Consistency:** By managing this data in one place, we ensure that exercises and workouts are built from a consistent and controlled vocabulary.
3.  **Streamline Content Creation:** Make it easy for Personal Trainers to understand and select from available options when creating new content.

## Technical Approach

The implementation of this feature follows the established architectural patterns of the application.

### Data Layer

-   **`ReferenceDataService`**: A dedicated service, `ReferenceDataService`, will be responsible for all communication with the API's `ReferenceTables` endpoints.
-   **API Communication**: The service will use `HttpClient` to fetch data from the API. The base URL will be retrieved from `appsettings.json`, and the service will construct the full endpoint path for each table.
-   **Client-Side Caching**: To optimize performance and reduce network traffic, the `ReferenceDataService` will implement a client-side caching mechanism using `IMemoryCache`. Data will be cached with a long expiration time, as it is not expected to change frequently. The service will also provide a method for explicit cache invalidation.

### UI Layer

-   **Technology**: The UI will be built using Blazor components and styled with **Tailwind CSS** to maintain visual consistency with the rest of the application.
-   **Component Structure**:
    -   A main landing page (`ReferenceTables.razor`) will display a grid of cards, each representing one reference table.
    -   A detail page (`ReferenceTableDetail.razor`) will display the items for a selected reference table in a styled table.
-   **Future-Proofing**: The UI will be designed to accommodate future maintenance operations (Create, Update, Delete) by including disabled buttons and placeholders in the layout.

For a detailed, step-by-step guide to implementing this feature, please see the [Tasks documentation](./tasks.md).

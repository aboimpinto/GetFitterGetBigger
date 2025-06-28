# Exercise Management Feature

## 1. Feature Overview

This feature provides Personal Trainers (PTs) with a dedicated interface within the Admin application to manage the entire lifecycle of exercises. A robust exercise library is critical as it forms the foundation for building all workouts and training plans.

This document outlines the UI and client-side logic required for the Exercise Management feature.

---

## 2. UI and Workflow

### Exercise Dashboard Page

- **Access:** A new "Exercises" item will be added to the main navigation sidebar.
- **Layout:** The page will feature a clean, table-based layout to display the list of exercises.
- **Content:** The table will display key information for each exercise, such as `Name`, `Difficulty`, and `Date Created`.

### Key UI Components

1.  **Exercise List:**
    *   Displays all exercises in a paginated table, sorted alphabetically by `Name` by default.
    *   Each row will have "Edit" and "Delete" buttons.
    *   Shows exercise types as badges/tags.
    *   Indicates active/inactive status.

2.  **Filtering and Searching:**
    *   A search bar to filter exercises by `Name`.
    *   A dropdown menu to filter exercises by `Difficulty`.
    *   A toggle to show/hide inactive exercises (default: show only active).
    *   Multi-select filters for muscle groups, equipment, and exercise types.

3.  **Pagination:**
    *   A pagination control showing current page, total pages, and navigation buttons.
    *   The number of items per page will be configurable (defaulting to 20).
    *   Display "Previous" and "Next" buttons based on `hasPreviousPage` and `hasNextPage`.

4.  **Create/Edit Modal:**
    *   A modal dialog will be used for both creating and editing exercises to provide a focused user experience.
    *   The form will contain fields for all exercise properties: `Name`, `Description`, `Coach Notes` (dynamic list), `Exercise Types`, `Difficulty`, `Image URL`, `Video URL`, and the `Is Unilateral` flag.
    *   The `Coach Notes` section will allow adding, removing, and reordering instructional steps.
    *   The `Exercise Types` field will be a multi-select dropdown (with validation for the "Rest" type).
    *   The `Difficulty` field will be a dropdown populated from the `ReferenceDataService`.

---

## 3. Business Logic

- **Data Fetching:** The page will use a dedicated service (e.g., `ExerciseService`) to fetch exercises from the API, including handling pagination and filtering parameters.
  - Parse the paginated response structure with `items` array and metadata.
  - Use `currentPage`, `pageSize`, `totalCount`, and `totalPages` for pagination UI.
- **State Management:** The component will manage the state of the exercise list, current page, filter selections, and active/inactive toggle.
- **Create/Update Operations:** 
  - POST requests return 201 Created with the created exercise.
  - PUT requests return 204 No Content on success.
  - Handle camelCase field names (isUnilateral, imageUrl, videoUrl).
- **Delete Operation:** 
  - The "Delete" button will trigger a confirmation dialog.
  - The API automatically handles soft-delete (sets isActive=false) if the exercise is referenced in workouts.
  - Returns 204 No Content on success.

---

## 4. API Integration

- The feature will communicate with the `/api/exercises` endpoints with the updated response structure.
- It will also integrate with the `/api/ReferenceTables/ExerciseTypes` endpoints for exercise type management.
- It will use a new `ExerciseService` (and `IExerciseService` interface) to encapsulate all API communication logic.
- The service will handle:
  - Construction of API requests with proper field naming (camelCase).
  - Query parameters for filtering (name, difficultyId, isActive, muscleGroupIds, equipmentIds, exerciseTypeIds).
  - Parsing paginated responses with items array and metadata.
  - Managing muscleGroups array with muscleGroupId and muscleRoleId.
- Coach Notes will be managed as an ordered array within exercise requests.
- Exercise Types will be validated to ensure "Rest" type is not combined with other types.

---

## 5. Implementation Tasks

- Create the main `ExerciseDashboard.razor` component.
- Implement the `ExerciseList` component with sorting, filtering, and pagination.
- Create the `ExerciseEditModal.razor` component with a form for all exercise fields.
- Implement the `CoachNotesEditor` component for managing ordered instructions.
- Implement the `ExerciseTypeSelector` component with validation logic.
- Implement the `IExerciseService` and `ExerciseService` to handle all API communication.
- Extend `ReferenceDataService` to include Exercise Types endpoints.
- Register the `ExerciseService` in `Program.cs`.
- Add the "Exercises" link to the `NavMenu.razor` component.
- Ensure the UI is responsive and styled correctly with Tailwind CSS.
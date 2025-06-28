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

2.  **Filtering and Searching:**
    *   A search bar to filter exercises by `Name`.
    *   A dropdown menu to filter exercises by `Difficulty`.
    *   Additional filter options can be added later (e.g., by muscle group, equipment).

3.  **Pagination:**
    *   A pagination control at the bottom of the table to navigate through pages of exercises.
    *   The number of items per page will be configurable (defaulting to 10).

4.  **Create/Edit Modal:**
    *   A modal dialog will be used for both creating and editing exercises to provide a focused user experience.
    *   The form will contain fields for all exercise properties: `Name`, `Description`, `Coach Notes` (dynamic list), `Exercise Types`, `Difficulty`, `Image URL`, `Video URL`, and the `Is Unilateral` flag.
    *   The `Coach Notes` section will allow adding, removing, and reordering instructional steps.
    *   The `Exercise Types` field will be a multi-select dropdown (with validation for the "Rest" type).
    *   The `Difficulty` field will be a dropdown populated from the `ReferenceDataService`.

---

## 3. Business Logic

- **Data Fetching:** The page will use a dedicated service (e.g., `ExerciseService`) to fetch exercises from the API, including handling pagination and filtering parameters.
- **State Management:** The component will manage the state of the exercise list, current page, and filter selections.
- **Create/Update Operations:** The "Save" button in the create/edit modal will trigger a call to the `ExerciseService` to either create a new exercise or update an existing one via the API.
- **Delete Operation:** The "Delete" button will trigger a confirmation dialog. Upon confirmation, it will call the `ExerciseService` to send a `DELETE` request to the API. The UI will then refresh the exercise list.

---

## 4. API Integration

- The feature will communicate with the `/api/exercises` endpoints documented in `api-docs/exercises.md`.
- It will also integrate with the `/api/ReferenceTables/ExerciseTypes` endpoints for exercise type management.
- It will use a new `ExerciseService` (and `IExerciseService` interface) to encapsulate all API communication logic.
- The service will handle the construction of API requests (including query parameters for filtering and pagination) and the deserialization of responses into shared DTOs.
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
# Exercise CRUD Implementation Tasks

## Feature Branch: `feature/exercise-management`

### API Service Layer
- **Task 1.1:** Create IExerciseService interface with all CRUD methods `[Implemented: 6ced60eb]`
- **Task 1.2:** Implement ExerciseService with API integration for all endpoints `[Implemented: 5842933f]`
- **Task 1.3:** Write unit tests for ExerciseService `[Implemented: 2888723d]`
- **Task 1.4:** Create ReferenceTableService for fetching reference data (difficulty, muscle groups, equipment, etc.) `[Skipped - Already exists as ReferenceDataService]`
- **Task 1.5:** Write unit tests for ReferenceTableService `[Skipped - Already exists in ReferenceDataServiceTests]`

### State Management
- **Task 2.1:** Create ExerciseStateService for state management `[Implemented: 4d6aeec9]`
- **Task 2.2:** Implement exercise list state with pagination and filtering `[Implemented: 4d6aeec9]`
- **Task 2.3:** Implement reference table caching in state service `[Implemented: 4d6aeec9]`
- **Task 2.4:** Write tests for ExerciseStateService `[Implemented: c5b6c469]`

### Components - Exercise List
- **Task 3.1:** Create ExerciseList component with table layout `[Implemented: d2c30ce2]`
- **Task 3.2:** Write component tests for ExerciseList `[ReadyToDevelop]`
- **Task 3.3:** Add pagination component to ExerciseList `[Implemented: d2c30ce2]`
- **Task 3.4:** Add filtering UI (name search, difficulty filter, muscle group filter) `[Implemented: d2c30ce2]`
- **Task 3.5:** Write tests for filtering and pagination `[ReadyToDevelop]`
- **Task 3.6:** Add "New Exercise" button with navigation `[Implemented: d2c30ce2]`
- **Task 3.7:** Add click-to-edit functionality on exercise rows `[Implemented: d2c30ce2]`

### Components - Exercise Form
- **Task 4.1:** Create ExerciseForm component structure `[Implemented: be30369a]`
- **Task 4.2:** Add basic fields (name, description, instructions) with validation `[Implemented: be30369a]`
- **Task 4.3:** Write tests for basic field validation `[ReadyToDevelop]`
- **Task 4.4:** Add difficulty dropdown populated from reference table `[Implemented: be30369a]`
- **Task 4.5:** Add isUnilateral checkbox field `[Implemented: be30369a]`
- **Task 4.6:** Add muscle groups multi-select with role assignment (Primary/Secondary/Stabilizer) `[Implemented: be30369a]`
- **Task 4.7:** Write tests for muscle group selection `[ReadyToDevelop]`
- **Task 4.8:** Add equipment multi-select dropdown `[Implemented: be30369a]`
- **Task 4.9:** Add body parts multi-select dropdown `[Implemented: be30369a]`
- **Task 4.10:** Add movement patterns multi-select dropdown `[Implemented: be30369a]`
- **Task 4.11:** Add optional image and video URL fields `[Implemented: be30369a]`
- **Task 4.12:** Implement form submission for create/update `[Implemented: be30369a]`
- **Task 4.13:** Write comprehensive tests for ExerciseForm `[ReadyToDevelop]`

### Components - Exercise Detail/View
- **Task 5.1:** Create ExerciseDetail component for viewing single exercise `[Implemented: 9802e42d]`
- **Task 5.2:** Add edit and delete buttons with proper authorization `[Implemented: 9802e42d]`
- **Task 5.3:** Write tests for ExerciseDetail component `[ReadyToDevelop]`

### Pages & Routing
- **Task 6.1:** Create ExercisesPage container component `[Skipped - Using @page directive in Blazor]`
- **Task 6.2:** Create ExerciseEditPage for add/edit functionality `[Skipped - Using @page directive in ExerciseForm]`
- **Task 6.3:** Create ExerciseDetailPage for viewing single exercise `[Skipped - Using @page directive in ExerciseDetail]`
- **Task 6.4:** Configure React Router routes for exercise pages `[Skipped - Blazor routing via @page]`
- **Task 6.5:** Update navigation menu to highlight active Exercise menu item `[Skipped - Already exists in NavMenu]`
- **Task 6.6:** Write integration tests for page navigation `[ReadyToDevelop]`

### UI/UX Polish
- **Task 7.1:** Add loading skeletons for exercise list `[ReadyToDevelop]`
- **Task 7.2:** Add loading states for form submissions `[Implemented: be30369a]`
- **Task 7.3:** Implement error handling with user-friendly messages `[Implemented: d2c30ce2]`
- **Task 7.4:** Add success notifications for CRUD operations `[Skipped - Navigate after success]`
- **Task 7.5:** Ensure responsive design for mobile/tablet/desktop `[Implemented: d2c30ce2]`
- **Task 7.6:** Add confirmation dialog for delete operations `[Implemented: 9802e42d]`
- **Task 7.7:** Write tests for loading and error states `[ReadyToDevelop]`

### Integration Testing
- **Task 8.1:** Write end-to-end tests for create exercise flow `[ReadyToDevelop]`
- **Task 8.2:** Write end-to-end tests for edit exercise flow `[ReadyToDevelop]`
- **Task 8.3:** Write end-to-end tests for delete exercise flow `[ReadyToDevelop]`
- **Task 8.4:** Write end-to-end tests for list filtering and pagination `[ReadyToDevelop]`

## Notes
- Each implementation task must be immediately followed by its test task
- No task is complete until build passes and all tests are green
- Keep build warnings to minimum
- Follow existing UI patterns and component library
- Use Tailwind CSS for styling
- Ensure proper error handling for all API calls
- Reference tables should be cached to minimize API calls
- Form validation should match API requirements exactly
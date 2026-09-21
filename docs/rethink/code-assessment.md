# What is recoverable

Source assessment, 6 September 2026. Repository: `aboimpinto/GetFitterGetBigger`, `master`, commit `f56aa61c5fedd69fe525e035b37ab153b838d034`.

There is a substantial exercise and template foundation. The athlete logging, analytics, goals, and competition experience still needs to be built. Existing documentation is useful for recovering intent but does not consistently describe implemented behavior.

## Evidence by area

| Area | Evidence in source | Assessment |
| --- | --- | --- |
| Exercise catalogue | [Exercise](../../GetFitterGetBigger.API/GetFitterGetBigger.API/Models/Entities/Exercise.cs), controllers, repositories, services, admin list/detail/edit forms | Substantial implementation. Preserve the domain knowledge and review the code for reuse. |
| Muscle roles | [ExerciseMuscleGroup](../../GetFitterGetBigger.API/GetFitterGetBigger.API/Models/Entities/ExerciseMuscleGroup.cs) associates exercises, muscles, and roles | Supports primary/secondary classification. Does not itself calculate a person's accumulated muscle workload. |
| Preparation and alternatives | [ExerciseLink](../../GetFitterGetBigger.API/GetFitterGetBigger.API/Models/Entities/ExerciseLink.cs), [link types](../../GetFitterGetBigger.API/GetFitterGetBigger.API/Models/Enums/ExerciseLinkType.cs), four-way admin link manager | Warm-up, cool-down, workout, and alternative link concepts exist. Good foundation for a reviewed relationship catalogue. |
| Additional exercise knowledge | Exercise equipment, body parts, movement patterns, unilateral flag, weight types, coach notes, media URLs | Valuable metadata. Media URLs and seed records are not proof that all previously curated content remains available. |
| Workout templates | [WorkoutTemplatesController](../../GetFitterGetBigger.API/GetFitterGetBigger.API/Controllers/WorkoutTemplatesController.cs), template services, admin forms | Template CRUD, duplication and state transitions are represented in code. Admin includes template metadata editing and exercise display. |
| Template exercises and sets | [WorkoutTemplateExercisesController](../../GetFitterGetBigger.API/GetFitterGetBigger.API/Controllers/WorkoutTemplateExercisesController.cs), [SetConfigurationsController](../../GetFitterGetBigger.API/GetFitterGetBigger.API/Controllers/SetConfigurationsController.cs), services and repositories | More implemented API behavior than the old feature status suggests. A complete admin exercise/set editing workflow was not established by inspection; FEAT-031 still describes further work. |
| Actual workout records | [WorkoutLog](../../GetFitterGetBigger.API/GetFitterGetBigger.API/Models/Entities/WorkoutLog.cs), [WorkoutLogSet](../../GetFitterGetBigger.API/GetFitterGetBigger.API/Models/Entities/WorkoutLogSet.cs), EF mappings | Persistence entities exist. No corresponding logging controller/service/repository workflow was found in the inspected API layers. |
| Goals and progress | [WorkoutLogging specification](../../Features/Workouts/WorkoutLogging/WorkoutLogging_RAW.md), training-program specifications | Recoverable requirements. No implemented goal engine, progress analytics, personal-record engine, or competition backend was found in the inspected application layers. |
| Mobile client | [InitializationWorkflow](../../GetFitterGetBigger.Clients/GetFitterGetBigger/Workflows/InitializationWorkflow.cs), exercise/countdown/rest/workflow views | Real Avalonia prototype, initialized with hard-coded workouts in memory. The inspected weighted exercise view model displays prescribed values; it does not capture actual performance. |
| Admin dashboard | [Dashboard.razor](../../GetFitterGetBigger.Admin/GetFitterGetBigger.Admin/Components/Pages/Dashboard.razor), [NavMenu](../../GetFitterGetBigger.Admin/GetFitterGetBigger.Admin/Components/Layout/NavMenu.razor) | Counters are hard-coded. Plans and clients navigation links have no matching pages in the inspected Pages tree. These are not evidence of working features. |
| Tests | API unit/integration test projects and admin test project | Significant recoverable verification material. Tests were inspected as source, not run; no pass rate or coverage claim is made. |

## Technical baseline

The API and admin target .NET 9; the admin is Blazor with Tailwind assets. The actual API uses ASP.NET Core controllers, despite documentation describing it as a minimal API. The client project references Avalonia, despite a client README mentioning Xamarin/MAUI. Android, iOS, browser and desktop project shells exist; their existence does not establish a working release on every platform.

The backend uses EF Core/PostgreSQL, specialized IDs, repositories, services, and reference-data caching. Those are useful assets; changing the product direction does not by itself require replacing the backend language or database.

## Rebuilding concerns that matter to this product

1. **Identity is not verified at the API login boundary.** [AuthenticationRequest](../../GetFitterGetBigger.API/GetFitterGetBigger.API/DTOs/AuthenticationRequest.cs) contains only an email. [AuthService](../../GetFitterGetBigger.API/GetFitterGetBigger.API/Services/Authentication/AuthService.cs) looks up or creates that user and generates a JWT. No identity-provider proof is checked in that path. The inspected API [Program](../../GetFitterGetBigger.API/GetFitterGetBigger.API/Program.cs) has no authentication/authorization middleware configuration, and controllers have no authorization attributes. Admin OAuth cannot substitute for an enforced API boundary. Identity verification, ownership and role checks must be established before recording real athlete data.

2. **A fresh database cannot be assumed reproducible.** The repository [.gitignore](../../.gitignore) excludes `Migrations/`; no EF migration source files were found, although API startup calls `Database.Migrate()`. SQL seed files and the EF model survive, but those do not establish restoration of a previous database. Rebuild and track a verified schema baseline in an isolated database before importing recovered records.

3. **Historical measurements need more than the existing log schema.** `WorkoutLogSet` records exercise, order, reps, weight, duration, distance, and notes. It lacks explicit set purpose, effort, side/load convention, definition version and sync identity. `WorkoutLog` does not yet capture the template/version relationship described in the specification. These omissions affect comparability, attribution and offline synchronization.

4. **Muscle concepts overlap.** `ExerciseMuscleGroup` and `ExerciseTargetedMuscle` both associate exercise/muscle/role. `WorkoutMuscles` contains 1–10 engagement/load values and a raw template GUID with outdated future-work comments. Establish one canonical mapping and one documented calculation policy; do not treat existing scores as validated measurements.

5. **Some history protections are unfinished.** [ExerciseRepository](../../GetFitterGetBigger.API/GetFitterGetBigger.API/Repositories/Implementations/ExerciseRepository.cs) has a commented-out check for workout references; [WorkoutTemplateQueryDataService](../../GetFitterGetBigger.API/GetFitterGetBigger.API/Services/WorkoutTemplate/DataServices/WorkoutTemplateQueryDataService.cs) contains a future workout-log check. The new product must preserve completed sessions when catalogue/template definitions change.

## Recovery boundary

Feature descriptions were not all lost: `Features/`, `api-docs/`, component memory banks, and the [September 2025 meeting note](../../meeting-with-Alexa-202509.md) remain. The meeting note describes a coach/program marketplace; the current request changes the commercial emphasis to measurement.

Tracked SQL files include catalogue and template seed material. No full historical production data restoration has been performed. If previous exercise authoring or athlete records lived only in a database or media storage, Git history alone cannot recreate those records. Do not mistake demo seeds for the full curated database.

The only additional fetched remote branch is `feature/kinetic-chain-type-for-exercise`; it has no commits beyond the reviewed `master`. That branch does not reveal a later missing implementation. This is not an audit of other machines, deleted branches, backups, or unpushed work.

## Recommended disposition

- **Preserve:** exercise knowledge, stable catalogue identities, relationships, reference vocabulary, template/set domain rules, useful tests, and feature specifications.
- **Rework:** authentication and access boundaries, schema restoration, definition versioning, canonical muscle mappings, admin navigation and workflows.
- **Build:** actual-performance logging, offline persistence/sync, personal goals and benchmarks, explainable analytics, import review, sharing and competition.
- **Replace in the experience:** the client's promotional plan carousel and static dashboard with today's session and goal progress. Keep the existing workout runner as an interaction reference.

Before a rewrite, restore a representative catalogue subset in isolation and prove one complete path: edit an exercise → use it in a session → record actual sets → retain them through restart → show the same numbers in client and authorized coach views. Use that result to decide how much implementation to retain.

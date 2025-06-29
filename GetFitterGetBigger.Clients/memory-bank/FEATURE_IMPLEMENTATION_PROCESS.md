# Feature Implementation Process - Client Projects

This document outlines the standard process for implementing new features across GetFitterGetBigger client applications (Mobile, Web, Desktop).

## Process Overview

### 1. Feature Analysis & Planning
- User provides detailed feature requirements and platform-specific UI/UX specifications
- Create a comprehensive implementation plan with tasks for each platform
- **MANDATORY**: Create a task tracking file at `memory-bank/features/[feature-name]-tasks.md`
- Each task must be marked with status `[ReadyToDevelop]`
- Tasks should be specific, actionable, and independently verifiable
- The task file must include:
  - Feature branch name at the top
  - Tasks organized by platform (Mobile, Web, Desktop) and then by logical categories
  - **Unit/Component test tasks immediately following each implementation task**
  - Clear description of what each task entails
  - Platform-specific considerations
  - Space for commit hashes to be added as tasks are completed

### 2. Branch Creation
- Create a dedicated feature branch from master
- Naming convention: `feature/[descriptive-feature-name]`
- All development work occurs in this isolated branch

### 3. Implementation Phase
- Execute tasks sequentially from the task tracking file
- **For EVERY task implementation:**
  1. Write the implementation code
  2. **MANDATORY: Write unit/component tests for the implemented code in the immediately following task**
  3. **MANDATORY: Keep build warnings to a minimum** (address platform-specific warnings)
  4. **MANDATORY: Run platform-specific build commands:**
     - Mobile: `npm run build` or `npx react-native run-android/ios`
     - Web: `npm run build`
     - Desktop: `dotnet build`
  5. **MANDATORY: Run platform-specific tests:**
     - Mobile: `npm test`
     - Web: `npm test`
     - Desktop: `dotnet test`
  6. **MANDATORY: Ensure code follows platform standards (linting, formatting)**
  7. Only after build succeeds and ALL tests pass, commit the changes
  8. Update the task status in the tracking file from `[ReadyToDevelop]` to `[Implemented: <git-commit-hash>]`
- **For EVERY checkpoint:**
  1. Run platform-specific builds - BUILD MUST BE SUCCESSFUL (no errors)
  2. Run platform-specific tests - ALL TESTS MUST BE GREEN (no failures)
  3. Verify no excessive warnings exist
  4. **MANDATORY: Update checkpoint status from 🛑 to ✅ when all conditions pass**
- Follow platform-specific patterns and conventions
- The task tracking file serves as both documentation and audit trail
- **CRITICAL RULES**:
  - **NO broken builds between tasks** - each task must leave all platforms in a working state
  - **ALL tests must be green** after implementing a task (no skipped, no failures)
  - **Tests are written immediately after implementation** (not deferred to a later phase)
  - Never move to the next task if:
    - Any platform build is failing
    - Build warnings are excessive
    - Tests are not written
    - Any test is failing

### 4. Test Development & Handling
- Write unit tests for all business logic
- Write component/UI tests for all platforms
- Write platform-specific integration tests
- If a test requires complex setup:
  - Create a `[BUG: <detailed-reason>]` entry
  - Mark test appropriately per platform
  - Include bug reference in test comment

### 5. Manual Testing Phase (DEFAULT BEHAVIOR)
- After all implementation tasks are complete
- Provide user with platform-specific testing instructions:
  - **Mobile**: Device/emulator setup, gestures, navigation
  - **Web**: Browser compatibility, responsive breakpoints
  - **Desktop**: OS-specific behaviors, window management
- Include test scenarios covering all user flows
- Wait for user acceptance on all platforms before proceeding

### 6. Feature Finalization
After user explicitly states feature acceptance:
1. Create descriptive commit message summarizing all changes
2. Push feature branch to remote repository
3. Merge feature branch into master locally
4. **MANDATORY: Push the merged master branch to remote repository**
5. Delete the feature branch locally
6. Optionally delete the feature branch on remote

### 7. Special Conditions
- **Platform-Specific Implementation**: Some features may not apply to all platforms
- **Skipping Manual Tests**: Only when user explicitly requests
- **Interrupted Implementation**: Next session can resume using existing task list
- **Git Operations**: All git operations require explicit user approval

## Task Status Definitions
- `[ReadyToDevelop]` - Task identified and ready to implement
- `[Implemented: <hash>]` - Task completed with reference commit
- `[BUG: <reason>]` - Known issue requiring future resolution
- `[Skipped]` - Task deferred or determined unnecessary
- `[N/A - Platform]` - Task not applicable to specific platform
- `[INCOMPLETE: <reason>]` - Task cannot be completed due to external dependency
- `[BLOCKED: BUG-<id>]` - Task blocked by a specific bug

## Important Notes
- This process ensures code quality across all client platforms
- User maintains control over deployment and merge decisions
- Clear audit trail through commit hashes and bug tracking
- Supports both continuous and interrupted development workflows
- Platform-specific code should be clearly isolated

## Handling Blocked Features
When a feature cannot be completed due to external dependencies:
1. Mark the blocking task as `[BLOCKED: BUG-<id>]` with reference to the bug
2. Create a bug entry with platform information
3. Mark the overall feature as `[INCOMPLETE]` at the end of the feature file
4. When the bug is fixed, update statuses accordingly

## Implementation Verification Checklist

Before marking any task as `[Implemented]`, verify:

### All Platforms
- [ ] Implementation code is complete
- [ ] Unit/component tests are written (in the immediately following task)
- [ ] Build runs without errors
- [ ] Build warnings are minimal
- [ ] All tests pass (100% green)
- [ ] Code follows platform conventions
- [ ] No commented-out code or debug statements

### Mobile-Specific
- [ ] Works on both iOS and Android
- [ ] Handles different screen sizes
- [ ] Offline functionality works as expected
- [ ] Performance is acceptable on older devices

### Web-Specific
- [ ] Works across target browsers
- [ ] Responsive design is implemented
- [ ] Accessibility standards are met
- [ ] SEO considerations addressed

### Desktop-Specific
- [ ] Works on Windows, macOS, Linux
- [ ] Window management behaves correctly
- [ ] Native features integrate properly
- [ ] Performance meets desktop standards

## Task Tracking File Template

```markdown
# [Feature Name] Implementation Tasks

## Feature Branch: `feature/[branch-name]`

## Mobile Tasks (React Native)

### API Integration
- **Task M1.1:** Create [Name]Service for API calls `[ReadyToDevelop]`
- **Task M1.2:** Write unit tests for [Name]Service `[ReadyToDevelop]`
- **Task M1.3:** Implement offline caching `[ReadyToDevelop]`

### Components
- **Task M2.1:** Create [Name]Screen component `[ReadyToDevelop]`
- **Task M2.2:** Write component tests for [Name]Screen `[ReadyToDevelop]`
- **Task M2.3:** Create [Name]Card component `[ReadyToDevelop]`
- **Task M2.4:** Write component tests for [Name]Card `[ReadyToDevelop]`

### Navigation
- **Task M3.1:** Add navigation routes `[ReadyToDevelop]`
- **Task M3.2:** Implement deep linking `[ReadyToDevelop]`

### Platform-Specific
- **Task M4.1:** iOS-specific styling adjustments `[ReadyToDevelop]`
- **Task M4.2:** Android-specific permissions `[ReadyToDevelop]`

## Web Tasks (React)

### API Integration
- **Task W1.1:** Create [Name]Service for API calls `[ReadyToDevelop]`
- **Task W1.2:** Write unit tests for [Name]Service `[ReadyToDevelop]`

### Components
- **Task W2.1:** Create [Name]Page component `[ReadyToDevelop]`
- **Task W2.2:** Write component tests for [Name]Page `[ReadyToDevelop]`
- **Task W2.3:** Create responsive [Name]Grid `[ReadyToDevelop]`
- **Task W2.4:** Write component tests for [Name]Grid `[ReadyToDevelop]`

### State Management
- **Task W3.1:** Implement Redux/Context state `[ReadyToDevelop]`
- **Task W3.2:** Write tests for state management `[ReadyToDevelop]`

## Desktop Tasks (Avalonia)

### Services
- **Task D1.1:** Create [Name]Service for API integration `[ReadyToDevelop]`
- **Task D1.2:** Write unit tests for [Name]Service `[ReadyToDevelop]`

### ViewModels
- **Task D2.1:** Create [Name]ViewModel with ReactiveUI `[ReadyToDevelop]`
- **Task D2.2:** Write tests for [Name]ViewModel `[ReadyToDevelop]`

### Views
- **Task D3.1:** Create [Name]View XAML layout `[ReadyToDevelop]`
- **Task D3.2:** Implement data bindings `[ReadyToDevelop]`

### Platform Integration
- **Task D4.1:** Implement native menu integration `[ReadyToDevelop]`
- **Task D4.2:** Add keyboard shortcuts `[ReadyToDevelop]`

## Shared Tasks (If Applicable)

### Shared Models/Logic
- **Task S1.1:** Create shared data models `[ReadyToDevelop]`
- **Task S1.2:** Write tests for shared models `[ReadyToDevelop]`

## Notes
- Tasks prefixed with M (Mobile), W (Web), D (Desktop), S (Shared)
- Each implementation task must be immediately followed by its test task
- Platform-specific tasks may be marked [N/A - Platform] if not applicable
```

## Example Task Status Updates

- `[ReadyToDevelop]` - Initial status for all tasks
- `[Implemented: a1b2c3d4]` - Task completed with commit hash
- `[BUG: React Native navigation issue on Android]` - Platform-specific bug
- `[N/A - Desktop]` - Task not applicable to desktop platform
- `[Skipped]` - Task determined unnecessary during implementation

## Platform-Specific Considerations

### Mobile (React Native)
- Test on both iOS and Android devices/emulators
- Handle platform differences (navigation, permissions, styling)
- Implement proper offline support
- Optimize for battery and performance
- Handle different screen sizes and orientations

### Web (React)
- Test across major browsers
- Implement responsive design for all breakpoints
- Ensure accessibility compliance
- Optimize for SEO where applicable
- Handle browser-specific features gracefully

### Desktop (Avalonia)
- Test on Windows, macOS, and Linux
- Implement native OS integrations
- Handle window management properly
- Provide keyboard shortcuts
- Ensure performance for desktop usage patterns

### Code Sharing
- Identify opportunities for shared logic
- Keep platform-specific code isolated
- Use dependency injection for platform services
- Maintain consistent API contracts across platforms
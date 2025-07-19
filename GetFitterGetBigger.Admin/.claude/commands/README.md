# Claude Slash Commands Reference - Admin Project

This directory contains custom slash commands for Claude to use in the GetFitterGetBigger Admin (Blazor) project.

## Code Review Commands

### `/feature-code-review`
Performs a comprehensive final code review for the entire current feature with Blazor-specific checks.
- Focuses on: Component architecture, state management, performance, accessibility
- Output: Complete feature review report
- Location: `/memory-bank/features/2-IN_PROGRESS/FEAT-XXX/code-reviews/`
- When to use: Before moving a feature to COMPLETED status

### `/component-code-review`
Performs a detailed code review of a specific Blazor component or page.
- Reviews: Both .razor and .razor.cs files
- Checks: Lifecycle management, rendering performance, accessibility
- Output: Detailed component analysis with line-by-line feedback
- When to use: When reviewing individual components for best practices

### `/service-code-review`
Performs a detailed code review of a service class (API service, state service, etc.).
- Focuses on: HTTP client usage, error handling, state management patterns
- Checks: Async patterns, dependency injection, testability
- When to use: When reviewing service implementations

## Development Commands

### `/start-implementing`
Begin implementing a feature from the READY_TO_DEVELOP folder.
- Creates Blazor-specific implementation plan
- Focuses on component structure and UI/UX

### `/new-page`
Create a new Blazor page following project patterns.
- Creates .razor and optional .razor.cs files
- Includes proper routing, authorization, and structure
- Adds to navigation if needed

### `/new-component`
Create a new reusable Blazor component.
- Follows component best practices
- Includes proper parameter and event handling
- Creates in appropriate location (Shared or Feature-specific)

## Other Commands

### `/catch-up`
Read the memory-bank and catch up on Admin project context.
- Focuses on Blazor patterns and current development
- Reviews UI/UX standards and component library

## Usage Examples

### Reviewing a Component
```
/component-code-review
[Claude will ask]: Which component would you like me to review?
[You provide]: /Components/Equipment/EquipmentList.razor
```

### Creating a New Page
```
/new-page
[Claude will ask]: What is the name and purpose of the new page?
[You provide]: Create a WorkoutPlan page for managing workout plans
```

### Starting Feature Implementation
```
/start-implementing
[Claude will]: Check available features and begin implementation with Blazor focus
```

## Best Practices

1. **Component Reviews**: Always review both .razor and .razor.cs files together
2. **Service Reviews**: Pay special attention to HTTP client usage and error handling
3. **New Components**: Ensure accessibility and responsive design from the start
4. **Feature Reviews**: Check for consistent UI/UX across all new components

## Blazor-Specific Considerations

These commands are tailored for Blazor development and include checks for:
- Component lifecycle management
- State management patterns
- Rendering performance
- Memory leak prevention
- Accessibility compliance
- CSS isolation and styling
- Form handling and validation
- Error boundaries and recovery

All commands follow the standards defined in:
- `CODE_QUALITY_STANDARDS.md` (universal)
- `ADMIN-CODE_QUALITY_STANDARDS.md` (Blazor-specific)
- `CODE_REVIEW_PROCESS.md` (review process)
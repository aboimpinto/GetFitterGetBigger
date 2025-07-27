# Refactor with Standards

Use the blazor-refactoring-expert agent to analyze and create a comprehensive refactoring plan for C# Blazor code while strictly adhering to the project's code quality standards and UI design patterns.

## Usage

`/refactor-with-standards <target description>`

Examples:
- `/refactor-with-standards the ExerciseForm component to follow UI standards`
- `/refactor-with-standards the WorkoutService class to be smaller and more maintainable`
- `/refactor-with-standards the ReferenceTable components to use proper patterns`

## Instructions for the Agent

/blazor-refactoring-expert Analyze the specified code and create a comprehensive refactoring plan following these standards:

### 1. Development Process (@memory-bank/DEVELOPMENT_PROCESS.md)
- Follow the established development workflow
- Ensure refactoring maintains build health (zero errors, zero warnings)
- Plan for proper testing at each refactoring phase
- Consider feature workflow states if refactoring is part of a larger feature

### 2. Code Quality Standards (@memory-bank/CODE_QUALITY_STANDARDS.md)
- **Single Exit Point**: Every method should have ONE exit point at the end
- **Pattern Matching**: Use pattern matching over if-else chains
- **Method Length**: Target < 20 lines, maximum complexity of 10
- **Null Safety**: Enable nullable reference types, validate at boundaries
- **No Fake Async**: Don't create async methods without actual async operations
- **Avoid God Classes**: Break down large classes using patterns like Strategy
- **Namespace Usage**: Use using statements instead of fully qualified names
- **Defensive Programming Balance**: Validate at boundaries, trust internal components

### 3. UI Design Standards (for UI components)
- **List Pages**: Follow @memory-bank/UI_LIST_PAGE_DESIGN_STANDARDS.md
  - Consistent container layout with proper spacing
  - Breadcrumb navigation
  - Card styling with shadows
  - Empty states with icons
  - Responsive grid layouts
- **Form Pages**: Follow established form patterns
  - Proper validation feedback
  - Consistent button styling
  - Loading states
  - Error handling

### 4. Reference Tables Pattern (@memory-bank/REFERENCE_TABLES_GUIDE.md)
If refactoring reference table components:
- Use the established Strategy Pattern
- Implement proper caching (24-hour default)
- Follow the type marker pattern with IReferenceTableEntity
- Use GetReferenceDataAsync<T>() generic method
- Ensure automatic registration via assembly scanning

### 5. Testing Requirements (@memory-bank/COMPREHENSIVE-TESTING-GUIDE.md)
Plan for comprehensive testing:
- **Blazor Components**: Use bUnit with proper patterns
  - Add data-testid attributes for testability
  - Make methods/fields internal for test access
  - Test both UI interactions and direct logic
  - Handle async operations with InvokeAsync
- **Services**: Use xUnit with Moq
  - Test happy path, edge cases, and error scenarios
  - Mock external dependencies appropriately
  - Verify caching behavior where applicable
  - Test state rollback for optimistic updates

### Refactoring Plan Structure

Your refactoring plan MUST include:

1. **Executive Summary**
   - Brief overview of main issues found
   - Key improvements proposed
   - Expected benefits after refactoring

2. **Current State Analysis**
   - Specific code quality violations with examples
   - Performance issues identified
   - Maintainability concerns
   - Testing gaps

3. **Proposed Refactoring by Priority**
   - **Critical**: Must fix immediately (build errors, security issues)
   - **High**: Important for code quality (Single Exit violations, God Classes)
   - **Medium**: Beneficial improvements (method length, naming)
   - **Low**: Nice-to-have optimizations

4. **Implementation Roadmap**
   - Phase 1: Non-breaking changes (naming, small extractions)
   - Phase 2: Structural improvements (pattern implementations)
   - Phase 3: Advanced optimizations (performance, caching)

5. **Code Examples**
   - Show BEFORE and AFTER code for each major change
   - Include complete, working code snippets
   - Demonstrate pattern applications

6. **Testing Strategy**
   - New tests required for refactored code
   - Existing tests that need updates
   - Test scenarios to verify refactoring success

7. **Risk Assessment**
   - Breaking changes and migration strategy
   - Performance implications
   - Backward compatibility considerations

### Critical Reminders

- Always check against project-specific standards first
- Provide working code examples, not theoretical advice
- Consider incremental refactoring for large changes
- Ensure all refactored code is more testable than before
- Balance ideal solutions with practical constraints
- Include specific file paths and line numbers when possible

### After Agent Completion

Once the agent provides the refactoring plan:
1. Review the plan with the user for acceptance
2. If accepted, follow @memory-bank/DEVELOPMENT_PROCESS.md to implement
3. Create proper feature/bug tickets if needed
4. Implement changes incrementally with testing at each phase

Remember: The goal is not just cleaner code, but more maintainable, testable, and performant code that follows all project standards.
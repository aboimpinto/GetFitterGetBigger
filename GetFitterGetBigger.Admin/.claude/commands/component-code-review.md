Perform a detailed code review of a specific Blazor component or page against Blazor best practices and project standards.

Instructions:
1. Ask the user for the component/page path to review (or use the currently open/mentioned file)
2. Read and analyze the entire component (.razor and .razor.cs if code-behind exists)
3. Review against CODE_QUALITY_STANDARDS.md and ADMIN-CODE_QUALITY_STANDARDS.md
4. Check for Blazor-specific patterns and anti-patterns
5. Analyze component dependencies and interactions
6. Provide actionable feedback with code examples

Review checklist:
- **Component Architecture**: Proper separation of concerns, focused components (<200 lines)
- **Lifecycle Management**: IDisposable implementation, event handler cleanup, memory leak prevention
- **State Management**: Proper state handling, no direct cross-component communication
- **Rendering Performance**: Unnecessary re-renders avoided, computed values cached
- **Data Binding**: Two-way binding used appropriately, proper parameter usage
- **Event Handling**: Async operations handled correctly, no blocking calls
- **Forms & Validation**: EditForm usage, validation feedback, loading states
- **Error Handling**: ErrorBoundary usage, graceful error recovery
- **Accessibility**: ARIA attributes, keyboard navigation, screen reader support
- **CSS & Styling**: Scoped styles, responsive design, theme support
- **Security**: XSS prevention, authorization checks, no sensitive data in URLs

Output format:
1. Component Overview (purpose, dependencies, interactions)
2. Architecture Assessment (component design, separation of concerns)
3. Issues Found (categorized by severity: Critical/High/Medium/Low)
4. Performance Analysis (rendering efficiency, memory usage)
5. Positive Aspects (what's done well)
6. Specific Line-by-Line Feedback (with line numbers for both .razor and .razor.cs)
7. Recommendations for Improvement
8. Testing Suggestions
9. Overall Assessment (APPROVED/NEEDS_WORK)

The review should be constructive, specific to Blazor patterns, and include examples of improvements.
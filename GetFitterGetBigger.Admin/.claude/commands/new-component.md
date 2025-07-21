Create a new reusable Blazor component following the Admin project patterns.

Instructions:
1. Ask for the component name, purpose, and type (shared/feature-specific)
2. Determine the appropriate location (/Components/Shared or /Components/[Feature])
3. Create the component with proper structure and patterns
4. Implement proper parameter handling and event callbacks
5. Add any required CSS (scoped or in separate file)
6. Create unit tests if the component has complex logic

Component requirements:
- Clear, single responsibility
- Proper parameter definitions with [Parameter] attribute
- Event callbacks for parent communication
- Child content support if applicable
- Proper CSS isolation if styling is needed
- Accessibility attributes (ARIA labels, roles)
- Responsive design

Patterns to follow:
- No direct parent manipulation
- Use EventCallback for parent communication
- Keep components under 200 lines
- Extract complex logic to services
- Proper null/empty handling
- Loading and error states if applicable

The component should be reusable, testable, and follow Blazor best practices.
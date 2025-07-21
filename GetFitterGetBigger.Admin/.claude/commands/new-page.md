Create a new Blazor page following the Admin project patterns and standards.

Instructions:
1. Ask for the page name and purpose
2. Determine the appropriate location in the project structure
3. Create the page component (.razor) with proper structure
4. Create code-behind (.razor.cs) if complex logic is needed
5. Create or update the corresponding service if data access is required
6. Add the page to navigation if needed
7. Follow Blazor component patterns from ADMIN-CODE_QUALITY_STANDARDS.md

Page structure should include:
- Proper @page directive with route
- Authorization attribute if needed
- Loading states for async operations
- Error handling with user-friendly messages
- Responsive design considerations
- Accessibility attributes

Component patterns:
- Dependency injection via [Inject] or constructor (for code-behind)
- Proper lifecycle method usage (OnInitializedAsync, OnParametersSetAsync)
- IDisposable implementation if event subscriptions exist
- State management via cascading parameters or state service

The page should follow the established UI/UX patterns and integrate seamlessly with the existing application.
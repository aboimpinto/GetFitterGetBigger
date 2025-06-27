# Testing Guidelines - Admin Project

## Why Testing Coverage Matters

Testing coverage measures how many code paths and UI interactions are tested. In a React application, this includes component rendering, user interactions, state changes, and API integrations.

## Testing Principles

### 1. Test All Scenarios, Not Just Happy Path

Every component and function should be tested for:
- **Happy Path**: Normal, expected behavior
- **Edge Cases**: Empty states, loading states, error boundaries
- **Error Cases**: API failures, network errors, invalid inputs
- **User Interactions**: Clicks, form submissions, keyboard navigation
- **Accessibility**: Screen reader compatibility, keyboard-only navigation

### 2. Common Patterns to Test

#### For React Components
```javascript
// Happy path
- Component renders without crashing
- Props are displayed correctly
- User interactions trigger expected callbacks

// Edge cases
- Component handles missing/null props
- Empty lists show appropriate messages
- Loading states display correctly
- Error states are user-friendly
```

#### For API Service Methods
```javascript
// Happy path
- Valid requests return expected data
- Data transformation works correctly

// Error cases
- Network errors are caught and handled
- 401/403 responses trigger re-authentication
- 404 responses show appropriate messages
- 500 errors display fallback UI
- Timeout scenarios are handled
```

#### For Form Components
```javascript
// Test all validation branches
- Valid input submits successfully
- Required fields show errors when empty
- Email/URL format validation works
- Character length limits are enforced
- Real-time validation provides feedback
- Submit button disables during submission
```

#### For State Management (Redux/Context)
```javascript
// Actions and Reducers
- Actions create correct payloads
- Reducers update state immutably
- Async actions handle loading/success/error
- State selectors return correct data
```

### 3. Example: Comprehensive Component Testing

```javascript
// ExerciseForm.test.js
describe('ExerciseForm', () => {
  it('renders all form fields') // Happy path
  it('validates required fields on submit') // Validation branch
  it('shows error when name already exists') // Business rule branch
  it('disables submit during API call') // Loading state branch
  it('handles API errors gracefully') // Error branch
  it('clears form after successful submit') // Success branch
  it('supports keyboard navigation') // Accessibility branch
})
```

## Areas Requiring Test Coverage

### 1. Component Testing
- **Rendering Logic**: All conditional renders based on props/state
- **Event Handlers**: onClick, onChange, onSubmit handlers
- **Lifecycle**: useEffect hooks, cleanup functions
- **Error Boundaries**: Component error handling

### 2. Integration Testing
- **API Integration**: Mock API responses for all scenarios
- **Routing**: Navigation between pages
- **Authentication Flow**: Login, logout, token refresh
- **Data Flow**: From API to UI through state management

### 3. Accessibility Testing
- **ARIA Labels**: Proper labeling for screen readers
- **Keyboard Navigation**: Tab order, focus management
- **Color Contrast**: Meets WCAG standards
- **Responsive Design**: Works on all screen sizes

### 4. Performance Testing
- **Component Re-renders**: Minimize unnecessary renders
- **Bundle Size**: Lazy loading, code splitting
- **API Call Optimization**: Caching, debouncing

## Testing Checklist

Before considering a feature fully tested, verify:

- [ ] All components have unit tests
- [ ] User interactions are tested (clicks, typing, etc.)
- [ ] Form validation covers all cases
- [ ] API error scenarios are handled
- [ ] Loading and error states render correctly
- [ ] Accessibility requirements are met
- [ ] Component props are validated
- [ ] State changes are tested
- [ ] Navigation/routing works correctly
- [ ] Performance metrics are acceptable

## Running Test Coverage

```bash
# Run tests with coverage
npm test -- --coverage --watchAll=false

# View coverage report
npm run test:coverage

# Open HTML coverage report
open coverage/lcov-report/index.html

# Run specific test file
npm test ExerciseForm.test.js

# Run tests in watch mode
npm test
```

## Testing Tools and Libraries

### Essential Testing Stack
- **Jest**: Test runner and assertion library
- **React Testing Library**: Component testing
- **MSW (Mock Service Worker)**: API mocking
- **jest-axe**: Accessibility testing
- **user-event**: Simulating user interactions

### Example Test Setup
```javascript
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { rest } from 'msw';
import { setupServer } from 'msw/node';
import { ExerciseForm } from './ExerciseForm';

// Mock API
const server = setupServer(
  rest.post('/api/exercises', (req, res, ctx) => {
    return res(ctx.json({ id: '123', ...req.body }));
  })
);

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

test('creates exercise successfully', async () => {
  const user = userEvent.setup();
  render(<ExerciseForm />);
  
  await user.type(screen.getByLabelText('Name'), 'Push-up');
  await user.click(screen.getByText('Save'));
  
  await waitFor(() => {
    expect(screen.getByText('Exercise created!')).toBeInTheDocument();
  });
});
```

## Best Practices

### 1. Write Tests First (TDD)
- Define expected behavior before implementation
- Write minimal code to make tests pass
- Refactor with confidence

### 2. Keep Tests Simple and Focused
- One assertion per test when possible
- Clear test names that describe behavior
- Avoid testing implementation details

### 3. Use Data-TestId Sparingly
- Prefer accessible queries (getByRole, getByLabelText)
- Use data-testid only when necessary
- Keep selectors maintainable

### 4. Mock External Dependencies
- Mock API calls, not implementation
- Use realistic mock data
- Test loading and error states

### 5. Maintain Test Performance
- Use shallow rendering when appropriate
- Avoid unnecessary waits
- Clean up after tests (unmount, clear timers)

## Remember

**High test coverage != bug-free code**, but it does mean:
- Confidence when refactoring
- Faster debugging with clear test failures
- Living documentation of component behavior
- Better accessibility and user experience
- Reduced regression bugs
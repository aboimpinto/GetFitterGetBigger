# Testing Quick Reference - MUST READ

**IMPORTANT**: This document contains critical patterns for the Admin project testing. Reference this when debugging test issues.

## 🚨 Most Common Test Failures (Check These First!)

### 1. Component Test Setup Issues
```typescript
// ❌ NEVER DO THIS
render(<MyComponent />)  // Missing providers!

// ✅ ALWAYS USE THIS
render(
  <BrowserRouter>
    <AuthProvider>
      <MyComponent />
    </AuthProvider>
  </BrowserRouter>
)
```
**Pattern**: Always wrap components with required providers (Router, Auth, Theme, etc.)

### 2. Missing Mock Setup in Tests
```typescript
// If test expects API data but gets undefined/error, YOU FORGOT TO MOCK!
const mockService = {
  getExercises: jest.fn().mockResolvedValue(mockData)
};

// Mock the service before rendering
jest.mock('../services/ExerciseService', () => mockService);
```

### 3. Async Component Testing
```typescript
// ❌ NEVER DO THIS
const button = screen.getByText('Submit');  // May not be rendered yet!

// ✅ ALWAYS USE THIS
const button = await screen.findByText('Submit');  // Waits for element
// OR
await waitFor(() => {
  expect(screen.getByText('Submit')).toBeInTheDocument();
});
```

## 🔍 Quick Debugging Checklist

When tests fail, check IN THIS ORDER:
1. **Component Providers** - Are all required providers wrapped?
2. **Mock Setup** - Did you mock ALL service calls?
3. **Async Handling** - Are you using findBy/waitFor for async operations?
4. **Test Environment** - Is the test environment properly configured?

## 🎯 Key Patterns to Remember

### Service Mock Pattern
```typescript
// Create mock service
const mockExerciseService = {
  getAll: jest.fn(),
  getById: jest.fn(),
  create: jest.fn(),
  update: jest.fn(),
  delete: jest.fn()
};

// Mock the module
jest.mock('../../services/ExerciseService', () => ({
  ExerciseService: mockExerciseService
}));
```

### Component Test Pattern
```typescript
describe('MyComponent', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('renders correctly', async () => {
    // Arrange
    mockService.getData.mockResolvedValue(testData);
    
    // Act
    render(<MyComponent />, { wrapper: AllProviders });
    
    // Assert
    await waitFor(() => {
      expect(screen.getByText('Expected Text')).toBeInTheDocument();
    });
  });
});
```

### Form Testing Pattern
```typescript
test('submits form with correct data', async () => {
  const user = userEvent.setup();
  
  render(<FormComponent />);
  
  // Type in input
  await user.type(screen.getByLabelText('Name'), 'Test Name');
  
  // Submit form
  await user.click(screen.getByRole('button', { name: 'Submit' }));
  
  // Verify submission
  await waitFor(() => {
    expect(mockService.create).toHaveBeenCalledWith({ name: 'Test Name' });
  });
});
```

## ⚠️ Common Gotchas

1. **React Testing Library** = Use semantic queries (getByRole, getByLabelText)
2. **User Events** = Always use userEvent.setup() for v14+
3. **Async Operations** = Always await user interactions
4. **State Updates** = Wrap assertions in waitFor() for state changes

## 📊 Testing Tools

- **Jest** - Test runner and assertion library
- **React Testing Library** - Component testing
- **MSW (Mock Service Worker)** - API mocking (if implemented)
- **Testing Playground** - Helper for finding queries

## 🛠️ Useful Commands

```bash
# Run all tests
npm test

# Run tests in watch mode
npm test -- --watch

# Run tests with coverage
npm test -- --coverage

# Run specific test file
npm test MyComponent.test.tsx

# Debug specific test
npm test -- --no-coverage MyComponent.test.tsx
```

**Remember**: When tests fail, it's often the test setup, not the component code!
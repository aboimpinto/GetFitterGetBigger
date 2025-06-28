# Testing Quick Reference - MUST READ

**IMPORTANT**: This document contains critical patterns for testing across all client platforms. Reference this when debugging test issues.

## 🚨 Most Common Test Failures (Check These First!)

### 1. Platform-Specific Test Setup Issues

#### Mobile (React Native)
```javascript
// ❌ NEVER DO THIS
render(<MyComponent />)  // Missing navigation context!

// ✅ ALWAYS USE THIS
render(
  <NavigationContainer>
    <MyComponent />
  </NavigationContainer>
)
```

#### Web (React)
```javascript
// ❌ NEVER DO THIS
render(<MyComponent />)  // Missing providers!

// ✅ ALWAYS USE THIS
render(
  <BrowserRouter>
    <Provider store={store}>
      <MyComponent />
    </Provider>
  </BrowserRouter>
)
```

#### Desktop (Avalonia)
```csharp
// ❌ NEVER DO THIS
var viewModel = new MyViewModel();  // Missing dependencies!

// ✅ ALWAYS USE THIS
var services = new ServiceCollection()
    .AddSingleton<INavigationManager, NavigationManager>()
    .BuildServiceProvider();
var viewModel = new MyViewModel(services.GetService<INavigationManager>());
```

### 2. Missing Mock Setup in Tests

#### Mobile/Web
```javascript
// If test expects API data but gets undefined/error, YOU FORGOT TO MOCK!
jest.mock('../services/WorkoutService', () => ({
  getWorkouts: jest.fn().mockResolvedValue(mockData)
}));
```

#### Desktop
```csharp
// Set up mocks before testing
var mockService = new Mock<IWorkoutService>();
mockService.Setup(s => s.GetWorkoutsAsync())
    .ReturnsAsync(mockData);
```

### 3. Async Testing Issues

#### Mobile/Web
```javascript
// ❌ NEVER DO THIS
const button = screen.getByText('Start Workout');  // May not be rendered yet!

// ✅ ALWAYS USE THIS
const button = await screen.findByText('Start Workout');  // Waits for element
// OR
await waitFor(() => {
  expect(screen.getByText('Start Workout')).toBeInTheDocument();
});
```

#### Desktop
```csharp
// ❌ NEVER DO THIS
viewModel.LoadData();
Assert.NotNull(viewModel.Data);  // Data not loaded yet!

// ✅ ALWAYS USE THIS
await viewModel.LoadData();
Assert.NotNull(viewModel.Data);
```

### 4. Platform Navigation Issues

#### Mobile
```javascript
// Mock navigation prop
const mockNavigation = {
  navigate: jest.fn(),
  goBack: jest.fn(),
};
render(<MyScreen navigation={mockNavigation} />);
```

#### Desktop
```csharp
// Mock navigation manager
var mockNav = new Mock<INavigationManager>();
mockNav.Setup(n => n.NavigateTo<TargetViewModel>())
    .Returns(Task.CompletedTask);
```

## 🔍 Quick Debugging Checklist

When tests fail, check IN THIS ORDER:
1. **Platform Setup** - Are all platform-specific contexts provided?
2. **Mock Setup** - Did you mock ALL service calls and dependencies?
3. **Async Handling** - Are you properly awaiting async operations?
4. **Navigation** - Is navigation properly mocked for the platform?
5. **Platform Features** - Are platform-specific features properly handled?

## 🎯 Key Patterns to Remember

### Service Mock Pattern (Mobile/Web)
```javascript
// Create mock service
const mockWorkoutService = {
  getAll: jest.fn(),
  getById: jest.fn(),
  create: jest.fn(),
  update: jest.fn(),
  delete: jest.fn(),
  sync: jest.fn()
};

// Mock the module
jest.mock('../../services/WorkoutService', () => mockWorkoutService);
```

### Service Mock Pattern (Desktop)
```csharp
// Create mock service
var mockService = new Mock<IWorkoutService>();
mockService.Setup(s => s.GetAllAsync())
    .ReturnsAsync(new List<Workout>());

// Inject into view model
var viewModel = new WorkoutViewModel(mockService.Object);
```

### Platform Test Pattern
```javascript
// Mobile/Web
describe('WorkoutScreen', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    // Platform-specific setup
  });

  test('renders correctly on platform', async () => {
    render(<WorkoutScreen />);
    await waitFor(() => {
      expect(screen.getByText('Workouts')).toBeInTheDocument();
    });
  });
});
```

## ⚠️ Common Gotchas

### Mobile
1. **React Navigation** - Always provide navigation context
2. **Native Modules** - Mock native modules that don't exist in test env
3. **Platform.OS** - Mock Platform.select calls
4. **AsyncStorage** - Mock storage operations

### Web
1. **Router Context** - Always wrap with Router
2. **Redux/Context** - Provide store/context providers
3. **LocalStorage** - Mock storage operations
4. **Window/Document** - Mock browser APIs

### Desktop
1. **Dispatcher** - Use SynchronousDispatcher for tests
2. **ReactiveUI** - Handle ReactiveCommand testing properly
3. **File System** - Mock file operations
4. **Platform Services** - Mock platform-specific services

## 📊 Platform-Specific Testing Tools

### Mobile
- **Jest** - Test runner
- **React Native Testing Library** - Component testing
- **Detox** - E2E testing (optional)

### Web
- **Jest** - Test runner
- **React Testing Library** - Component testing
- **Cypress** - E2E testing (optional)

### Desktop
- **xUnit** - Test framework
- **Moq** - Mocking framework
- **Avalonia.Headless** - UI testing

## 🛠️ Useful Commands

### Mobile
```bash
# Run all tests
npm test

# Run specific test file
npm test WorkoutScreen.test.js

# Run with coverage
npm test -- --coverage

# Run in watch mode
npm test -- --watch
```

### Web
```bash
# Run all tests
npm test

# Run specific test
npm test -- --testNamePattern="Workout"

# Run with coverage
npm test -- --coverage --watchAll=false
```

### Desktop
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test GetFitterGetBigger.Desktop.Tests

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific test
dotnet test --filter "FullyQualifiedName~WorkoutViewModel"
```

## 🔥 Platform-Specific Quick Fixes

### Mobile - "Cannot find module" errors
```javascript
// Add to jest.config.js
moduleNameMapper: {
  '^@/(.*)$': '<rootDir>/src/$1',
}
```

### Web - "Invalid hook call" errors
```javascript
// Ensure single React instance
// Check package.json for duplicate React versions
```

### Desktop - "No synchronization context" errors
```csharp
// Use in test setup
Avalonia.Threading.Dispatcher.UIThread.Post(() => {
    // UI operations here
});
```

**Remember**: When tests fail, it's often the test setup, not the application code! Check platform-specific requirements first.
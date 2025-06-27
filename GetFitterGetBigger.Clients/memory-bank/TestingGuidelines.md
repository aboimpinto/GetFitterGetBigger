# Testing Guidelines - Client Projects

## Why Testing Coverage Matters

Testing coverage across multiple platforms (Mobile, Web, Desktop) ensures consistent behavior and user experience. Each platform has unique testing requirements while sharing core business logic tests.

## Testing Principles

### 1. Test All Scenarios Across All Platforms

Every feature should be tested for:
- **Happy Path**: Normal, expected behavior on all platforms
- **Platform-Specific Edge Cases**: Device rotations, app backgrounding, window resizing
- **Error Cases**: Network failures, offline mode, platform permissions
- **Cross-Platform Consistency**: Same business logic, adapted UI
- **Performance**: Platform-specific performance requirements

### 2. Platform-Specific Testing Patterns

#### Mobile (React Native)
```javascript
// Component Testing
- Renders correctly on iOS and Android
- Handles device rotation
- Touch gestures work properly
- Offline mode functions correctly

// Platform Differences
- iOS-specific components render
- Android-specific permissions requested
- Platform-specific styling applied
- Navigation gestures work
```

#### Web (React)
```javascript
// Browser Testing
- Works in Chrome, Firefox, Safari, Edge
- Responsive design at all breakpoints
- Keyboard navigation fully supported
- SEO meta tags render correctly

// Progressive Enhancement
- Works without JavaScript (critical features)
- Service worker caches correctly
- Accessibility standards met
```

#### Desktop (Avalonia)
```csharp
// ViewModel Testing
- Commands execute correctly
- Property changes notify UI
- Validation works properly
- Async operations handled

// Platform Integration
- Window management works
- Native menus function
- File system access works
- Keyboard shortcuts respond
```

### 3. Shared Business Logic Testing

```javascript
// Shared Services/Models
- Data validation consistent across platforms
- Business rules enforced uniformly
- API integration works identically
- Data transformation correct
```

## Common Testing Patterns

### Mobile Testing Examples

```javascript
// ExerciseScreen.test.js
describe('ExerciseScreen', () => {
  it('renders exercise list on iOS') // Platform-specific
  it('renders exercise list on Android') // Platform-specific
  it('handles pull-to-refresh') // Touch gesture
  it('works offline with cached data') // Offline mode
  it('handles device back button (Android)') // Platform navigation
  it('handles swipe back (iOS)') // Platform navigation
  it('requests camera permission for photos') // Permissions
})
```

### Web Testing Examples

```javascript
// ExercisePage.test.js
describe('ExercisePage', () => {
  it('renders in mobile viewport') // Responsive
  it('renders in desktop viewport') // Responsive
  it('supports keyboard navigation') // Accessibility
  it('has proper ARIA labels') // Accessibility
  it('loads data on route change') // Routing
  it('updates URL on filter change') // URL state
  it('prints exercise details') // Print styles
})
```

### Desktop Testing Examples

```csharp
// ExerciseViewModelTests.cs
[Test]
public void LoadExercises_Success_PopulatesList() // Happy path
[Test]
public void LoadExercises_NetworkError_ShowsOfflineData() // Error handling
[Test]
public void DeleteCommand_CanExecute_WhenExerciseSelected() // Command state
[Test]
public void SearchFilter_Updates_FilteredListUpdates() // Reactive binding
[Test]
public void WindowClose_UnsavedChanges_PromptsUser() // Native integration
```

## Platform-Specific Testing Requirements

### Mobile (React Native)

#### Tools
- **Jest**: Unit testing
- **React Native Testing Library**: Component testing
- **Detox**: E2E testing on devices
- **Platform module mocking**: iOS/Android differences

#### Key Areas
- Gesture handling (swipe, pinch, long press)
- Device capabilities (camera, GPS, contacts)
- App lifecycle (foreground/background)
- Deep linking
- Push notifications
- Offline storage

### Web (React)

#### Tools
- **Jest**: Unit testing
- **React Testing Library**: Component testing
- **Cypress/Playwright**: E2E testing
- **Lighthouse**: Performance testing
- **axe-core**: Accessibility testing

#### Key Areas
- Browser compatibility
- Responsive breakpoints
- SEO optimization
- PWA functionality
- Cookie/localStorage handling
- Print styles

### Desktop (Avalonia)

#### Tools
- **NUnit/xUnit**: Unit testing
- **Moq**: Mocking framework
- **Avalonia.Testing**: UI testing
- **BenchmarkDotNet**: Performance testing

#### Key Areas
- Window lifecycle
- Menu integration
- File dialogs
- System tray
- Keyboard shortcuts
- Multi-window support

## Cross-Platform Testing Checklist

Before considering a feature fully tested:

### All Platforms
- [ ] Business logic has unit tests
- [ ] API integration tested with mocks
- [ ] Error scenarios handled gracefully
- [ ] Loading states display correctly
- [ ] Data validation works consistently

### Mobile Specific
- [ ] Works on iOS simulator/device
- [ ] Works on Android emulator/device
- [ ] Handles orientation changes
- [ ] Gestures work correctly
- [ ] Offline mode functions
- [ ] App performance acceptable on older devices

### Web Specific
- [ ] Tested in all major browsers
- [ ] Responsive at all breakpoints
- [ ] Accessible via keyboard only
- [ ] Passes accessibility audit
- [ ] SEO requirements met
- [ ] Works with slow network

### Desktop Specific
- [ ] Tested on Windows, macOS, Linux
- [ ] Window management correct
- [ ] Native features integrated
- [ ] Keyboard shortcuts work
- [ ] High DPI displays supported
- [ ] Memory usage acceptable

## Running Tests by Platform

### Mobile
```bash
# Run all tests
npm test

# Run with coverage
npm test -- --coverage

# Run specific platform tests
npm test -- --testNamePattern="iOS"
npm test -- --testNamePattern="Android"

# E2E tests
detox test --configuration ios
detox test --configuration android
```

### Web
```bash
# Unit tests
npm test

# Coverage report
npm run test:coverage

# E2E tests
npm run cypress:open
npm run cypress:run

# Accessibility audit
npm run test:a11y

# Performance test
npm run lighthouse
```

### Desktop
```bash
# Run all tests
dotnet test

# With coverage
dotnet test /p:CollectCoverage=true

# Run specific test category
dotnet test --filter Category=Integration

# Performance tests
dotnet run -c Release --project Benchmarks
```

## Best Practices for Multi-Platform Testing

### 1. Share Test Logic Where Possible
```javascript
// shared/testUtils.js
export const mockExerciseData = { /* shared mock */ };
export const testBusinessRules = (implementation) => {
  // Test same rules across platforms
};
```

### 2. Platform-Specific Test Utilities
```javascript
// mobile/testUtils.js
export const renderWithNavigation = (component) => {
  // Mobile-specific navigation wrapper
};

// web/testUtils.js
export const renderWithRouter = (component) => {
  // Web-specific router wrapper
};
```

### 3. Consistent Mock Data
- Use same mock API responses
- Share test fixtures
- Consistent error scenarios

### 4. Platform Test Matrix
| Feature | Mobile | Web | Desktop |
|---------|--------|-----|---------|
| Exercise List | ✓ | ✓ | ✓ |
| Offline Mode | ✓ | ✓ | ✓ |
| Touch Gestures | ✓ | - | - |
| Keyboard Shortcuts | - | ✓ | ✓ |
| Print View | - | ✓ | ✓ |

### 5. Continuous Integration
- Run platform-specific tests in parallel
- Use appropriate OS for each platform
- Cache dependencies per platform
- Generate unified coverage report

## Remember

**Platform-specific tests ensure quality**, meaning:
- Consistent experience across devices
- Platform features properly utilized
- Performance optimized per platform
- Accessibility on all platforms
- Reduced platform-specific bugs
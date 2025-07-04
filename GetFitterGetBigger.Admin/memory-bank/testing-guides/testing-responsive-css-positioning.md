# Testing Responsive CSS Positioning

## Overview
Testing responsive CSS positioning, especially with media queries and calc() functions, presents unique challenges. Traditional unit tests have limitations, requiring a multi-layered testing approach.

## Testing Limitations in Unit Tests

### What Unit Tests CANNOT Test
1. **Actual pixel positions** - Unit tests don't render in a real browser
2. **Media query behavior** - No viewport size changes in unit tests
3. **CSS calc() computations** - These are browser-computed values
4. **Visual appearance** - How elements actually look on screen
5. **Responsive breakpoints** - When layouts change at different sizes

### What Unit Tests CAN Test
1. **Element presence** - Buttons exist in the DOM
2. **CSS class application** - Correct classes are applied
3. **HTML structure** - Proper nesting and attributes
4. **Event handlers** - Click events work correctly
5. **State changes** - Loading states, disabled states
6. **Data attributes** - data-testid presence for testability

## Testing Strategy for Floating Buttons

### 1. Unit Tests (bUnit) ✅
```csharp
// Test structure and behavior
[Fact]
public void FloatingButtons_HaveCorrectStructure()
{
    var component = RenderComponent<ExerciseForm>();
    var cancelButton = component.Find("[data-testid='floating-cancel-button']");
    
    // Verify structure
    cancelButton.Should().NotBeNull();
    cancelButton.QuerySelector("svg").Should().NotBeNull();
    
    // Verify CSS classes
    var container = cancelButton.ParentElement;
    container.GetClasses().Should().Contain("fixed");
    container.GetClasses().Should().Contain("cancelPostionStyle");
}
```

### 2. Integration Tests (Browser-Based) 🌐
Using tools like Playwright or Selenium:

```csharp
[Fact]
public async Task FloatingButtons_PositionCorrectly_OnLargeScreen()
{
    // Set viewport to large screen
    await Page.SetViewportSizeAsync(1920, 1080);
    await Page.GotoAsync("/exercises/new");
    
    // Get computed styles
    var cancelButton = await Page.Locator("[data-testid='floating-cancel-button']");
    var boundingBox = await cancelButton.BoundingBoxAsync();
    
    // Verify position relative to viewport
    var expectedLeft = (1920 / 2) - (25 * 16); // 50% - 25rem
    boundingBox.X.Should().BeCloseTo(expectedLeft, 5);
}

[Fact]
public async Task FloatingButtons_PositionCorrectly_OnSmallScreen()
{
    // Set viewport to mobile size
    await Page.SetViewportSizeAsync(375, 667);
    await Page.GotoAsync("/exercises/new");
    
    var cancelButton = await Page.Locator("[data-testid='floating-cancel-button']");
    var boundingBox = await cancelButton.BoundingBoxAsync();
    
    // On small screens, should be 1rem from left
    boundingBox.X.Should().BeCloseTo(16, 5); // 1rem = 16px
}
```

### 3. Visual Regression Tests 📸
Using tools like Percy, Chromatic, or BackstopJS:

```javascript
// backstop.json configuration
{
  "scenarios": [
    {
      "label": "Exercise Form - Desktop",
      "url": "http://localhost:5000/exercises/new",
      "viewports": [
        { "width": 1920, "height": 1080 }
      ],
      "selectors": ["[data-testid='floating-cancel-button']", "[data-testid='floating-save-button']"]
    },
    {
      "label": "Exercise Form - Mobile",
      "url": "http://localhost:5000/exercises/new",
      "viewports": [
        { "width": 375, "height": 667 }
      ],
      "selectors": ["[data-testid='floating-cancel-button']", "[data-testid='floating-save-button']"]
    }
  ]
}
```

### 4. Manual Testing Checklist ✋
```markdown
## Floating Button Position Testing

### Desktop (≥1280px)
- [ ] Cancel button aligns with left edge of form container
- [ ] Save button aligns with right edge of form container
- [ ] Buttons maintain position during scroll
- [ ] Buttons stay above all content (z-index)

### Tablet (768px - 1279px)
- [ ] Cancel button is 1rem from left edge
- [ ] Save button is 1rem from right edge
- [ ] Buttons don't overlap form content

### Mobile (<768px)
- [ ] Cancel button visible and not under navbar
- [ ] Save button fully visible on right
- [ ] Buttons don't overlap with form fields
- [ ] Text labels remain readable
```

## Recommended Testing Approach

### For This Feature
1. **Unit Tests** ✅ - Test structure, classes, and behavior (what we created)
2. **Manual Testing** ✅ - Verify visual appearance across screen sizes
3. **Optional: E2E Tests** - Automate viewport testing if critical
4. **Optional: Visual Regression** - Catch unintended CSS changes

### Testing Priority
```
High Priority (Required):
├── Unit Tests (structure & behavior)
└── Manual Testing (visual verification)

Medium Priority (Recommended):
├── E2E Tests (automated viewport testing)
└── CSS Linting (enforce conventions)

Low Priority (Nice to have):
├── Visual Regression Tests
└── Performance Tests (rendering speed)
```

## Example E2E Test with Playwright

```csharp
public class FloatingButtonE2ETests : IAsyncLifetime
{
    private IPlaywright _playwright;
    private IBrowser _browser;
    private IPage _page;

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync();
        _page = await _browser.NewPageAsync();
    }

    [Theory]
    [InlineData(1920, 1080, 960 - 400, "Desktop")] // 50% - 25rem
    [InlineData(1024, 768, 16, "Tablet")] // 1rem
    [InlineData(375, 667, 16, "Mobile")] // 1rem
    public async Task CancelButton_PositionsCorrectly_AtDifferentViewports(
        int width, int height, int expectedLeft, string description)
    {
        // Arrange
        await _page.SetViewportSizeAsync(width, height);
        await _page.GotoAsync("http://localhost:5000/exercises/new");
        await _page.WaitForSelectorAsync("[data-testid='floating-cancel-button']");

        // Act
        var buttonLocator = _page.Locator("[data-testid='floating-cancel-button']");
        var boundingBox = await buttonLocator.BoundingBoxAsync();

        // Assert
        boundingBox.Should().NotBeNull($"{description}: Button should be visible");
        boundingBox.X.Should().BeCloseTo(expectedLeft, 10, 
            $"{description}: Button should be positioned correctly");
    }

    public async Task DisposeAsync()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }
}
```

## CSS Testing Tools

### Static Analysis
- **Stylelint** - Enforce CSS conventions
- **PurgeCSS** - Find unused styles
- **CSS Stats** - Analyze CSS complexity

### Runtime Testing
- **Playwright** - Cross-browser automation
- **Cypress** - Component testing with real rendering
- **Puppeteer** - Headless Chrome automation

### Visual Testing
- **Percy** - Visual regression as a service
- **Chromatic** - Storybook visual testing
- **BackstopJS** - Open-source visual regression

## Best Practices

1. **Layer Your Tests**
   - Unit tests for logic and structure
   - Integration tests for actual positioning
   - Visual tests for appearance

2. **Use Data Attributes**
   - Always add `data-testid` for reliable selection
   - Don't rely on CSS classes for selection

3. **Test Critical Breakpoints**
   - Test at exact breakpoint boundaries
   - Test common device sizes
   - Test edge cases (very small/large)

4. **Document Limitations**
   - Be clear about what each test type covers
   - Document manual testing requirements
   - Explain why certain tests aren't possible

5. **Maintain Test Data**
   - Keep screenshots for visual regression
   - Document expected positions
   - Update tests when design changes
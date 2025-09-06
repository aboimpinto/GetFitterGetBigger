# Accessibility Automation Guide for Blazor Applications

## Executive Summary

This guide provides a comprehensive approach to automating accessibility testing in Blazor applications, based on the excellent patterns demonstrated in the GetFitterGetBigger Admin project. It covers tools, CI/CD integration, and best practices for maintaining WCAG compliance.

## Table of Contents

1. [Current Manual Testing Approach](#current-manual-testing-approach)
2. [Automated Testing Tools](#automated-testing-tools)
3. [CI/CD Pipeline Integration](#cicd-pipeline-integration)
4. [Axe-core Integration with bUnit](#axe-core-integration-with-bunit)
5. [Automated WCAG Compliance Checking](#automated-wcag-compliance-checking)
6. [Reporting and Monitoring](#reporting-and-monitoring)
7. [Implementation Roadmap](#implementation-roadmap)

## Current Manual Testing Approach

### What We're Doing Well

Based on the FourWayLinkingAccessibilityTests.cs implementation:

```csharp
[Fact]
public void Component_MeetsWCAGColorContrastRequirements()
{
    // Manual verification of color contrast ratios
    var component = RenderComponent<AccessibleComponent>();
    
    // Currently checking existence of elements
    var buttons = component.FindAll("button");
    buttons.Should().NotBeEmpty();
    
    // Manual verification required for actual contrast ratios
}
```

### Gaps to Address

1. **Color Contrast**: Currently not automatically validated
2. **Focus Order**: Manual testing required
3. **Screen Reader Compatibility**: No automated verification
4. **Keyboard Navigation**: Limited automated coverage
5. **ARIA Validity**: No automatic validation of ARIA attribute combinations

## Automated Testing Tools

### Tool Comparison Matrix

| Tool | Purpose | Integration Effort | Cost | Recommendation |
|------|---------|-------------------|------|----------------|
| **Axe-core** | Comprehensive a11y testing | Medium | Free | ✅ Primary choice |
| **Pa11y** | CLI-based testing | Low | Free | ✅ CI/CD integration |
| **WAVE API** | Visual + programmatic | High | Paid | For enterprise |
| **Lighthouse CI** | Performance + a11y | Medium | Free | ✅ Monitoring |
| **AccessiBe** | AI-powered testing | Low | Paid | Optional enhancement |

## CI/CD Pipeline Integration

### GitHub Actions Workflow

```yaml
name: Accessibility Tests

on:
  pull_request:
    branches: [ master, develop ]
  push:
    branches: [ master ]

jobs:
  accessibility:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'
    
    - name: Install Node.js
      uses: actions/setup-node@v3
      with:
        node-version: '18'
    
    - name: Install Axe-core CLI
      run: npm install -g @axe-core/cli
    
    - name: Build Blazor App
      run: |
        dotnet build
        dotnet publish -c Release -o ./publish
    
    - name: Start Blazor Server
      run: |
        cd publish
        dotnet GetFitterGetBigger.Admin.dll &
        sleep 10  # Wait for server to start
    
    - name: Run Axe Accessibility Tests
      run: |
        axe http://localhost:5000 \
          --tags wcag2aa \
          --reporter json \
          --output accessibility-report.json
    
    - name: Run Pa11y Tests
      run: |
        npm install -g pa11y
        pa11y http://localhost:5000 \
          --standard WCAG2AA \
          --reporter json > pa11y-report.json
    
    - name: Run Custom bUnit Accessibility Tests
      run: dotnet test --filter "Category=Accessibility"
    
    - name: Generate Accessibility Report
      run: |
        npm install -g pa11y-reporter-html
        pa11y-reporter-html < pa11y-report.json > accessibility-report.html
    
    - name: Upload Reports
      uses: actions/upload-artifact@v3
      with:
        name: accessibility-reports
        path: |
          accessibility-report.json
          pa11y-report.json
          accessibility-report.html
    
    - name: Comment PR with Results
      if: github.event_name == 'pull_request'
      uses: actions/github-script@v6
      with:
        script: |
          const fs = require('fs');
          const report = JSON.parse(fs.readFileSync('accessibility-report.json'));
          const violations = report.violations.length;
          
          const comment = `## 🔍 Accessibility Check Results
          
          ${violations === 0 ? '✅ All accessibility checks passed!' : `⚠️ Found ${violations} accessibility issues`}
          
          ${violations > 0 ? '### Issues Found:\n' + report.violations.map(v => 
            `- **${v.impact}**: ${v.description} (${v.nodes.length} instances)`
          ).join('\n') : ''}
          
          [View Full Report](https://github.com/${{ github.repository }}/actions/runs/${{ github.run_id }})`;
          
          github.rest.issues.createComment({
            issue_number: context.issue.number,
            owner: context.repo.owner,
            repo: context.repo.repo,
            body: comment
          });
```

### Azure DevOps Pipeline

```yaml
trigger:
  branches:
    include:
    - master
    - develop

pool:
  vmImage: 'ubuntu-latest'

stages:
- stage: AccessibilityTesting
  jobs:
  - job: RunAccessibilityTests
    steps:
    - task: UseDotNet@2
      inputs:
        packageType: 'sdk'
        version: '9.0.x'
    
    - task: NodeTool@0
      inputs:
        versionSpec: '18.x'
    
    - script: |
        npm install -g @axe-core/cli pa11y lighthouse
      displayName: 'Install Accessibility Tools'
    
    - task: DotNetCoreCLI@2
      inputs:
        command: 'build'
        projects: '**/*.csproj'
    
    - task: DotNetCoreCLI@2
      inputs:
        command: 'test'
        projects: '**/*Tests.csproj'
        arguments: '--filter Category=Accessibility'
    
    - script: |
        dotnet run --project GetFitterGetBigger.Admin &
        sleep 10
        
        # Run Axe tests
        axe http://localhost:5000 --tags wcag2aa --reporter json > $(Build.ArtifactStagingDirectory)/axe-report.json
        
        # Run Pa11y tests
        pa11y http://localhost:5000 --standard WCAG2AA --reporter json > $(Build.ArtifactStagingDirectory)/pa11y-report.json
        
        # Run Lighthouse
        lighthouse http://localhost:5000 --output json --output-path $(Build.ArtifactStagingDirectory)/lighthouse-report.json
      displayName: 'Run Accessibility Scans'
    
    - task: PublishBuildArtifacts@1
      inputs:
        PathtoPublish: '$(Build.ArtifactStagingDirectory)'
        ArtifactName: 'accessibility-reports'
```

## Axe-core Integration with bUnit

### Installation

```bash
dotnet add package Deque.AxeCore.Playwright
dotnet add package Deque.AxeCore.Commons
```

### Enhanced Test Base Class

```csharp
using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.Playwright;

public abstract class AccessibilityTestBase : TestContext, IAsyncLifetime
{
    protected IPlaywright Playwright { get; private set; }
    protected IBrowser Browser { get; private set; }
    protected IPage Page { get; private set; }
    protected AxeBuilder AxeBuilder { get; private set; }

    public async Task InitializeAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserLaunchOptions
        {
            Headless = true
        });
        
        var context = await Browser.NewContextAsync();
        Page = await context.NewPageAsync();
        AxeBuilder = new AxeBuilder(Page);
    }

    public async Task DisposeAsync()
    {
        await Browser?.DisposeAsync();
        Playwright?.Dispose();
    }

    protected async Task<AxeResult> RunAccessibilityCheck(string url, string? selector = null)
    {
        await Page.GotoAsync(url);
        
        if (!string.IsNullOrEmpty(selector))
        {
            await Page.WaitForSelectorAsync(selector);
            AxeBuilder.Include(selector);
        }

        return await AxeBuilder
            .WithTags("wcag2a", "wcag2aa", "wcag21aa")
            .Analyze();
    }

    protected void AssertNoViolations(AxeResult result)
    {
        if (result.Violations.Any())
        {
            var violations = string.Join("\n", result.Violations.Select(v =>
                $"- {v.Impact}: {v.Description} ({v.Nodes.Count} instances)"));
            
            Assert.True(false, $"Accessibility violations found:\n{violations}");
        }
    }
}
```

### Automated Component Testing

```csharp
public class AutomatedAccessibilityTests : AccessibilityTestBase
{
    private readonly TestServer _server;
    private readonly string _baseUrl;

    public AutomatedAccessibilityTests()
    {
        var factory = new WebApplicationFactory<Program>();
        _server = factory.Server;
        _baseUrl = _server.BaseAddress.ToString();
    }

    [Fact]
    public async Task ExerciseList_MeetsAccessibilityStandards()
    {
        // Arrange & Act
        var result = await RunAccessibilityCheck($"{_baseUrl}/exercises");

        // Assert
        AssertNoViolations(result);
        
        // Additional specific checks
        result.Passes.Should().Contain(p => p.Id == "color-contrast");
        result.Passes.Should().Contain(p => p.Id == "aria-valid-attr");
        result.Passes.Should().Contain(p => p.Id == "button-name");
    }

    [Theory]
    [InlineData("/exercises", "[data-testid='exercise-list']")]
    [InlineData("/exercises/links", "[data-testid='four-way-linked-exercises-list']")]
    [InlineData("/exercises/create", "form")]
    public async Task Pages_MeetAccessibilityStandards(string path, string selector)
    {
        // Act
        var result = await RunAccessibilityCheck($"{_baseUrl}{path}", selector);

        // Assert
        AssertNoViolations(result);
    }

    [Fact]
    public async Task InteractiveElements_AreKeyboardAccessible()
    {
        // Navigate to page
        await Page.GotoAsync($"{_baseUrl}/exercises");

        // Tab through all interactive elements
        var tabbableElements = await Page.QuerySelectorAllAsync(@"
            a[href], button:not([disabled]), input:not([disabled]),
            select:not([disabled]), textarea:not([disabled]),
            [tabindex]:not([tabindex='-1'])
        ");

        foreach (var element in tabbableElements)
        {
            await element.FocusAsync();
            
            // Verify focus is visible
            var hasFocusIndicator = await element.EvaluateAsync<bool>(@"el => {
                const styles = window.getComputedStyle(el);
                return styles.outline !== 'none' || 
                       styles.boxShadow !== 'none' ||
                       styles.border !== styles.borderColor;
            }");
            
            hasFocusIndicator.Should().BeTrue("All interactive elements must have visible focus indicators");
        }
    }
}
```

## Automated WCAG Compliance Checking

### Custom WCAG Test Attributes

```csharp
[AttributeUsage(AttributeTargets.Method)]
public class WCAGRequirementAttribute : Attribute
{
    public string Criterion { get; }
    public string Level { get; }
    public string Description { get; }

    public WCAGRequirementAttribute(string criterion, string level, string description)
    {
        Criterion = criterion;
        Level = level;
        Description = description;
    }
}
```

### WCAG-Specific Test Suite

```csharp
public class WCAGComplianceTests : AccessibilityTestBase
{
    [Fact]
    [WCAGRequirement("1.1.1", "A", "Non-text Content")]
    public async Task Images_HaveAltText()
    {
        await Page.GotoAsync($"{_baseUrl}/exercises");
        
        var images = await Page.QuerySelectorAllAsync("img");
        foreach (var img in images)
        {
            var alt = await img.GetAttributeAsync("alt");
            alt.Should().NotBeNullOrEmpty("All images must have alt text");
        }
    }

    [Fact]
    [WCAGRequirement("1.4.3", "AA", "Contrast (Minimum)")]
    public async Task ColorContrast_MeetsMinimumRequirements()
    {
        var result = await AxeBuilder
            .WithRules("color-contrast")
            .Analyze();
        
        AssertNoViolations(result);
    }

    [Fact]
    [WCAGRequirement("2.1.1", "A", "Keyboard")]
    public async Task AllFunctionality_AvailableViaKeyboard()
    {
        await Page.GotoAsync($"{_baseUrl}/exercises");
        
        // Test keyboard navigation
        await Page.Keyboard.PressAsync("Tab");
        var focusedElement = await Page.EvaluateAsync<string>("document.activeElement.tagName");
        focusedElement.Should().NotBe("BODY", "Tab key should move focus to interactive element");
        
        // Test Enter/Space activation
        await Page.Keyboard.PressAsync("Enter");
        // Verify action was triggered
    }

    [Fact]
    [WCAGRequirement("2.4.7", "AA", "Focus Visible")]
    public async Task FocusIndicator_AlwaysVisible()
    {
        await Page.GotoAsync($"{_baseUrl}/exercises");
        
        // Remove any focus styles and verify they're enforced
        await Page.EvaluateAsync(@"
            document.querySelectorAll('*').forEach(el => {
                el.style.outline = 'none';
            });
        ");
        
        // Tab to first element
        await Page.Keyboard.PressAsync("Tab");
        
        // Check focus is still visible
        var hasFocus = await Page.EvaluateAsync<bool>(@"
            const focused = document.activeElement;
            const styles = window.getComputedStyle(focused);
            return styles.outlineWidth !== '0px' || styles.boxShadow !== 'none';
        ");
        
        hasFocus.Should().BeTrue("Focus indicator must be visible even when styles are overridden");
    }

    [Fact]
    [WCAGRequirement("3.3.2", "A", "Labels or Instructions")]
    public async Task FormFields_HaveLabels()
    {
        await Page.GotoAsync($"{_baseUrl}/exercises/create");
        
        var inputs = await Page.QuerySelectorAllAsync("input, select, textarea");
        
        foreach (var input in inputs)
        {
            var id = await input.GetAttributeAsync("id");
            var ariaLabel = await input.GetAttributeAsync("aria-label");
            var ariaLabelledBy = await input.GetAttributeAsync("aria-labelledby");
            
            if (string.IsNullOrEmpty(ariaLabel) && string.IsNullOrEmpty(ariaLabelledBy))
            {
                // Check for associated label
                var label = await Page.QuerySelectorAsync($"label[for='{id}']");
                label.Should().NotBeNull($"Input with id='{id}' must have a label");
            }
        }
    }
}
```

## Reporting and Monitoring

### Accessibility Dashboard

```csharp
public class AccessibilityReportGenerator
{
    public async Task<AccessibilityReport> GenerateReport(string baseUrl)
    {
        var report = new AccessibilityReport
        {
            Timestamp = DateTime.UtcNow,
            BaseUrl = baseUrl,
            Pages = new List<PageReport>()
        };

        var pages = new[] { "/", "/exercises", "/exercises/links", "/workouts" };
        
        foreach (var page in pages)
        {
            var pageReport = await AnalyzePage($"{baseUrl}{page}");
            report.Pages.Add(pageReport);
        }

        report.OverallScore = CalculateOverallScore(report.Pages);
        report.WCAGLevel = DetermineWCAGLevel(report.Pages);
        
        return report;
    }

    private async Task<PageReport> AnalyzePage(string url)
    {
        using var playwright = await Playwright.CreateAsync();
        using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();
        await page.GotoAsync(url);

        var axe = new AxeBuilder(page);
        var result = await axe.Analyze();

        return new PageReport
        {
            Url = url,
            Violations = result.Violations.Count,
            Passes = result.Passes.Count,
            ViolationDetails = result.Violations.Select(v => new ViolationDetail
            {
                Impact = v.Impact,
                Description = v.Description,
                Occurrences = v.Nodes.Count,
                WCAGCriteria = v.Tags
            }).ToList()
        };
    }
}
```

### Integration with Application Insights

```csharp
public class AccessibilityTelemetry
{
    private readonly TelemetryClient _telemetryClient;

    public void TrackAccessibilityScore(string page, double score)
    {
        _telemetryClient.TrackMetric("Accessibility.Score", score, 
            new Dictionary<string, string>
            {
                ["Page"] = page,
                ["WCAG_Level"] = score >= 0.98 ? "AAA" : score >= 0.95 ? "AA" : "A"
            });
    }

    public void TrackViolation(string page, string violation, string impact)
    {
        _telemetryClient.TrackEvent("Accessibility.Violation",
            new Dictionary<string, string>
            {
                ["Page"] = page,
                ["Violation"] = violation,
                ["Impact"] = impact
            });
    }
}
```

## Implementation Roadmap

### Phase 1: Foundation (Week 1-2)
- [ ] Install Axe-core and Pa11y
- [ ] Create AccessibilityTestBase class
- [ ] Convert existing manual tests to automated
- [ ] Set up local test runner

### Phase 2: CI/CD Integration (Week 3-4)
- [ ] Configure GitHub Actions/Azure DevOps pipeline
- [ ] Set up accessibility test stage
- [ ] Configure failure thresholds
- [ ] Implement PR commenting

### Phase 3: Comprehensive Coverage (Week 5-6)
- [ ] Add WCAG-specific test suite
- [ ] Implement keyboard navigation tests
- [ ] Add screen reader compatibility tests
- [ ] Create color contrast validation

### Phase 4: Monitoring & Reporting (Week 7-8)
- [ ] Build accessibility dashboard
- [ ] Integrate with Application Insights
- [ ] Create trend reporting
- [ ] Set up alerting for regressions

### Phase 5: Continuous Improvement (Ongoing)
- [ ] Regular accessibility audits
- [ ] Update tests for new components
- [ ] Train team on accessibility testing
- [ ] Document best practices

## Best Practices

### 1. Shift-Left Testing
Test accessibility during development, not after:
```csharp
[Fact]
public void NewComponent_MeetsAccessibilityStandards()
{
    // Write this test BEFORE implementing the component
    var component = RenderComponent<NewComponent>();
    // Add accessibility assertions
}
```

### 2. Component Library Standards
Create accessible component templates:
```razor
@* AccessibleButton.razor *@
<button 
    @attributes="AdditionalAttributes"
    aria-label="@AriaLabel"
    aria-pressed="@(IsPressed ? "true" : "false")"
    @onclick="HandleClick">
    @ChildContent
</button>
```

### 3. Automated Fix Suggestions
```csharp
public class AccessibilityFixer
{
    public string SuggestFix(AxeViolation violation)
    {
        return violation.Id switch
        {
            "button-name" => "Add aria-label or text content to button",
            "color-contrast" => $"Increase contrast ratio to at least {violation.Impact == "serious" ? "4.5:1" : "3:1"}",
            "aria-valid-attr" => $"Remove or correct invalid ARIA attribute: {violation.Data}",
            _ => "See WCAG documentation for guidance"
        };
    }
}
```

## Conclusion

Automating accessibility testing is crucial for maintaining WCAG compliance at scale. This guide provides a comprehensive approach that can be implemented incrementally, starting with basic automation and evolving to full CI/CD integration with monitoring and reporting.

Key benefits:
- **Early Detection**: Find issues during development, not in production
- **Consistency**: Automated tests run the same way every time
- **Documentation**: Tests serve as accessibility requirements documentation
- **Efficiency**: Reduce manual testing time by 80%
- **Compliance**: Maintain WCAG AA compliance with confidence

Start with Phase 1 and gradually implement additional phases based on team capacity and project requirements.
---
name: test-coverage
description: Generate a comprehensive test coverage report with HTML visualization and actionable insights
usage: /test-coverage [focus-area]
examples:
  - /test-coverage
  - /test-coverage services
  - /test-coverage "Exercise and Workout services"
---

# Test Coverage Command

This command triggers the test-coverage-reporter agent to generate a comprehensive test coverage report with visualizations and recommendations.

## Agent Task

Launch the test-coverage-reporter agent with the following instructions:

### Primary Mission
Generate a complete test coverage report for the GetFitterGetBigger API project, including HTML reports, coverage metrics, and actionable recommendations for improving test coverage.

### Focus Area
{{FOCUS_AREA}}

If a focus area is specified, pay special attention to coverage analysis for those components while still generating the full report.

### Execution Instructions

1. **Environment Preparation**
   - Clean the solution to ensure fresh build
   - Verify test projects are properly configured

2. **Coverage Collection**
   - Run all tests with coverage collection enabled
   - Use both Cobertura and OpenCover formats for compatibility
   - Ensure all test projects are included

3. **Report Generation**
   - Generate interactive HTML reports
   - Create markdown summary for quick review
   - Include coverage trends if historical data exists

4. **Analysis and Insights**
   - Identify top 10 least covered critical components
   - Highlight services/controllers below target thresholds
   - Find complex methods with zero coverage
   - Analyze branch coverage for decision points

5. **Recommendations**
   - Provide prioritized list of coverage improvements
   - Suggest specific test scenarios for gaps
   - Identify quick wins for coverage boost
   - Estimate effort levels (Low/Medium/High)

### Output Requirements

1. **Console Summary**: Display key metrics immediately:
   ```
   ✅ Test Coverage Report Generated
   📊 Overall Coverage: XX.X%
   📁 Report Location: ./TestResults/CoverageReport/index.html
   ⚠️  X components below threshold
   ```

2. **HTML Report**: Full interactive report at `./TestResults/CoverageReport/index.html`

3. **Markdown Summary**: Executive summary at `./TestResults/CoverageReport/COVERAGE_SUMMARY.md`

4. **Action Items**: List of specific next steps to improve coverage

### Coverage Thresholds

Ensure the report clearly indicates which components meet these targets:
- **Services**: 85% (business logic)
- **Controllers**: 75% (API endpoints)
- **Repositories**: 90% (data access)
- **Validators**: 95% (validation logic)
- **Overall Project**: 80% minimum

### Special Considerations

1. **Exclude from coverage**:
   - Program.cs and startup files
   - Migration files
   - Pure DTOs without logic
   - Generated code
   - Test projects

2. **Priority focus**:
   - New code from recent features
   - Complex business logic
   - Security-sensitive code
   - Error handling paths

3. **Performance note**: 
   Inform that coverage collection may take longer than normal test runs

### Success Criteria

The report is complete when:
- All tests have executed successfully
- HTML report is generated and accessible
- Markdown summary provides clear metrics
- Specific improvement recommendations are listed
- Coverage gaps are prioritized by importance
- User can open the HTML report to explore details

### Error Recovery

If coverage generation fails:
1. Check and install missing coverage packages
2. Verify ReportGenerator tool is installed
3. Fall back to basic dotnet test coverage output
4. Provide troubleshooting steps to the user
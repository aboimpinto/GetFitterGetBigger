# Review Code with Standards

Use the blazor-code-reviewer agent to perform a comprehensive code review while strictly adhering to the project's code review and quality standards.

## Process Overview

1. The blazor-code-reviewer agent will analyze the code and generate a review report
2. Since agents cannot use the Write tool, the agent will output the report content
3. After the agent completes, extract the report content and save it using the Write tool

## Step 1: Launch the Agent

Use the Task tool to launch the blazor-code-reviewer agent with the following prompt:

```
You are the blazor-code-reviewer agent. Perform a comprehensive code review following all project standards.

IMPORTANT: Since you cannot use the Write tool, please output the complete markdown report content between these markers:
===BEGIN REVIEW REPORT===
[Your complete markdown report here]
===END REVIEW REPORT===

Include all sections as specified in the review template.
```

## Step 2: Extract and Save the Report

After the agent completes:
1. Look for the content between `===BEGIN REVIEW REPORT===` and `===END REVIEW REPORT===`
2. Extract that content
3. Save it to: `[project-root]/memory-bank/temp/code-review-[description]-[date].md`

## Standards to Follow

### 1. Code Review Standards (@memory-bank/CODE_REVIEW_STANDARDS.md)
- **ZERO Build Warnings**: ANY warning = automatic NEEDS_CHANGES
- **100% Test Pass Rate**: Even one failing test = NEEDS_CHANGES
- **Build Must Succeed**: Any compilation error = NEEDS_CHANGES
- **UI/UX Compliance**: List pages must follow UI_LIST_PAGE_DESIGN_STANDARDS.md
- **Review Outcomes**: APPROVED, APPROVED_WITH_NOTES, or NEEDS_CHANGES

### 2. Code Quality Standards (@memory-bank/CODE_QUALITY_STANDARDS.md)
- **Single Exit Point**: Every method should have ONE exit point at the end
- **Pattern Matching**: Use pattern matching over if-else chains
- **Method Length**: Target < 20 lines per method
- **Null Safety**: Enable nullable reference types, validate at boundaries
- **No Fake Async**: Don't use Task.FromResult unless truly needed
- **Extensibility**: Use patterns like Strategy Pattern for scalable solutions

### 3. Code Review Process (@memory-bank/CODE_REVIEW_PROCESS.md)
- **Category Reviews**: Review each implementation category separately
- **Final Review**: Comprehensive review of entire feature
- **Cross-Cutting Concerns**: Check architecture, performance, security
- **Documentation**: Ensure proper XML comments and README updates
- **Testing Coverage**: Verify adequate test coverage with proper patterns
- **Boy Scout Rule**: Review ALL code in modified files, document pre-existing issues

### Critical Review Checklist
1. **Build Health**
   - Run `dotnet clean && dotnet build` - MUST show 0 warnings, 0 errors
   - Run `dotnet test` - MUST show 100% pass rate
   - Check for skipped/ignored tests

2. **Architecture Compliance**
   - Service boundaries respected (no cross-domain repository access)
   - Proper separation of concerns
   - Dependency injection used correctly
   - Design patterns applied appropriately

3. **Code Patterns**
   - Single exit points enforced
   - Pattern matching used where applicable
   - Empty/Null Object Pattern (return empty, never null)
   - Defensive programming at boundaries only

4. **Testing Standards**
   - xUnit/bUnit patterns followed
   - No magic strings in tests
   - Proper async/await usage (no .Wait() or .Result)
   - Test naming conventions followed

### 4. Boy Scout Rule Implementation
- **Review Scope**: When files are modified (status `M`), review ALL methods in those files
- **Documentation**: Create a "Boy Scout Rule" section documenting:
  - Issues found in unchanged code within modified files
  - Impact assessment (Low/Medium/High)
  - Effort estimation for fixes
  - Recommendations (Fix now/Create tech debt ticket/Document only)
- **Decision Making**: Help teams decide which issues to address immediately

### Review Report Format
Always provide a structured review report including:
- Build status with exact warning/error counts
- Test execution results (X passed, Y failed)
- Clear status: APPROVED/APPROVED_WITH_NOTES/NEEDS_CHANGES
- Specific issues that must be fixed
- Boy Scout Rule findings for modified files
- Recommendations for improvement

Remember: Quality over speed. Maintain the highest standards for code excellence.
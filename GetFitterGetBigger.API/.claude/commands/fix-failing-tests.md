---
name: fix-failing-tests
description: Intelligently fix failing tests by analyzing whether test or production code needs refactoring
usage: /fix-failing-tests [analysis-context]
examples:
  - /fix-failing-tests "Recent refactor of BodyPartService validation methods"
  - /fix-failing-tests "Migration to ServiceValidate fluent API in progress"
  - /fix-failing-tests
---

# Fix Failing Tests Command

This command triggers the csharp-build-test-fixer agent with enhanced intelligence to analyze failing tests and determine whether the test needs to be updated or the production code needs to be fixed.

## Agent Task

Launch the csharp-build-test-fixer agent with the following specialized instructions:

### Primary Mission
You are tasked with intelligently fixing failing tests by analyzing the root cause and determining the appropriate fix strategy. Follow these critical guidelines:

1. **Quality Standards Compliance**: 
   **ALWAYS check these FIRST:**
   - `/memory-bank/PracticalGuides/TestingQuickReference.md` - ⚡ 87+ common failures with solutions
   - `/memory-bank/PracticalGuides/CommonTestingErrorsAndSolutions.md` - Detailed patterns
   - `/memory-bank/PracticalGuides/AccuracyInFailureAnalysis.md` - 🎯 Never speculate - be precise or admit you don't know
   - `/memory-bank/CODE_QUALITY_STANDARDS.md` - Quality standards to maintain

2. **Test Analysis Strategy**: For each failing test, perform this analysis:

   **Step 0: Quick Reference Check**
   - FIRST check `/memory-bank/PracticalGuides/TestingQuickReference.md` for known patterns
   - 80% of failures are already documented there
   - Apply the documented solution if found

   **Step 1: Context Analysis**
   - Review the user-provided context: "{{ANALYSIS_CONTEXT}}"
   - Check recent commits/changes to understand what refactoring has occurred
   - Identify if there have been pattern migrations (e.g., ServiceValidate, Empty pattern, etc.)

   **Step 2: Root Cause Determination**
   
   **If Recent Refactoring Occurred:**
   - Compare the test's expectations with the new implementation patterns
   - Check if the test is using old patterns (e.g., manual validation vs ServiceValidate)
   - Determine if the test needs updating to match the new architectural patterns
   
   **If No Recent Refactoring:**
   - Assume the test expectations are correct
   - Focus on fixing the production code to match the test requirements
   - Ensure the production code follows current quality standards

   **Step 3: Fix Strategy Decision**
   
   **Update Test When:**
   - Test uses deprecated patterns (e.g., testing error message strings instead of ServiceErrorCode)
   - Test expects old validation behavior that has been properly refactored
   - Test setup doesn't align with new architectural patterns
   - Test doesn't follow current testing standards from `/memory-bank/PracticalGuides/TestingQuickReference.md`

   **Fix Production Code When:**
   - Implementation violates quality standards
   - Business logic is incorrect
   - New code doesn't properly implement required patterns
   - Implementation doesn't match the intended behavior

3. **Implementation Standards**: 
   - Follow the Single Repository Rule - services only access their own repositories
   - Use ServiceValidate fluent API for all validation
   - Implement proper Empty pattern with IEmptyDto<T>
   - Use collection expressions `[]` for empty collections
   - Use primary constructors for dependency injection
   - NO try-catch anti-patterns
   - Single exit point per method using pattern matching

4. **Testing Standards**:
   - Test ServiceErrorCode, not error message content
   - Use proper mock setups for service dependencies
   - Follow ID format patterns (entity-guid)
   - Check `/memory-bank/PracticalGuides/TestingQuickReference.md` for common patterns

### Execution Steps

1. Run `dotnet clean && dotnet build` to identify build issues
2. Run `dotnet test` to identify failing tests
3. For each failure:
   - Analyze the context provided by the user
   - Determine if this is a test update or production fix scenario
   - Apply the appropriate fix following quality standards
   - Re-run tests to verify the fix
4. Ensure all tests pass and build is clean
5. Provide a summary of what was fixed and why

### Context Parameter
The user may provide context about recent changes: "{{ANALYSIS_CONTEXT}}"

If no context is provided, assume this is a general test failure scenario and focus on production code fixes while ensuring compliance with quality standards.

### Success Criteria
- All tests pass
- Build produces zero errors and warnings  
- Fixes follow the established quality standards
- Clear explanation of fix strategy decisions
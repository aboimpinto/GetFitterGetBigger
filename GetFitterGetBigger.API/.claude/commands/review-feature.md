You will perform a comprehensive code review for the specified feature using the feature-code-reviewer agent.

When the user types `/review-feature FEAT-XXX`, you should:

1. **Find the Feature Folder**:
   - Search in /memory-bank/features/ subdirectories (0-SUBMITTED, 1-READY_TO_DEVELOP, 2-IN_PROGRESS, 3-COMPLETED, 4-BLOCKED, 5-SKIPPED)
   - Confirm the feature exists

2. **Launch the Review Agent**:
   - Use the Task tool to launch the 'feature-code-reviewer' agent
   - Provide clear instructions to review the specific feature folder
   - Ask it to return the report path and summary

3. **Verify Report Generation**:
   - Check if the code review report was created in the feature folder
   - If not found in expected location, check agent output and save it yourself

4. **Update Feature Tasks**:
   - Open the feature-tasks.md file in the feature folder
   - Find the "Final Code Review" checkpoint section
   - Add the report link with timestamp and approval status

5. **Provide Summary to User**:
   - Feature reviewed
   - Report location
   - Overall approval rate
   - Number of critical violations
   - Recommendation (Approved/Needs Revision/etc.)

Example response format:
```
✅ Code Review Completed for FEAT-030

📊 Report: /memory-bank/features/3-COMPLETED/FEAT-030/code-review-report-2025-01-04.md

Summary:
- Overall Approval Rate: 85%
- Critical Violations: 2
- Minor Violations: 5
- Status: NEEDS REVISION

Critical Issues Found:
1. Multiple exit points in MatchAsync (Line 45, ExerciseService.cs)
2. Entity returned from DataService (Line 120, ExerciseLinkQueryDataService.cs)

The report has been added to the Final Code Review checkpoint in feature-tasks.md.
```
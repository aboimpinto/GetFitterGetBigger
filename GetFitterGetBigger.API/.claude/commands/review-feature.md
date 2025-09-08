You will perform a comprehensive code review for the specified feature using the feature-code-reviewer agent.

**Report Naming Convention**: 
- First review of the day: `code-review-report-YYYY-MM-DD.md`
- Subsequent reviews same day: `code-review-report-YYYY-MM-DD-001.md`, `code-review-report-YYYY-MM-DD-002.md`, etc.

When the user types `/review-feature FEAT-XXX [options]`, you should:

1. **Parse Command Options**:
   - `--incremental`: Review only new commits since last review
   - `--fix-only`: Only review files from Code Review Fixes phase
   - `--quick`: Use Sonnet model for faster (less thorough) review
   - `--thorough`: Force Opus 4.1 model for maximum accuracy (default)
   - Default to full review with Opus 4.1 if no flags provided
   
   Examples:
   - `/review-feature FEAT-030` - Full review with Opus 4.1
   - `/review-feature FEAT-030 --incremental` - Incremental review with Opus 4.1
   - `/review-feature FEAT-030 --quick` - Full review with Sonnet (faster)
   - `/review-feature FEAT-030 --incremental --quick` - Quick incremental review

2. **Find the Feature Folder**:
   - Search in /memory-bank/features/ subdirectories (0-SUBMITTED, 1-READY_TO_DEVELOP, 2-IN_PROGRESS, 3-COMPLETED, 4-BLOCKED, 5-SKIPPED)
   - Confirm the feature exists
   - Check if previous review reports exist (for incremental reviews)

3. **Pre-Review Checks**:
   - Check if feature-tasks.md exists and has commits
   - If incremental, verify previous review report exists
   - Note current phase and any existing Code Review Fixes phase

4. **Launch the Review Agent**:
   - Use the Task tool to launch the 'feature-code-reviewer' agent
   - If `--quick` flag is used:
     - Override agent's default model by specifying in prompt:
       "Use Sonnet model for this quick review. Focus on critical violations."
     - Note: The agent defaults to Opus 4.1 for thorough reviews
   - Provide clear instructions including:
     - Feature folder path
     - Review type (initial/incremental)
     - Model preference (if --quick flag used)
     - Last reviewed commit (if incremental)
   - Ask it to return:
     - Report path
     - Summary of findings
     - Number of tasks created
     - Critical violations list
     - Model used for review

5. **Verify Report Generation**:
   - Check if the code review report was created in the feature folder
   - Report will be named using pattern: `code-review-report-YYYY-MM-DD.md`
   - If multiple reviews on same day: `code-review-report-YYYY-MM-DD-001.md`, `code-review-report-YYYY-MM-DD-002.md`, etc.
   - If not found in expected location, check agent output and save it yourself
   - Verify fix tasks were added to feature-tasks.md if violations found

6. **Update Feature Tasks or Phase Checkpoint**:
   - Check if feature has `Phases/` directory:
     - If yes: Find active phase document and update its checkpoint
     - Look for `## CHECKPOINT:` section in the phase document
     - Update the phase checkpoint, NOT feature-tasks.md
   - If no phases: Update feature-tasks.md as before
   - If initial review: Add report link in Code Review checkpoint
   - If incremental: Update existing checkpoint with new iteration
   - If violations found: Verify "Code Review" section exists
     - Each review adds a numbered entry with status and summary
   - Add review commit hash for tracking

7. **Provide Enhanced Summary to User**:
   - Feature reviewed with type (Initial/Incremental)
   - Report location
   - Build and test status
   - Overall approval rate (and improvement if incremental)
   - Number of unique files reviewed
   - Number of critical violations
   - Number of fix tasks created
   - Recommendation with clear next steps

Example response formats:

### Initial Review with Violations:
```
✅ Code Review Completed for FEAT-030

📊 Report: /memory-bank/features/2-IN_PROGRESS/FEAT-030/code-review-report-2025-01-04.md
         (If multiple reviews same day: code-review-report-2025-01-04-001.md)

Review Summary:
- Review Type: Initial
- Review Model: Opus 4.1 (Thorough)
- Build Status: ✅ Passing
- Test Status: ✅ Passing (98% coverage)
- Files Reviewed: 12 unique files across 8 commits
- Overall Approval Rate: 85%
- Critical Violations: 2
- Minor Violations: 5
- Status: NEEDS REVISION

Critical Issues Found:
1. Multiple exit points in MatchAsync (ExerciseService.cs:45)
2. Entity returned from DataService (ExerciseLinkQueryDataService.cs:120)

📝 7 fix tasks created in "Code Review" phase in feature-tasks.md

Next Steps:
1. Address critical violations first
2. Fix minor violations
3. Run `/review-feature FEAT-030 --incremental` after fixes
```

### Incremental Review After Fixes:
```
✅ Incremental Code Review Completed for FEAT-030

📊 Report: /memory-bank/features/2-IN_PROGRESS/FEAT-030/code-review-report-2025-01-04-001.md
         (New report file for second review on same day)

Review Summary:
- Review Type: Incremental (3 new commits since last review)
- Review Model: Opus 4.1 (Thorough)
- Files Reviewed: 5 files modified in fix commits
- Overall Approval Rate: 96% (↑ 11% improvement)
- Critical Violations: 0 (✅ all fixed)
- Minor Violations: 1
- Status: APPROVED

Improvements Since Last Review:
✅ Fixed: Multiple exit points issue
✅ Fixed: Entity leakage from DataService
✅ Fixed: 4 out of 5 minor violations

Remaining Minor Issue:
1. Magic string in ExerciseValidator.cs:78 (Low priority)

Next Steps:
Feature is ready for merge after addressing the remaining minor issue.
```

### Quick Review Example (Using Sonnet):
```
✅ Quick Code Review Completed for FEAT-040

📊 Report: /memory-bank/features/2-IN_PROGRESS/FEAT-040/code-review-report-2025-01-06.md

Review Summary:
- Review Type: Initial
- Review Model: Sonnet (Quick)
- Build Status: ✅ Passing
- Test Status: ✅ Passing (95% coverage)
- Files Reviewed: 6 unique files across 3 commits
- Critical Violations: 1
- Status: NEEDS REVISION

Critical Issue Found:
1. ServiceResult not used in new service method (PaymentService.cs:89)

⚡ Quick review completed. Run full review with `/review-feature FEAT-040` for comprehensive analysis.
```

### Clean Review (No Violations):
```
✅ Code Review Completed for FEAT-035

📊 Report: /memory-bank/features/3-COMPLETED/FEAT-035/code-review-report-2025-01-05.md

Review Summary:
- Review Type: Initial
- Review Model: Opus 4.1 (Thorough)
- Build Status: ✅ Passing
- Test Status: ✅ Passing (99.2% coverage)
- Files Reviewed: 8 unique files across 5 commits
- Overall Approval Rate: 98%
- Critical Violations: 0
- Minor Violations: 0
- Status: APPROVED

🎉 Excellent code quality! All CODE_QUALITY_STANDARDS.md rules followed.

Next Steps:
Feature is ready for merge to master branch.
```
Review an implemented feature phase to ensure all requirements are met before approving the checkpoint.

## Purpose
This command performs a comprehensive review of a completed feature phase to verify:
- All tasks are properly tracked with times and commits
- Build and tests are passing
- Code review is completed and approved
- Checkpoint can be marked as APPROVED

## Review Process

### 1. Task Tracking Verification
Check each task in the current phase for:
- ✅ Status changed from `[ReadyToDevelop]` to `[Completed]`
- ✅ Start date/time recorded: `Started: YYYY-MM-DD HH:MM`
- ✅ End date/time recorded: `Finished: YYYY-MM-DD HH:MM`
- ✅ Actual time vs estimated time documented
- ❌ Any task missing proper tracking = CHECKPOINT REJECTED

### 2. Git Commit Verification
- ✅ All tasks should reference git commit hash(es)
- ✅ Checkpoint must list all relevant commits with:
  - Commit hash (short form)
  - Commit message summary
  - Date/time of commit
- ✅ Use `git log --oneline -n 20` to verify commits
- ❌ Missing commit references = CHECKPOINT REJECTED

### 3. Build & Test Verification
Run fresh build and test cycle:
```bash
# Clean build
dotnet clean
dotnet build

# Run all tests
dotnet test
```

Requirements:
- ✅ Build succeeds with 0 errors
- ✅ Build has 0 warnings (or warnings documented and approved)
- ✅ All tests pass
- ✅ Test count should be >= baseline (no tests lost)
- ❌ Any failures = CHECKPOINT REJECTED

### 4. Code Review Verification
Check for code review document:
- Look in `/memory-bank/features/[phase]/[feature]/code-reviews/`
- If exists:
  - ✅ Must have status: APPROVED
  - ❌ Status: NEEDS-CHANGES = Fix issues and re-review
  - ❌ Status: REJECTED = Major rework needed
- If not exists:
  - Note: "Code review pending"
  - Can be CONDITIONALLY APPROVED with note

### 5. Checkpoint Summary Update
Update checkpoint with:
```markdown
## CHECKPOINT: Phase X Complete - [Phase Name]
`[STATUS]` - Date: YYYY-MM-DD HH:MM

Build Report:
- Admin Project: ✅ 0 errors, 0 warnings
- Test Project: ✅ 0 errors, 0 warnings

Test Summary:
- **Total Tests**: XXX (baseline: YYY)
- **All Tests**: ✅ PASSING
- **New Tests Added**: ZZ
- **Coverage**: XX% Line, YY% Branch, ZZ% Method

Git Commits:
- `hash1` - Commit message 1
- `hash2` - Commit message 2

Code Review: 
- Status: [APPROVED/PENDING/NEEDS-CHANGES]
- Document: `path/to/review.md`

Time Tracking:
- **Estimated**: X hours
- **Actual**: Y hours Z minutes
- **Efficiency**: XX% faster/slower

Status: [APPROVED/REJECTED/CONDITIONAL]
Notes: 
- [Any important notes]
- [Issues found]
- [Conditions for conditional approval]

[✅/❌] Checkpoint [APPROVED/REJECTED] - [Ready to proceed/Issues must be resolved]
```

## Review Checklist

### Task Tracking
- [ ] All tasks marked [Completed]
- [ ] All tasks have start/end times
- [ ] All tasks have actual time recorded
- [ ] Phase time summary calculated

### Git Tracking
- [ ] All tasks reference commits
- [ ] Checkpoint lists all commits
- [ ] Commit messages follow conventions

### Build & Test
- [ ] Clean build performed
- [ ] Build has 0 errors
- [ ] Build has 0 warnings (or approved)
- [ ] All tests pass
- [ ] Test count >= baseline
- [ ] Coverage maintained/improved

### Code Review
- [ ] Review document exists (or noted as pending)
- [ ] Review status is APPROVED (if exists)
- [ ] All review issues addressed

### Documentation
- [ ] Checkpoint fully updated
- [ ] Time tracking complete
- [ ] All metrics recorded

## Decision Matrix

### ✅ APPROVED
- All checklist items pass
- Ready for next phase

### 🟡 CONDITIONAL APPROVAL
- Minor issues only
- Code review pending but can proceed
- Document conditions clearly

### ❌ REJECTED
- Any critical failures
- Must fix before proceeding
- List all issues to resolve

## Example Usage

```
/review-implemented-feature

Reviewing Phase 1 of FEAT-021...

✅ Task Tracking: All 6 tasks properly tracked
✅ Git Commits: Found 2 commits referenced
✅ Build: Success - 0 errors, 0 warnings
✅ Tests: All 851 tests passing
⚠️ Code Review: Not found - marking as pending

CHECKPOINT STATUS: CONDITIONALLY APPROVED
- Condition: Complete code review before Phase 3
- All technical requirements met
- Can proceed with Phase 2
```

## Important Notes
- This review should be run after claiming a phase is complete
- Any REJECTED status blocks progress to next phase
- Conditional approvals should be tracked and resolved
- Re-run this review after fixing any issues
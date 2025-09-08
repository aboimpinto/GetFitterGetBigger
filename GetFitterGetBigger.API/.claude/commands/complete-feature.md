Complete the current in-progress feature following @memory-bank/DEVELOPMENT_PROCESS.md and @memory-bank/FEATURE_WORKFLOW_PROCESS.md.

Instructions:
1. Verify feature is in 2-IN_PROGRESS state
2. Check all implementation tasks are completed
3. Ensure all tests are passing (unit and integration)
4. Perform final code review using /feature-code-review
5. **🔴 CRITICAL**: Verify code review status is APPROVED
6. Update feature documentation
7. **ONLY IF** code review is APPROVED: Move feature to 3-COMPLETED folder

Pre-completion checklist:
- [ ] All feature tasks completed
- [ ] All unit tests passing
- [ ] All integration tests passing
- [ ] **🔴 Code review APPROVED (NOT REQUIRES_CHANGES or REJECTED)**
- [ ] Documentation updated
- [ ] No unresolved TODOs
- [ ] API documentation generated (if applicable)
- [ ] Memory bank documentation current

**⚠️ CRITICAL REQUIREMENT**: 
- NEVER move to 3-COMPLETED if code review status is REQUIRES_CHANGES or REJECTED
- If fixes were made after a failed review, a NEW code review must be generated and APPROVED
- Partial fixes are NOT acceptable - ALL issues must be resolved

Validation steps:
- Run all tests: `dotnet test`
- Check code coverage meets standards
- Verify no regression in existing functionality
- Ensure all acceptance criteria met

If feature is not ready:
- Report what's missing or blocking completion
- List remaining tasks
- Identify any failing tests
- Note any unresolved code review issues

The feature can only be marked complete when ALL criteria are met.
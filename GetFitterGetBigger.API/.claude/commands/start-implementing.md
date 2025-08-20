Start implementing a feature from the READY_TO_DEVELOP folder by orchestrating validation agents.
Usage: /start-implementing [FEAT-XXX or feature selection]

## Overview

This command orchestrates two specialized agents to validate and prepare features for implementation:
1. **feature-pre-validator**: Validates feature readiness with zero-tolerance for assumptions
2. **feature-post-validator**: Enhances documentation and transitions to IN_PROGRESS

## Implementation Guides

**Use these while coding:**
- `/memory-bank/PracticalGuides/ServiceImplementationChecklist.md` - 📋 Step-by-step checklist
- `/memory-bank/PracticalGuides/CommonImplementationPitfalls.md` - ⚠️ Mistakes to avoid
- `/memory-bank/CODE_QUALITY_STANDARDS.md` - Quality standards to follow

## Execution Process

### Step 1: Feature Selection
1. List features in `/memory-bank/features/1-READY_TO_DEVELOP/`
2. Select the specified feature or highest priority if none specified
3. Verify feature ID format (FEAT-XXX)

### Step 2: Pre-Validation Agent
**IMPORTANT**: Always read the latest agent configuration to understand current behavior:
```
Read: /home/esqueleto/myWork/GetFitterGetBigger/GetFitterGetBigger.API/.claude/agents/feature-pre-validator.md
```

Trigger the `feature-pre-validator` agent with:
- **Input**: Feature ID (e.g., FEAT-030)
- **Expected Output**: Validation report with APPROVED/REJECTED status

**On Success (APPROVED)**:
- Save the validation report to: `/memory-bank/features/1-READY_TO_DEVELOP/[FEAT-ID]/pre-validation-report-[YYYY-MM-DD-HH-MM].md`
- Proceed to Step 3

**On Failure (REJECTED)**:
- Save the validation report to: `/memory-bank/features/1-READY_TO_DEVELOP/[FEAT-ID]/pre-validation-report-REJECTED-[YYYY-MM-DD-HH-MM].md`
- STOP execution
- Display rejection reasons to user
- Suggest running `/refine-feature` to address issues

### Step 3: Post-Validation Agent
**IMPORTANT**: Always read the latest agent configuration to understand current behavior:
```
Read: /home/esqueleto/myWork/GetFitterGetBigger/GetFitterGetBigger.API/.claude/agents/feature-post-validator.md
```

Trigger the `feature-post-validator` agent with:
- **Input 1**: Feature ID
- **Input 2**: Pre-validation report content (paste the APPROVED report)
- **Context**: "The feature [FEAT-ID] has been pre-validated with APPROVED status. Please proceed with post-validation tasks."

**Expected Tasks**:
1. Enhance feature-tasks.md with time estimates
2. Add implementation guidance
3. Ensure checkpoint sections
4. Create/verify feature branch
5. Move folder to `/memory-bank/features/2-IN_PROGRESS/`
6. Execute baseline health check
7. Fill health check report

**On Success (READY)**:
- Feature is now in IN_PROGRESS
- Save post-validation report to: `/memory-bank/features/2-IN_PROGRESS/[FEAT-ID]/post-validation-report-[YYYY-MM-DD-HH-MM].md`
- Display success message
- Ready to begin implementation

**On Failure (BLOCKED)**:
- Save post-validation report to feature folder with issues
- Display blocking issues
- Provide remediation steps
- Feature remains in current state until issues resolved

## Implementation Approach

Once successfully transitioned:
1. Start with foundational components (Models, Database)
2. Build bottom-up (Repository → Service → Controller)
3. Write tests alongside implementation
4. Commit after each completed component
5. Run tests frequently
6. Stop at first checkpoint for user review

## Error Handling

### Pre-Validation Failures
- Document specific validation failures
- Keep feature in READY_TO_DEVELOP
- Suggest refinement actions

### Post-Validation Failures
- Document blocking issues
- Rollback any partial changes
- Provide clear next steps

### Agent Communication
- All context passing via saved reports in feature folder
- Maintain audit trail of all validation attempts
- Use timestamped filenames for traceability

## Notes

- Only one feature should be in 2-IN_PROGRESS at a time
- Agents enforce strict validation - no assumptions allowed
- All reports are saved for audit and learning purposes
- Always check latest agent configurations for behavior updates
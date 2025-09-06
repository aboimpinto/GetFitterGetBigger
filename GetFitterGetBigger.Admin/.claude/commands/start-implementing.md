# Start Implementing Command - Admin Blazor Project

Start implementing a Blazor feature from the READY_TO_DEVELOP folder by orchestrating validation agents.

## Usage

`/start-implementing [FEAT-XXX or feature selection]`

## Overview

This command orchestrates two specialized agents to validate and prepare Blazor features for implementation:
1. **feature-pre-validator**: Validates feature readiness with zero-tolerance for missing UI documentation
2. **feature-post-validator**: Enhances documentation with Blazor patterns and transitions to IN_PROGRESS

## Critical Validation Focus (Blazor-Specific)

**UI Documentation Requirements:**
- ✅ Wireframes/screenshots for EVERY screen
- ✅ ALL user interactions documented
- ✅ ALL form validations specified
- ✅ State management patterns clear
- ✅ API contracts with full DTOs

**If ANY of these are missing, the feature will be REJECTED!**

## Implementation Guides

**Use these while coding:**
- `/memory-bank/UI_LIST_PAGE_DESIGN_STANDARDS.md` - List/grid view standards
- `/memory-bank/UI_FORM_PAGE_DESIGN_STANDARDS.md` - Form design patterns
- `/memory-bank/COMPREHENSIVE-TESTING-GUIDE.md` - bUnit testing patterns
- `/memory-bank/CODE_QUALITY_STANDARDS.md` - Quality standards to follow

## Execution Process

### Step 1: Feature Selection
1. List features in `/memory-bank/features/1-READY_TO_DEVELOP/`
2. Select the specified feature or let user choose if none specified
3. Verify feature ID format (FEAT-XXX)

### Step 2: Pre-Validation Agent

**Trigger the `feature-pre-validator` agent:**

```markdown
Task: feature-pre-validator
Input: Feature ID (e.g., FEAT-022)
Purpose: Validate that ALL UI documentation, interactions, validations, and API contracts are complete
```

**Expected Validations:**
- Build health (0 errors, 0 warnings)
- Test health (100% pass rate)
- Wireframes for every screen
- Interactions for every component
- Validations for every form field
- State management patterns
- API endpoint documentation
- Blazor component structure

**On Success (APPROVED):**
- Save report to: `/memory-bank/features/1-READY_TO_DEVELOP/[FEAT-ID]/pre-validation-report-APPROVED-[YYYY-MM-DD-HH-MM].md`
- Display approval summary
- Proceed to Step 3

**On Failure (REJECTED):**
- Save report to: `/memory-bank/features/1-READY_TO_DEVELOP/[FEAT-ID]/pre-validation-report-REJECTED-[YYYY-MM-DD-HH-MM].md`
- **STOP execution immediately**
- Display detailed rejection reasons:
  - List missing wireframes
  - List undefined interactions
  - List fields without validation
  - List incomplete API documentation
- Suggest actions:
  - "Add wireframes for [screens]"
  - "Document interactions for [components]"
  - "Specify validations for [fields]"
  - "Complete API contracts for [endpoints]"
- Recommend running `/refine-feature` or requesting UX documentation

### Step 3: User Approval Gate

**IMPORTANT**: After pre-validation APPROVED, ask user for confirmation:

```markdown
## Pre-Validation APPROVED ✅

The feature [FEAT-XXX] has passed pre-validation checks:
- All UI documentation present
- All interactions documented
- All validations specified
- Build and tests passing

**Report saved to:** [path]

Would you like to proceed with post-validation to transition to IN_PROGRESS?
[Type 'yes' to continue or 'no' to stop]
```

**User Response Handling:**
- If "yes" or approval: Continue to Step 4
- If "no" or rejection: Stop process, feature remains in READY_TO_DEVELOP
- If unclear: Ask for clarification

### Step 4: Post-Validation Agent

**Trigger the `feature-post-validator` agent:**

```markdown
Task: feature-post-validator
Input 1: Feature ID
Input 2: Pre-validation report content (the APPROVED report)
Context: "The feature [FEAT-ID] has been pre-validated with APPROVED status. Please proceed with post-validation tasks."
```

**Expected Tasks:**
1. Enhance feature-tasks.md with:
   - Time estimates for Blazor components
   - Component lifecycle guidance
   - State management patterns
   - Form handling approaches
   - API integration patterns
   - bUnit testing strategies
2. Add checkpoint sections per phase
3. Create/verify feature branch
4. Move folder to `/memory-bank/features/2-IN_PROGRESS/`
5. Execute baseline health check
6. Generate comprehensive report

**On Success (READY):**
- Feature is now in IN_PROGRESS
- Save report to: `/memory-bank/features/2-IN_PROGRESS/[FEAT-ID]/post-validation-report-READY-[YYYY-MM-DD-HH-MM].md`
- Display success message with:
  - Total time estimates added
  - Blazor patterns referenced
  - Branch name created/verified
  - Health check results
- Show next steps for implementation

**On Failure (BLOCKED):**
- Save report to: `/memory-bank/features/1-READY_TO_DEVELOP/[FEAT-ID]/post-validation-report-BLOCKED-[YYYY-MM-DD-HH-MM].md`
- Feature remains in READY_TO_DEVELOP
- Display blocking issues:
  - Build errors/warnings
  - Test failures
  - File system issues
- Provide remediation steps

## Implementation Approach

Once successfully transitioned to IN_PROGRESS:

### Blazor Component Development Order:
1. **Models & DTOs** - Define data structures
2. **State Services** - Implement IStateService interfaces
3. **Base Components** - Build reusable components
4. **Pages** - Implement full page components
5. **Forms** - Add validation and submission logic
6. **API Integration** - Connect to backend services
7. **Tests** - Write bUnit tests for all components

### Development Practices:
- Follow UI standards for every component
- Add data-testid attributes for testing
- Implement loading and error states
- Handle all edge cases in forms
- Write tests alongside implementation
- Commit after each completed component
- Run tests frequently
- Stop at checkpoints for review

## Error Handling

### Missing UI Documentation
**Most Common Rejection Reason!**
- Feature lacks wireframes
- Interactions not documented
- Validations not specified
- Request UX team support
- Use ux-researcher and ux-storyteller agents if needed

### Pre-Validation Failures
- Document specific validation failures
- Keep detailed list of missing items
- Feature stays in READY_TO_DEVELOP
- Clear actionable feedback provided

### Post-Validation Failures
- Document blocking issues
- Rollback any partial changes
- Provide clear remediation steps
- Feature remains in current state

### Agent Communication
- All reports saved with timestamps
- Full audit trail maintained
- Reports used for context passing between agents

## Common Issues and Solutions

### Issue: "Missing wireframes for component X"
**Solution:** Request UX documentation or use ux-storyteller agent to create wireframes

### Issue: "Undefined interaction for button Y"
**Solution:** Document what happens when button is clicked, including:
- Navigation target
- API calls triggered
- State changes
- Success/error feedback

### Issue: "No validation rules for field Z"
**Solution:** Specify:
- Required/optional status
- Data type and format
- Min/max length or values
- Error messages for each validation

### Issue: "API endpoint documentation incomplete"
**Solution:** Document:
- Full endpoint URL and HTTP method
- Request body structure with all fields
- Response structure for success and errors
- Status codes and their meanings

## Report Naming Convention

All reports are timestamped and categorized:
- `pre-validation-report-[STATUS]-[YYYY-MM-DD-HH-MM].md`
- `post-validation-report-[STATUS]-[YYYY-MM-DD-HH-MM].md`

Where STATUS is:
- APPROVED / REJECTED (for pre-validation)
- READY / BLOCKED (for post-validation)

## Notes

- Only one feature should be in 2-IN_PROGRESS at a time
- Pre-validator has ZERO tolerance for missing UI documentation
- All validation reports are saved for audit and learning
- User approval required between pre and post validation
- Focus on Blazor-specific patterns and standards
- Always check for wireframes, interactions, and validations

## Quick Reference

**Command Flow:**
1. `/start-implementing FEAT-022`
2. Pre-validator runs → Report saved
3. If APPROVED → User confirmation requested
4. If user approves → Post-validator runs
5. Feature transitions to IN_PROGRESS
6. Ready for Blazor development

**Key Files Generated:**
- Pre-validation report (always)
- Post-validation report (if pre-validation approved)
- Enhanced feature-tasks.md (in IN_PROGRESS)
- Checkpoint templates added (per phase)
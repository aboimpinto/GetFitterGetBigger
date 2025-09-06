# Feature Checkpoint Template - Admin Blazor Project

## Standard Checkpoint Format for feature-tasks.md

When adding checkpoints to feature-tasks.md in the Admin project, always use this format to ensure code reviews are placed in the correct location:

### Checkpoint Template:
```markdown
## CHECKPOINT: [Phase Name] Complete - [Description]
`[STATUS]` - Date: [YYYY-MM-DD HH:MM]

Build Report:
- Admin Project: [STATUS] [X errors, Y warnings]
- Test Project (bUnit): [STATUS] [X errors, Y warnings]

[Additional Status Reports as needed]

Code Review: `/memory-bank/features/2-IN_PROGRESS/FEAT-XXX-[feature-name]/code-reviews/[Phase_Name]/Code-Review-[Phase-Name]-YYYY-MM-DD-HH-MM-[STATUS].md` - [[STATUS]]

Git Commit: `[COMMIT_HASH]` - [commit message summary]

Status: [STATUS] [Phase Name]
Notes: 
- [Key notes about the phase completion]
- [Important Blazor component details]
- [Readiness for next phase]
```

### Code Review Folder Structure:
```
/memory-bank/features/2-IN_PROGRESS/FEAT-XXX-[feature-name]/
├── feature-tasks.md
├── code-reviews/
│   ├── Phase_1_[Name]/
│   │   ├── Code-Review-Phase-1-[Name]-YYYY-MM-DD-HH-MM-[STATUS].md
│   │   ├── Phase-1-[Name]-Final-Report.md
│   │   └── Implementation-Overview.md
│   ├── Phase_2_[Name]/
│   │   └── [similar structure]
│   └── Phase_N_[Name]/
│       └── [similar structure]
└── [other feature files]
```

### Code Review File Naming Convention:
- **Main Review**: `Code-Review-[Phase-Name]-YYYY-MM-DD-HH-MM-[STATUS].md`
- **Final Report**: `[Phase-Name]-Final-Report.md`
- **Overview**: `Implementation-Overview.md`

### Status Values:
- `APPROVED` - All checks passed, ready to proceed
- `APPROVED_WITH_NOTES` - Minor issues, can proceed with notes
- `REQUIRES_CHANGES` - Critical issues, must fix before proceeding

### Example Usage for Blazor Features:
```markdown
## CHECKPOINT: Phase 3 Complete - Blazor Components
`[COMPLETE]` - Date: 2025-01-28 14:30

Build Report:
- Admin Project: ✅ 0 errors, 0 warnings (builds successfully)
- Test Project (bUnit): ✅ 0 errors, 0 warnings (all 45 tests passing)

Component Implementation Summary:
- **ExerciseLinkManager.razor**: ✅ Context-aware UI implemented
- **AlternativeExerciseCard.razor**: ✅ New component created
- **State Service Updates**: ✅ IExerciseLinkStateService enhanced
- **Accessibility**: ✅ WCAG AA compliant with full keyboard support

Code Review: `/memory-bank/features/2-IN_PROGRESS/FEAT-022-four-way-linking/code-reviews/Phase_3_Components/Code-Review-Phase-3-Components-2025-01-28-14-30-APPROVED.md` - [APPROVED ✅]

Git Commit: `a4c7b2f9` - feat(FEAT-022): implement four-way linking Blazor components with context-aware UI

Status: ✅ Phase 3 COMPLETE
Notes: 
- All Blazor components follow established UI standards
- State management pattern implemented correctly
- Ready to proceed with Phase 4: API Integration
```

## 🚨 Critical Requirements - MANDATORY

### Git Commit Hash Requirement
**NEVER proceed to the next phase without adding the git commit hash!**

1. **Complete Phase Implementation** ✅
2. **Create Code Review** ✅  
3. **Create Git Commit** ✅
4. **🔴 ADD COMMIT HASH TO CHECKPOINT** ⚠️ MANDATORY
5. **Only then proceed to next phase** ✅

### Why Git Commit Hash is Critical:
- **Traceability**: Links checkpoint to exact code state
- **Rollback**: Enables easy rollback if issues found later
- **Code Review Validation**: Proves code review matches actual commit
- **Team Collaboration**: Others can checkout exact state for review
- **Audit Trail**: Complete history of phase implementations

### Blazor-Specific Checklist:
- [ ] Phase implementation complete
- [ ] Blazor components tested with bUnit
- [ ] UI standards compliance verified
- [ ] Accessibility requirements met
- [ ] Code review created and approved
- [ ] Git commit created with descriptive message
- [ ] **Git commit hash added to checkpoint** ⚠️ MANDATORY
- [ ] Ready to proceed to next phase

## Admin Project Specific Phases

Common phases for Blazor features:

1. **Phase 1: Planning & Analysis**
   - Study existing Blazor components
   - Review UI standards and patterns
   - Analyze API integration requirements

2. **Phase 2: Models & State Management**
   - Create/update DTOs
   - Implement state services
   - Setup dependency injection

3. **Phase 3: Blazor Components**
   - Create component structure
   - Implement UI logic
   - Add data binding and event handlers

4. **Phase 4: API Integration**
   - Connect to API services
   - Implement error handling
   - Add loading states

5. **Phase 5: Testing & Polish**
   - Write bUnit tests
   - Manual testing with different scenarios
   - Accessibility testing
   - Performance optimization

6. **Phase 6: Documentation & Deployment**
   - Update user documentation
   - Create feature quick reference
   - Deployment preparation

## Benefits:
1. **Clear Path**: Explicit folder path prevents confusion
2. **Consistency**: Standardized naming and structure
3. **Traceability**: Easy to find code reviews for any phase
4. **Git Integration**: Direct link to implementation commits
5. **Organization**: Clean separation by phase
6. **Blazor Focus**: Tailored for component-based development

## Implementation:
- Always create the `code-reviews/[Phase_Name]/` folder structure first
- Use the full absolute path in the Code Review line
- **Add git commit hash immediately after creating commit**
- Include status and date for traceability
- Follow the naming convention exactly
- Ensure Blazor component tests are passing before marking complete
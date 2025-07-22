# Feature Checkpoint Template

## Standard Checkpoint Format for feature-tasks.md

When adding checkpoints to feature-tasks.md, always use this format to ensure code reviews are placed in the correct location:

### Checkpoint Template:
```markdown
## CHECKPOINT: [Phase Name] Complete - [Description]
`[STATUS]` - Date: [YYYY-MM-DD HH:MM]

Build Report:
- API Project: [STATUS] [X errors, Y warnings]
- Test Project (Unit): [STATUS] [X errors, Y warnings]  
- Test Project (Integration): [STATUS] [X errors, Y warnings]

[Additional Status Reports as needed]

Code Review: `/memory-bank/features/2-IN_PROGRESS/FEAT-XXX-[feature-name]/code-reviews/[Phase_Name]/Code-Review-[Phase-Name]-YYYY-MM-DD-HH-MM-[STATUS].md` - [[STATUS]]

Git Commit: `[COMMIT_HASH]` - [commit message summary]

Status: [STATUS] [Phase Name]
Notes: 
- [Key notes about the phase completion]
- [Important implementation details]
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

### Example Usage:
```markdown
## CHECKPOINT: Phase 3 Complete - Repository Layer
`[COMPLETE]` - Date: 2025-07-23 01:30

Build Report:
- API Project: ✅ 0 errors, 0 warnings (builds successfully)
- Test Project (Unit): ✅ 0 errors, 0 warnings (builds successfully)
- Test Project (Integration): ✅ 0 errors, 0 warnings (builds successfully)

Repository Implementation Summary:
- **WorkoutTemplate Repository**: ✅ 13 methods implemented
- **WorkoutTemplateExercise Repository**: ✅ 12 methods implemented  
- **SetConfiguration Repository**: ✅ 14 methods implemented

Code Review: `/memory-bank/features/2-IN_PROGRESS/FEAT-026-workout-template-core/code-reviews/Phase_3_Repository/Code-Review-Phase-3-Repository-2025-07-23-01-30-APPROVED.md` - [APPROVED ✅]

Git Commit: `38019727` - feat(FEAT-026): complete Phase 3 Repository Layer with comprehensive data access

Status: ✅ Phase 3 COMPLETE
Notes: 
- All repository interfaces and implementations follow established patterns
- Code quality standards fully compliant
- Ready to proceed with Phase 4: Service Layer
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

### Template Checklist:
- [ ] Phase implementation complete
- [ ] Code review created and approved
- [ ] Git commit created with descriptive message
- [ ] **Git commit hash added to checkpoint** ⚠️ MANDATORY
- [ ] Ready to proceed to next phase

## Benefits:
1. **Clear Path**: Explicit folder path prevents confusion
2. **Consistency**: Standardized naming and structure
3. **Traceability**: Easy to find code reviews for any phase
4. **Git Integration**: Direct link to implementation commits
5. **Organization**: Clean separation by phase
6. **Future Proof**: Template works for any feature/phase

## Implementation:
- Always create the `code-reviews/[Phase_Name]/` folder structure first
- Use the full absolute path in the Code Review line
- **Add git commit hash immediately after creating commit**
- Include status and date for traceability
- Follow the naming convention exactly
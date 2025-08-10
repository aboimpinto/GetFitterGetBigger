Start implementing a feature from the READY_TO_DEVELOP folder following @memory-bank/DEVELOPMENT_PROCESS.md and @memory-bank/CODE_QUALITY_STANDARDS.md.
Usage: /start-implementing [FEAT-XXX or feature selection]

## Implementation Guides

**Use these while coding:**
- `/memory-bank/PracticalGuides/ServiceImplementationChecklist.md` - 📋 Step-by-step checklist
- `/memory-bank/PracticalGuides/CommonImplementationPitfalls.md` - ⚠️ Mistakes to avoid
- `/memory-bank/CODE_QUALITY_STANDARDS.md` - Quality standards to follow

Instructions:
1. List features in /memory-bank/features/1-READY_TO_DEVELOP/
2. Select the specified feature or highest priority if none specified
3. Move feature folder to /memory-bank/features/2-IN_PROGRESS/
4. Create a feature branch: `git checkout -b feature/[feature-name]`
5. Begin implementation with the first task category

Pre-implementation checks:
- [ ] Feature has complete feature-description.md
- [ ] Feature has detailed feature-tasks.md
- [ ] No other feature is currently in progress
- [ ] Current branch is clean (no uncommitted changes)
- [ ] All tests are passing on main branch

Implementation approach:
1. Start with foundational components (Models, Database)
2. Build bottom-up (Repository → Service → Controller)
3. Write tests alongside implementation
4. Commit after each completed component
5. Run tests frequently

First steps:
- Review the feature requirements thoroughly
- Check for any architectural decisions needed
- Set up the feature branch
- Begin with Category 1 tasks
- Create initial project structure if needed

Stop at first checkpoint for user review and confirmation.

Note: Only one feature should be in 2-IN_PROGRESS at a time. If another feature is active, complete or pause it first.
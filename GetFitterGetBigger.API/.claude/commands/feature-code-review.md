Perform a comprehensive code review for the current feature phase or final review.

Instructions:
1. Identify the current feature being worked on from the git branch or active context
2. **CRITICAL**: Detect if the feature uses phase-based structure (check for Phases/ directory)
3. If phases exist:
   - Identify the active phase (look for IN_PROGRESS markers)
   - Determine if this is a phase review or final review
   - REQUEST USER CONFIRMATION of detected context
4. Review ALL changes made in the feature branch against CODE_QUALITY_STANDARDS.md
5. Check all previous reviews in the appropriate phase folder
6. Perform a full scan of all files created/modified by the feature
7. Assess cross-cutting concerns (architecture, security, performance, testing)
8. Create the review report with appropriate status (APPROVED/REQUIRES_CHANGES/REJECTED)
9. Save the report with proper naming:
   - Phase review: `Code-Review-Phase-X-[Name]-{YYYY-MM-DD}-{HH-MM}-{STATUS}.md`
   - Final review: `Final-Code-Review-{YYYY-MM-DD}-{HH-MM}-{STATUS}.md`
10. Place it in the correct location:
   - Phase review: `/memory-bank/features/2-IN_PROGRESS/FEAT-XXX/code-reviews/Phase_X_[Name]/`
   - Final review: `/memory-bank/features/2-IN_PROGRESS/FEAT-XXX/code-reviews/`
11. Update the checkpoint in the appropriate document:
   - Phase review: Update checkpoint in `Phases/Phase X: [Name].md`
   - Non-phase: Update checkpoint in `feature-tasks.md`

Focus areas:
- Architecture integrity and Clean Architecture compliance
- CODE_QUALITY_STANDARDS.md compliance (all 28 Golden Rules)
- Testing coverage and quality
- Performance and security considerations
- Technical debt assessment
- Documentation completeness

**IMPORTANT**: 
- Always detect and confirm the current phase before proceeding
- Save reports in the appropriate phase-specific folder
- Update checkpoints in the correct document (phase file vs feature-tasks.md)
- The review should be thorough and actionable, clearly indicating whether the phase/feature can progress
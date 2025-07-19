Perform a comprehensive final code review for the current feature following @memory-bank/FINAL-CODE-REVIEW-TEMPLATE.md.

Instructions:
1. Identify the current feature being worked on from the git branch or active context
2. Review ALL changes made in the feature branch against CODE_QUALITY_STANDARDS.md
3. Check all previous category reviews if they exist
4. Perform a full scan of all files created/modified by the feature
5. Assess cross-cutting concerns (architecture, security, performance, testing)
6. Create the review report with appropriate status (APPROVED/APPROVED_WITH_NOTES/REQUIRES_CHANGES)
7. Save the report as: `Final-Code-Review-{YYYY-MM-DD}-{HH-MM}-{STATUS}.md`
8. Place it in: `/memory-bank/features/2-IN_PROGRESS/FEAT-XXX/code-reviews/`

Focus areas:
- Architecture integrity and Clean Architecture compliance
- CODE_QUALITY_STANDARDS.md compliance (pattern matching, Empty pattern, single exit point, etc.)
- Testing coverage and quality
- Performance and security considerations
- Technical debt assessment
- Documentation completeness

The review should be thorough and actionable, clearly indicating whether the feature is ready for completion.
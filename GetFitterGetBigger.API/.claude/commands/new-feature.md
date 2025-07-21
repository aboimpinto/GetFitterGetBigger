Create a new feature request following @memory-bank/FEATURE_WORKFLOW_PROCESS.md.

Usage: /new-feature [feature description]

Instructions:
1. Generate next feature ID from /memory-bank/features/NEXT_FEATURE_ID.txt
2. Create feature folder in /memory-bank/features/0-SUBMITTED/
3. Create feature-description.md with requirements
4. Update NEXT_FEATURE_ID.txt

Feature description should include:
- **Feature ID**: FEAT-XXX
- **Title**: Clear, descriptive title
- **Status**: SUBMITTED
- **Priority**: High/Medium/Low
- **Description**: Detailed explanation of the feature
- **User Story**: As a [user], I want [feature] so that [benefit]
- **Acceptance Criteria**: Measurable success criteria
- **Technical Considerations**: Architecture impact, dependencies
- **Estimated Effort**: T-shirt size (S/M/L/XL)

Feature folder structure:
```
/memory-bank/features/0-SUBMITTED/FEAT-XXX-brief-description/
└── feature-description.md
```

Note: Features start in SUBMITTED status. They need to be refined and moved to READY_TO_DEVELOP before implementation can begin.

Next steps after creation:
1. Refine requirements with /refine-feature
2. Create implementation tasks
3. Move to READY_TO_DEVELOP when ready
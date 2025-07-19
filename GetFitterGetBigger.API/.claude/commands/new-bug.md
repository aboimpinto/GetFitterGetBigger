Create a new bug report following @memory-bank/BUG_WORKFLOW_PROCESS.md.

Usage: /new-bug [bug description]

Instructions:
1. Generate next bug ID from /memory-bank/bugs/NEXT_BUG_ID.txt
2. Create bug folder structure in /memory-bank/bugs/1-OPEN/
3. Create bug-report.md with detailed information
4. Create bug-tasks.md with implementation steps
5. Update NEXT_BUG_ID.txt

Bug report should include:
- **Bug ID**: BUG-XXX
- **Summary**: Clear, concise description
- **Severity**: Critical/High/Medium/Low
- **Component**: Affected part of the system
- **Steps to Reproduce**: Detailed reproduction steps
- **Expected Behavior**: What should happen
- **Actual Behavior**: What actually happens
- **Root Cause**: Initial analysis if possible
- **Proposed Solution**: Suggested fix approach

Bug folder structure:
```
/memory-bank/bugs/1-OPEN/BUG-XXX-brief-description/
├── bug-report.md
└── bug-tasks.md
```

The bug will be created in 1-OPEN status, ready for implementation.
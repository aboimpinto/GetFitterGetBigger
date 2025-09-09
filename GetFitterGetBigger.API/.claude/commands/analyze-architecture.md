---
name: analyze-architecture
description: Analyzes service architecture for violations and suggests refactoring strategies
requires_arg: false
---

Analyzes the architectural health of services in the codebase, identifying violations of the 400-line rule, missing handler patterns, and extension method opportunities.

## What this command does:

1. **Service Size Analysis** - Finds all services and checks line counts
2. **Pattern Detection** - Identifies missing handlers and extensions
3. **Complexity Analysis** - Locates complex inline logic
4. **Comparison** - Compares with neighboring services
5. **Refactoring Plan** - Generates actionable improvement plan

## Usage:
```
/analyze-architecture [ServiceName]
```

If no service name provided, analyzes all services in the codebase.

## Architecture Standards:
- **Service Size**: Maximum 400 lines (non-negotiable)
- **Handler Pattern**: Complex logic must be in separate handlers
- **Extension Pattern**: Static helpers must be extensions
- **Folder Structure**: Proper organization for maintainability

## Output Includes:

### 1. Service Health Report
```
=== Service Size Report ===
🔴 VIOLATION: 956 lines - WorkoutTemplateExerciseService.cs
🟡 WARNING: 387 lines - ExerciseService.cs  
✅ OK: 245 lines - UserService.cs
```

### 2. Violation Analysis
- Services exceeding limits
- Missing handler opportunities
- Extension method candidates
- Pattern compliance issues

### 3. Refactoring Recommendations
- Specific handlers to create
- Extensions to extract
- Expected line reduction
- Priority order

### 4. Pattern Comparison
Shows how neighboring services handle similar complexity

## Red Flags:
- 🔴 Service > 600 lines (critical)
- 🟠 Service > 400 lines (violation)
- 🟡 Service > 300 lines (warning)
- ⚠️ Method > 40 lines
- ⚠️ 3+ private methods
- ⚠️ Complex inline logic

## Benefits:
- Prevents monolithic services
- Ensures maintainability
- Promotes consistent patterns
- Identifies refactoring opportunities early
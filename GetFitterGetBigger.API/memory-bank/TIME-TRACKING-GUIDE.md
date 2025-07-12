# Time Tracking Guide - Quick Reference

## 🎯 Purpose
Track actual time vs estimates to measure productivity and AI assistance impact.

## 📝 Format for Task Status Updates

### When Starting a Task
```
[InProgress: Started: YYYY-MM-DD HH:MM]
```

### When Completing a Task
```
[Implemented: <hash> | Started: YYYY-MM-DD HH:MM | Finished: YYYY-MM-DD HH:MM | Duration: Xh Ym | Est: Xh Ym]
```

**Key Components:**
- `Duration`: Actual time taken (**MUST calculate immediately when finishing**)
- `Est`: Original estimate from task definition
- **REQUIREMENT**: Calculate duration on the spot when marking task as finished
- Format shows productivity gain/loss at a glance (Est vs Duration comparison)

## 📊 Examples

### Example 1: Task Completed Faster Than Estimated
```
Task 2.1: Create IExerciseRepository interface
[Implemented: abc123 | Started: 2025-01-15 14:00 | Finished: 2025-01-15 14:05 | Duration: 0h 5m | Est: 0h 15m]
```
**Interpretation**: Estimated 15 minutes, actually took 5 minutes (67% time saved)
**Quick Comparison**: Duration: 0h 5m | Est: 0h 15m (10 minutes under estimate)

### Example 2: Task Took Longer Than Estimated
```
Task 3.2: Implement ExerciseService with business logic
[Implemented: def456 | Started: 2025-01-15 15:00 | Finished: 2025-01-15 16:30 | Duration: 1h 30m | Est: 1h 0m]
```
**Interpretation**: Estimated 1 hour, actually took 1.5 hours (50% over estimate)

### Example 3: Task with Interruptions
```
Task 4.1: Create ExerciseController with CRUD endpoints
[Implemented: ghi789 | Started: 2025-01-15 09:00 | Finished: 2025-01-15 11:30 | Duration: 1h 45m | Est: 2h 0m]
Note: Work sessions: 09:00-10:00 (1h), 11:00-11:45 (45m)
```

## 🧮 How to Calculate Duration

1. **Simple Case** (continuous work):
   - Started: 14:00, Finished: 14:45
   - Duration: 0h 45m

2. **With Breaks**:
   - Track actual work time only
   - Started: 09:00, Lunch: 12:00-13:00, Finished: 15:00
   - Total elapsed: 6 hours
   - Actual work: 5 hours
   - Duration: 5h 0m

3. **Multi-day Tasks**:
   - Day 1: 14:00-17:00 (3h)
   - Day 2: 09:00-10:30 (1h 30m)
   - Duration: 4h 30m

## 📈 Benefits

1. **Immediate Feedback**: See if estimates are realistic
2. **AI Impact**: Quantify time saved with AI assistance
3. **Better Estimates**: Learn from actual times for future tasks
4. **Transparency**: Clear record of effort invested

## 🚀 Quick Tips

1. **Be Honest**: Record actual work time, not elapsed time
2. **Round to 5 minutes**: For consistency (5m, 10m, 15m, etc.)
3. **Update Immediately**: Calculate duration right when you finish
4. **Include Notes**: For interrupted tasks, add work session details

## 📊 Summary Calculation

At feature completion, calculate total impact:
```
Total Estimated: Sum of all Est values
Total Actual: Sum of all Duration values
AI Impact: ((Estimated - Actual) / Estimated) × 100%
```

### Example Summary
```
Tasks Completed: 25
Total Estimated: 41h 0m
Total Actual: 2h 40m
AI Impact: 93.5% reduction in time
```

## 🔗 Related Documentation
- `FEATURE_IMPLEMENTATION_PROCESS.md` - Full task status definitions
- `BUG_IMPLEMENTATION_PROCESS.md` - Bug task tracking
- `BUILD-BEST-PRACTICES.md` - Quality checks between tasks